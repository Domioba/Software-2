using ClinicaAdministrador.DAL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.Script.Serialization;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ClinicaAdministrador
{
    public partial class Tratamientos : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            ScriptManager scriptManager = ScriptManager.GetCurrent(this.Page);
            if (scriptManager == null)
            {
                scriptManager = new ScriptManager();
                this.Page.Form.Controls.AddAt(0, scriptManager);
            }
            scriptManager.EnablePageMethods = true;

            if (!IsPostBack)
            {
                // MODIFICADO: Comprobar si se ha pasado un IDCita en la URL
                if (Request.QueryString["IDCita"] != null)
                {
                    // Se viene desde la página de Citas. Cargar el formulario automáticamente.
                    int idCita;
                    if (int.TryParse(Request.QueryString["IDCita"], out idCita))
                    {
                        CargarCitasDDL(); // Cargamos el DDL para que no dé error
                        ddlCita.SelectedValue = idCita.ToString();
                        CargarServiciosDeLaCita();
                        CargarDatosPacienteYFechaDesdeCita(idCita); // NUEVO: Cargar paciente y fecha

                        tituloFormularioTratamiento.InnerText = "Registrar Tratamiento para Cita Seleccionada";
                        pnlFormularioTratamiento.Visible = true;
                    }
                }
                else
                {
                    // Flujo normal: se accede a la página sin parámetros
                    CargarCitasDDL();
                    CargarTratamientos();
                }
            }
        }

        #region Carga de Datos Iniciales

        private void CargarCitasDDL()
        {
            using (SqlConnection con = DatabaseHelper.GetConnection())
            {
                string query = @"
                    SELECT C.IDCita, 
                           P.NombreCompleto + ' - ' + CONVERT(NVARCHAR, C.Fecha, 105) + ' ' + CAST(C.Hora AS NVARCHAR(5)) AS DescripcionCita
                    FROM Citas C
                    INNER JOIN Pacientes P ON C.IDPaciente = P.IDPaciente
                    WHERE C.Estado = 'Realizada'
                    ORDER BY C.Fecha DESC, C.Hora DESC";

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

        private void CargarTratamientos()
        {
            using (SqlConnection con = DatabaseHelper.GetConnection())
            {
                string query = @"
                    SELECT t.IDTratamiento, p.NombreCompleto AS NombrePaciente, 
                           t.FechaTratamiento, 
                           COUNT(td.IDDetalle) AS NumProductosUsados
                    FROM TratamientosRealizados t
                    INNER JOIN Pacientes p ON t.IDPaciente = p.IDPaciente
                    LEFT JOIN TratamientosRealizados_Detalle td ON t.IDTratamiento = td.IDTratamiento
                    GROUP BY t.IDTratamiento, p.NombreCompleto, t.FechaTratamiento
                    ORDER BY t.FechaTratamiento DESC";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        sda.Fill(dt);
                        gvTratamientos.DataSource = dt;
                        gvTratamientos.DataBind();
                    }
                }
            }
        }

        #endregion

        #region Eventos de la UI

        protected void btnNuevoTratamiento_Click(object sender, EventArgs e)
        {
            LimpiarFormulario();
            tituloFormularioTratamiento.InnerText = "Registrar Nuevo Tratamiento";
            pnlFormularioTratamiento.Visible = true;
        }

        protected void ddlCita_SelectedIndexChanged(object sender, EventArgs e)
        {
            CargarServiciosDeLaCita();
            // MODIFICADO: Llamamos al método centralizado para cargar datos
            int idCita = Convert.ToInt32(ddlCita.SelectedValue);
            CargarDatosPacienteYFechaDesdeCita(idCita);
        }

        protected void btnCancelarTratamiento_Click(object sender, EventArgs e)
        {
            pnlFormularioTratamiento.Visible = false;
        }

        #endregion

        #region Lógica del Repeater de Servicios/Productos

        private void CargarServiciosDeLaCita()
        {
            if (ddlCita.SelectedValue == "0")
            {
                rptServiciosProductos.DataSource = null;
                rptServiciosProductos.DataBind();
                return;
            }

            using (SqlConnection con = DatabaseHelper.GetConnection())
            {
                string query = @"
                    SELECT 
                        s.IDServicio, 
                        s.NombreServicio, 
                        i.IDProducto, 
                        i.NombreProducto,
                        ISNULL(i.CantidadDisponible, 0) AS InventarioDisponible
                    FROM Citas_Servicios cs
                    INNER JOIN Servicios s ON cs.IDServicio = s.IDServicio
                    LEFT JOIN Inventario i ON s.IDProductoPorDefecto = i.IDProducto
                    WHERE cs.IDCita = @IDCita";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@IDCita", ddlCita.SelectedValue);
                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        sda.Fill(dt);
                        rptServiciosProductos.DataSource = dt;
                        rptServiciosProductos.DataBind();
                    }
                }
            }
        }

        protected void rptServiciosProductos_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                decimal inventarioDisponible = Convert.ToDecimal(DataBinder.Eval(e.Item.DataItem, "InventarioDisponible"));

                TextBox txtInventarioDisponible = (TextBox)e.Item.FindControl("txtInventarioDisponible");
                if (txtInventarioDisponible != null)
                {
                    txtInventarioDisponible.Text = inventarioDisponible.ToString("N2");
                    if (inventarioDisponible <= 0)
                    {
                        txtInventarioDisponible.BackColor = System.Drawing.Color.LightCoral;
                        txtInventarioDisponible.ToolTip = "Producto agotado";
                    }
                    else if (inventarioDisponible < 10)
                    {
                        txtInventarioDisponible.BackColor = System.Drawing.Color.LightYellow;
                        txtInventarioDisponible.ToolTip = "Stock bajo";
                    }
                }

                TextBox txtCantidad = (TextBox)e.Item.FindControl("txtCantidad");
                if (txtCantidad != null)
                {
                    txtCantidad.Attributes["max"] = inventarioDisponible.ToString("F2");
                    if (inventarioDisponible <= 0)
                    {
                        txtCantidad.Enabled = false;
                        txtCantidad.Attributes["placeholder"] = "Producto agotado";
                        txtCantidad.BackColor = System.Drawing.Color.LightGray;
                    }
                }
            }
        }

        #endregion

        #region Guardado del Tratamiento (INSERT y UPDATE)

        protected void btnGuardarTratamiento_Click(object sender, EventArgs e)
        {
            if (!ValidarFormulario())
            {
                return;
            }

            List<DetalleTratamiento> detalles = new List<DetalleTratamiento>();

            foreach (RepeaterItem item in rptServiciosProductos.Items)
            {
                if (item.ItemType == ListItemType.Item || item.ItemType == ListItemType.AlternatingItem)
                {
                    HiddenField hfIDProducto = (HiddenField)item.FindControl("hfIDProducto");
                    TextBox txtCantidad = (TextBox)item.FindControl("txtCantidad");
                    HiddenField hfIDServicio = (HiddenField)item.FindControl("hfIDServicio");
                    HiddenField hfInventarioDisponible = (HiddenField)item.FindControl("hfInventarioDisponible");

                    if (!string.IsNullOrEmpty(hfIDProducto.Value) && !string.IsNullOrWhiteSpace(txtCantidad.Text))
                    {
                        decimal cantidad = Convert.ToDecimal(txtCantidad.Text);
                        decimal inventarioDisponible = Convert.ToDecimal(hfInventarioDisponible.Value);

                        if (cantidad < 1)
                        {
                            MostrarMensaje($"La cantidad para el servicio no puede ser menor a 1.", true);
                            return;
                        }

                        if (cantidad > inventarioDisponible)
                        {
                            MostrarMensaje($"La cantidad para el servicio excede el inventario disponible ({inventarioDisponible}).", true);
                            return;
                        }

                        detalles.Add(new DetalleTratamiento
                        {
                            IDServicio = Convert.ToInt32(hfIDServicio.Value),
                            IDProducto = Convert.ToInt32(hfIDProducto.Value),
                            CantidadUtilizada = cantidad
                        });
                    }
                }
            }

            if (detalles.Count == 0)
            {
                MostrarMensaje("Debe ingresar al menos una cantidad para un producto.", true);
                return;
            }

            using (SqlConnection con = DatabaseHelper.GetConnection())
            {
                con.Open();
                SqlTransaction transaction = con.BeginTransaction();
                try
                {
                    int idTratamiento;

                    if (!string.IsNullOrEmpty(hfIDTratamiento.Value))
                    {
                        idTratamiento = Convert.ToInt32(hfIDTratamiento.Value);
                        RestaurarInventarioDesdeTratamiento(idTratamiento, con, transaction);
                        string queryDeleteDetalles = "DELETE FROM TratamientosRealizados_Detalle WHERE IDTratamiento = @IDTratamiento";
                        using (SqlCommand cmdDeleteDetalles = new SqlCommand(queryDeleteDetalles, con, transaction))
                        {
                            cmdDeleteDetalles.Parameters.AddWithValue("@IDTratamiento", idTratamiento);
                            cmdDeleteDetalles.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        string queryCabecera = @"
                            INSERT INTO TratamientosRealizados (IDCita, IDPaciente, IDAdmin, FechaTratamiento, ObservacionesGenerales)
                            OUTPUT INSERTED.IDTratamiento
                            VALUES (@IDCita, @IDPaciente, @IDAdmin, @FechaTratamiento, @ObservacionesGenerales)";

                        using (SqlCommand cmdCabecera = new SqlCommand(queryCabecera, con, transaction))
                        {
                            cmdCabecera.Parameters.AddWithValue("@IDCita", ddlCita.SelectedValue);
                            cmdCabecera.Parameters.AddWithValue("@IDPaciente", hfIDPaciente.Value);
                            cmdCabecera.Parameters.AddWithValue("@IDAdmin", Session["IDAdmin"] ?? 1);
                            cmdCabecera.Parameters.AddWithValue("@FechaTratamiento", txtFechaTratamiento.Text);
                            cmdCabecera.Parameters.AddWithValue("@ObservacionesGenerales", txtObservaciones.Text.Trim());
                            idTratamiento = (int)cmdCabecera.ExecuteScalar();
                        }
                    }

                    foreach (var detalle in detalles)
                    {
                        string queryDetalle = @"
                            INSERT INTO TratamientosRealizados_Detalle (IDTratamiento, IDServicio, IDProducto, CantidadUtilizada)
                            VALUES (@IDTratamiento, @IDServicio, @IDProducto, @CantidadUtilizada)";

                        using (SqlCommand cmdDetalle = new SqlCommand(queryDetalle, con, transaction))
                        {
                            cmdDetalle.Parameters.AddWithValue("@IDTratamiento", idTratamiento);
                            cmdDetalle.Parameters.AddWithValue("@IDServicio", detalle.IDServicio);
                            cmdDetalle.Parameters.AddWithValue("@IDProducto", detalle.IDProducto);
                            cmdDetalle.Parameters.AddWithValue("@CantidadUtilizada", detalle.CantidadUtilizada);
                            cmdDetalle.ExecuteNonQuery();
                        }

                        string queryInventario = "UPDATE Inventario SET CantidadDisponible = CantidadDisponible - @CantidadUtilizada WHERE IDProducto = @IDProducto";
                        using (SqlCommand cmdInventario = new SqlCommand(queryInventario, con, transaction))
                        {
                            cmdInventario.Parameters.AddWithValue("@CantidadUtilizada", detalle.CantidadUtilizada);
                            cmdInventario.Parameters.AddWithValue("@IDProducto", detalle.IDProducto);
                            cmdInventario.ExecuteNonQuery();
                        }
                    }

                    transaction.Commit();
                    MostrarMensaje("Tratamiento guardado e inventario actualizado con éxito.", false);
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MostrarMensaje("Error al guardar el tratamiento: " + ex.Message, true);
                    return;
                }
            }

            pnlFormularioTratamiento.Visible = false;
            CargarTratamientos();
            CargarCitasDDL();
        }

        private bool ValidarFormulario()
        {
            if (ddlCita.SelectedValue == "0" || string.IsNullOrEmpty(hfIDPaciente.Value))
            {
                MostrarMensaje("Debe seleccionar una cita válida.", true);
                return false;
            }
            if (!ValidarFechaInalterada())
            {
                MostrarMensaje("La fecha del tratamiento no puede ser modificada. Debe coincidir con la fecha de la cita.", true);
                return false;
            }
            return true;
        }

        private bool ValidarFechaInalterada()
        {
            string fechaOriginal = "";
            using (SqlConnection con = DatabaseHelper.GetConnection())
            {
                string query = "SELECT Fecha FROM Citas WHERE IDCita = @IDCita";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@IDCita", ddlCita.SelectedValue);
                    con.Open();
                    var result = cmd.ExecuteScalar();
                    if (result != null)
                    {
                        fechaOriginal = Convert.ToDateTime(result).ToString("yyyy-MM-dd");
                    }
                }
            }
            return txtFechaTratamiento.Text == fechaOriginal;
        }

        #endregion

        #region Lógica de Eliminación y Edición

        protected void gvTratamientos_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int idTratamiento = Convert.ToInt32(e.CommandArgument);

            if (e.CommandName == "EditarTratamiento")
            {
                CargarTratamientoParaEditar(idTratamiento);
            }
            else if (e.CommandName == "EliminarTratamiento")
            {
                EliminarTratamiento(idTratamiento);
            }
        }

        private void EliminarTratamiento(int idTratamiento)
        {
            using (SqlConnection con = DatabaseHelper.GetConnection())
            {
                con.Open();
                SqlTransaction transaction = con.BeginTransaction();
                try
                {
                    RestaurarInventarioDesdeTratamiento(idTratamiento, con, transaction);
                    string queryDeleteCabecera = "DELETE FROM TratamientosRealizados WHERE IDTratamiento = @IDTratamiento";
                    using (SqlCommand cmd = new SqlCommand(queryDeleteCabecera, con, transaction))
                    {
                        cmd.Parameters.AddWithValue("@IDTratamiento", idTratamiento);
                        cmd.ExecuteNonQuery();
                    }
                    transaction.Commit();
                    MostrarMensaje("Tratamiento eliminado e inventario restaurado con éxito.", false);
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MostrarMensaje("Error al eliminar el tratamiento: " + ex.Message, true);
                }
            }
            CargarTratamientos();
            CargarCitasDDL();
        }

        private void CargarTratamientoParaEditar(int idTratamiento)
        {
            using (SqlConnection conCabecera = DatabaseHelper.GetConnection())
            {
                string queryCabecera = "SELECT * FROM TratamientosRealizados WHERE IDTratamiento = @IDTratamiento";
                using (SqlCommand cmdCabecera = new SqlCommand(queryCabecera, conCabecera))
                {
                    cmdCabecera.Parameters.AddWithValue("@IDTratamiento", idTratamiento);
                    conCabecera.Open();
                    using (SqlDataReader reader = cmdCabecera.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            hfIDTratamiento.Value = reader["IDTratamiento"].ToString();
                            hfIDPaciente.Value = reader["IDPaciente"].ToString();
                            ddlCita.SelectedValue = reader["IDCita"].ToString();
                            txtFechaTratamiento.Text = Convert.ToDateTime(reader["FechaTratamiento"]).ToString("yyyy-MM-dd");
                            txtObservaciones.Text = reader["ObservacionesGenerales"].ToString();
                        }
                    }
                }
            }

            using (SqlConnection conDetalles = DatabaseHelper.GetConnection())
            {
                string queryDetalles = @"
                    SELECT td.IDServicio, td.IDProducto, td.CantidadUtilizada, 
                           s.NombreServicio, i.NombreProducto, i.CantidadDisponible + td.CantidadUtilizada AS InventarioDisponible
                    FROM TratamientosRealizados_Detalle td
                    INNER JOIN Servicios s ON td.IDServicio = s.IDServicio
                    INNER JOIN Inventario i ON td.IDProducto = i.IDProducto
                    WHERE td.IDTratamiento = @IDTratamiento";

                DataTable dtDetalles = new DataTable();
                using (SqlCommand cmdDetalles = new SqlCommand(queryDetalles, conDetalles))
                {
                    cmdDetalles.Parameters.AddWithValue("@IDTratamiento", idTratamiento);
                    using (SqlDataAdapter sda = new SqlDataAdapter(cmdDetalles))
                    {
                        sda.Fill(dtDetalles);
                    }
                }
                rptServiciosProductos.DataSource = dtDetalles;
                rptServiciosProductos.DataBind();
            }

            tituloFormularioTratamiento.InnerText = "Editar Tratamiento";
            pnlFormularioTratamiento.Visible = true;
        }

        private void RestaurarInventarioDesdeTratamiento(int idTratamiento, SqlConnection con, SqlTransaction transaction)
        {
            var detallesParaRestaurar = new List<(int idProducto, decimal cantidad)>();
            string querySelect = @"
                SELECT IDProducto, CantidadUtilizada 
                FROM TratamientosRealizados_Detalle 
                WHERE IDTratamiento = @IDTratamiento";

            using (SqlCommand cmdSelect = new SqlCommand(querySelect, con, transaction))
            {
                cmdSelect.Parameters.AddWithValue("@IDTratamiento", idTratamiento);
                using (SqlDataReader reader = cmdSelect.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        detallesParaRestaurar.Add((
                            idProducto: Convert.ToInt32(reader["IDProducto"]),
                            cantidad: Convert.ToDecimal(reader["CantidadUtilizada"])
                        ));
                    }
                }
            }

            foreach (var detalle in detallesParaRestaurar)
            {
                string queryUpdate = "UPDATE Inventario SET CantidadDisponible = CantidadDisponible + @Cantidad WHERE IDProducto = @IDProducto";
                using (SqlCommand cmdUpdate = new SqlCommand(queryUpdate, con, transaction))
                {
                    cmdUpdate.Parameters.AddWithValue("@Cantidad", detalle.cantidad);
                    cmdUpdate.Parameters.AddWithValue("@IDProducto", detalle.idProducto);
                    cmdUpdate.ExecuteNonQuery();
                }
            }
        }

        #endregion

        #region Métodos Auxiliares

        // NUEVO: Método para cargar datos del paciente y fecha sin depender del SelectedIndexChanged del DDL
        private void CargarDatosPacienteYFechaDesdeCita(int idCita)
        {
            using (SqlConnection con = DatabaseHelper.GetConnection())
            {
                string query = @"
                    SELECT P.IDPaciente, P.NombreCompleto, C.Fecha
                    FROM Citas C
                    INNER JOIN Pacientes P ON C.IDPaciente = P.IDPaciente
                    WHERE C.IDCita = @IDCita";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@IDCita", idCita);
                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            hfIDPaciente.Value = reader["IDPaciente"].ToString();
                            txtFechaTratamiento.Text = Convert.ToDateTime(reader["Fecha"]).ToString("yyyy-MM-dd");

                            ddlPaciente.Items.Clear();
                            ddlPaciente.Items.Add(new ListItem(reader["NombreCompleto"].ToString(), reader["IDPaciente"].ToString()));
                        }
                    }
                }
            }
        }

        [WebMethod]
        public static string ObtenerDatosDeCitaParaTratamiento(int idCita)
        {
            var connStr = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            var datos = new Dictionary<string, object> { { "paciente", null } };

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
                                    id = reader["IDPaciente"].ToString(),
                                    nombre = reader["NombreCompleto"].ToString(),
                                    fechaCita = Convert.ToDateTime(reader["Fecha"]).ToString("yyyy-MM-dd")
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return new JavaScriptSerializer().Serialize(new { error = "Error en el servidor: " + ex.Message.Replace("\"", "\\\"") });
            }

            var js = new JavaScriptSerializer();
            return js.Serialize(datos);
        }

        private void LimpiarFormulario()
        {
            hfIDTratamiento.Value = "";
            hfIDPaciente.Value = "";
            ddlCita.ClearSelection();
            ddlPaciente.Items.Clear();
            txtFechaTratamiento.Text = "";
            txtObservaciones.Text = "";
            rptServiciosProductos.DataSource = null;
            rptServiciosProductos.DataBind();
            lblMensaje.Visible = false;
        }

        private void MostrarMensaje(string mensaje, bool esError)
        {
            lblMensaje.Text = mensaje;
            lblMensaje.Visible = true;
            if (esError)
            {
                lblMensaje.CssClass = "alert-custom alert-error d-block mb-3";
            }
            else
            {
                lblMensaje.CssClass = "alert-custom alert-success d-block mb-3";
            }
        }

        public class DetalleTratamiento
        {
            public int IDServicio { get; set; }
            public int IDProducto { get; set; }
            public decimal CantidadUtilizada { get; set; }
        }

        #endregion
    }
}