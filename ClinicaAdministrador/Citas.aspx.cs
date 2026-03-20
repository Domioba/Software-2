using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using ClinicaAdministrador.DAL;

namespace ClinicaAdministrador
{
    public partial class Citas : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // --- LÓGICA DE SEGURIDAD ---
            if (Session["Usuario"] == null)
            {
                Response.Redirect("Login.aspx");
                return; // Importante salir del método
            }
            // --- FIN DE LA LÓGICA DE SEGURIDAD ---

            if (!IsPostBack)
            {
                CargarPacientesDDL();
                CargarServiciosCheckBoxList();
                CargarCitas();
            }
        }

        private void CargarPacientesDDL()
        {
            using (SqlConnection con = DatabaseHelper.GetConnection())
            {
                string query = "SELECT IDPaciente, NombreCompleto FROM Pacientes WHERE Estado = 1 ORDER BY NombreCompleto";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        sda.Fill(dt);
                        ddlPaciente.DataSource = dt;
                        ddlPaciente.DataTextField = "NombreCompleto";
                        ddlPaciente.DataValueField = "IDPaciente";
                        ddlPaciente.DataBind();
                    }
                }
            }
            ddlPaciente.Items.Insert(0, new ListItem("-- Seleccione un Paciente --", "0"));
        }

        private void CargarServiciosCheckBoxList()
        {
            cblServicios.Items.Clear();
            using (SqlConnection con = DatabaseHelper.GetConnection())
            {
                string query = @"SELECT IDServicio, NombreServicio, Precio, Duracion, TipoProcedimiento 
                                FROM Servicios 
                                WHERE Estado = 'Activo' 
                                ORDER BY TipoProcedimiento, NombreServicio";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        string categoriaActual = "";
                        while (reader.Read())
                        {
                            string categoria = reader["TipoProcedimiento"].ToString();

                            // Agregar separador de categoría
                            if (categoria != categoriaActual)
                            {
                                cblServicios.Items.Add(new ListItem($"--- {categoria.ToUpper()} ---", "0")
                                {
                                    Attributes = { ["style"] = "font-weight: bold; color: #2c5aa0; background-color: #f0f8ff; padding: 5px; margin: 5px 0;" },
                                    Enabled = false
                                });
                                categoriaActual = categoria;
                            }

                            ListItem item = new ListItem();
                            item.Text = $"{reader["NombreServicio"]} - ${Convert.ToDecimal(reader["Precio"]).ToString("N2")} ({reader["Duracion"]} min)";
                            item.Value = reader["IDServicio"].ToString();
                            item.Attributes["title"] = $"Tipo: {categoria}";
                            item.Attributes["class"] = "servicio-item";
                            cblServicios.Items.Add(item);
                        }
                    }
                }
            }
        }

        private void CargarCitas()
        {
            using (SqlConnection con = DatabaseHelper.GetConnection())
            {
                string query = @"SELECT c.IDCita, p.NombreCompleto AS NombrePaciente, c.Fecha, c.Hora, c.DuracionTotal, c.Estado, 
                                        ISNULL(STRING_AGG(s.NombreServicio, ', ') WITHIN GROUP (ORDER BY s.NombreServicio), 'N/A') AS Servicios,
                                        ISNULL(SUM(cs.PrecioUnitario), 0) AS Total
                                 FROM Citas c
                                 INNER JOIN Pacientes p ON c.IDPaciente = p.IDPaciente
                                 LEFT JOIN Citas_Servicios cs ON c.IDCita = cs.IDCita
                                 LEFT JOIN Servicios s ON cs.IDServicio = s.IDServicio
                                 WHERE 1=1";

                if (!string.IsNullOrEmpty(txtFiltroFecha.Text))
                {
                    query += " AND c.Fecha = @Fecha";
                }
                if (!string.IsNullOrEmpty(ddlFiltroEstado.SelectedValue))
                {
                    query += " AND c.Estado = @Estado";
                }

                query += " GROUP BY c.IDCita, p.NombreCompleto, c.Fecha, c.Hora, c.DuracionTotal, c.Estado ORDER BY c.Fecha, c.Hora";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    if (!string.IsNullOrEmpty(txtFiltroFecha.Text))
                    {
                        cmd.Parameters.AddWithValue("@Fecha", txtFiltroFecha.Text);
                    }
                    if (!string.IsNullOrEmpty(ddlFiltroEstado.SelectedValue))
                    {
                        cmd.Parameters.AddWithValue("@Estado", ddlFiltroEstado.SelectedValue);
                    }

                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        sda.Fill(dt);
                        gvCitas.DataSource = dt;
                        gvCitas.DataBind();
                    }
                }
            }
        }

        protected void btnNuevaCita_Click(object sender, EventArgs e)
        {
            hfIDCita.Value = "";
            ddlPaciente.ClearSelection();
            ddlEstado.ClearSelection();
            txtFecha.Text = "";
            txtHora.Text = "";
            txtNotas.Text = "";
            lblTotalPrecio.Text = "$0.00";
            lblDuracionTotal.Text = "0 minutos";

            foreach (ListItem item in cblServicios.Items) { item.Selected = false; }

            tituloFormularioCita.InnerText = "Nueva Cita";
            pnlFormularioCita.Visible = true;
            ClearMessages();
        }

        protected void btnGuardarCita_Click(object sender, EventArgs e)
        {
            try
            {
                // Validaciones en el servidor
                if (!ValidarDatosCita())
                {
                    return;
                }

                // Calcular duración total ANTES de la transacción
                int duracionTotal = CalcularDuracionTotal();

                using (SqlConnection con = DatabaseHelper.GetConnection())
                {
                    con.Open();
                    using (SqlTransaction transaction = con.BeginTransaction())
                    {
                        try
                        {
                            int idCita;
                            decimal total = 0;

                            if (string.IsNullOrEmpty(hfIDCita.Value))
                            {
                                // Nueva cita
                                string queryCita = @"INSERT INTO Citas (IDPaciente, Fecha, Hora, Estado, Notas, DuracionTotal) 
                                                   OUTPUT INSERTED.IDCita
                                                   VALUES (@IDPaciente, @Fecha, @Hora, @Estado, @Notas, @DuracionTotal)";
                                using (SqlCommand cmd = new SqlCommand(queryCita, con, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@IDPaciente", ddlPaciente.SelectedValue);
                                    cmd.Parameters.AddWithValue("@Fecha", txtFecha.Text);
                                    cmd.Parameters.AddWithValue("@Hora", txtHora.Text);
                                    cmd.Parameters.AddWithValue("@Estado", ddlEstado.SelectedValue);
                                    cmd.Parameters.AddWithValue("@Notas", txtNotas.Text.Trim());
                                    cmd.Parameters.AddWithValue("@DuracionTotal", duracionTotal);
                                    idCita = (int)cmd.ExecuteScalar();
                                }
                            }
                            else
                            {
                                // Editar cita existente
                                idCita = Convert.ToInt32(hfIDCita.Value);
                                string queryCita = @"UPDATE Citas 
                                                   SET IDPaciente = @IDPaciente, Fecha = @Fecha, Hora = @Hora, 
                                                       Estado = @Estado, Notas = @Notas, DuracionTotal = @DuracionTotal 
                                                   WHERE IDCita = @IDCita";
                                using (SqlCommand cmd = new SqlCommand(queryCita, con, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@IDPaciente", ddlPaciente.SelectedValue);
                                    cmd.Parameters.AddWithValue("@Fecha", txtFecha.Text);
                                    cmd.Parameters.AddWithValue("@Hora", txtHora.Text);
                                    cmd.Parameters.AddWithValue("@Estado", ddlEstado.SelectedValue);
                                    cmd.Parameters.AddWithValue("@Notas", txtNotas.Text.Trim());
                                    cmd.Parameters.AddWithValue("@DuracionTotal", duracionTotal);
                                    cmd.Parameters.AddWithValue("@IDCita", idCita);
                                    cmd.ExecuteNonQuery();
                                }

                                // Eliminar los servicios anteriores
                                string queryDeleteDetalles = "DELETE FROM Citas_Servicios WHERE IDCita = @IDCita";
                                using (SqlCommand cmdDelete = new SqlCommand(queryDeleteDetalles, con, transaction))
                                {
                                    cmdDelete.Parameters.AddWithValue("@IDCita", idCita);
                                    cmdDelete.ExecuteNonQuery();
                                }
                            }

                            // Agregar los servicios seleccionados
                            var serviciosSeleccionados = cblServicios.Items.Cast<ListItem>().Where(item => item.Selected).ToList();
                            foreach (ListItem item in serviciosSeleccionados)
                            {
                                int idServicio = Convert.ToInt32(item.Value);
                                decimal precio = ObtenerPrecioServicio(idServicio, con, transaction);
                                total += precio;

                                string queryDetalle = @"INSERT INTO Citas_Servicios (IDCita, IDServicio, PrecioUnitario) 
                                                        VALUES (@IDCita, @IDServicio, @Precio)";
                                using (SqlCommand cmdDetalle = new SqlCommand(queryDetalle, con, transaction))
                                {
                                    cmdDetalle.Parameters.AddWithValue("@IDCita", idCita);
                                    cmdDetalle.Parameters.AddWithValue("@IDServicio", idServicio);
                                    cmdDetalle.Parameters.AddWithValue("@Precio", precio);
                                    cmdDetalle.ExecuteNonQuery();
                                }
                            }

                            transaction.Commit();
                            MostrarMensajeExito("Cita guardada exitosamente.");
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            MostrarMensajeError("Error al guardar la cita: " + ex.Message);
                            return;
                        }
                    }
                }

                pnlFormularioCita.Visible = false;
                CargarCitas();
            }
            catch (Exception ex)
            {
                MostrarMensajeError("Error inesperado: " + ex.Message);
            }
        }

        #region Métodos de Validación y Cálculo

        private bool ValidarDatosCita()
        {
            // Validar que se haya seleccionado un paciente
            if (ddlPaciente.SelectedValue == "0")
            {
                MostrarMensajeError("Debe seleccionar un paciente para la cita.");
                return false;
            }

            // Validar que se haya seleccionado al menos un servicio
            var serviciosSeleccionadosItems = cblServicios.Items.Cast<ListItem>().Where(item => item.Selected).ToList();
            if (serviciosSeleccionadosItems.Count == 0)
            {
                MostrarMensajeError("Debe seleccionar al menos un servicio para la cita.");
                return false;
            }

            // *** NUEVA VALIDACIÓN: Límite de servicios por cita ***
            List<int> serviciosSeleccionadosIds = serviciosSeleccionadosItems
                .Select(item => Convert.ToInt32(item.Value))
                .ToList();

            if (serviciosSeleccionadosIds.Count > 4)
            {
                MostrarMensajeError("No se pueden seleccionar más de 4 servicios por cita por seguridad del paciente.");
                return false;
            }

            // *** NUEVA VALIDACIÓN: Compatibilidad de servicios ***
            if (!ValidarCompatibilidadServicios(serviciosSeleccionadosIds))
            {
                return false;
            }

            // Validar fecha
            if (string.IsNullOrEmpty(txtFecha.Text))
            {
                MostrarMensajeError("Debe seleccionar una fecha para la cita.");
                return false;
            }

            DateTime fechaCita;
            if (!DateTime.TryParse(txtFecha.Text, out fechaCita))
            {
                MostrarMensajeError("La fecha seleccionada no es válida.");
                return false;
            }

            // Validar que la fecha no sea en el pasado
            if (fechaCita < DateTime.Today)
            {
                MostrarMensajeError("La fecha de la cita no puede ser en el pasado.");
                return false;
            }

            // Validar que la fecha no sea mayor a un año
            DateTime fechaMaxima = DateTime.Today.AddYears(1);
            if (fechaCita > fechaMaxima)
            {
                MostrarMensajeError("La fecha de la cita no puede ser mayor a un año desde hoy.");
                return false;
            }

            // Validar hora
            if (string.IsNullOrEmpty(txtHora.Text))
            {
                MostrarMensajeError("Debe seleccionar una hora para la cita.");
                return false;
            }

            TimeSpan horaCita;
            if (!TimeSpan.TryParse(txtHora.Text, out horaCita))
            {
                MostrarMensajeError("La hora seleccionada no es válida.");
                return false;
            }

            // Validar que la hora esté dentro del horario de atención (7:00 AM - 8:00 PM)
            TimeSpan horaApertura = new TimeSpan(7, 0, 0);
            TimeSpan horaCierre = new TimeSpan(20, 0, 0);
            if (horaCita < horaApertura || horaCita >= horaCierre)
            {
                MostrarMensajeError("La hora de la cita debe estar dentro del horario de atención (7:00 AM - 8:00 PM).");
                return false;
            }

            // Calcular duración total y verificar solapamiento
            int duracionTotal = CalcularDuracionTotal();

            // Verificar que la duración total no exceda el horario de atención
            TimeSpan horaFinCalculada = horaCita.Add(new TimeSpan(0, duracionTotal, 0));
            if (horaFinCalculada > horaCierre)
            {
                MostrarMensajeError($"La cita excede el horario de atención. Duración total: {duracionTotal} minutos. Hora de fin: {horaFinCalculada:hh\\:mm}");
                return false;
            }

            // Verificar solapamiento con otras citas
            int idCitaExcluir = string.IsNullOrEmpty(hfIDCita.Value) ? 0 : Convert.ToInt32(hfIDCita.Value);
            if (ExisteSolapamientoCitas(fechaCita, horaCita, duracionTotal, idCitaExcluir))
            {
                MostrarMensajeError($"Ya existe una cita programada en el horario seleccionado. La cita actual tiene una duración de {duracionTotal} minutos.");
                return false;
            }

            // Validar que no exista una cita duplicada para el mismo paciente en el mismo horario
            if (ExisteCitaDuplicada(Convert.ToInt32(ddlPaciente.SelectedValue), fechaCita, horaCita))
            {
                MostrarMensajeError("El paciente ya tiene una cita programada en el mismo día y hora.");
                return false;
            }

            return true;
        }

        private bool ValidarCompatibilidadServicios(List<int> serviciosSeleccionados)
        {
            if (serviciosSeleccionados.Count < 2) return true;

            using (SqlConnection con = DatabaseHelper.GetConnection())
            {
                // Crear lista de parámetros
                var parametros = new List<string>();
                var commandParams = new SqlParameter[serviciosSeleccionados.Count];

                for (int i = 0; i < serviciosSeleccionados.Count; i++)
                {
                    parametros.Add($"@p{i}");
                    commandParams[i] = new SqlParameter($"@p{i}", serviciosSeleccionados[i]);
                }

                string query = $@"SELECT DISTINCT s1.NombreServicio as Servicio1, s2.NombreServicio as Servicio2, r.Motivo
                                FROM ReglasServicios r
                                INNER JOIN Servicios s1 ON r.IDServicio1 = s1.IDServicio
                                INNER JOIN Servicios s2 ON r.IDServicio2 = s2.IDServicio
                                WHERE r.SonCompatibles = 0
                                AND ((s1.IDServicio IN ({string.Join(",", parametros)}) AND s2.IDServicio IN ({string.Join(",", parametros)})))";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddRange(commandParams);
                    con.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string servicio1 = reader["Servicio1"].ToString();
                            string servicio2 = reader["Servicio2"].ToString();
                            string motivo = reader["Motivo"].ToString();

                            MostrarMensajeError($"COMBINACIÓN NO PERMITIDA: '{servicio1}' con '{servicio2}'. {motivo} Por favor, programe estos tratamientos en diferentes fechas.");
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        private int CalcularDuracionTotal()
        {
            int duracionTotal = 0;
            var serviciosSeleccionados = cblServicios.Items.Cast<ListItem>().Where(item => item.Selected).ToList();

            using (SqlConnection con = DatabaseHelper.GetConnection())
            {
                con.Open();
                foreach (ListItem item in serviciosSeleccionados)
                {
                    int idServicio = Convert.ToInt32(item.Value);
                    duracionTotal += ObtenerDuracionServicio(idServicio, con);
                }
            }
            return duracionTotal;
        }

        private int ObtenerDuracionServicio(int idServicio, SqlConnection con)
        {
            string query = "SELECT Duracion FROM Servicios WHERE IDServicio = @IDServicio";
            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@IDServicio", idServicio);
                object result = cmd.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : 0;
            }
        }

        private bool ExisteSolapamientoCitas(DateTime fecha, TimeSpan horaInicio, int duracionMinutos, int idCitaExcluir = 0)
        {
            TimeSpan horaFin = horaInicio.Add(new TimeSpan(0, duracionMinutos, 0));

            using (SqlConnection con = DatabaseHelper.GetConnection())
            {
                string query = @"SELECT COUNT(*) 
                                FROM Citas 
                                WHERE Fecha = @Fecha 
                                AND Estado <> 'Cancelada'
                                AND IDCita <> @IDCitaExcluir
                                AND (
                                    -- Nueva cita empieza durante una cita existente
                                    (@HoraInicio >= Hora AND @HoraInicio < DATEADD(MINUTE, DuracionTotal, Hora))
                                    OR
                                    -- Nueva cita termina durante una cita existente
                                    (@HoraFin > Hora AND @HoraFin <= DATEADD(MINUTE, DuracionTotal, Hora))
                                    OR
                                    -- Nueva cita engloba una cita existente
                                    (@HoraInicio <= Hora AND @HoraFin >= DATEADD(MINUTE, DuracionTotal, Hora))
                                )";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Fecha", fecha);
                    cmd.Parameters.AddWithValue("@HoraInicio", horaInicio);
                    cmd.Parameters.AddWithValue("@HoraFin", horaFin);
                    cmd.Parameters.AddWithValue("@IDCitaExcluir", idCitaExcluir);

                    con.Open();
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count > 0;
                }
            }
        }

        private bool ExisteCitaDuplicada(int idPaciente, DateTime fecha, TimeSpan hora)
        {
            using (SqlConnection con = DatabaseHelper.GetConnection())
            {
                string query = @"SELECT COUNT(*) 
                                FROM Citas 
                                WHERE IDPaciente = @IDPaciente 
                                AND Fecha = @Fecha 
                                AND Hora = @Hora 
                                AND Estado <> 'Cancelada'";

                // Si estamos editando una cita, excluimos la cita actual de la verificación
                if (!string.IsNullOrEmpty(hfIDCita.Value))
                {
                    query += " AND IDCita <> @IDCita";
                }

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@IDPaciente", idPaciente);
                    cmd.Parameters.AddWithValue("@Fecha", fecha);
                    cmd.Parameters.AddWithValue("@Hora", hora);

                    if (!string.IsNullOrEmpty(hfIDCita.Value))
                    {
                        cmd.Parameters.AddWithValue("@IDCita", Convert.ToInt32(hfIDCita.Value));
                    }

                    con.Open();
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count > 0;
                }
            }
        }

        #endregion

        private decimal ObtenerPrecioServicio(int idServicio, SqlConnection con, SqlTransaction transaction)
        {
            string query = "SELECT Precio FROM Servicios WHERE IDServicio = @IDServicio";
            using (SqlCommand cmd = new SqlCommand(query, con, transaction))
            {
                cmd.Parameters.AddWithValue("@IDServicio", idServicio);
                object result = cmd.ExecuteScalar();
                return result != null ? Convert.ToDecimal(result) : 0;
            }
        }

        protected void btnCancelarCita_Click(object sender, EventArgs e)
        {
            pnlFormularioCita.Visible = false;
            ClearMessages();
        }

        protected void btnFiltrar_Click(object sender, EventArgs e)
        {
            CargarCitas();
        }

        protected void gvCitas_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int idCita = Convert.ToInt32(e.CommandArgument);

            if (e.CommandName == "Editar")
            {
                try
                {
                    // 1. Cargar datos de la cita principal
                    using (SqlConnection con = DatabaseHelper.GetConnection())
                    {
                        string queryCita = "SELECT * FROM Citas WHERE IDCita = @IDCita";
                        using (SqlCommand cmd = new SqlCommand(queryCita, con))
                        {
                            cmd.Parameters.AddWithValue("@IDCita", idCita);
                            con.Open();
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    hfIDCita.Value = reader["IDCita"].ToString();
                                    if (ddlPaciente.Items.FindByValue(reader["IDPaciente"].ToString()) != null)
                                    {
                                        ddlPaciente.SelectedValue = reader["IDPaciente"].ToString();
                                    }
                                    txtFecha.Text = Convert.ToDateTime(reader["Fecha"]).ToString("yyyy-MM-dd");
                                    txtHora.Text = reader["Hora"].ToString();
                                    ddlEstado.SelectedValue = reader["Estado"].ToString();
                                    txtNotas.Text = reader["Notas"].ToString();
                                    lblDuracionTotal.Text = reader["DuracionTotal"].ToString() + " minutos";
                                }
                            }
                        }
                    }

                    // 2. Cargar los servicios de esta cita y marcarlos en el CheckBoxList
                    using (SqlConnection con = DatabaseHelper.GetConnection())
                    {
                        string queryServicios = @"SELECT s.IDServicio, s.NombreServicio 
                                                FROM Citas_Servicios cs 
                                                INNER JOIN Servicios s ON cs.IDServicio = s.IDServicio 
                                                WHERE cs.IDCita = @IDCita";
                        using (SqlCommand cmd = new SqlCommand(queryServicios, con))
                        {
                            cmd.Parameters.AddWithValue("@IDCita", idCita);
                            con.Open();
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                var serviciosCita = new List<int>();
                                while (reader.Read())
                                {
                                    serviciosCita.Add(Convert.ToInt32(reader["IDServicio"]));
                                }

                                // Marcar los checkboxes correspondientes
                                foreach (ListItem item in cblServicios.Items)
                                {
                                    int idServicio = Convert.ToInt32(item.Value);
                                    item.Selected = serviciosCita.Contains(idServicio);
                                }
                            }
                        }
                    }

                    // 3. Actualizar el título y mostrar el formulario
                    tituloFormularioCita.InnerText = "Editar Cita";
                    pnlFormularioCita.Visible = true;
                    ClearMessages();

                    // 4. Llamar a la función de JavaScript para recalcular el total y duración
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "CalcularTotalYDuracion", "calcularTotalYDuracion();", true);
                }
                catch (Exception ex)
                {
                    MostrarMensajeError("Error al cargar la cita para editar: " + ex.Message);
                }
            }
            // NUEVO: Manejar el clic en el botón "Registrar Tratamiento"
            else if (e.CommandName == "RegistrarTratamiento")
            {
                // Redirigir a la página de Tratamientos.aspx, pasando el IDCita en la URL
                Response.Redirect($"Tratamientos.aspx?IDCita={idCita}");
            }
            else if (e.CommandName == "MarcarRealizada")
            {
                try
                {
                    // Verificar que la cita exista y esté en estado "Confirmada"
                    using (SqlConnection con = DatabaseHelper.GetConnection())
                    {
                        string queryVerificar = "SELECT Fecha, Hora FROM Citas WHERE IDCita = @IDCita AND Estado = 'Confirmada'";
                        using (SqlCommand cmd = new SqlCommand(queryVerificar, con))
                        {
                            cmd.Parameters.AddWithValue("@IDCita", idCita);
                            con.Open();
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    DateTime fechaCita = Convert.ToDateTime(reader["Fecha"]);
                                    TimeSpan horaCita = TimeSpan.Parse(reader["Hora"].ToString());
                                    DateTime fechaHoraCita = fechaCita.Date.Add(horaCita);

                                    // Verificar que ya haya pasado la fecha y hora de la cita
                                    if (DateTime.Now < fechaHoraCita)
                                    {
                                        MostrarMensajeError("No se puede marcar como realizada una cita que aún no ha ocurrido.");
                                        return;
                                    }

                                    reader.Close();

                                    // Actualizar el estado a "Realizada"
                                    string queryActualizar = "UPDATE Citas SET Estado = 'Realizada' WHERE IDCita = @IDCita";
                                    using (SqlCommand cmdActualizar = new SqlCommand(queryActualizar, con))
                                    {
                                        cmdActualizar.Parameters.AddWithValue("@IDCita", idCita);
                                        cmdActualizar.ExecuteNonQuery();

                                        MostrarMensajeExito("Cita marcada como realizada exitosamente.");
                                        CargarCitas();
                                    }
                                }
                                else
                                {
                                    MostrarMensajeError("La cita no existe o no está en estado 'Confirmada'.");
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MostrarMensajeError("Error al marcar la cita como realizada: " + ex.Message);
                }
            }
            else if (e.CommandName == "Eliminar")
            {
                try
                {
                    using (SqlConnection con = DatabaseHelper.GetConnection())
                    {
                        string query = "DELETE FROM Citas WHERE IDCita = @IDCita";
                        using (SqlCommand cmd = new SqlCommand(query, con))
                        {
                            cmd.Parameters.AddWithValue("@IDCita", idCita);
                            con.Open();
                            cmd.ExecuteNonQuery();

                            MostrarMensajeExito("Cita eliminada exitosamente.");
                            CargarCitas();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MostrarMensajeError("Error al eliminar la cita: " + ex.Message);
                }
            }
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