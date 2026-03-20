using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;
using ClinicaAdministrador.DAL;

namespace ClinicaAdministrador
{
    public partial class Inventario : System.Web.UI.Page
    {
        // Definimos los umbrales para las alertas
        private const int UmbralStockBajo = 10;
        private const int DiasParaVencimiento = 30;

        // Límites realistas para una clínica estética
        private const decimal MAX_CANTIDAD = 100;
        private const decimal MAX_COSTO = 10000;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarInventario();
                ClearMessages();
            }
        }

        private void CargarInventario()
        {
            using (SqlConnection con = DatabaseHelper.GetConnection())
            {
                string query = "SELECT * FROM Inventario ORDER BY Estado DESC, Categoria, NombreProducto";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        sda.Fill(dt);
                        gvInventario.DataSource = dt;
                        gvInventario.DataBind();
                    }
                }
            }
        }

        protected void btnNuevoProducto_Click(object sender, EventArgs e)
        {
            LimpiarFormulario();
            tituloFormularioProducto.InnerText = "Nuevo Producto";
            pnlFormularioProducto.Visible = true;
            ClearMessages();
        }

        protected void btnGuardarProducto_Click(object sender, EventArgs e)
        {
            try
            {
                // Validaciones del servidor
                if (!ValidarDatosProducto())
                {
                    return;
                }

                using (SqlConnection con = DatabaseHelper.GetConnection())
                {
                    string query;
                    if (string.IsNullOrEmpty(hfIDProducto.Value))
                    {
                        query = @"INSERT INTO Inventario (NombreProducto, Categoria, CantidadDisponible, UnidadMedida, FechaVencimiento, CostoUnitario, Estado) 
                                  VALUES (@Nombre, @Categoria, @Cantidad, @Unidad, @Vencimiento, @Costo, @Estado)";
                    }
                    else
                    {
                        query = @"UPDATE Inventario 
                                  SET NombreProducto = @Nombre, Categoria = @Categoria, CantidadDisponible = @Cantidad, UnidadMedida = @Unidad, 
                                      FechaVencimiento = @Vencimiento, CostoUnitario = @Costo, Estado = @Estado 
                                  WHERE IDProducto = @IDProducto";
                    }

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@Nombre", txtNombreProducto.Text.Trim());
                        cmd.Parameters.AddWithValue("@Categoria", txtCategoria.Text.Trim());
                        cmd.Parameters.AddWithValue("@Cantidad", Convert.ToDecimal(txtCantidad.Text));
                        cmd.Parameters.AddWithValue("@Unidad", ddlUnidadMedida.SelectedValue);

                        // Manejar fecha de vencimiento nula
                        if (string.IsNullOrEmpty(txtFechaVencimiento.Text))
                        {
                            cmd.Parameters.AddWithValue("@Vencimiento", DBNull.Value);
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@Vencimiento", txtFechaVencimiento.Text);
                        }

                        cmd.Parameters.AddWithValue("@Costo", Convert.ToDecimal(txtCostoUnitario.Text));
                        cmd.Parameters.AddWithValue("@Estado", ddlEstado.SelectedValue);

                        if (!string.IsNullOrEmpty(hfIDProducto.Value))
                        {
                            cmd.Parameters.AddWithValue("@IDProducto", hfIDProducto.Value);
                        }

                        con.Open();
                        cmd.ExecuteNonQuery();
                    }
                }

                pnlFormularioProducto.Visible = false;
                MostrarMensajeExito("Producto guardado exitosamente.");
                CargarInventario();
            }
            catch (Exception ex)
            {
                MostrarMensajeError("Error al guardar el producto: " + ex.Message);
            }
        }

        private bool ValidarDatosProducto()
        {
            // Validar nombre
            if (string.IsNullOrWhiteSpace(txtNombreProducto.Text))
            {
                MostrarMensajeError("El nombre del producto es requerido.");
                return false;
            }

            // Validar categoría
            if (string.IsNullOrWhiteSpace(txtCategoria.Text))
            {
                MostrarMensajeError("La categoría es requerida.");
                return false;
            }

            // Validar unidad de medida
            if (ddlUnidadMedida.SelectedValue == "")
            {
                MostrarMensajeError("Debe seleccionar una unidad de medida.");
                return false;
            }

            // Validar cantidad
            if (!decimal.TryParse(txtCantidad.Text, out decimal cantidad) || cantidad < 0)
            {
                MostrarMensajeError("La cantidad debe ser un número positivo o cero.");
                return false;
            }

            if (cantidad > MAX_CANTIDAD)
            {
                MostrarMensajeError($"La cantidad no puede ser mayor a {MAX_CANTIDAD} unidades.");
                return false;
            }

            // Validar que sea entero para unidades
            if (ddlUnidadMedida.SelectedValue == "unidades" && cantidad % 1 != 0)
            {
                MostrarMensajeError("Para unidades, la cantidad debe ser un número entero.");
                return false;
            }

            // Validar costo
            if (!decimal.TryParse(txtCostoUnitario.Text, out decimal costo) || costo < 0)
            {
                MostrarMensajeError("El costo unitario debe ser un número positivo o cero.");
                return false;
            }

            if (costo > MAX_COSTO)
            {
                MostrarMensajeError($"El costo unitario no puede ser mayor a ${MAX_COSTO:N2}.");
                return false;
            }

            // Validar fecha de vencimiento (si se especifica)
            if (!string.IsNullOrEmpty(txtFechaVencimiento.Text))
            {
                if (!DateTime.TryParse(txtFechaVencimiento.Text, out DateTime fechaVencimiento))
                {
                    MostrarMensajeError("La fecha de vencimiento no es válida.");
                    return false;
                }

                if (fechaVencimiento < DateTime.Today)
                {
                    MostrarMensajeError("La fecha de vencimiento no puede ser en el pasado.");
                    return false;
                }
            }

            // Advertencia para productos activos con cantidad cero
            if (ddlEstado.SelectedValue == "Activo" && cantidad == 0)
            {
                // Esto es solo una advertencia, no un error
                // El usuario ya confirmó en el cliente
            }

            return true;
        }

        protected void btnCancelarProducto_Click(object sender, EventArgs e)
        {
            pnlFormularioProducto.Visible = false;
            ClearMessages();
        }

        protected void gvInventario_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int idProducto = Convert.ToInt32(e.CommandArgument);

            if (e.CommandName == "Editar")
            {
                using (SqlConnection con = DatabaseHelper.GetConnection())
                {
                    string query = "SELECT * FROM Inventario WHERE IDProducto = @IDProducto";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@IDProducto", idProducto);
                        con.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                hfIDProducto.Value = reader["IDProducto"].ToString();
                                txtNombreProducto.Text = reader["NombreProducto"].ToString();
                                txtCategoria.Text = reader["Categoria"].ToString();
                                txtCantidad.Text = Convert.ToDecimal(reader["CantidadDisponible"]).ToString("F2");

                                // Cargar unidad de medida en el DropDownList
                                string unidad = reader["UnidadMedida"].ToString();
                                if (ddlUnidadMedida.Items.FindByValue(unidad) != null)
                                {
                                    ddlUnidadMedida.SelectedValue = unidad;
                                }

                                if (reader["FechaVencimiento"] != DBNull.Value)
                                {
                                    txtFechaVencimiento.Text = Convert.ToDateTime(reader["FechaVencimiento"]).ToString("yyyy-MM-dd");
                                }
                                else
                                {
                                    txtFechaVencimiento.Text = "";
                                }

                                txtCostoUnitario.Text = Convert.ToDecimal(reader["CostoUnitario"]).ToString("F2");
                                ddlEstado.SelectedValue = reader["Estado"].ToString();

                                tituloFormularioProducto.InnerText = "Editar Producto";
                                pnlFormularioProducto.Visible = true;
                                ClearMessages();
                            }
                        }
                    }
                }
            }
            else if (e.CommandName == "Eliminar")
            {
                try
                {
                    using (SqlConnection con = DatabaseHelper.GetConnection())
                    {
                        // Verificar si el producto está siendo usado en tratamientos
                        string queryVerificarUso = @"
                            SELECT COUNT(*) FROM TratamientosRealizados_Detalle 
                            WHERE IDProducto = @IDProducto";

                        using (SqlCommand cmdVerificar = new SqlCommand(queryVerificarUso, con))
                        {
                            cmdVerificar.Parameters.AddWithValue("@IDProducto", idProducto);
                            con.Open();
                            int usoCount = Convert.ToInt32(cmdVerificar.ExecuteScalar());

                            if (usoCount > 0)
                            {
                                // Si está siendo usado, solo lo marcamos como inactivo
                                string queryDesactivar = "UPDATE Inventario SET Estado = 'Inactivo' WHERE IDProducto = @IDProducto";
                                using (SqlCommand cmd = new SqlCommand(queryDesactivar, con))
                                {
                                    cmd.Parameters.AddWithValue("@IDProducto", idProducto);
                                    cmd.ExecuteNonQuery();
                                }
                                MostrarMensajeExito("Producto desactivado exitosamente (está siendo usado en tratamientos).");
                            }
                            else
                            {
                                // Si no está siendo usado, lo eliminamos permanentemente
                                string queryEliminar = "DELETE FROM Inventario WHERE IDProducto = @IDProducto";
                                using (SqlCommand cmd = new SqlCommand(queryEliminar, con))
                                {
                                    cmd.Parameters.AddWithValue("@IDProducto", idProducto);
                                    cmd.ExecuteNonQuery();
                                }
                                MostrarMensajeExito("Producto eliminado permanentemente.");
                            }
                        }
                    }
                    CargarInventario();
                }
                catch (Exception ex)
                {
                    MostrarMensajeError("Error al eliminar el producto: " + ex.Message);
                }
            }
        }

        protected void gvInventario_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                DataRowView rowView = (DataRowView)e.Row.DataItem;
                DateTime? fechaVencimiento = rowView["FechaVencimiento"] as DateTime?;
                decimal cantidad = Convert.ToDecimal(rowView["CantidadDisponible"]);
                string estado = rowView["Estado"].ToString();

                Label lblAlerta = (Label)e.Row.FindControl("lblAlerta");
                LinkButton btnEliminar = (LinkButton)e.Row.FindControl("btnEliminar");
                Literal ltTextoBoton = (Literal)e.Row.FindControl("ltTextoBoton");

                if (lblAlerta != null)
                {
                    string alertText = "";
                    string alertCssClass = "badge-inventory "; // Clase base

                    if (estado == "Inactivo")
                    {
                        alertText = "Inactivo";
                        alertCssClass += "alert-inactive";
                    }
                    else
                    {
                        if (cantidad < 0)
                        {
                            alertText = "Cantidad Negativa!";
                            alertCssClass += "alert-expired";
                        }
                        else if (fechaVencimiento.HasValue && fechaVencimiento.Value < DateTime.Today)
                        {
                            alertText = "Vencido";
                            alertCssClass += "alert-expired";
                        }
                        else if (cantidad <= UmbralStockBajo)
                        {
                            alertText = "Stock Bajo";
                            alertCssClass += "alert-stock-low";
                        }
                        else if (fechaVencimiento.HasValue && (fechaVencimiento.Value - DateTime.Today).TotalDays <= DiasParaVencimiento)
                        {
                            alertText = "Próximo a Vencer";
                            alertCssClass += "alert-stock-low";
                        }
                        else
                        {
                            alertText = "OK";
                            alertCssClass += "alert-ok";
                        }
                    }

                    lblAlerta.Text = alertText;
                    lblAlerta.CssClass = alertCssClass;
                }

                // Configurar texto del botón eliminar
                if (ltTextoBoton != null)
                {
                    ltTextoBoton.Text = estado == "Inactivo" ? "Eliminar" : "Desactivar";
                }

                // Resaltar filas con problemas críticos
                if (cantidad < 0)
                {
                    e.Row.Style.Add("background-color", "#ffe6e6");
                }
                else if (estado == "Inactivo")
                {
                    e.Row.Style.Add("background-color", "#f8f9fa");
                    e.Row.Style.Add("color", "#6c757d");
                }
                else if (cantidad <= UmbralStockBajo)
                {
                    e.Row.Style.Add("background-color", "#fff3cd"); // Amarillo claro para stock bajo
                }
            }
        }

        private void LimpiarFormulario()
        {
            hfIDProducto.Value = "";
            txtNombreProducto.Text = "";
            txtCategoria.Text = "";
            txtCantidad.Text = "0";
            ddlUnidadMedida.SelectedIndex = 0;
            txtFechaVencimiento.Text = "";
            txtCostoUnitario.Text = "0";
            ddlEstado.SelectedIndex = 0;
        }

        private void ClearMessages()
        {
            lblMensajeError.Visible = false;
            lblMensajeExito.Visible = false;
        }

        private void MostrarMensajeError(string mensaje)
        {
            lblMensajeError.Text = mensaje;
            lblMensajeError.Visible = true;
            lblMensajeExito.Visible = false;
        }

        private void MostrarMensajeExito(string mensaje)
        {
            lblMensajeExito.Text = mensaje;
            lblMensajeExito.Visible = true;
            lblMensajeError.Visible = false;
        }
    }
}