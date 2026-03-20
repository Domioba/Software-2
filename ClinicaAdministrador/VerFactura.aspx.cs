// VerFactura.aspx.cs
using System;
using System.Data.SqlClient;
using System.Web.UI;
using ClinicaAdministrador.DAL;

namespace ClinicaAdministrador
{
    public partial class VerFactura : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // 1. Obtener el ID desde la URL (query string)
                string idFacturaStr = Request.QueryString["id"];
                int idFactura;

                // 2. Validar que el ID sea un número válido
                if (string.IsNullOrEmpty(idFacturaStr) || !int.TryParse(idFacturaStr, out idFactura))
                {
                    // Si el ID no es válido, redirigimos de vuelta a la lista
                    Response.Redirect("Facturacion.aspx");
                    return;
                }

                // 3. Cargar los detalles de la factura
                CargarDetallesFactura(idFactura);
            }
        }

        private void CargarDetallesFactura(int idFactura)
        {
            using (SqlConnection con = DatabaseHelper.GetConnection())
            {
                string query = @"SELECT f.IDFactura, p.NombreCompleto, f.Fecha, f.Servicio, f.Total, f.MetodoPago, f.EstadoPago
                                 FROM Facturacion f
                                 INNER JOIN Pacientes p ON f.IDPaciente = p.IDPaciente
                                 WHERE f.IDFactura = @IDFactura";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@IDFactura", idFactura);
                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // 4. Llenar los controles con los datos de la BD
                            lblIDFactura.Text += reader["IDFactura"].ToString();
                            lblPaciente.Text = reader["NombreCompleto"].ToString();
                            lblFecha.Text = Convert.ToDateTime(reader["Fecha"]).ToString("dd/MM/yyyy");
                            lblMetodoPago.Text = reader["MetodoPago"].ToString();
                            lblServicios.Text = reader["Servicio"].ToString();
                            lblTotal.Text = Convert.ToDecimal(reader["Total"]).ToString("C");

                            string estadoPago = reader["EstadoPago"].ToString();
                            lblEstadoPago.Text = estadoPago;
                            lblEstadoPago.CssClass = estadoPago == "Pagado" ? "badge bg-success" : "badge bg-warning";
                        }
                        else
                        {
                            // Si no se encuentra la factura, redirigimos de vuelta
                            Response.Redirect("Facturacion.aspx");
                        }
                    }
                }
            }
        }
    }
}