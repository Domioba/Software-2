// Servicios.aspx.cs
using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;
using ClinicaAdministrador.DAL;

namespace ClinicaAdministrador
{
    public partial class Servicios : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarServicios();
            }
        }

        private void CargarServicios()
        {
            try
            {
                using (SqlConnection con = DatabaseHelper.GetConnection())
                {
                    string query = "SELECT * FROM Servicios ORDER BY Categoria, NombreServicio";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            sda.Fill(dt);
                            gvServicios.DataSource = dt;
                            gvServicios.DataBind();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MostrarMensajeError("Error al cargar la lista de servicios: " + ex.Message);
            }
        }

        protected void btnNuevoServicio_Click(object sender, EventArgs e)
        {
            // Limpiar el formulario
            hfIDServicio.Value = "";
            txtNombreServicio.Text = "";
            txtDescripcion.Text = "";
            txtPrecio.Text = "";
            txtDuracion.Text = "";
            ddlCategoria.SelectedIndex = 0;
            ddlEstado.SelectedIndex = 0;

            tituloFormularioServicio.InnerText = "Nuevo Servicio";
            pnlFormularioServicio.Visible = true;
            ClearMessages(); // Limpiamos cualquier mensaje previo
        }

        protected void btnGuardarServicio_Click(object sender, EventArgs e)
        {
            try
            {
                // --- VALIDACIONES EN EL SERVIDOR (Crucial para la seguridad) ---
                if (string.IsNullOrWhiteSpace(txtNombreServicio.Text))
                {
                    MostrarMensajeError("El nombre del servicio es obligatorio.");
                    return;
                }

                decimal precio;
                if (!decimal.TryParse(txtPrecio.Text, out precio) || precio < 0)
                {
                    MostrarMensajeError("Por favor, ingrese un precio válido y positivo.");
                    return;
                }

                int duracion;
                if (!int.TryParse(txtDuracion.Text, out duracion) || duracion <= 0)
                {
                    MostrarMensajeError("Por favor, ingrese una duración válida en minutos (mayor a cero).");
                    return;
                }
                // --- FIN DE LAS VALIDACIONES ---

                using (SqlConnection con = DatabaseHelper.GetConnection())
                {
                    con.Open();
                    string query;
                    if (string.IsNullOrEmpty(hfIDServicio.Value))
                    {
                        // INSERTAR NUEVO SERVICIO
                        query = @"INSERT INTO Servicios (NombreServicio, Categoria, Descripcion, Precio, Duracion, Estado) 
                                  VALUES (@Nombre, @Categoria, @Descripcion, @Precio, @Duracion, @Estado)";
                    }
                    else
                    {
                        // ACTUALIZAR SERVICIO EXISTENTE
                        query = @"UPDATE Servicios 
                                  SET NombreServicio = @Nombre, Categoria = @Categoria, Descripcion = @Descripcion, 
                                      Precio = @Precio, Duracion = @Duracion, Estado = @Estado 
                                  WHERE IDServicio = @IDServicio";
                    }

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@Nombre", txtNombreServicio.Text.Trim());
                        cmd.Parameters.AddWithValue("@Categoria", ddlCategoria.SelectedValue);
                        cmd.Parameters.AddWithValue("@Descripcion", txtDescripcion.Text.Trim());
                        cmd.Parameters.AddWithValue("@Precio", precio);
                        cmd.Parameters.AddWithValue("@Duracion", duracion);
                        cmd.Parameters.AddWithValue("@Estado", ddlEstado.SelectedValue);

                        if (!string.IsNullOrEmpty(hfIDServicio.Value))
                        {
                            cmd.Parameters.AddWithValue("@IDServicio", Convert.ToInt32(hfIDServicio.Value));
                        }

                        cmd.ExecuteNonQuery();
                    }
                }

                // Ocultar formulario, mostrar éxito y recargar lista
                pnlFormularioServicio.Visible = false;
                MostrarMensajeExito("Servicio guardado exitosamente.");
                CargarServicios();
            }
            catch (Exception ex)
            {
                MostrarMensajeError("Ocurrió un error inesperado al guardar el servicio: " + ex.Message);
            }
        }

        protected void btnCancelarServicio_Click(object sender, EventArgs e)
        {
            pnlFormularioServicio.Visible = false;
            ClearMessages(); // Limpiamos mensajes al cancelar
        }

        protected void gvServicios_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            try
            {
                int idServicio = Convert.ToInt32(e.CommandArgument);

                if (e.CommandName == "Editar")
                {
                    CargarServicioParaEditar(idServicio);
                }
                else if (e.CommandName == "Eliminar")
                {
                    // En lugar de eliminar, lo desactivamos para proteger la integridad de los datos.
                    // Un servicio que ya fue usado en citas no debería poder ser eliminado.
                    DesactivarServicio(idServicio);
                }
            }
            catch (Exception ex)
            {
                MostrarMensajeError("Error al procesar la acción: " + ex.Message);
            }
        }

        private void CargarServicioParaEditar(int idServicio)
        {
            using (SqlConnection con = DatabaseHelper.GetConnection())
            {
                string query = "SELECT * FROM Servicios WHERE IDServicio = @IDServicio";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@IDServicio", idServicio);
                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            hfIDServicio.Value = reader["IDServicio"].ToString();
                            txtNombreServicio.Text = reader["NombreServicio"].ToString();
                            txtDescripcion.Text = reader["Descripcion"].ToString();
                            txtPrecio.Text = reader["Precio"].ToString();
                            txtDuracion.Text = reader["Duracion"].ToString();
                            ddlCategoria.SelectedValue = reader["Categoria"].ToString();
                            ddlEstado.SelectedValue = reader["Estado"].ToString();

                            tituloFormularioServicio.InnerText = "Editar Servicio";
                            pnlFormularioServicio.Visible = true;
                            ClearMessages();
                        }
                        else
                        {
                            MostrarMensajeError("No se encontró el servicio a editar.");
                        }
                    }
                }
            }
        }

        private void DesactivarServicio(int idServicio)
        {
            using (SqlConnection con = DatabaseHelper.GetConnection())
            {
                string query = "UPDATE Servicios SET Estado = 'Inactivo' WHERE IDServicio = @IDServicio";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@IDServicio", idServicio);
                    con.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MostrarMensajeExito("Servicio desactivado correctamente.");
                    }
                    else
                    {
                        MostrarMensajeError("No se pudo desactivar el servicio. Es posible que ya esté inactivo.");
                    }
                }
            }
            CargarServicios();
        }

        #region Métodos de Aypara UI

        private void ClearMessages()
        {
            lblMensajeError.Visible = false;
            lblMensajeExito.Visible = false;
        }

        private void MostrarMensajeError(string mensaje)
        {
            // Asegúrate de tener estos Labels en tu archivo .aspx
            lblMensajeError.Text = mensaje;
            lblMensajeError.Visible = true;
            lblMensajeExito.Visible = false;
        }

        private void MostrarMensajeExito(string mensaje)
        {
            // Asegúrate de tener estos Labels en tu archivo .aspx
            lblMensajeExito.Text = mensaje;
            lblMensajeExito.Visible = true;
            lblMensajeError.Visible = false;
        }

        #endregion
    }
}