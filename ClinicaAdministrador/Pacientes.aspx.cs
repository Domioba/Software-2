using ClinicaAdministrador.DAL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ClinicaAdministrador
{
    public partial class Pacientes : System.Web.UI.Page
    {
        // Expresiones regulares compiladas para mejor rendimiento
        private static readonly Regex _nombreRegex = new Regex(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s']{2,80}$", RegexOptions.Compiled);
        private static readonly Regex _telefonoRegex = new Regex(@"^\d{8,15}$", RegexOptions.Compiled);
        // MODIFICADO: Expresión regular para email más restrictiva - solo permite letras y números en el nombre de usuario
        private static readonly Regex _emailRegex = new Regex(@"^[a-zA-Z0-9]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$", RegexOptions.Compiled);
        private static readonly Regex _htmlRegex = new Regex(@"<[^>]+>|javascript:|on\w+\s*=|data:|vbscript:|expression\s*\(|url\s*\(", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex _sqlInjectionRegex = new Regex(@"(?:\b(SELECT|INSERT|UPDATE|DELETE|DROP|UNION|EXEC|ALTER|CREATE|TRUNCATE)\b|--|;|\|\|)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

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
                // Obtener parámetros de búsqueda
                string nombre = txtBuscarNombre.Text.Trim();
                string telefono = txtBuscarTelefono.Text.Trim();
                string correo = txtBuscarCorreo.Text.Trim();
                string idPaciente = txtBuscarID.Text.Trim();
                string fechaDesde = txtBuscarFechaDesde.Text.Trim();
                string fechaHasta = txtBuscarFechaHasta.Text.Trim();

                using (SqlConnection con = DatabaseHelper.GetConnection())
                {
                    // Construir consulta dinámica con parámetros
                    string query = @"SELECT IDPaciente, NombreCompleto, Telefono, Correo, FechaNacimiento, FechaRegistro 
                                   FROM Pacientes 
                                   WHERE Estado = 1";

                    // Lista para almacenar los parámetros
                    var parametros = new List<SqlParameter>();

                    // Agregar condiciones según los filtros
                    if (!string.IsNullOrEmpty(nombre))
                    {
                        query += " AND NombreCompleto LIKE @NombreCompleto";
                        parametros.Add(new SqlParameter("@NombreCompleto", SqlDbType.NVarChar, 80) { Value = "%" + nombre + "%" });
                    }

                    if (!string.IsNullOrEmpty(telefono))
                    {
                        query += " AND Telefono LIKE @Telefono";
                        parametros.Add(new SqlParameter("@Telefono", SqlDbType.VarChar, 15) { Value = "%" + telefono + "%" });
                    }

                    if (!string.IsNullOrEmpty(correo))
                    {
                        query += " AND Correo LIKE @Correo";
                        parametros.Add(new SqlParameter("@Correo", SqlDbType.VarChar, 100) { Value = "%" + correo + "%" });
                    }

                    if (!string.IsNullOrEmpty(idPaciente) && int.TryParse(idPaciente, out int id))
                    {
                        query += " AND IDPaciente = @IDPaciente";
                        parametros.Add(new SqlParameter("@IDPaciente", SqlDbType.Int) { Value = id });
                    }

                    if (!string.IsNullOrEmpty(fechaDesde) && DateTime.TryParse(fechaDesde, out DateTime desde))
                    {
                        query += " AND FechaRegistro >= @FechaDesde";
                        parametros.Add(new SqlParameter("@FechaDesde", SqlDbType.DateTime) { Value = desde });
                    }

                    if (!string.IsNullOrEmpty(fechaHasta) && DateTime.TryParse(fechaHasta, out DateTime hasta))
                    {
                        // Añadir un día a la fecha hasta para incluir todo el día seleccionado
                        DateTime hastaConDia = hasta.AddDays(1);
                        query += " AND FechaRegistro < @FechaHasta";
                        parametros.Add(new SqlParameter("@FechaHasta", SqlDbType.DateTime) { Value = hastaConDia });
                    }

                    query += " ORDER BY FechaRegistro DESC";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.CommandTimeout = 30;

                        // Agregar todos los parámetros al comando
                        cmd.Parameters.AddRange(parametros.ToArray());

                        using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            sda.Fill(dt);
                            gvPacientes.DataSource = dt;
                            gvPacientes.DataBind();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogError("Error al cargar la lista de pacientes", ex);
                MostrarMensajeError("Error al cargar la lista de pacientes: " + ex.Message);
            }
        }

        protected void btnBuscar_Click(object sender, EventArgs e)
        {
            // Validar que al menos un campo de búsqueda esté lleno
            if (string.IsNullOrWhiteSpace(txtBuscarNombre.Text) &&
                string.IsNullOrWhiteSpace(txtBuscarTelefono.Text) &&
                string.IsNullOrWhiteSpace(txtBuscarCorreo.Text) &&
                string.IsNullOrWhiteSpace(txtBuscarID.Text) &&
                string.IsNullOrWhiteSpace(txtBuscarFechaDesde.Text) &&
                string.IsNullOrWhiteSpace(txtBuscarFechaHasta.Text))
            {
                MostrarMensajeAdvertencia("Por favor, ingrese al menos un criterio de búsqueda.");
                return;
            }

            // Validar formato de fechas si se proporcionaron
            if (!string.IsNullOrWhiteSpace(txtBuscarFechaDesde.Text) && !DateTime.TryParse(txtBuscarFechaDesde.Text, out _))
            {
                MostrarMensajeError("La fecha 'Desde' no tiene un formato válido.");
                return;
            }

            if (!string.IsNullOrWhiteSpace(txtBuscarFechaHasta.Text) && !DateTime.TryParse(txtBuscarFechaHasta.Text, out _))
            {
                MostrarMensajeError("La fecha 'Hasta' no tiene un formato válido.");
                return;
            }

            // Validar que la fecha desde sea anterior a la fecha hasta
            if (!string.IsNullOrWhiteSpace(txtBuscarFechaDesde.Text) &&
                !string.IsNullOrWhiteSpace(txtBuscarFechaHasta.Text) &&
                DateTime.TryParse(txtBuscarFechaDesde.Text, out DateTime desde) &&
                DateTime.TryParse(txtBuscarFechaHasta.Text, out DateTime hasta) &&
                desde > hasta)
            {
                MostrarMensajeError("La fecha 'Desde' debe ser anterior o igual a la fecha 'Hasta'.");
                return;
            }

            // Validar formato de ID si se proporcionó
            if (!string.IsNullOrWhiteSpace(txtBuscarID.Text) && !int.TryParse(txtBuscarID.Text, out _))
            {
                MostrarMensajeError("El ID de paciente debe ser un número válido.");
                return;
            }

            // Realizar la búsqueda
            CargarPacientes();
        }

        protected void btnLimpiarBusqueda_Click(object sender, EventArgs e)
        {
            // Limpiar todos los campos de búsqueda
            txtBuscarNombre.Text = string.Empty;
            txtBuscarTelefono.Text = string.Empty;
            txtBuscarCorreo.Text = string.Empty;
            txtBuscarID.Text = string.Empty;
            txtBuscarFechaDesde.Text = string.Empty;
            txtBuscarFechaHasta.Text = string.Empty;

            // Recargar todos los pacientes
            CargarPacientes();
        }

        protected void btnGuardarPaciente_Click(object sender, EventArgs e)
        {
            try
            {
                if (!ValidarDatosPacienteCompleto())
                {
                    // Si falla la validación, salimos
                    return;
                }

                using (SqlConnection con = DatabaseHelper.GetConnection())
                {
                    string query;
                    bool esNuevo = string.IsNullOrEmpty(hfIDPaciente.Value);

                    // CORRECCIÓN: Declaramos la variable fuera del if/else para que tenga alcance en todo el método
                    int idPacienteActual = 0;

                    if (!esNuevo)
                    {
                        idPacienteActual = Convert.ToInt32(hfIDPaciente.Value);
                    }

                    string nombreCompleto = SanitizeInput(txtNombreCompleto.Text.Trim());
                    string telefono = SanitizeInput(txtTelefono.Text.Trim());
                    string correo = SanitizeInput(txtCorreo.Text.Trim().ToLower());

                    // Concatenamos la condición seleccionada con el detalle para el campo HistorialMedico de la BD
                    string condicion = ddlCondicionMedica.SelectedValue;
                    string detalle = SanitizeInput(txtDetalleMedico.Text.Trim());

                    // Construimos el texto completo que se guardará en la BD
                    string historialFinal = condicion;
                    if (!string.IsNullOrEmpty(detalle))
                    {
                        historialFinal += " - " + detalle;
                    }

                    // Verificar duplicados
                    if (esNuevo)
                    {
                        if (ExistePacienteDuplicado(nombreCompleto, telefono, correo, 0))
                        {
                            MostrarMensajeError("Ya existe un paciente con el mismo nombre, teléfono o correo electrónico.");
                            ScriptManager.RegisterStartupScript(this, GetType(), "restoreButton", "restaurarBotonGuardar();", true);
                            return;
                        }
                        query = @"INSERT INTO Pacientes (NombreCompleto, FechaNacimiento, Telefono, Correo, Observaciones, HistorialMedico, Estado) 
                          VALUES (@NombreCompleto, @FechaNacimiento, @Telefono, @Correo, @Observaciones, @HistorialMedico, 1)";
                    }
                    else
                    {
                        // Usamos la variable idPacienteActual que ya declaramos arriba
                        if (ExistePacienteDuplicado(nombreCompleto, telefono, correo, idPacienteActual))
                        {
                            MostrarMensajeError("Ya existe otro paciente con el mismo nombre, teléfono o correo electrónico.");
                            ScriptManager.RegisterStartupScript(this, GetType(), "restoreButton", "restaurarBotonGuardar();", true);
                            return;
                        }
                        query = @"UPDATE Pacientes 
                          SET NombreCompleto = @NombreCompleto, FechaNacimiento = @FechaNacimiento, Telefono = @Telefono, 
                              Correo = @Correo, Observaciones = @Observaciones, HistorialMedico = @HistorialMedico
                          WHERE IDPaciente = @IDPaciente AND Estado = 1";
                    }

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.Add("@NombreCompleto", SqlDbType.NVarChar, 80).Value = nombreCompleto;

                        if (!string.IsNullOrEmpty(txtFechaNacimiento.Text) && DateTime.TryParse(txtFechaNacimiento.Text, out DateTime fechaNacimiento))
                        {
                            cmd.Parameters.Add("@FechaNacimiento", SqlDbType.Date).Value = fechaNacimiento;
                        }
                        else
                        {
                            MostrarMensajeError("La fecha de nacimiento no es válida.");
                            ScriptManager.RegisterStartupScript(this, GetType(), "restoreButton", "restaurarBotonGuardar();", true);
                            return;
                        }

                        cmd.Parameters.Add("@Telefono", SqlDbType.VarChar, 15).Value = telefono;
                        cmd.Parameters.Add("@Correo", SqlDbType.VarChar, 100).Value = correo;
                        cmd.Parameters.Add("@Observaciones", SqlDbType.NVarChar, 500).Value = SanitizeInput(txtObservaciones.Text.Trim());
                        cmd.Parameters.Add("@HistorialMedico", SqlDbType.NVarChar, 3000).Value = historialFinal;

                        if (!esNuevo)
                        {
                            cmd.Parameters.Add("@IDPaciente", SqlDbType.Int).Value = idPacienteActual;
                        }

                        con.Open();
                        int resultado = cmd.ExecuteNonQuery();

                        if (resultado > 0)
                        {
                            pnlFormularioPaciente.Visible = false;
                            LimpiarFormulario(); // Limpiamos para la próxima
                            CargarPacientes();  // Recargamos la grilla
                            MostrarMensajeExito(esNuevo ? "Paciente creado exitosamente." : "Paciente actualizado exitosamente.");
                        }
                        else
                        {
                            MostrarMensajeError("No se pudo guardar la información del paciente.");
                            ScriptManager.RegisterStartupScript(this, GetType(), "restoreButton", "restaurarBotonGuardar();", true);
                        }
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                LogError("Error de base de datos", sqlEx);
                MostrarMensajeError("Error al guardar los datos. Por favor intente nuevamente.");
                ScriptManager.RegisterStartupScript(this, GetType(), "restoreButton", "restaurarBotonGuardar();", true);
            }
            catch (Exception ex)
            {
                LogError("Error inesperado", ex);
                MostrarMensajeError("Error inesperado. Por favor contacte al administrador.");
                ScriptManager.RegisterStartupScript(this, GetType(), "restoreButton", "restaurarBotonGuardar();", true);
            }
        }

        #region Validaciones Completas de Backend

        private bool ValidarDatosPacienteCompleto()
        {
            // Validar Nombre Completo
            if (!ValidarNombreCompleto(txtNombreCompleto.Text.Trim()))
                return false;

            // Validar Fecha de Nacimiento
            if (!ValidarFechaNacimiento(txtFechaNacimiento.Text.Trim()))
                return false;

            // Validar Teléfono
            if (!ValidarTelefono(txtTelefono.Text.Trim()))
                return false;

            // Validar Correo Electrónico
            if (!ValidarCorreoElectronico(txtCorreo.Text.Trim()))
                return false;

            // Validar Observaciones
            if (!ValidarObservaciones(txtObservaciones.Text.Trim()))
                return false;

            // Validar Historial Médico (Ahora valida el DDL y el detalle)
            if (!ValidarHistorialMedico(txtDetalleMedico.Text.Trim()))
                return false;

            return true;
        }

        private bool ValidarNombreCompleto(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
            {
                MostrarMensajeError("El nombre completo es obligatorio.");
                return false;
            }

            if (nombre.Length < 2)
            {
                MostrarMensajeError("El nombre debe tener al menos 2 caracteres.");
                return false;
            }

            if (nombre.Length > 80)
            {
                MostrarMensajeError("El nombre no puede exceder 80 caracteres.");
                return false;
            }

            if (!_nombreRegex.IsMatch(nombre))
            {
                MostrarMensajeError("El nombre solo puede contener letras, espacios y acentos.");
                return false;
            }

            // Validar que tenga al menos nombre y apellido
            var palabras = nombre.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (palabras.Length < 2)
            {
                MostrarMensajeError("Debe ingresar al menos un nombre y un apellido.");
                return false;
            }

            // Validar que cada palabra tenga al menos 2 caracteres
            foreach (string palabra in palabras)
            {
                if (palabra.Length < 2)
                {
                    MostrarMensajeError("Cada palabra del nombre debe tener al menos 2 caracteres.");
                    return false;
                }
            }

            return true;
        }

        private bool ValidarFechaNacimiento(string fechaTexto)
        {
            if (string.IsNullOrWhiteSpace(fechaTexto))
            {
                MostrarMensajeError("La fecha de nacimiento es obligatoria.");
                return false;
            }

            // Validar formato de fecha
            if (!DateTime.TryParseExact(fechaTexto, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fechaNacimiento))
            {
                MostrarMensajeError("El formato de fecha no es válido. Use: AAAA-MM-DD");
                return false;
            }

            // Evitar fechas inválidas como 0000-00-00
            if (fechaNacimiento.Year < 1900)
            {
                MostrarMensajeError("La fecha de nacimiento no es válida.");
                return false;
            }

            DateTime hoy = DateTime.Today;
            int edad = hoy.Year - fechaNacimiento.Year;

            // Ajustar edad si aún no ha cumplido años este año
            if (fechaNacimiento.Date > hoy.AddYears(-edad))
            {
                edad--;
            }

            // Validar rango de edad (12-85 años)
            if (edad < 12)
            {
                MostrarMensajeError("El paciente debe tener al menos 12 años (clínica para adultos).");
                return false;
            }

            if (edad > 85)
            {
                MostrarMensajeError("La edad no puede ser mayor a 85 años.");
                return false;
            }

            // Validar que no sea fecha futura
            if (fechaNacimiento > hoy)
            {
                MostrarMensajeError("La fecha de nacimiento no puede ser mayor a la fecha actual.");
                return false;
            }

            return true;
        }

        private bool ValidarTelefono(string telefono)
        {
            if (string.IsNullOrWhiteSpace(telefono))
            {
                MostrarMensajeError("El teléfono es obligatorio.");
                return false;
            }

            // Remover cualquier carácter que no sea número
            string telefonoLimpio = Regex.Replace(telefono, @"\D", "");

            if (telefonoLimpio.Length < 8)
            {
                MostrarMensajeError("El teléfono debe tener al menos 8 dígitos.");
                return false;
            }

            if (telefonoLimpio.Length > 15)
            {
                MostrarMensajeError("El teléfono no puede exceder 15 dígitos.");
                return false;
            }

            if (!_telefonoRegex.IsMatch(telefonoLimpio))
            {
                MostrarMensajeError("El teléfono solo puede contener números.");
                return false;
            }

            return true;
        }

        private bool ValidarCorreoElectronico(string correo)
        {
            if (string.IsNullOrWhiteSpace(correo))
            {
                MostrarMensajeError("El correo electrónico es obligatorio.");
                return false;
            }

            // Normalizar a minúsculas
            correo = correo.Trim().ToLower();

            if (correo.Length > 100)
            {
                MostrarMensajeError("El correo no puede exceder 100 caracteres.");
                return false;
            }

            // Validar formato básico con expresión regular más restrictiva
            if (!_emailRegex.IsMatch(correo))
            {
                MostrarMensajeError("El formato del correo electrónico no es válido. Solo se permiten letras y números en el nombre de usuario (antes del @).");
                return false;
            }

            // Validar estructura del dominio
            string[] partes = correo.Split('@');
            if (partes.Length != 2)
            {
                MostrarMensajeError("El formato del correo electrónico no es válido.");
                return false;
            }

            string usuario = partes[0];
            string dominio = partes[1];

            // Validación adicional para el usuario (solo letras y números)
            if (!Regex.IsMatch(usuario, @"^[a-zA-Z0-9]+$"))
            {
                MostrarMensajeError("El nombre de usuario del correo solo puede contener letras y números, sin caracteres especiales.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(usuario))
            {
                MostrarMensajeError("La parte del usuario del correo no puede estar vacía.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(dominio))
            {
                MostrarMensajeError("El dominio del correo no puede estar vacío.");
                return false;
            }

            // Validar que el dominio tenga al menos un punto
            if (!dominio.Contains("."))
            {
                MostrarMensajeError("El dominio del correo debe contener una extensión válida.");
                return false;
            }

            // Validar que no sea un dominio vacío como "@.com"
            string[] dominioPartes = dominio.Split('.');
            if (dominioPartes.Length < 2 || dominioPartes.Any(string.IsNullOrWhiteSpace))
            {
                MostrarMensajeError("El dominio del correo no es válido.");
                return false;
            }

            // Validar extensiones de dominio comunes
            string ultimaParte = dominioPartes[dominioPartes.Length - 1];
            if (ultimaParte.Length < 2)
            {
                MostrarMensajeError("La extensión del dominio debe tener al menos 2 caracteres.");
                return false;
            }

            return true;
        }

        private bool ValidarObservaciones(string observaciones)
        {
            // No es obligatorio, pero si tiene contenido debe ser válido
            if (string.IsNullOrWhiteSpace(observaciones))
            {
                return true;
            }

            if (observaciones.Length > 500)
            {
                MostrarMensajeError("Las observaciones no pueden exceder 500 caracteres.");
                return false;
            }

            // Validar contenido seguro
            if (!ValidarContenidoSeguro(observaciones, "observaciones"))
            {
                return false;
            }

            return true;
        }

        // --- MÉTODO ACTUALIZADO: Validación con Dropdown ---
        private bool ValidarHistorialMedico(string detalle)
        {
            // Validamos el DropDownList directo
            string condicion = ddlCondicionMedica.SelectedValue;

            if (string.IsNullOrWhiteSpace(condicion))
            {
                MostrarMensajeError("Debe seleccionar una condición médica de la lista.");
                return false;
            }

            // Ya no rechazamos "Ninguna", es una opción válida.
            // El trigger de SQL detectará "Ninguna" y no bloqueará.

            // Validamos el detalle (opcional, pero si hay texto, que sea seguro)
            if (!string.IsNullOrEmpty(detalle))
            {
                if (detalle.Length > 3000)
                {
                    MostrarMensajeError("Los detalles médicos no pueden exceder 3000 caracteres.");
                    return false;
                }
                if (!ValidarContenidoSeguro(detalle, "detalles médicos"))
                {
                    return false;
                }
            }

            return true;
        }

        private bool ValidarContenidoSeguro(string contenido, string campoNombre)
        {
            // Detectar y prevenir XSS
            if (_htmlRegex.IsMatch(contenido))
            {
                MostrarMensajeError($"El {campoNombre} contiene código HTML o scripts no permitidos.");
                return false;
            }

            // Detectar y prevenir SQL Injection
            if (_sqlInjectionRegex.IsMatch(contenido))
            {
                MostrarMensajeError($"El {campoNombre} contiene patrones de inyección SQL no permitidos.");
                return false;
            }

            // Validar caracteres permitidos (texto normal)
            var caracteresInvalidos = contenido.Where(c =>
                char.IsControl(c) && c != '\t' && c != '\r' && c != '\n').ToList();

            if (caracteresInvalidos.Any())
            {
                MostrarMensajeError($"El {campoNombre} contiene caracteres de control no permitidos.");
                return false;
            }

            return true;
        }

        #endregion

        #region Métodos de Seguridad y Utilidades

        private string SanitizeInput(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            // Remover caracteres potencialmente peligrosos
            input = _htmlRegex.Replace(input, string.Empty);
            input = _sqlInjectionRegex.Replace(input, string.Empty);

            // Escapar comillas simples para SQL
            input = input.Replace("'", "''");

            // Limitar longitud máxima según el campo
            if (input.Length > 4000)
                input = input.Substring(0, 4000);

            return input.Trim();
        }

        private bool ExistePacienteDuplicado(string nombre, string telefono, string correo, int idPacienteActual)
        {
            using (SqlConnection con = DatabaseHelper.GetConnection())
            {
                string query = @"
                    SELECT COUNT(*) 
                    FROM Pacientes 
                    WHERE Estado = 1 
                    AND (LOWER(RTRIM(LTRIM(NombreCompleto))) = LOWER(RTRIM(LTRIM(@NombreCompleto)))
                         OR Telefono = @Telefono
                         OR (Correo = @Correo AND @Correo != ''))";

                if (idPacienteActual > 0)
                {
                    query += " AND IDPaciente != @IDPaciente";
                }

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.Add("@NombreCompleto", SqlDbType.NVarChar, 80).Value = nombre;
                    cmd.Parameters.Add("@Telefono", SqlDbType.VarChar, 15).Value = telefono;
                    cmd.Parameters.Add("@Correo", SqlDbType.VarChar, 100).Value = correo ?? string.Empty;

                    if (idPacienteActual > 0)
                    {
                        cmd.Parameters.Add("@IDPaciente", SqlDbType.Int).Value = idPacienteActual;
                    }

                    con.Open();
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count > 0;
                }
            }
        }

        private void LogError(string mensaje, Exception ex)
        {
            // En producción, esto debería ir a un sistema de logging
            System.Diagnostics.Debug.WriteLine($"{mensaje}: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
        }

        #endregion

        #region Métodos de Interfaz de Usuario

        protected void btnNuevoPaciente_Click(object sender, EventArgs e)
        {
            LimpiarFormulario();
            tituloFormulario.InnerText = "Nuevo Paciente";
            pnlFormularioPaciente.Visible = true;
            ClearMessages();
        }

        protected void btnCancelar_Click(object sender, EventArgs e)
        {
            pnlFormularioPaciente.Visible = false;
            ClearMessages();
        }

        protected void gvPacientes_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (string.IsNullOrEmpty(e.CommandArgument?.ToString()))
            {
                MostrarMensajeError("ID de paciente no válido.");
                return;
            }

            if (!int.TryParse(e.CommandArgument.ToString(), out int idPaciente))
            {
                MostrarMensajeError("ID de paciente no válido.");
                return;
            }

            if (e.CommandName == "Editar")
            {
                CargarPacienteParaEditar(idPaciente);
            }
            else if (e.CommandName == "Eliminar")
            {
                EliminarPaciente(idPaciente);
            }
        }

        protected void gvPacientes_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvPacientes.PageIndex = e.NewPageIndex;
            CargarPacientes();
        }

        // CORRECCIÓN AQUÍ: Se excluye la última columna (Acciones) de la codificación HTML para no dañar los botones.
        protected void gvPacientes_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            // Seguridad: prevenir XSS en los datos mostrados
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                // Excluir la última columna (acciones) de la codificación HTML
                for (int i = 0; i < e.Row.Cells.Count - 1; i++)
                {
                    string textoOriginal = e.Row.Cells[i].Text;
                    e.Row.Cells[i].Text = HttpUtility.HtmlEncode(textoOriginal);
                }
            }
        }

        private void CargarPacienteParaEditar(int idPaciente)
        {
            try
            {
                using (SqlConnection con = DatabaseHelper.GetConnection())
                {
                    string query = "SELECT * FROM Pacientes WHERE IDPaciente = @IDPaciente AND Estado = 1";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@IDPaciente", idPaciente);
                        con.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                hfIDPaciente.Value = reader["IDPaciente"].ToString();
                                txtNombreCompleto.Text = reader["NombreCompleto"].ToString();

                                if (reader["FechaNacimiento"] != DBNull.Value)
                                {
                                    DateTime fechaNacimiento = Convert.ToDateTime(reader["FechaNacimiento"]);
                                    txtFechaNacimiento.Text = fechaNacimiento.ToString("yyyy-MM-dd");
                                }
                                else
                                {
                                    txtFechaNacimiento.Text = "";
                                }

                                txtTelefono.Text = reader["Telefono"].ToString();
                                txtCorreo.Text = reader["Correo"].ToString();
                                txtObservaciones.Text = reader["Observaciones"].ToString();

                                // LÓGICA NUEVA PARA SEPARAR HISTORIAL
                                string historialCompleto = reader["HistorialMedico"].ToString();

                                // Intentamos encontrar si coincide exactamente con alguna opción del DDL
                                bool encontradoEnLista = false;
                                foreach (ListItem item in ddlCondicionMedica.Items)
                                {
                                    // Si el historial empieza con el valor de la lista (ej: "Diabetes Tipo 1 - ...")
                                    if (historialCompleto.StartsWith(item.Value))
                                    {
                                        ddlCondicionMedica.SelectedValue = item.Value;
                                        encontradoEnLista = true;

                                        // Extraemos el detalle quitando la condición seleccionada y el guión
                                        if (historialCompleto.Length > item.Value.Length)
                                        {
                                            txtDetalleMedico.Text = historialCompleto.Substring(item.Value.Length).Trim().TrimStart('-').Trim();
                                        }
                                        else
                                        {
                                            txtDetalleMedico.Text = "";
                                        }
                                        break;
                                    }
                                }

                                // Si no coincide (dato antiguo), lo ponemos en detalles y dejamos la lista en seleccionar
                                if (!encontradoEnLista)
                                {
                                    ddlCondicionMedica.SelectedValue = "";
                                    txtDetalleMedico.Text = historialCompleto;
                                }

                                tituloFormulario.InnerText = "Editar Paciente";
                                pnlFormularioPaciente.Visible = true;
                                ClearMessages();
                            }
                            else
                            {
                                MostrarMensajeError("No se encontró el paciente seleccionado.");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogError("Error al cargar paciente para editar", ex);
                MostrarMensajeError("Error al cargar los datos del paciente.");
            }
        }

        // MÉTODO CORREGIDO: EliminarPaciente con hard delete y mejor manejo de errores
        private void EliminarPaciente(int idPaciente)
        {
            try
            {
                // Verificar que el ID sea válido
                if (idPaciente <= 0)
                {
                    MostrarMensajeError("ID de paciente no válido.");
                    return;
                }

                // Verificar registros asociados
                bool tieneRegistrosAsociados = VerificarRegistrosAsociados(idPaciente);

                if (tieneRegistrosAsociados)
                {
                    MostrarMensajeError("No se puede eliminar al paciente. Tiene citas o facturas registradas a su nombre.");
                    return;
                }

                using (SqlConnection con = DatabaseHelper.GetConnection())
                {
                    con.Open();

                    // Usar una transacción para asegurar la integridad de los datos
                    using (SqlTransaction transaction = con.BeginTransaction())
                    {
                        try
                        {
                            // Hard delete en lugar de soft delete
                            string query = "DELETE FROM Pacientes WHERE IDPaciente = @IDPaciente";

                            using (SqlCommand cmd = new SqlCommand(query, con, transaction))
                            {
                                cmd.Parameters.AddWithValue("@IDPaciente", idPaciente);
                                int resultado = cmd.ExecuteNonQuery();

                                if (resultado > 0)
                                {
                                    // Confirmar la transacción
                                    transaction.Commit();
                                    MostrarMensajeExito("Paciente eliminado exitosamente.");
                                    CargarPacientes();
                                }
                                else
                                {
                                    // Revertir la transacción
                                    transaction.Rollback();
                                    MostrarMensajeError("No se pudo eliminar el paciente. Es posible que ya haya sido eliminado o que el ID no sea válido.");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            // Revertir la transacción en caso de error
                            transaction.Rollback();
                            throw ex; // Relanzar la excepción para que sea manejada por el bloque catch exterior
                        }
                    }
                }
            }
            // Capturamos excepciones de SQL específicamente
            catch (SqlException sqlEx)
            {
                LogError("Error de base de datos al eliminar paciente", sqlEx);
                // Mostramos el mensaje de error REAL al usuario
                MostrarMensajeError($"Error de base de datos: {sqlEx.Message}");
            }
            catch (Exception ex)
            {
                LogError("Error inesperado al eliminar paciente", ex);
                // Mostramos el mensaje de error REAL al usuario
                MostrarMensajeError($"Error inesperado: {ex.Message}");
            }
        }

        // MÉTODO CORREGIDO: VerificarRegistrosAsociados con mejor manejo de errores
        private bool VerificarRegistrosAsociados(int idPaciente)
        {
            bool tieneCitas = false;
            bool tieneFacturas = false;

            using (SqlConnection con = DatabaseHelper.GetConnection())
            {
                try
                {
                    con.Open();

                    // Verificar citas activas
                    try
                    {
                        string queryCitas = "SELECT COUNT(*) FROM Citas WHERE IDPaciente = @IDPaciente AND Estado = 1";
                        using (SqlCommand cmdCitas = new SqlCommand(queryCitas, con))
                        {
                            cmdCitas.Parameters.AddWithValue("@IDPaciente", idPaciente);
                            int citasCount = Convert.ToInt32(cmdCitas.ExecuteScalar());
                            tieneCitas = citasCount > 0;
                        }
                    }
                    catch (SqlException ex)
                    {
                        // Si la tabla Citas no existe, lo registramos pero continuamos
                        LogError($"Error al verificar tabla Citas para el paciente {idPaciente}", ex);
                        tieneCitas = false; // Asumimos que no hay citas si la tabla no existe
                    }

                    // Verificar facturas pendientes
                    try
                    {
                        string queryFacturas = "SELECT COUNT(*) FROM Facturacion WHERE IDPaciente = @IDPaciente AND Estado = 'Pendiente'";
                        using (SqlCommand cmdFacturas = new SqlCommand(queryFacturas, con))
                        {
                            cmdFacturas.Parameters.AddWithValue("@IDPaciente", idPaciente);
                            int facturasCount = Convert.ToInt32(cmdFacturas.ExecuteScalar());
                            tieneFacturas = facturasCount > 0;
                        }
                    }
                    catch (SqlException ex)
                    {
                        // Si la tabla Facturacion no existe, lo registramos pero continuamos
                        LogError($"Error al verificar tabla Facturacion para el paciente {idPaciente}", ex);
                        tieneFacturas = false; // Asumimos que no hay facturas si la tabla no existe
                    }
                }
                catch (Exception ex)
                {
                    LogError($"Error general al verificar registros asociados para el paciente {idPaciente}", ex);
                    // En caso de error general, asumimos que hay registros asociados por seguridad
                    return true;
                }
            }

            // Devolvemos true si hay citas o facturas
            return tieneCitas || tieneFacturas;
        }

        private void LimpiarFormulario()
        {
            hfIDPaciente.Value = "";
            txtNombreCompleto.Text = "";
            txtFechaNacimiento.Text = "";
            txtTelefono.Text = "";
            txtCorreo.Text = "";
            txtObservaciones.Text = "";
            // Limpiar nuevos campos
            ddlCondicionMedica.ClearSelection();
            txtDetalleMedico.Text = "";
        }

        private void ClearMessages()
        {
            lblMensajeError.Visible = false;
            lblMensajeExito.Visible = false;
            lblMensajeAdvertencia.Visible = false;
        }

        private void MostrarMensajeError(string mensaje)
        {
            lblMensajeError.Text = mensaje;
            lblMensajeError.Visible = true;
            lblMensajeExito.Visible = false;
            lblMensajeAdvertencia.Visible = false;
        }

        private void MostrarMensajeExito(string mensaje)
        {
            lblMensajeExito.Text = mensaje;
            lblMensajeExito.Visible = true;
            lblMensajeError.Visible = false;
            lblMensajeAdvertencia.Visible = false;
        }

        private void MostrarMensajeAdvertencia(string mensaje)
        {
            lblMensajeAdvertencia.Text = mensaje;
            lblMensajeAdvertencia.Visible = true;
            lblMensajeError.Visible = false;
            lblMensajeExito.Visible = false;
        }

        #endregion
    }
}