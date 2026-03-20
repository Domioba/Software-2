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
            <!-- Timestamp para prevenir doble envío -->
            <asp:HiddenField ID="hfTimestamp" runat="server" />

            <!-- Primera Fila: Nombre y Fecha de Nacimiento -->
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
                    <div class="form-text">Ingrese nombre y apellido,(Numeros no Permitidos))</div>
                </div>
                <div class="col-md-6 mb-3">
                    <label for="txtFechaNacimiento" class="form-label fw-bold">Fecha de Nacimiento <span class="text-danger">*</span></label>
                    <asp:TextBox ID="txtFechaNacimiento" runat="server" TextMode="Date" CssClass="form-control" 
                        onchange="validarEdad()"
                        max='<%= DateTime.Now.AddYears(-12).ToString("yyyy-MM-dd") %>'
                        min='<%= DateTime.Now.AddYears(-85).ToString("yyyy-MM-dd") %>'></asp:TextBox>
                    <small class="text-danger" id="errorFecha" style="display:none;"></small>
                    <div class="form-text">Edad valida, entre 12 y 85 años </div>
                </div>
            </div> <!-- Cierre de la primera fila -->

            <!-- Segunda Fila: Teléfono y Correo -->
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
            </div> <!-- Cierre de la segunda fila -->

            <!-- Tercera Fila: Observaciones -->
            <div class="mb-3">
                <label for="txtObservaciones" class="form-label fw-bold">Observaciones</label>
                <asp:TextBox ID="txtObservaciones" runat="server" TextMode="MultiLine" Rows="3" CssClass="form-control" 
                    MaxLength="500" 
                    oninput="validarContenidoSeguro(this, 'observaciones')"
                    placeholder="Observaciones generales del paciente"></asp:TextBox>
                <div class="form-text"><span id="contadorObservaciones">0</span>/500 caracteres - Escribe únicamente información válida.</div>
                <small class="text-danger" id="errorObservaciones" style="display:none;"></small>
            </div>

           <!-- NUEVO: Historial Médico Estructurado -->
            <div class="mb-3">
                <label for="ddlCondicionMedica" class="form-label fw-bold">Condición Médica Principal <span class="text-danger">*</span></label>
                <asp:DropDownList ID="ddlCondicionMedica" runat="server" CssClass="form-select">
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
                </asp:DropDownList>
                <div class="form-text">Seleccione la condición principal para la validación de seguridad.</div>
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
        </div> <!-- Cierre de card-body -->
    </asp:Panel> <!-- Cierre de Panel -->

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

           // Permitir teclas de control
           if (charCode < 33 || (charCode >= 37 && charCode <= 40)) {
               return true;
           }

           // Permitir espacios (32)
           if (charCode === 32) {
               return true;
           }

           // Permitir letras (a-z, A-Z) y caracteres con acento
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
           var errorElement = document.getElementById('errorNombre');
           var valor = nombre.value.trim();

           // Remover números y caracteres no permitidos
           var valorLimpio = valor.replace(/[^a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s']/g, '');
           if (valor !== valorLimpio) {
               nombre.value = valorLimpio;
           }

           return true;
       }

       function validarNombreCompletoCompleto() {
           var nombre = document.getElementById('<%= txtNombreCompleto.ClientID %>');
           var errorElement = document.getElementById('errorNombre');
           var valor = nombre.value.trim();
           var palabras = valor.split(' ').filter(function (palabra) {
               return palabra.length > 0;
           });

           if (!valor) {
               errorElement.style.display = 'block';
               errorElement.textContent = 'El nombre completo es obligatorio';
               return false;
           } else if (valor.length < 2) {
               errorElement.style.display = 'block';
               errorElement.textContent = 'El nombre debe tener al menos 2 caracteres';
               return false;
           } else if (valor.length > 80) {
               errorElement.style.display = 'block';
               errorElement.textContent = 'El nombre no puede exceder 80 caracteres';
               return false;
           } else if (!/^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s']+$/.test(valor)) {
               errorElement.style.display = 'block';
               errorElement.textContent = 'Solo se permiten letras, espacios y acentos';
               return false;
           } else if (palabras.length < 2) {
               errorElement.style.display = 'block';
               errorElement.textContent = 'Debe ingresar al menos un nombre y un apellido';
               return false;
           }

           // Validar que cada palabra tenga al menos 2 caracteres
           for (var i = 0; i < palabras.length; i++) {
               if (palabras[i].length < 2) {
                   errorElement.style.display = 'block';
                   errorElement.textContent = 'Cada palabra del nombre debe tener al menos 2 caracteres';
                   return false;
               }
           }

           errorElement.style.display = 'none';
           return true;
       }

       // ========== VALIDACIONES DE FECHA DE NACIMIENTO ==========
       function validarEdad() {
           var fechaNacimientoInput = document.getElementById('<%= txtFechaNacimiento.ClientID %>');
           var errorElement = document.getElementById('errorFecha');
           var fechaNacimiento = fechaNacimientoInput.value;

           if (!fechaNacimiento) {
               errorElement.style.display = 'block';
               errorElement.textContent = 'La fecha de nacimiento es obligatoria';
               return false;
           }

           var fecha = new Date(fechaNacimiento);
           var hoy = new Date();
           var edad = hoy.getFullYear() - fecha.getFullYear();
           var mes = hoy.getMonth() - fecha.getMonth();

           if (mes < 0 || (mes === 0 && hoy.getDate() < fecha.getDate())) {
               edad--;
           }

           if (fecha > hoy) {
               errorElement.style.display = 'block';
               errorElement.textContent = 'La fecha no puede ser mayor a la actual';
               return false;
           } else if (edad < 12) {
               errorElement.style.display = 'block';
               errorElement.textContent = 'El paciente debe tener al menos 12 años (clínica para adultos)';
               return false;
           } else if (edad > 85) {
               errorElement.style.display = 'block';
               errorElement.textContent = 'La edad no puede ser mayor a 85 años';
               return false;
           }

           errorElement.style.display = 'none';
           return true;
       }

       // ========== VALIDACIONES DE TELÉFONO ==========
       function validarSoloNumerosTelefono(event) {
           var charCode = event.which ? event.which : event.keyCode;

           // Permitir teclas de control
           if (charCode < 33 || (charCode >= 37 && charCode <= 40)) {
               return true;
           }

           // Solo permitir números (48-57)
           if (charCode >= 48 && charCode <= 57) {
               return true;
           }

           event.preventDefault();
           mostrarErrorTemporal('telefono', 'Solo se permiten números');
           return false;
       }

       function validarFormatoTelefono() {
           var telefono = document.getElementById('<%= txtTelefono.ClientID %>');
           // Remover cualquier carácter que no sea número
           telefono.value = telefono.value.replace(/\D/g, '');
       }

       function validarTelefonoCompleto() {
           var telefonoInput = document.getElementById('<%= txtTelefono.ClientID %>');
           var errorElement = document.getElementById('errorTelefono');
           var valor = telefonoInput.value.replace(/\D/g, '');

           if (!valor) {
               errorElement.style.display = 'block';
               errorElement.textContent = 'El teléfono es obligatorio';
               return false;
           }

           if (valor.length < 8) {
               errorElement.style.display = 'block';
               errorElement.textContent = 'El teléfono debe tener al menos 8 dígitos';
               return false;
           } else if (valor.length > 15) {
               errorElement.style.display = 'block';
               errorElement.textContent = 'El teléfono no puede exceder 15 dígitos';
               return false;
           } else if (!/^\d+$/.test(valor)) {
               errorElement.style.display = 'block';
               errorElement.textContent = 'Solo se permiten números';
               return false;
           }

           errorElement.style.display = 'none';
           return true;
       }

       // ========== VALIDACIONES DE CORREO ELECTRÓNICO ==========
       function validarCorreoCompleto() {
           var correoInput = document.getElementById('<%= txtCorreo.ClientID %>');
        var errorElement = document.getElementById('errorCorreo');
        var valor = correoInput.value.trim().toLowerCase();
        
        if (!valor) {
            errorElement.style.display = 'block';
            errorElement.textContent = 'El correo electrónico es obligatorio';
            return false;
        }
        
        // Expresión regular robusta para validar correos
        var emailRegex = /^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$/;
        
        if (!emailRegex.test(valor)) {
            errorElement.style.display = 'block';
            errorElement.textContent = 'Formato de correo inválido. Use: usuario@dominio.com';
            return false;
        }
        
        // Validar que no tenga dominios vacíos
        var partes = valor.split('@');
        if (partes.length !== 2 || !partes[0] || !partes[1] || partes[1].indexOf('.') === -1) {
            errorElement.style.display = 'block';
            errorElement.textContent = 'El dominio del correo no es válido';
            return false;
        }
        
        // Validar que el dominio tenga extensión
        var dominioPartes = partes[1].split('.');
        if (dominioPartes.length < 2 || !dominioPartes[dominioPartes.length - 1]) {
            errorElement.style.display = 'block';
            errorElement.textContent = 'El dominio del correo debe tener una extensión válida';
            return false;
        }
        
        correoInput.value = valor; // Normalizar a minúsculas
        errorElement.style.display = 'none';
        return true;
    }

    // ========== VALIDACIONES DE CONTENIDO SEGURO ==========
    function validarContenidoSeguro(elemento, tipo) {
        var valor = elemento.value;
        var errorElement = document.getElementById('error' + tipo.charAt(0).toUpperCase() + tipo.slice(1));
        var maxCaracteres = tipo === 'observaciones' ? 500 : 3000;
        
        // Validar longitud
        if (valor.length > maxCaracteres) {
            errorElement.style.display = 'block';
            errorElement.textContent = 'El texto excede el límite de ' + maxCaracteres + ' caracteres';
            elemento.value = valor.substring(0, maxCaracteres);
            return false;
        }
        
        // Detectar y eliminar contenido peligroso
        var patronesPeligrosos = [
            /<script\b[^<]*(?:(?!<\/script>)<[^<]*)*<\/script>/gi,
            /javascript:/gi,
            /on\w+\s*=/gi,
            /<\w+[^>]*>/gi,
            /data:/gi,
            /vbscript:/gi,
            /expression\s*\(/gi,
            /url\s*\(/gi
        ];
        
        var contenidoLimpio = valor;
        var contenidoPeligrosoEncontrado = false;
        
        for (var i = 0; i < patronesPeligrosos.length; i++) {
            if (patronesPeligrosos[i].test(valor)) {
                contenidoPeligrosoEncontrado = true;
                contenidoLimpio = contenidoLimpio.replace(patronesPeligrosos[i], '');
            }
        }
        
        if (contenidoPeligrosoEncontrado) {
            errorElement.style.display = 'block';
            errorElement.textContent = 'Se ha detectado y eliminado contenido no permitido';
            elemento.value = contenidoLimpio;
            return false;
        }
        
        errorElement.style.display = 'none';
        return true;
    }

    // ========== VALIDACIÓN COMPLETA DEL FORMULARIO ==========
    function validarFormularioCompleto() {
        var validaciones = [
            validarNombreCompletoCompleto(),
            validarEdad(),
            validarTelefonoCompleto(),
            validarCorreoCompleto(),
            validarContenidoSeguro(document.getElementById('<%= txtObservaciones.ClientID %>'), 'observaciones'),
            validarContenidoSeguro(document.getElementById('<%= txtDetalleMedico.ClientID %>'), 'historial') // CORREGIDO: txtDetalleMedico
        ];

        var esValido = validaciones.every(function(validacion) {
            return validacion === true;
        });

        if (esValido) {
            // Deshabilitar botón para prevenir doble envío
            var btnGuardar = document.getElementById('<%= btnGuardarPaciente.ClientID %>');
            if (btnGuardar) {
                btnGuardar.disabled = true;
                btnGuardar.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Guardando...';
            }
            return true;
        } else {
            // Enfocar el primer campo con error
            var primerosErrores = document.querySelectorAll('.text-danger[style*="display: block"]');
            if (primerosErrores.length > 0) {
                var campoId = primerosErrores[0].id.replace('error', 'txt');
                var campo = document.getElementById(campoId);
                if (campo) {
                    campo.focus();
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
            setTimeout(function() {
                errorElement.style.display = 'none';
            }, 3000);
        }
    }

    function confirmarEliminacion(boton) {
        var fila = boton.closest('tr');
        var nombrePaciente = fila.cells[1].textContent;
        return confirm('¿Está seguro de que desea eliminar al paciente: ' + nombrePaciente + '?\nEsta acción no se puede deshacer.');
    }

    // Contadores de caracteres
    document.addEventListener('DOMContentLoaded', function() {
        var txtObservaciones = document.getElementById('<%= txtObservaciones.ClientID %>');
        var txtDetalleMedico = document.getElementById('<%= txtDetalleMedico.ClientID %>'); // CORREGIDO: txtDetalleMedico
        var contadorObservaciones = document.getElementById('contadorObservaciones');
        var contadorHistorial = document.getElementById('contadorHistorial');

        function actualizarContador(elemento, contador, maximo) {
            if (elemento && contador) {
                contador.textContent = elemento.value.length;
                if (elemento.value.length > maximo * 0.9) {
                    contador.className = 'text-warning';
                } else if (elemento.value.length > maximo) {
                    contador.className = 'text-danger';
                } else {
                    contador.className = 'text-muted';
                }
            }
        }

        if (txtObservaciones && contadorObservaciones) {
            actualizarContador(txtObservaciones, contadorObservaciones, 500);
            txtObservaciones.addEventListener('input', function() {
                actualizarContador(this, contadorObservaciones, 500);
            });
        }

        if (txtDetalleMedico && contadorHistorial) {
            actualizarContador(txtDetalleMedico, contadorHistorial, 3000);
            txtDetalleMedico.addEventListener('input', function() {
                actualizarContador(this, contadorHistorial, 3000);
            });
        }
    });

    // Restaurar estado del botón si hay error de validación
    window.restaurarBotonGuardar = function() {
        var btnGuardar = document.getElementById('<%= btnGuardarPaciente.ClientID %>');
           if (btnGuardar) {
               btnGuardar.disabled = false;
               btnGuardar.innerHTML = 'Guardar Paciente';
           }
       };
   </script>
</asp:Content>