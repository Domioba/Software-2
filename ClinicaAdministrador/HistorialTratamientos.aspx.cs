using ClinicaAdministrador.DAL;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ClinicaAdministrador
{
    public partial class HistorialTratamientos : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarPacientes();
            }
        }

        private void CargarPacientes()
        {
            try
            {
                string query = "SELECT IDPaciente, NombreCompleto FROM Pacientes ORDER BY NombreCompleto";

                using (SqlConnection con = DatabaseHelper.GetConnection())
                {
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            sda.Fill(dt);

                            if (dt.Rows.Count > 0)
                            {
                                ddlPaciente.DataSource = dt;
                                ddlPaciente.DataTextField = "NombreCompleto";
                                ddlPaciente.DataValueField = "IDPaciente";
                                ddlPaciente.DataBind();
                            }
                            else
                            {
                                ClientScript.RegisterStartupScript(this.GetType(), "alert", "alert('No se encontraron pacientes en la base de datos. Verifique que la tabla Pacientes tenga registros.');", true);
                            }
                        }
                    }
                }

                ddlPaciente.Items.Insert(0, new ListItem("-- Seleccione un Paciente --", "0"));
            }
            catch (Exception ex)
            {
                ClientScript.RegisterStartupScript(this.GetType(), "alert", $"alert('Ocurrió un error al cargar los pacientes: {ex.Message}');", true);
            }
        }

        protected void btnConsultar_Click(object sender, EventArgs e)
        {
            if (ddlPaciente.SelectedValue == "0")
            {
                gvHistorial.DataSource = null;
                gvHistorial.DataBind();
                return;
            }

            using (SqlConnection con = DatabaseHelper.GetConnection())
            {
                // CONSULTA SQL CORREGIDA
                // Se une la tabla cabecera (TratamientosRealizados) con la de detalle (TratamientosRealizados_Detalle)
                // y con las tablas de Servicios, Inventario y Administrador para obtener todos los nombres.
                string query = @"
                    SELECT 
                        tr.FechaTratamiento, 
                        s.NombreServicio,
                        i.NombreProducto, 
                        s.Descripcion,
                        td.CantidadUtilizada AS Dosis,
                        tr.ObservacionesGenerales AS Observaciones,
                        a.NombreCompleto AS NombreAdmin
                    FROM TratamientosRealizados tr
                    JOIN TratamientosRealizados_Detalle td ON tr.IDTratamiento = td.IDTratamiento
                    JOIN Servicios s ON td.IDServicio = s.IDServicio
                    JOIN Inventario i ON td.IDProducto = i.IDProducto
                    JOIN Administrador a ON tr.IDAdmin = a.IDAdmin
                    WHERE tr.IDPaciente = @IDPaciente
                    ORDER BY tr.FechaTratamiento DESC";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@IDPaciente", ddlPaciente.SelectedValue);
                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        sda.Fill(dt);
                        gvHistorial.DataSource = dt;
                        gvHistorial.DataBind();
                    }
                }
            }
        }
    }
}