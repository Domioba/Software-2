<%@ Page Title="Gestión de Pacientes" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Pacientes.aspx.cs" Inherits="ClinicaAdministrador.Pacientes" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link href="css/estiloPacientes.css" rel="stylesheet" type="text/css" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
</asp:Content>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    
    <!-- Protección contra CSRF -->
    <asp:HiddenField ID="hfAntiForgeryToken" runat="server" />
    
    <div class="d-flex justify-content-between align-items-center mb-4">
        <h2 class="fw-bold">Gestión de Pacientes</h2>
        <asp:Button ID="btnNuevoPaciente" runat="server" Text="Nuevo Paciente" CssClass="btn-add-new" OnClick="btnNuevoPaciente_Click" />
    </div>

    <!-- Mensajes -->
    <asp:Label ID="lblMensajeError" runat="server" Visible="False" CssClass="alert-custom alert-error d-block mb-3" role="alert"></asp:Label>
    <asp:Label ID="lblMensajeExito" runat="server" Visible="False" CssClass="alert-custom alert-success d-block mb-3" role="alert"></asp:Label>
    <asp:Label ID="lblMensajeAdvertencia" runat="server" Visible="False" CssClass="alert-custom alert-warning d-block mb-3" role="alert"></asp:Label>

    <!-- PANEL DE BÚSQUEDA -->
    <asp:Panel ID="pnlBusqueda" runat="server" CssClass="card card-custom mb-4 shadow">
        <div class="card-header">
            <h4 class="mb-0">Filtros de Búsqueda</h4>
        </div>
        <div class="card-body">
            <div class="row">
                <div class="col-md-4 mb-3">
                    <label for="txtBuscarNombre" class="form-label fw-bold">Nombre</label>
                    <asp:TextBox ID="txtBuscarNombre" runat="server" CssClass="form-control" 
                        placeholder="Buscar por nombre completo" MaxLength="80"></asp:TextBox>
                </div>
                <div class="col-md-4 mb-3">
                    <label for="txtBuscarTelefono" class="form-label fw-bold">Teléfono</label>
                    <asp:TextBox ID="txtBuscarTelefono" runat="server" CssClass="form-control" 
                        placeholder="Buscar por teléfono" MaxLength="15"></asp:TextBox>
                </div>
                <div class="col-md-4 mb-3">
                    <label for="txtBuscarCorreo" class="form-label fw-bold">Correo</label>
                    <asp:TextBox ID="txtBuscarCorreo" runat="server" CssClass="form-control" 
                        placeholder="Buscar por correo" MaxLength="100"></asp:TextBox>
                </div>
            </div>
            <div class="row">
                <div class="col-md-4 mb-3">
                    <label for="txtBuscarID" class="form-label fw-bold">ID Paciente</label>
                    <asp:TextBox ID="txtBuscarID" runat="server" CssClass="form-control" 
                        placeholder="Buscar por ID" MaxLength="10"></asp:TextBox>
                </div>
                <div class="col-md-4 mb-3">
                    <label for="txtBuscarFechaDesde" class="form-label fw-bold">Fecha de Registro (Desde)</label>
                    <asp:TextBox ID="txtBuscarFechaDesde" runat="server" TextMode="Date" CssClass="form-control"></asp:TextBox>
                </div>
                <div class="col-md-4 mb-3">
                    <label for="txtBuscarFechaHasta" class="form-label fw-bold">Fecha de Registro (Hasta)</label>
                    <asp:TextBox ID="txtBuscarFechaHasta" runat="server" TextMode="Date" CssClass="form-control"></asp:TextBox>
                </div>
            </div>
            <div class="d-flex gap-2">
                <asp:Button ID="btnBuscar" runat="server" Text="Buscar" CssClass="btn btn-primary" OnClick="btnBuscar_Click" />
                <asp:Button ID="btnLimpiarBusqueda" runat="server" Text="Limpiar Filtros" CssClass="btn btn-secondary-custom" OnClick="btnLimpiarBusqueda_Click" />
            </div>
        </div>
    </asp:Panel>

    <!-- FORMULARIO -->
    <asp:Panel ID="pnlFormularioPaciente" runat="server" CssClass="card card-custom mb-4 shadow" Visible="false">
        <div class="card-header">
            <h4 id="tituloFormulario" runat="server" class="mb-0">Nuevo Paciente</h4>
        </div>
        <div class="card-body">
            <asp:HiddenField ID="hfIDPaciente" runat="server" />
            <asp:HiddenField ID="hfTimestamp" runat="server" />

            <!-- Fila 1: Nombre y Fecha de Nacimiento -->
            <div class="row">
                <div class="col-md-6 mb-3">
                    <label for="txtNombreCompleto" class="form-label fw-bold">Nombre Completo <span class="text-danger">*</span></label>
                    <asp:TextBox ID="txtNombreCompleto" runat="server" CssClass="form-control" 
                        MaxLength="80" 
                        onkeypress="return validarSoloLetras(event)" 
                        oninput="validarNombreCompleto()"
                        onblur="validarNombreCompletoCompleto()"
                        autocomplete="name"
                        placeholder="Ej: María José Rodríguez López"></asp:TextBox>
                    <small class="text-danger" id="errorNombre" style="display:none;"></small>
                    <div class="form-text">Ingrese nombre y apellido (números no permitidos)</div>
                </div>
                <div class="col-md-6 mb-3">
                    <label for="txtFechaNacimiento" class="form-label fw-bold">Fecha de Nacimiento <span class="text-danger">*</span></label>
                    <asp:TextBox ID="txtFechaNacimiento" runat="server" TextMode="Date" CssClass="form-control" 
                        onchange="validarEdad()"
                        max='<%= DateTime.Now.AddYears(-12).ToString("yyyy-MM-dd") %>'
                        min='<%= DateTime.Now.AddYears(-85).ToString("yyyy-MM-dd") %>'></asp:TextBox>
                    <small class="text-danger" id="errorFecha" style="display:none;"></small>
                    <div class="form-text">Edad válida: entre 12 y 85 años</div>
                </div>
            </div>

            <!-- Fila 2: Teléfono y Correo -->
            <div class="row">
                <div class="col-md-6 mb-3">
                    <label for="txtTelefono" class="form-label fw-bold">Teléfono <span class="text-danger">*</span></label>
                    <asp:TextBox ID="txtTelefono" runat="server" CssClass="form-control" MaxLength="15" 
                        onkeypress="return validarSoloNumerosTelefono(event)"
                        oninput="validarFormatoTelefono()"
                        onblur="validarTelefonoCompleto()"
                        autocomplete="tel"
                        placeholder="Ej: 612345678"></asp:TextBox>
                    <small class="text-danger" id="errorTelefono" style="display:none;"></small>
                    <div class="form-text">8-15 dígitos, solo números</div>
                </div>
                <div class="col-md-6 mb-3">
                    <label for="txtCorreo" class="form-label fw-bold">Correo Electrónico <span class="text-danger">*</span></label>
                    <asp:TextBox ID="txtCorreo" runat="server" TextMode="Email" CssClass="form-control" MaxLength="100" 
                        onblur="validarCorreoCompleto()"
                        autocomplete="email"
                        placeholder="ejemplo@dominio.com"></asp:TextBox>
                    <small class="text-danger" id="errorCorreo" style="display:none;"></small>
                    <div class="form-text">Formato válido: usuario@dominio.com</div>
                </div>
            </div>

            <!-- Fila 3: Observaciones -->
            <div class="mb-3">
                <label for="txtObservaciones" class="form-label fw-bold">Observaciones</label>
                <asp:TextBox ID="txtObservaciones" runat="server" TextMode="MultiLine" Rows="3" CssClass="form-control" 
                    MaxLength="500" 
                    oninput="validarContenidoSeguro(this, 'observaciones')"
                    placeholder="Observaciones generales del paciente"></asp:TextBox>
                <div class="form-text"><span id="contadorObservaciones">0</span>/500 caracteres - Información válida únicamente</div>
                <small class="text-danger" id="errorObservaciones" style="display:none;"></small>
            </div>

            <!-- Historial Médico Estructurado -->
            <div class="mb-3">
                <label for="ddlCondicionMedica" class="form-label fw-bold">Condición Médica Principal <span class="text-danger">*</span></label>
                <asp:DropDownList ID="ddlCondicionMedica" runat="server" CssClass="form-select" onchange="actualizarMensajeCondicion()">
                    <asp:ListItem Text="-- Seleccione una condición --" Value="" Selected="True" />
                    <asp:ListItem Text="Ninguna / Sano sin antecedentes" Value="Ninguna" />
                    <asp:ListItem Text="Diabetes Tipo 1" Value="Diabetes Tipo 1" />
                    <asp:ListItem Text="Diabetes Tipo 2" Value="Diabetes Tipo 2" />
                    <asp:ListItem Text="Hipertensión Arterial" Value="Hipertensión" />
                    <asp:ListItem Text="Problemas Cardíacos" Value="Cardíaco" />
                    <asp:ListItem Text="Epilepsia / Convulsiones" Value="Epilepsia" />
                    <asp:ListItem Text="Hepatitis" Value="Hepatitis" />
                    <asp:ListItem Text="Cáncer / Oncológico" Value="Cáncer" />
                    <asp:ListItem Text="Embarazo" Value="Embarazo" />
                    <asp:ListItem Text="Otras Enfermedades Crónicas" Value="Otras" />
                 
                    <asp:ListItem Text="Trastornos de Coagulación / Anticoagulantes" Value="Coagulacion" />
                    <asp:ListItem Text="Alergias Severas / Anafilaxia" Value="Alergias" />
                    <asp:ListItem Text="Enfermedad Autoinmune" Value="Autoinmune" />
                    <asp:ListItem Text="Fotosensibilidad / Lupus" Value="Fotosensibilidad" />
                    <asp:ListItem Text="Quimioterapia / Inmunosupresión" Value="Inmunosupresion" />
                    <asp:ListItem Text="Marcapasos / Dispositivos Electrónicos" Value="Marcapasos" />
                    <asp:ListItem Text="Cicatrización Queloide" Value="Queloide" />
                </asp:DropDownList>
                <div class="form-text">Seleccione la condición principal para validación de seguridad en citas.</div>
                <small id="mensajeCondicion" class="form-text text-info" style="display:none;"></small>
            </div>

            <div class="mb-3">
                <label for="txtDetalleMedico" class="form-label fw-bold">Detalles Adicionales / Observaciones Médicas</label>
                <asp:TextBox ID="txtDetalleMedico" runat="server" TextMode="MultiLine" Rows="3" CssClass="form-control" 
                    MaxLength="3000" 
                    placeholder="Especifique medicamentos, alergias o detalles de la condición seleccionada..." 
                    oninput="validarContenidoSeguro(this, 'historial')"></asp:TextBox>
                <div class="form-text"><span id="contadorHistorial">0</span>/3000 caracteres (Opcional pero recomendado)</div>
            </div>
            
            <!-- Botones -->
            <div class="d-flex gap-2">
                <asp:Button ID="btnGuardarPaciente" runat="server" Text="Guardar" 
                    OnClick="btnGuardarPaciente_Click" CssClass="btn btn-primary" OnClientClick="return validarFormularioCompleto();" />
                <asp:Button ID="btnCancelar" runat="server" Text="Cancelar" CssClass="btn btn-secondary-custom" 
                    OnClick="btnCancelar_Click" CausesValidation="false" />
            </div>
        </div>
    </asp:Panel>

    <!-- GRIDVIEW -->
    <div class="table-responsive-custom">
        <asp:GridView ID="gvPacientes" runat="server" CssClass="table table-custom" AutoGenerateColumns="False" 
            DataKeyNames="IDPaciente" OnRowCommand="gvPacientes_RowCommand" AllowPaging="True" PageSize="10" 
            OnPageIndexChanging="gvPacientes_PageIndexChanging" OnRowDataBound="gvPacientes_RowDataBound"
            EmptyDataText="No hay pacientes registrados">
            <Columns>
                <asp:BoundField DataField="IDPaciente" HeaderText="ID" ReadOnly="True" ItemStyle-CssClass="fw-bold" />
                <asp:BoundField DataField="NombreCompleto" HeaderText="Nombre Completo" />
                <asp:BoundField DataField="Telefono" HeaderText="Teléfono" />
                <asp:BoundField DataField="Correo" HeaderText="Correo" />
                <asp:BoundField DataField="FechaNacimiento" HeaderText="Fecha Nacimiento" DataFormatString="{0:dd/MM/yyyy}" />
                <asp:BoundField DataField="FechaRegistro" HeaderText="Fecha de Registro" DataFormatString="{0:dd/MM/yyyy HH:mm}" />
                <asp:TemplateField HeaderText="Acciones">
                    <ItemTemplate>
                        <div class="table-actions">
                            <asp:LinkButton ID="btnEditar" runat="server" CommandName="Editar" CommandArgument='<%# Eval("IDPaciente") %>' 
                                CssClass="btn-action btn-action-edit" CausesValidation="false" ToolTip="Editar paciente">
                                <i class="bi bi-pencil"></i> Editar
                            </asp:LinkButton>
                            <asp:LinkButton ID="btnEliminar" runat="server" CommandName="Eliminar" CommandArgument='<%# Eval("IDPaciente") %>' 
                                CssClass="btn-action btn-action-delete" 
                                OnClientClick="return confirmarEliminacion(this);" CausesValidation="false" ToolTip="Eliminar paciente">
                                <i class="bi bi-trash"></i> Eliminar
                            </asp:LinkButton>
                        </div>
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
            <EmptyDataTemplate>
                <div class="alert-custom alert-info text-center py-4">
                    <i class="bi bi-person-x fs-1 d-block mb-2"></i>
                    No hay pacientes registrados en el sistema.
                </div>
            </EmptyDataTemplate>
            <PagerSettings Mode="NumericFirstLast" FirstPageText="Primera" LastPageText="Última" />
            <PagerStyle CssClass="gridview-pager" />
        </asp:GridView>
    </div>

    <script type="text/javascript">
        // ========== VALIDACIONES DE NOMBRE ==========
        function validarSoloLetras(event) {
            var charCode = event.which ? event.which : event.keyCode;
            if (charCode < 33 || (charCode >= 37 && charCode <= 40)) return true;
            if (charCode === 32) return true;
            var charStr = String.fromCharCode(charCode);
            var regex = /^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s']$/;
            if (!regex.test(charStr)) {
                event.preventDefault();
                mostrarErrorTemporal('nombre', 'Solo se permiten letras, espacios y acentos');
                return false;
            }
            return true;
        }

        function validarNombreCompleto() {
            var nombre = document.getElementById('<%= txtNombreCompleto.ClientID %>');
            var valor = nombre.value.trim();
            var valorLimpio = valor.replace(/[^a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s']/g, '');
            if (valor !== valorLimpio) nombre.value = valorLimpio;
            return true;
        }

        function validarNombreCompletoCompleto() {
            var nombre = document.getElementById('<%= txtNombreCompleto.ClientID %>');
            var errorElement = document.getElementById('errorNombre');
            var valor = nombre.value.trim();
            var palabras = valor.split(' ').filter(function (p) { return p.length > 0; });

            if (!valor) { errorElement.style.display = 'block'; errorElement.textContent = 'El nombre completo es obligatorio'; return false; }
            if (valor.length < 2) { errorElement.style.display = 'block'; errorElement.textContent = 'Mínimo 2 caracteres'; return false; }
            if (valor.length > 80) { errorElement.style.display = 'block'; errorElement.textContent = 'Máximo 80 caracteres'; return false; }
            if (!/^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s']+$/.test(valor)) { errorElement.style.display = 'block'; errorElement.textContent = 'Solo letras, espacios y acentos'; return false; }
            if (palabras.length < 2) { errorElement.style.display = 'block'; errorElement.textContent = 'Debe ingresar nombre y apellido'; return false; }
            for (var i = 0; i < palabras.length; i++) { if (palabras[i].length < 2) { errorElement.style.display = 'block'; errorElement.textContent = 'Cada palabra debe tener mínimo 2 caracteres'; return false; } }
            errorElement.style.display = 'none';
            return true;
        }

        // ========== VALIDACIONES DE FECHA ==========
        function validarEdad() {
            var input = document.getElementById('<%= txtFechaNacimiento.ClientID %>');
            var errorElement = document.getElementById('errorFecha');
            var fechaNacimiento = input.value;
            if (!fechaNacimiento) { errorElement.style.display = 'block'; errorElement.textContent = 'Fecha obligatoria'; return false; }
            var fecha = new Date(fechaNacimiento);
            var hoy = new Date();
            var edad = hoy.getFullYear() - fecha.getFullYear();
            var mes = hoy.getMonth() - fecha.getMonth();
            if (mes < 0 || (mes === 0 && hoy.getDate() < fecha.getDate())) edad--;
            if (fecha > hoy) { errorElement.style.display = 'block'; errorElement.textContent = 'Fecha no puede ser futura'; return false; }
            if (edad < 12) { errorElement.style.display = 'block'; errorElement.textContent = 'Mínimo 12 años'; return false; }
            if (edad > 85) { errorElement.style.display = 'block'; errorElement.textContent = 'Máximo 85 años'; return false; }
            errorElement.style.display = 'none';
            return true;
        }

        // ========== VALIDACIONES DE TELÉFONO ==========
        function validarSoloNumerosTelefono(event) {
            var charCode = event.which ? event.which : event.keyCode;
            if (charCode < 33 || (charCode >= 37 && charCode <= 40)) return true;
            if (charCode >= 48 && charCode <= 57) return true;
            event.preventDefault();
            mostrarErrorTemporal('telefono', 'Solo números permitidos');
            return false;
        }

        function validarFormatoTelefono() {
            var telefono = document.getElementById('<%= txtTelefono.ClientID %>');
            telefono.value = telefono.value.replace(/\D/g, '');
        }

        function validarTelefonoCompleto() {
            var input = document.getElementById('<%= txtTelefono.ClientID %>');
            var errorElement = document.getElementById('errorTelefono');
            var valor = input.value.replace(/\D/g, '');
            if (!valor) { errorElement.style.display = 'block'; errorElement.textContent = 'Teléfono obligatorio'; return false; }
            if (valor.length < 8) { errorElement.style.display = 'block'; errorElement.textContent = 'Mínimo 8 dígitos'; return false; }
            if (valor.length > 15) { errorElement.style.display = 'block'; errorElement.textContent = 'Máximo 15 dígitos'; return false; }
            if (!/^\d+$/.test(valor)) { errorElement.style.display = 'block'; errorElement.textContent = 'Solo números'; return false; }
            errorElement.style.display = 'none';
            return true;
        }

        // ========== VALIDACIONES DE CORREO ==========
        function validarCorreoCompleto() {
            var input = document.getElementById('<%= txtCorreo.ClientID %>');
            var errorElement = document.getElementById('errorCorreo');
            var valor = input.value.trim().toLowerCase();
            if (!valor) { errorElement.style.display='block'; errorElement.textContent='Correo obligatorio'; return false; }
            var emailRegex = /^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$/;
            if (!emailRegex.test(valor)) { errorElement.style.display='block'; errorElement.textContent='Formato inválido'; return false; }
            var partes = valor.split('@');
            if (partes.length !== 2 || !partes[0] || !partes[1] || partes[1].indexOf('.') === -1) { errorElement.style.display='block'; errorElement.textContent='Dominio inválido'; return false; }
            var dominioPartes = partes[1].split('.');
            if (dominioPartes.length < 2 || !dominioPartes[dominioPartes.length - 1]) { errorElement.style.display='block'; errorElement.textContent='Extensión de dominio inválida'; return false; }
            input.value = valor;
            errorElement.style.display='none';
            return true;
        }

        // ========== VALIDACIONES DE CONTENIDO SEGURO ==========
        function validarContenidoSeguro(elemento, tipo) {
            var valor = elemento.value;
            var errorElement = document.getElementById('error' + tipo.charAt(0).toUpperCase() + tipo.slice(1));
            var maxCaracteres = tipo === 'observaciones' ? 500 : 3000;
            if (valor.length > maxCaracteres) {
                if (errorElement) { errorElement.style.display='block'; errorElement.textContent='Excede límite de '+maxCaracteres+' caracteres'; }
                elemento.value = valor.substring(0, maxCaracteres);
                return false;
            }
            var patronesPeligrosos = [/<script\b[^<]*(?:(?!<\/script>)<[^<]*)*<\/script>/gi, /javascript:/gi, /on\w+\s*=/gi, /<\w+[^>]*>/gi, /data:/gi, /vbscript:/gi, /expression\s*\(/gi, /url\s*\(/gi];
            var contenidoLimpio = valor;
            var contenidoPeligroso = false;
            for (var i=0; i<patronesPeligrosos.length; i++) {
                if (patronesPeligrosos[i].test(valor)) { contenidoPeligroso = true; contenidoLimpio = contenidoLimpio.replace(patronesPeligrosos[i], ''); }
            }
            if (contenidoPeligroso) {
                if (errorElement) { errorElement.style.display='block'; errorElement.textContent='Contenido no permitido eliminado'; }
                elemento.value = contenidoLimpio;
                return false;
            }
            if (errorElement) errorElement.style.display='none';
            return true;
        }

        // ========== MENSAJE INFORMATIVO POR CONDICIÓN MÉDICA ==========
        function actualizarMensajeCondicion() {
            var ddl = document.getElementById('<%= ddlCondicionMedica.ClientID %>');
            var mensaje = document.getElementById('mensajeCondicion');
            var valor = ddl.value;
            var mensajes = {
                'Embarazo': '⚠️ Con esta condición, no se podrán agendar: Inyectables ni Láser en la misma cita.',
                'Diabetes Tipo 1': '⚠️ Restricciones: Procedimientos con microlesiones o láser requieren evaluación previa.',
                'Diabetes Tipo 2': '⚠️ Precaución: Inyectables y procedimientos invasivos requieren autorización médica.',
                'Hipertensión': '⚠️ Los tratamientos inyectables pueden interactuar con medicamentos para la presión.',
                'Cardíaco': '⚠️ Inyectables requieren evaluación cardiológica previa.',
                'Epilepsia': '⚠️ Tratamientos con láser/luz pulsada están contraindicados por fotosensibilidad.',
                'Hepatitis': '⚠️ Procedimientos invasivos requieren evaluación hepática previa.',
                'Cáncer': '⚠️ Cualquier tratamiento invasivo requiere autorización oncológica.',
                'Coagulacion': '⚠️ Procedimientos que rompen la piel tienen riesgo elevado de sangrado.',
                'Alergias': '⚠️ Inyectables requieren prueba de alergia previa.',
                'Fotosensibilidad': '⚠️ Tratamientos con láser están contraindicados.',
                'Inmunosupresion': '⚠️ Procedimientos que rompen la barrera cutánea tienen riesgo de infección.',
                'Marcapasos': '⚠️ Equipos láser pueden interferir con dispositivos electrónicos implantados.',
                'Queloide': '⚠️ Procedimientos con microagujas pueden causar cicatrices queloides.'
            };
            if (mensajes[valor]) {
                mensaje.textContent = mensajes[valor];
                mensaje.style.display = 'block';
            } else {
                mensaje.style.display = 'none';
            }
        }

        // ========== VALIDACIÓN COMPLETA DEL FORMULARIO ==========
        // ========== VALIDACIÓN COMPLETA DEL FORMULARIO ==========
        // ========== VALIDACIÓN COMPLETA DEL FORMULARIO (CON DEBUG) ==========
        // ========== VALIDACIÓN COMPLETA DEL FORMULARIO (CORREGIDA) ==========
        function validarFormularioCompleto() {
            console.log("🔍 Iniciando validación del formulario...");

            var resultados = {
                nombre: validarNombreCompletoCompleto(),
                fecha: validarEdad(),
                telefono: validarTelefonoCompleto(),
                correo: validarCorreoCompleto(),
                observaciones: validarContenidoSeguro(document.getElementById('<%= txtObservaciones.ClientID %>'), 'observaciones'),
                historial: validarContenidoSeguro(document.getElementById('<%= txtDetalleMedico.ClientID %>'), 'historial')
            };
    
            console.log("📋 Resultados validación:", resultados);
    
            var esValido = Object.values(resultados).every(function(v) { return v === true; });
    
            if (esValido) {
                console.log("✅ Validaciones cliente OK. Enviando al servidor...");
                // RETIRADO: No deshabilitamos el botón aquí para evitar bloqueos si el servidor rechaza los datos.
                // return true permite el postback.
                return true;
            } else {
                console.log("❌ Validación fallida. Campos con error:", Object.keys(resultados).filter(k => !resultados[k]));
        
                // Enfocar primer campo con error
                var primerosErrores = document.querySelectorAll('.text-danger[style*="display: block"]');
                if (primerosErrores.length > 0) {
                    var campoId = primerosErrores[0].id.replace('error', 'txt'); // Ajuste genérico
                    // Caso especial para drop-downs si fuera necesario, aquí asumimos txt
                    if (campoId === 'txtCondicion') campoId = '<%= ddlCondicionMedica.ClientID %>';

                    var campo = document.getElementById(campoId);
                    if (campo) {
                        campo.focus();
                        campo.scrollIntoView({ behavior: 'smooth', block: 'center' });
                    }
                }

                return false;
            }
        }
        // ========== FUNCIONES AUXILIARES ==========
        function mostrarErrorTemporal(campo, mensaje) {
            var errorElement = document.getElementById('error' + campo.charAt(0).toUpperCase() + campo.slice(1));
            if (errorElement) {
                errorElement.style.display = 'block';
                errorElement.textContent = mensaje;
                setTimeout(function() { errorElement.style.display = 'none'; }, 3000);
            }
        }

        function confirmarEliminacion(boton) {
            var fila = boton.closest('tr');
            var nombrePaciente = fila.cells[1].textContent;
            return confirm('¿Eliminar al paciente: ' + nombrePaciente + '?\nEsta acción no se puede deshacer.');
        }

        // Contadores de caracteres
        document.addEventListener('DOMContentLoaded', function() {
            var txtObservaciones = document.getElementById('<%= txtObservaciones.ClientID %>');
            var txtDetalleMedico = document.getElementById('<%= txtDetalleMedico.ClientID %>');
            var contadorObservaciones = document.getElementById('contadorObservaciones');
            var contadorHistorial = document.getElementById('contadorHistorial');

            function actualizarContador(elemento, contador, maximo) {
                if (elemento && contador) {
                    contador.textContent = elemento.value.length;
                    if (elemento.value.length > maximo * 0.9) contador.className = 'text-warning';
                    else if (elemento.value.length > maximo) contador.className = 'text-danger';
                    else contador.className = 'text-muted';
                }
            }
            if (txtObservaciones && contadorObservaciones) {
                actualizarContador(txtObservaciones, contadorObservaciones, 500);
                txtObservaciones.addEventListener('input', function() { actualizarContador(this, contadorObservaciones, 500); });
            }
            if (txtDetalleMedico && contadorHistorial) {
                actualizarContador(txtDetalleMedico, contadorHistorial, 3000);
                txtDetalleMedico.addEventListener('input', function() { actualizarContador(this, contadorHistorial, 3000); });
            }
        });

        // Restaurar botón después de error
        window.restaurarBotonGuardar = function() {
            var btnGuardar = document.getElementById('<%= btnGuardarPaciente.ClientID %>');
            if (btnGuardar) {
                btnGuardar.disabled = false;
                btnGuardar.innerHTML = 'Guardar';
            }
        };
    </script>
</asp:Content>