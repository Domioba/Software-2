using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web.UI.WebControls;
using ClinicaAdministrador.DAL;

namespace ClinicaAdministrador
{
    public partial class Reportes : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarAnios();
                if (!String.IsNullOrEmpty(Request.QueryString["type"]))
                {
                    CargarFiltrosDesdeURL();
                    btnGenerar_Click(null, null);
                }
            }
        }

        private void CargarAnios()
        {
            int anioActual = DateTime.Now.Year;
            for (int i = anioActual; i >= anioActual - 5; i--)
            {
                ddlAnio.Items.Add(new System.Web.UI.WebControls.ListItem(i.ToString(), i.ToString()));
            }
            ddlAnio.SelectedValue = anioActual.ToString();
            ddlMes.SelectedValue = DateTime.Now.Month.ToString();
        }

        private void CargarFiltrosDesdeURL()
        {
            string tipo = Request.QueryString["type"];
            string mes = Request.QueryString["month"];
            string anio = Request.QueryString["year"];

            ddlTipoReporte.SelectedValue = tipo;
            ddlMes.SelectedValue = mes;
            ddlAnio.SelectedValue = anio;
        }

        protected void btnGenerar_Click(object sender, EventArgs e)
        {
            string tipoReporte = ddlTipoReporte.SelectedValue;
            int mes = Convert.ToInt32(ddlMes.SelectedValue);
            int anio = Convert.ToInt32(ddlAnio.SelectedValue);

            DateTime fechaInicio = new DateTime(anio, mes, 1);
            DateTime fechaFin = fechaInicio.AddMonths(1).AddDays(-1);

            gvReporte.Columns.Clear();
            DataTable dt = new DataTable();

            // Limpiamos el label del total antes de generar un nuevo reporte para evitar que se muestre un valor anterior.
            lblTotal.Text = string.Empty;

            switch (tipoReporte)
            {
                case "ingresos":
                    dt = GenerarReporteIngresosMejorado(fechaInicio, fechaFin);
                    gvReporte.Columns.Add(new BoundField { DataField = "Fecha", HeaderText = "Fecha" });
                    gvReporte.Columns.Add(new BoundField { DataField = "Paciente", HeaderText = "Paciente" });
                    gvReporte.Columns.Add(new BoundField { DataField = "MetodoPago", HeaderText = "Método de Pago" });
                    gvReporte.Columns.Add(new BoundField { DataField = "Total", HeaderText = "Total Ingresos", DataFormatString = "{0:C}" });
                    tituloResultado.InnerText = $"Reporte de Ingresos de {fechaInicio:MMMM yyyy}";
                    break;
                case "servicios":
                    dt = GenerarReporteServiciosMejorado(fechaInicio, fechaFin);
                    gvReporte.Columns.Add(new BoundField { DataField = "Servicio", HeaderText = "Servicio" });
                    gvReporte.Columns.Add(new BoundField { DataField = "Cantidad", HeaderText = "Cantidad" });
                    gvReporte.Columns.Add(new BoundField { DataField = "Ingresos Generados", HeaderText = "Ingresos Generados", DataFormatString = "{0:C}" });
                    tituloResultado.InnerText = $"Reporte de Servicios de {fechaInicio:MMMM yyyy}";
                    break;
                case "clientes":
                    dt = GenerarReporteClientesMejorado(fechaInicio, fechaFin);
                    gvReporte.Columns.Add(new BoundField { DataField = "Métrica", HeaderText = "Métrica" });
                    gvReporte.Columns.Add(new BoundField { DataField = "Valor", HeaderText = "Total" });
                    tituloResultado.InnerText = $"Reporte de Clientes de {fechaInicio:yyyy}";
                    break;
            }

            Session["DatosReporte"] = dt;
            Session["TituloReporte"] = tituloResultado.InnerText;

            gvReporte.DataSource = dt;
            gvReporte.DataBind();
            pnlResultado.Visible = true;
        }

        // --- MÉTODOS MEJORADOS (sin cambios en la lógica, solo en las columnas devueltas) ---
        private DataTable GenerarReporteIngresosMejorado(DateTime inicio, DateTime fin)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Fecha", typeof(string));
            dt.Columns.Add("Paciente", typeof(string));
            dt.Columns.Add("MetodoPago", typeof(string));
            dt.Columns.Add("Total", typeof(decimal));
            decimal totalGeneral = 0;

            using (SqlConnection con = DatabaseHelper.GetConnection())
            {
                string query = "SELECT f.Fecha, p.NombreCompleto AS Paciente, f.MetodoPago, f.Total FROM facturacion f INNER JOIN Pacientes p ON f.IDPaciente = p.IDPaciente WHERE f.Fecha BETWEEN @Inicio AND @Fin AND f.EstadoPago = 'Pagado' ORDER BY f.Fecha";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Inicio", inicio);
                    cmd.Parameters.AddWithValue("@Fin", fin);
                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            dt.Rows.Add(Convert.ToDateTime(reader["Fecha"]).ToString("dd/MM/yyyy"), reader["Paciente"], reader["MetodoPago"], reader["Total"]);
                            totalGeneral += Convert.ToDecimal(reader["Total"]);
                        }
                    }
                }
            }
            // Ahora que lblTotal existe en el .aspx, esta línea funcionará.
            lblTotal.Text = $"Total del Periodo: {totalGeneral:C}";
            return dt;
        }

        private DataTable GenerarReporteServiciosMejorado(DateTime inicio, DateTime fin)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Servicio", typeof(string));
            dt.Columns.Add("Cantidad", typeof(int));
            dt.Columns.Add("Ingresos Generados", typeof(decimal));

            using (SqlConnection con = DatabaseHelper.GetConnection())
            {
                string query = @"
                    SELECT s.NombreServicio, COUNT(cs.IDCita) AS Cantidad, SUM(cs.PrecioUnitario) AS IngresosGenerados
                    FROM Citas c
                    INNER JOIN Citas_Servicios cs ON c.IDCita = cs.IDCita
                    INNER JOIN Servicios s ON cs.IDServicio = s.IDServicio
                    WHERE c.Fecha BETWEEN @Inicio AND @Fin
                    GROUP BY s.NombreServicio
                    ORDER BY IngresosGenerados DESC";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Inicio", inicio);
                    cmd.Parameters.AddWithValue("@Fin", fin);
                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            dt.Rows.Add(reader["NombreServicio"], reader["Cantidad"], reader["IngresosGenerados"]);
                        }
                    }
                }
            }
            return dt;
        }

        private DataTable GenerarReporteClientesMejorado(DateTime inicio, DateTime fin)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Métrica", typeof(string));
            dt.Columns.Add("Valor", typeof(int));
            int totalClientes = 0;

            using (SqlConnection con = DatabaseHelper.GetConnection())
            {
                string query = "SELECT COUNT(DISTINCT IDPaciente) AS Total FROM Citas WHERE Fecha BETWEEN @Inicio AND @Fin";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Inicio", inicio);
                    cmd.Parameters.AddWithValue("@Fin", fin);
                    con.Open();
                    totalClientes = (int)cmd.ExecuteScalar();
                }
            }
            dt.Rows.Add("Clientes Únicos Atendidos", totalClientes);
            return dt;
        }
    }
}