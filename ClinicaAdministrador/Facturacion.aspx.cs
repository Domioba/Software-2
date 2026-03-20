using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using ClinicaAdministrador.DAL;
using System.Web.Services;
using System.Web.Script.Serialization;

namespace ClinicaAdministrador
{
    public partial class Facturacion : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarCitasDDL();
                CargarMetodosPagoDDL();
                CargarEstadosPagoDDL();
                CargarFacturas();
            }
        }

        private void CargarCitasDDL()
        {
            using (SqlConnection con = DatabaseHelper.GetConnection())
            {
                string query = @"
                    SELECT C.IDCita, 
                           P.NombreCompleto + ' - ' + CONVERT(NVARCHAR, C.Fecha, 105) AS DescripcionCita
                    FROM Citas C
                    INNER JOIN Pacientes P ON C.IDPaciente = P.IDPaciente
                    WHERE C.Estado = 'Realizada'
                    ORDER BY C.Fecha DESC";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        sda.Fill(dt);
                        ddlCita.DataSource = dt;
                        ddlCita.DataTextField = "DescripcionCita";
                        ddlCita.DataValueField = "IDCita";
                        ddlCita.DataBind();
                    }
                }
            }
            ddlCita.Items.Insert(0, new ListItem("-- Seleccione una Cita --", "0"));
        }

        [WebMethod]
        public static string ObtenerDatosPorCita(int idCita)
        {
            var connStr = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            var datos = new Dictionary<string, object>
            {
                { "paciente", null },
                { "servicios", new List<object>() }
            };

            try
            {
                using (SqlConnection con = new SqlConnection(connStr))
                {
                    con.Open();

                    string queryPaciente = @"
                        SELECT P.IDPaciente, P.NombreCompleto, C.Fecha
                        FROM Citas C
                        INNER JOIN Pacientes P ON C.IDPaciente = P.IDPaciente
                        WHERE C.IDCita = @IDCita";

                    using (SqlCommand cmd = new SqlCommand(queryPaciente, con))
                    {
                        cmd.Parameters.AddWithValue("@IDCita", idCita);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                datos["paciente"] = new
                                {
                                    id = reader["IDPaciente"],
                                    nombre = reader["NombreCompleto"],
                                    fechaCita = Convert.ToDateTime(reader["Fecha"]).ToString("yyyy-MM-dd")
                                };
                            }
                        }
                    }

                    var listaServicios = (List<object>)datos["servicios"];
                    string queryServicios = @"
                        SELECT S.IDServicio, S.NombreServicio, CS.PrecioUnitario
                        FROM Citas_Servicios CS
                        INNER JOIN Servicios S ON CS.IDServicio = S.IDServicio
                        WHERE CS.IDCita = @IDCita";

                    using (SqlCommand cmd = new SqlCommand(queryServicios, con))
                    {
                        cmd.Parameters.AddWithValue("@IDCita", idCita);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                listaServicios.Add(new
                                {
                                    id = reader["IDServicio"],
                                    nombre = reader["NombreServicio"],
                                    precio = Convert.ToDecimal(reader["PrecioUnitario"]).ToString("N2")
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return "{\"error\":\"Error en el servidor: " + ex.Message.Replace("\"", "\\\"") + "\"}";
            }

            var js = new JavaScriptSerializer();
            return js.Serialize(datos);
        }

        private void CargarMetodosPagoDDL()
        {
            ddlMetodoPago.Items.Add(new ListItem("-- Seleccione --", "0"));
            ddlMetodoPago.Items.Add(new ListItem("Efectivo", "Efectivo"));
            ddlMetodoPago.Items.Add(new ListItem("Tarjeta de Crédito/Débito", "Tarjeta"));
            ddlMetodoPago.Items.Add(new ListItem("Transferencia Bancaria", "Transferencia"));
        }

        private void CargarEstadosPagoDDL()
        {
            ddlEstadoPago.Items.Add(new ListItem("-- Seleccione --", "0"));
            ddlEstadoPago.Items.Add(new ListItem("Pendiente", "Pendiente"));
            ddlEstadoPago.Items.Add(new ListItem("Pagado", "Pagado"));
            ddlEstadoPago.Items.Add(new ListItem("Anulado", "Anulado"));
        }

        private void CargarFacturas()
        {
            using (SqlConnection con = DatabaseHelper.GetConnection())
            {
                string query = @"SELECT IDFactura, IDPaciente, NombrePaciente, Fecha, Total, MetodoPago, EstadoPago, Servicio
                                 FROM (
                                     SELECT f.IDFactura, f.IDPaciente, p.NombreCompleto AS NombrePaciente, f.Fecha, f.Total, f.MetodoPago, f.EstadoPago, f.Servicio,
                                            ROW_NUMBER() OVER (PARTITION BY f.IDFactura ORDER BY f.IDFactura) as rn
                                     FROM Facturacion f
                                     INNER JOIN Pacientes p ON f.IDPaciente = p.IDPaciente
                                 ) AS Subquery
                                 WHERE rn = 1
                                 ORDER BY Fecha DESC";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        sda.Fill(dt);
                        gvFacturas.DataSource = dt;
                        gvFacturas.DataBind();
                    }
                }
            }
        }

        protected void btnNuevaFactura_Click(object sender, EventArgs e)
        {
            hfIDFactura.Value = "";
            ddlCita.ClearSelection();
            ddlPaciente.Items.Clear();
            txtFechaFactura.Text = DateTime.Now.ToString("yyyy-MM-dd");
            lblTotalPrecio.Text = "$0.00";

            tituloFormularioFactura.InnerText = "Nueva Factura";
            pnlFormularioFactura.Visible = true;
        }

        protected void btnGuardarFactura_Click(object sender, EventArgs e)
        {
            if (ddlCita.SelectedValue == "0" || ddlMetodoPago.SelectedValue == "0" || ddlEstadoPago.SelectedValue == "0")
            {
                lblMensajeError.Text = "Debe seleccionar una cita, un método de pago y un estado de pago.";
                lblMensajeError.Visible = true;
                return;
            }

            int idCita = Convert.ToInt32(ddlCita.SelectedValue);
            List<string> nombresServiciosFinal = new List<string>();
            decimal totalFinal = 0;

            using (SqlConnection con = DatabaseHelper.GetConnection())
            {
                con.Open();
                string queryServicios = @"
                    SELECT S.NombreServicio, CS.PrecioUnitario
                    FROM Citas_Servicios CS
                    INNER JOIN Servicios S ON CS.IDServicio = S.IDServicio
                    WHERE CS.IDCita = @IDCita";

                using (SqlCommand cmd = new SqlCommand(queryServicios, con))
                {
                    cmd.Parameters.AddWithValue("@IDCita", idCita);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            nombresServiciosFinal.Add(reader["NombreServicio"].ToString());
                            totalFinal += Convert.ToDecimal(reader["PrecioUnitario"]);
                        }
                    }
                }
            }

            string serviciosString = string.Join(", ", nombresServiciosFinal);

            using (SqlConnection con = DatabaseHelper.GetConnection())
            {
                con.Open();
                try
                {
                    string query;
                    if (string.IsNullOrEmpty(hfIDFactura.Value))
                    {
                        query = @"INSERT INTO Facturacion (IDPaciente, Fecha, Total, MetodoPago, EstadoPago, Servicio) 
                                  VALUES (@IDPaciente, @Fecha, @Total, @MetodoPago, @EstadoPago, @Servicio)";
                    }
                    else
                    {
                        query = @"UPDATE Facturacion 
                                  SET IDPaciente = @IDPaciente, Fecha = @Fecha, Total = @Total, MetodoPago = @MetodoPago, EstadoPago = @EstadoPago, Servicio = @Servicio
                                  WHERE IDFactura = @IDFactura";
                    }

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@IDPaciente", hfIDPaciente.Value);
                        cmd.Parameters.AddWithValue("@Fecha", txtFechaFactura.Text);
                        cmd.Parameters.AddWithValue("@Total", totalFinal);
                        cmd.Parameters.AddWithValue("@MetodoPago", ddlMetodoPago.SelectedValue);
                        cmd.Parameters.AddWithValue("@EstadoPago", ddlEstadoPago.SelectedValue);
                        cmd.Parameters.AddWithValue("@Servicio", serviciosString);

                        if (!string.IsNullOrEmpty(hfIDFactura.Value))
                        {
                            cmd.Parameters.AddWithValue("@IDFactura", Convert.ToInt32(hfIDFactura.Value));
                        }

                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    lblMensajeError.Text = "Error al guardar la factura: " + ex.Message;
                    lblMensajeError.Visible = true;
                    return;
                }
            }

            pnlFormularioFactura.Visible = false;
            CargarFacturas();
        }

        protected void btnCancelarFactura_Click(object sender, EventArgs e)
        {
            pnlFormularioFactura.Visible = false;
        }

        protected void gvFacturas_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int idFactura = Convert.ToInt32(e.CommandArgument);

            if (e.CommandName == "VerFactura")
            {
                Response.Redirect($"VerFactura.aspx?id={idFactura}");
            }
            else if (e.CommandName == "Eliminar")
            {
                using (SqlConnection con = DatabaseHelper.GetConnection())
                {
                    string query = "DELETE FROM Facturacion WHERE IDFactura = @IDFactura";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@IDFactura", idFactura);
                        con.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
                CargarFacturas();
            }
        }
    }
}