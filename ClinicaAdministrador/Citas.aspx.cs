using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
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
                return;
            }
            // --- FIN DE LA LÓGICA DE SEGURIDAD ---

            if (!IsPostBack)
            {
                // CORRECCIÓN: Agregada la carga de estados para que el DDL no esté vacío
                CargarEstadosDDL();
                CargarPacientesDDL();
                CargarServiciosCheckBoxList();
                CargarCitas();
            }
        }

        // MÉTODO NUEVO: Carga los estados válidos para una cita
        private void CargarEstadosDDL()
        {
            if (ddlEstado.Items.Count == 0) // Cargar solo si está vacío para no duplicar
            {
                ddlEstado.Items.Add(new ListItem("Programada", "Programada"));
                ddlEstado.Items.Add(new ListItem("Confirmada", "Confirmada"));
                ddlEstado.Items.Add(new ListItem("Realizada", "Realizada"));
                ddlEstado.Items.Add(new ListItem("Cancelada", "Cancelada"));
            }

            // Opcional: Si usas un filtro de estado con la misma lista, descomenta esto:
            /*
            if (ddlFiltroEstado.Items.Count == 0)
            {
                ddlFiltroEstado.Items.Add(new ListItem("-- Todos --", ""));
                ddlFiltroEstado.Items.Add(new ListItem("Programada", "Programada"));
                ddlFiltroEstado.Items.Add(new ListItem("Confirmada", "Confirmada"));
                ddlFiltroEstado.Items.Add(new ListItem("Realizada", "Realizada"));
                ddlFiltroEstado.Items.Add(new ListItem("Cancelada", "Cancelada"));
            }
            */
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
            ddlPaciente.SelectedValue = "0";

            // CORRECCIÓN: Usar FindByValue para evitar error si la lista no tiene el valor (aunque ya la cargamos en Page_Load)
            ListItem liProgramada = ddlEstado.Items.FindByValue("Programada");
            if (liProgramada != null) ddlEstado.SelectedValue = "Programada";

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
                    ScriptManager.RegisterStartupScript(this, GetType(), "restoreButton", "if(typeof(restaurarBotonGuardar)==='function')restaurarBotonGuardar();", true);
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
                                // === NUEVA CITA ===
                                string queryCita = @"INSERT INTO Citas (IDPaciente, Fecha, Hora, Estado, Notas, DuracionTotal, FechaRegistro) 
                                                   OUTPUT INSERTED.IDCita
                                                   VALUES (@IDPaciente, @Fecha, @Hora, @Estado, @Notas, @DuracionTotal, GETDATE())";
                                using (SqlCommand cmd = new SqlCommand(queryCita, con, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@IDPaciente", Convert.ToInt32(ddlPaciente.SelectedValue));
                                    cmd.Parameters.AddWithValue("@Fecha", DateTime.ParseExact(txtFecha.Text, "yyyy-MM-dd", CultureInfo.InvariantCulture).Date);
                                    cmd.Parameters.AddWithValue("@Hora", TimeSpan.Parse(txtHora.Text));
                                    cmd.Parameters.AddWithValue("@Estado", ddlEstado.SelectedValue);
                                    cmd.Parameters.AddWithValue("@Notas", string.IsNullOrEmpty(txtNotas.Text.Trim()) ? (object)DBNull.Value : txtNotas.Text.Trim());
                                    cmd.Parameters.AddWithValue("@DuracionTotal", duracionTotal);
                                    idCita = (int)cmd.ExecuteScalar();
                                }
                            }
                            else
                            {
                                // === EDITAR CITA EXISTENTE ===
                                idCita = Convert.ToInt32(hfIDCita.Value);
                                string queryCita = @"UPDATE Citas 
                                                   SET IDPaciente = @IDPaciente, 
                                                       Fecha = @Fecha, 
                                                       Hora = @Hora, 
                                                       Estado = @Estado, 
                                                       Notas = @Notas, 
                                                       DuracionTotal = @DuracionTotal,
                                                       FechaModificacion = GETDATE()
                                                   WHERE IDCita = @IDCita";
                                using (SqlCommand cmd = new SqlCommand(queryCita, con, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@IDPaciente", Convert.ToInt32(ddlPaciente.SelectedValue));
                                    cmd.Parameters.AddWithValue("@Fecha", DateTime.ParseExact(txtFecha.Text, "yyyy-MM-dd", CultureInfo.InvariantCulture).Date);
                                    cmd.Parameters.AddWithValue("@Hora", TimeSpan.Parse(txtHora.Text));
                                    cmd.Parameters.AddWithValue("@Estado", ddlEstado.SelectedValue);
                                    cmd.Parameters.AddWithValue("@Notas", string.IsNullOrEmpty(txtNotas.Text.Trim()) ? (object)DBNull.Value : txtNotas.Text.Trim());
                                    cmd.Parameters.AddWithValue("@DuracionTotal", duracionTotal);
                                    cmd.Parameters.AddWithValue("@IDCita", idCita);
                                    cmd.ExecuteNonQuery();
                                }

                                // Eliminar los servicios anteriores de esta cita
                                string queryDeleteDetalles = "DELETE FROM Citas_Servicios WHERE IDCita = @IDCita";
                                using (SqlCommand cmdDelete = new SqlCommand(queryDeleteDetalles, con, transaction))
                                {
                                    cmdDelete.Parameters.AddWithValue("@IDCita", idCita);
                                    cmdDelete.ExecuteNonQuery();
                                }
                            }

                            // === AGREGAR LOS SERVICIOS SELECCIONADOS ===
                            var serviciosSeleccionados = cblServicios.Items.Cast<ListItem>()
                                .Where(item => item.Selected && !string.IsNullOrEmpty(item.Value) && item.Value != "0")
                                .ToList();

                            foreach (ListItem item in serviciosSeleccionados)
                            {
                                if (!int.TryParse(item.Value, out int idServicio)) continue;

                                decimal precio = ObtenerPrecioServicio(idServicio, con, transaction);
                                total += precio;

                                string queryDetalle = @"INSERT INTO Citas_Servicios (IDCita, IDServicio, PrecioUnitario, FechaAsignacion) 
                                                        VALUES (@IDCita, @IDServicio, @Precio, GETDATE())";
                                using (SqlCommand cmdDetalle = new SqlCommand(queryDetalle, con, transaction))
                                {
                                    cmdDetalle.Parameters.AddWithValue("@IDCita", idCita);
                                    cmdDetalle.Parameters.AddWithValue("@IDServicio", idServicio);
                                    cmdDetalle.Parameters.AddWithValue("@Precio", precio);
                                    cmdDetalle.ExecuteNonQuery();
                                }
                            }

                            transaction.Commit();

                            // Actualizar labels de total y duración
                            lblTotalPrecio.Text = $"${total.ToString("N2")}";
                            lblDuracionTotal.Text = $"{duracionTotal} minutos";

                            MostrarMensajeExito($"Cita {(string.IsNullOrEmpty(hfIDCita.Value) ? "creada" : "actualizada")} exitosamente. Total: ${total.ToString("N2")}");
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            System.Diagnostics.Debug.WriteLine($"Error en transacción de cita: {ex.Message}\n{ex.StackTrace}");
                            MostrarMensajeError("Error al guardar la cita: " + ex.Message);
                            ScriptManager.RegisterStartupScript(this, GetType(), "restoreButton", "if(typeof(restaurarBotonGuardar)==='function')restaurarBotonGuardar();", true);
                            return;
                        }
                    }
                }

                // Limpiar y recargar
                pnlFormularioCita.Visible = false;

                // Limpieza inline de controles
                hfIDCita.Value = "";
                ddlPaciente.ClearSelection();
                ddlPaciente.SelectedValue = "0";
                ddlEstado.ClearSelection();
                if (ddlEstado.Items.FindByValue("Programada") != null) ddlEstado.SelectedValue = "Programada";
                txtFecha.Text = "";
                txtHora.Text = "";
                txtNotas.Text = "";
                lblTotalPrecio.Text = "$0.00";
                lblDuracionTotal.Text = "0 minutos";
                foreach (ListItem item in cblServicios.Items) { item.Selected = false; }

                CargarCitas();
                ClearMessages();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error inesperado al guardar cita: {ex.Message}\n{ex.StackTrace}");
                MostrarMensajeError("Error inesperado: " + ex.Message);
                ScriptManager.RegisterStartupScript(this, GetType(), "restoreButton", "if(typeof(restaurarBotonGuardar)==='function')restaurarBotonGuardar();", true);
            }
        }

        #region Métodos de Validación y Cálculo

        private bool ValidarDatosCita()
        {
            // Validar que se haya seleccionado un paciente
            if (ddlPaciente.SelectedValue == "0" || string.IsNullOrEmpty(ddlPaciente.SelectedValue))
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

            // Obtener IDs de servicios seleccionados para validaciones
            List<int> serviciosSeleccionadosIds = serviciosSeleccionadosItems
                .Where(item => !string.IsNullOrEmpty(item.Value) && item.Value != "0")
                .Select(item => Convert.ToInt32(item.Value))
                .ToList();

            if (serviciosSeleccionadosIds.Count > 4)
            {
                MostrarMensajeError("No se pueden seleccionar más de 4 servicios por cita por seguridad del paciente.");
                return false;
            }

            if (!ValidarCompatibilidadServicios(serviciosSeleccionadosIds))
            {
                return false;
            }

            int idPacienteSeleccionado = Convert.ToInt32(ddlPaciente.SelectedValue);
            List<string> serviciosBloqueadosPorCondicion;

            if (!ValidarRestriccionesPorCondicionMedica(idPacienteSeleccionado, serviciosSeleccionadosIds, out serviciosBloqueadosPorCondicion))
            {
                string mensaje = "<div class='alert alert-warning'><strong>⚠️ SERVICIOS BLOQUEADOS POR CONDICIÓN MÉDICA:</strong><br/><br/>" +
                                 string.Join("<br/>", serviciosBloqueadosPorCondicion) +
                                 "<br/><br/><em>Por favor, consulte con el especialista para reprogramar estos tratamientos.</em></div>";
                MostrarMensajeError(mensaje);
                return false;
            }

            if (string.IsNullOrEmpty(txtFecha.Text))
            {
                MostrarMensajeError("Debe seleccionar una fecha para la cita.");
                return false;
            }

            DateTime fechaCita;
            if (!DateTime.TryParseExact(txtFecha.Text, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out fechaCita))
            {
                MostrarMensajeError("La fecha seleccionada no es válida. Formato requerido: AAAA-MM-DD");
                return false;
            }

            if (fechaCita.Date < DateTime.Today)
            {
                MostrarMensajeError("La fecha de la cita no puede ser en el pasado.");
                return false;
            }

            DateTime fechaMaxima = DateTime.Today.AddYears(1);
            if (fechaCita > fechaMaxima)
            {
                MostrarMensajeError("La fecha de la cita no puede ser mayor a un año desde hoy.");
                return false;
            }

            if (string.IsNullOrEmpty(txtHora.Text))
            {
                MostrarMensajeError("Debe seleccionar una hora para la cita.");
                return false;
            }

            TimeSpan horaCita;
            if (!TimeSpan.TryParse(txtHora.Text, out horaCita))
            {
                MostrarMensajeError("La hora seleccionada no es válida. Formato: HH:mm");
                return false;
            }

            TimeSpan horaApertura = new TimeSpan(7, 0, 0);
            TimeSpan horaCierre = new TimeSpan(20, 0, 0);
            if (horaCita < horaApertura || horaCita >= horaCierre)
            {
                MostrarMensajeError("La hora de la cita debe estar dentro del horario de atención (7:00 AM - 8:00 PM).");
                return false;
            }

            int duracionTotal = CalcularDuracionTotal();

            TimeSpan horaFinCalculada = horaCita.Add(new TimeSpan(0, duracionTotal, 0));
            if (horaFinCalculada > horaCierre)
            {
                MostrarMensajeError($"La cita excede el horario de atención. Duración total: {duracionTotal} minutos. Hora de fin estimada: {horaFinCalculada:hh\\:mm}");
                return false;
            }

            int idCitaExcluir = string.IsNullOrEmpty(hfIDCita.Value) ? 0 : Convert.ToInt32(hfIDCita.Value);
            if (ExisteSolapamientoCitas(fechaCita, horaCita, duracionTotal, idCitaExcluir))
            {
                MostrarMensajeError($"Ya existe una cita programada en el horario seleccionado. La cita actual tiene una duración de {duracionTotal} minutos.");
                return false;
            }

            if (ExisteCitaDuplicada(idPacienteSeleccionado, fechaCita, horaCita))
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
                var parametros = new List<string>();
                var commandParams = new List<SqlParameter>();

                for (int i = 0; i < serviciosSeleccionados.Count; i++)
                {
                    parametros.Add($"@p{i}");
                    commandParams.Add(new SqlParameter($"@p{i}", SqlDbType.Int) { Value = serviciosSeleccionados[i] });
                }

                string query = $@"SELECT DISTINCT s1.NombreServicio as Servicio1, s2.NombreServicio as Servicio2, r.Motivo
                                FROM ReglasServicios r
                                INNER JOIN Servicios s1 ON r.IDServicio1 = s1.IDServicio
                                INNER JOIN Servicios s2 ON r.IDServicio2 = s2.IDServicio
                                WHERE r.SonCompatibles = 0
                                AND ((s1.IDServicio IN ({string.Join(",", parametros)}) AND s2.IDServicio IN ({string.Join(",", parametros)})))";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddRange(commandParams.ToArray());
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

        private bool ValidarRestriccionesPorCondicionMedica(int idPaciente, List<int> serviciosSeleccionadosIds, out List<string> serviciosBloqueados)
        {
            serviciosBloqueados = new List<string>();

            using (SqlConnection con = DatabaseHelper.GetConnection())
            {
                con.Open();

                string condicionMedica = "";
                string queryPaciente = "SELECT HistorialMedico FROM Pacientes WHERE IDPaciente = @IDPaciente AND Estado = 1";
                using (SqlCommand cmdPaciente = new SqlCommand(queryPaciente, con))
                {
                    cmdPaciente.Parameters.AddWithValue("@IDPaciente", idPaciente);
                    object result = cmdPaciente.ExecuteScalar();
                    if (result != null && !string.IsNullOrEmpty(result.ToString()))
                    {
                        string historial = result.ToString();
                        if (historial.Contains(" - "))
                        {
                            condicionMedica = historial.Split(new[] { " - " }, StringSplitOptions.None)[0].Trim();
                        }
                        else
                        {
                            condicionMedica = historial.Trim();
                        }
                    }
                }

                if (string.IsNullOrEmpty(condicionMedica) || condicionMedica.Equals("Ninguna", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                if (serviciosSeleccionadosIds == null || serviciosSeleccionadosIds.Count == 0)
                    return true;

                var paramsServicios = new List<string>();
                var sqlParams = new List<SqlParameter>();
                for (int i = 0; i < serviciosSeleccionadosIds.Count; i++)
                {
                    paramsServicios.Add($"@s{i}");
                    sqlParams.Add(new SqlParameter($"@s{i}", SqlDbType.Int) { Value = serviciosSeleccionadosIds[i] });
                }

                string queryRestricciones = $@"
                    SELECT s.NombreServicio, s.TipoProcedimiento, 
                           CASE 
                               WHEN @Condicion = 'Embarazo' AND (s.EsInyectable = 1 OR s.TipoProcedimiento = 'Láser') 
                                    THEN 'Contraindicado durante el embarazo por seguridad fetal y materna.'
                               WHEN @Condicion IN ('Diabetes Tipo 1', 'Diabetes Tipo 2') AND s.TipoProcedimiento IN ('Facial Microlesiones', 'Láser') 
                                    THEN 'Riesgo de cicatrización lenta o infecciones en pacientes con diabetes.'
                               WHEN @Condicion = 'Diabetes Tipo 2' AND s.EsInyectable = 1 
                                    THEN 'Precaución: la diabetes puede afectar la respuesta a tratamientos inyectables.'
                               WHEN @Condicion = 'Hipertensión' AND s.EsInyectable = 1 
                                    THEN 'Los inyectables pueden interactuar con medicamentos para la presión arterial.'
                               WHEN @Condicion = 'Cardíaco' AND s.EsInyectable = 1 
                                    THEN 'Precaución con inyectables por posibles interacciones con medicación cardíaca.'
                               WHEN @Condicion = 'Epilepsia' AND s.TipoProcedimiento = 'Láser' 
                                    THEN 'La luz pulsada/láser puede desencadenar fotosensibilidad en pacientes epilépticos.'
                               WHEN @Condicion = 'Hepatitis' AND (s.EsInyectable = 1 OR s.TipoProcedimiento LIKE '%Microlesiones%') 
                                    THEN 'Procedimientos invasivos requieren evaluación hepática previa por riesgo de sangrado.'
                               WHEN @Condicion = 'Cáncer' AND (s.EsInyectable = 1 OR s.TipoProcedimiento IN ('Láser', 'Facial Microlesiones')) 
                                    THEN 'Contraindicado sin autorización oncológica: puede interferir con tratamientos.'
                               WHEN @Condicion = 'Coagulacion' AND (s.EsInyectable = 1 OR s.TipoProcedimiento LIKE '%Microlesiones%' OR s.TipoProcedimiento LIKE '%Exfoliante%') 
                                    THEN 'Riesgo elevado de hematomas o sangrado en procedimientos invasivos.'
                               WHEN @Condicion = 'Alergias' AND s.EsInyectable = 1 
                                    THEN 'Requiere prueba de alergia previa para componentes de inyectables.'
                               WHEN @Condicion = 'Fotosensibilidad' AND s.TipoProcedimiento = 'Láser' 
                                    THEN 'Contraindicado: riesgo de reacción fotosensible severa.'
                               WHEN @Condicion = 'Inmunosupresion' AND (s.TipoProcedimiento LIKE '%Microlesiones%' OR s.TipoProcedimiento LIKE '%Exfoliante%') 
                                    THEN 'Riesgo elevado de infección en procedimientos que rompen la barrera cutánea.'
                               WHEN @Condicion = 'Marcapasos' AND s.TipoProcedimiento = 'Láser' 
                                    THEN 'Algunos equipos láser pueden interferir con dispositivos electrónicos implantados.'
                               WHEN @Condicion = 'Queloide' AND s.TipoProcedimiento LIKE '%Microlesiones%' 
                                    THEN 'Riesgo de formación de cicatrices queloides en procedimientos invasivos.'
                               ELSE NULL
                           END AS Motivo
                    FROM Servicios s
                    WHERE s.IDServicio IN ({string.Join(",", paramsServicios)})
                    AND s.Estado = 'Activo'
                    AND (
                        (@Condicion = 'Embarazo' AND (s.EsInyectable = 1 OR s.TipoProcedimiento = 'Láser')) OR
                        (@Condicion IN ('Diabetes Tipo 1', 'Diabetes Tipo 2') AND s.TipoProcedimiento IN ('Facial Microlesiones', 'Láser')) OR
                        (@Condicion = 'Diabetes Tipo 2' AND s.EsInyectable = 1) OR
                        (@Condicion = 'Hipertensión' AND s.EsInyectable = 1) OR
                        (@Condicion = 'Cardíaco' AND s.EsInyectable = 1) OR
                        (@Condicion = 'Epilepsia' AND s.TipoProcedimiento = 'Láser') OR
                        (@Condicion = 'Hepatitis' AND (s.EsInyectable = 1 OR s.TipoProcedimiento LIKE '%Microlesiones%')) OR
                        (@Condicion = 'Cáncer' AND (s.EsInyectable = 1 OR s.TipoProcedimiento IN ('Láser', 'Facial Microlesiones'))) OR
                        (@Condicion = 'Coagulacion' AND (s.EsInyectable = 1 OR s.TipoProcedimiento LIKE '%Microlesiones%' OR s.TipoProcedimiento LIKE '%Exfoliante%')) OR
                        (@Condicion = 'Alergias' AND s.EsInyectable = 1) OR
                        (@Condicion = 'Fotosensibilidad' AND s.TipoProcedimiento = 'Láser') OR
                        (@Condicion = 'Inmunosupresion' AND (s.TipoProcedimiento LIKE '%Microlesiones%' OR s.TipoProcedimiento LIKE '%Exfoliante%')) OR
                        (@Condicion = 'Marcapasos' AND s.TipoProcedimiento = 'Láser') OR
                        (@Condicion = 'Queloide' AND s.TipoProcedimiento LIKE '%Microlesiones%')
                    )";

                using (SqlCommand cmd = new SqlCommand(queryRestricciones, con))
                {
                    cmd.Parameters.AddWithValue("@Condicion", condicionMedica);
                    cmd.Parameters.AddRange(sqlParams.ToArray());

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string servicio = reader["NombreServicio"].ToString();
                            string motivo = reader["Motivo"].ToString();
                            serviciosBloqueados.Add($"• <strong>{servicio}</strong>: {motivo}");
                        }
                    }
                }
            }

            return serviciosBloqueados.Count == 0;
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
                    if (!int.TryParse(item.Value, out int idServicio)) continue;
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
                                    (@HoraInicio >= Hora AND @HoraInicio < DATEADD(MINUTE, DuracionTotal, Hora))
                                    OR
                                    (@HoraFin > Hora AND @HoraFin <= DATEADD(MINUTE, DuracionTotal, Hora))
                                    OR
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

        #endregion

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
            if (string.IsNullOrEmpty(e.CommandArgument?.ToString())) return;
            if (!int.TryParse(e.CommandArgument.ToString(), out int idCita)) return;

            if (e.CommandName == "Editar")
            {
                try
                {
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

                                    // CORRECCIÓN: Asignación segura de estado para evitar ArgumentOutOfRangeException
                                    string estadoVal = reader["Estado"].ToString();
                                    ListItem liEstado = ddlEstado.Items.FindByValue(estadoVal);
                                    if (liEstado != null)
                                    {
                                        ddlEstado.SelectedValue = estadoVal;
                                    }
                                    else
                                    {
                                        // Fallback si el estado en DB no está en la lista
                                        ddlEstado.SelectedIndex = 0;
                                    }

                                    txtNotas.Text = reader["Notas"].ToString();
                                    lblDuracionTotal.Text = reader["DuracionTotal"].ToString() + " minutos";
                                }
                            }
                        }
                    }

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

                                foreach (ListItem item in cblServicios.Items)
                                {
                                    int idServicio = Convert.ToInt32(item.Value);
                                    item.Selected = serviciosCita.Contains(idServicio);
                                }
                            }
                        }
                    }

                    tituloFormularioCita.InnerText = "Editar Cita";
                    pnlFormularioCita.Visible = true;
                    ClearMessages();
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "CalcularTotalYDuracion", "calcularTotalYDuracion();", true);
                }
                catch (Exception ex)
                {
                    MostrarMensajeError("Error al cargar la cita para editar: " + ex.Message);
                }
            }
            else if (e.CommandName == "RegistrarTratamiento")
            {
                Response.Redirect($"Tratamientos.aspx?IDCita={idCita}");
            }
            else if (e.CommandName == "MarcarRealizada")
            {
                try
                {
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

                                    if (DateTime.Now < fechaHoraCita)
                                    {
                                        MostrarMensajeError("No se puede marcar como realizada una cita que aún no ha ocurrido.");
                                        return;
                                    }

                                    reader.Close();

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