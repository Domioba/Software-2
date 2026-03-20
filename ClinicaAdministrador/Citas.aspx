<%@ Page Title="Gestión de Citas" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Citas.aspx.cs" Inherits="ClinicaAdministrador.Citas" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link href="css/estiloCitas.css" rel="stylesheet" type="text/css" />
</asp:Content>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <!-- NUEVO: Contenedor para las notificaciones tipo "toast" -->
    <div id="notification-container"></div>

    <div class="d-flex justify-content-between align-items-center mb-4">
        <h2 class="fw-bold">Gestión de Citas</h2>
        <asp:Button ID="btnNuevaCita" runat="server" Text="Nueva Cita" CssClass="btn-add-new" OnClick="btnNuevaCita_Click" />
    </div>

    <!-- PANEL PARA EL FORMULARIO DE CREAR/EDITAR CITA -->
    <asp:Panel ID="pnlFormularioCita" runat="server" CssClass="card card-custom mb-4 shadow" Visible="false">
        <div class="card-header">
            <h4 id="tituloFormularioCita" runat="server" class="mb-0">Nueva Cita</h4>
        </div>
        <div class="card-body">
            <asp:HiddenField ID="hfIDCita" runat="server" />
            
            <div class="alert alert-warning">
                <strong>⚠️ IMPORTANTE:</strong> Por seguridad del paciente, algunos servicios no pueden combinarse el mismo día. 
                Límite máximo: <strong>4 servicios por cita</strong>. Las combinaciones no permitidas se bloquearán automáticamente.
            </div>
            
            <div class="row">
                <div class="col-md-12 mb-3">
                    <label for="ddlPaciente" class="form-label fw-bold">Paciente</label>
                    <asp:DropDownList ID="ddlPaciente" runat="server" CssClass="form-select" required></asp:DropDownList>
                    <div class="form-text">Seleccione el paciente para la cita</div>
                </div>
            </div>

            <div class="row">
                <div class="col-md-12 mb-3">
                    <label class="form-label fw-bold">Seleccionar Servicios</label>
                    <div class="alert alert-info">
                        <small>💡 <strong>Tip:</strong> Los servicios están agrupados por tipo. Algunas combinaciones no están permitidas por seguridad.</small>
                    </div>
                    <asp:CheckBoxList ID="cblServicios" runat="server" CssClass="service-checkbox-list" RepeatLayout="UnorderedList"></asp:CheckBoxList>
                    <div class="form-text">Seleccione los servicios para la cita (máximo 4 servicios)</div>
                </div>
            </div>

            <div class="row">
                <div class="col-md-6 mb-3">
                    <label for="lblTotalPrecio" class="form-label fw-bold">Total Estimado:</label>
                    <asp:Label ID="lblTotalPrecio" runat="server" Text="$0.00" CssClass="form-control-plaintext fs-4" style="color: var(--verde-whatsapp); font-weight: 700;"></asp:Label>
                </div>
                <div class="col-md-6 mb-3">
                    <label for="lblDuracionTotal" class="form-label fw-bold">Duración Total:</label>
                    <asp:Label ID="lblDuracionTotal" runat="server" Text="0 minutos" CssClass="form-control-plaintext fs-4" style="color: var(--azul-primario); font-weight: 700;"></asp:Label>
                </div>
            </div>

            <div id="advertenciaLimites" class="alert alert-warning mt-2" style="display: none;"></div>

            <div class="row">
                <div class="col-md-4 mb-3">
                    <label for="txtFecha" class="form-label fw-bold">Fecha</label>
                    <asp:TextBox ID="txtFecha" runat="server" TextMode="Date" CssClass="form-control" required></asp:TextBox>
                    <div class="form-text">La fecha debe ser hoy o posterior y no mayor a un año</div>
                </div>
                <div class="col-md-4 mb-3">
                    <label for="txtHora" class="form-label fw-bold">Hora</label>
                    <asp:TextBox ID="txtHora" runat="server" TextMode="Time" CssClass="form-control" required></asp:TextBox>
                    <div class="form-text">Horario de atención: 7:00 AM - 8:00 PM</div>
                </div>
                <div class="col-md-4 mb-3">
                    <label for="ddlEstado" class="form-label fw-bold">Estado</label>
                    <asp:DropDownList ID="ddlEstado" runat="server" CssClass="form-select">
                        <asp:ListItem Text="Pendiente" Value="Pendiente" />
                        <asp:ListItem Text="Confirmada" Value="Confirmada" />
                        <asp:ListItem Text="Cancelada" Value="Cancelada" />
                        <asp:ListItem Text="Realizada" Value="Realizada" />
                    </asp:DropDownList>
                    <div class="form-text">Estado inicial de la cita</div>
                </div>
            </div>

            <div class="mb-4">
                <label for="txtNotas" class="form-label fw-bold">Notas</label>
                <asp:TextBox ID="txtNotas" runat="server" TextMode="MultiLine" Rows="3" CssClass="form-control"></asp:TextBox>
                <div class="form-text">Información adicional sobre la cita (opcional)</div>
            </div>
            
            <div class="d-flex gap-2">
                <asp:Button ID="btnGuardarCita" runat="server" Text="Guardar Cita" CssClass="btn btn-success-custom" OnClick="btnGuardarCita_Click" OnClientClick="return validarCita();" />
                <asp:Button ID="btnCancelarCita" runat="server" Text="Cancelar" CssClass="btn btn-secondary-custom" OnClick="btnCancelarCita_Click" UseSubmitBehavior="false" />
            </div>
        </div>
    </asp:Panel>

    <!-- LABEL DE ERROR CON ESTILO PERSONALIZADO -->
    <asp:Label ID="lblMensajeError" runat="server" Visible="False" CssClass="alert-custom alert-error d-block mb-3" role="alert"></asp:Label>
    <asp:Label ID="lblMensajeExito" runat="server" Visible="False" CssClass="alert-custom alert-success d-block mb-3" role="alert"></asp:Label>

    <!-- PANEL DE FILTROS -->
    <div class="card card-custom mb-4">
        <div class="card-body">
            <h5 class="card-title mb-3">Filtrar Citas</h5>
            <div class="row g-3">
                <div class="col-md-4">
                    <label for="txtFiltroFecha" class="form-label">Fecha</label>
                    <asp:TextBox ID="txtFiltroFecha" runat="server" TextMode="Date" CssClass="form-control"></asp:TextBox>
                </div>
                <div class="col-md-4">
                    <label for="ddlFiltroEstado" class="form-label">Estado</label>
                    <asp:DropDownList ID="ddlFiltroEstado" runat="server" CssClass="form-select">
                        <asp:ListItem Text="Todos los estados" Value="" />
                        <asp:ListItem Text="Pendiente" Value="Pendiente" />
                        <asp:ListItem Text="Confirmada" Value="Confirmada" />
                        <asp:ListItem Text="Cancelada" Value="Cancelada" />
                        <asp:ListItem Text="Realizada" Value="Realizada" />
                    </asp:DropDownList>
                </div>
                <div class="col-md-4 d-flex align-items-end">
                    <asp:Button ID="btnFiltrar" runat="server" Text="Aplicar Filtros" CssClass="btn btn-primary-custom w-100" OnClick="btnFiltrar_Click" />
                </div>
            </div>
        </div>
    </div>

    <!-- GRIDVIEW PARA MOSTRAR LA LISTA DE CITAS -->
    <div class="table-responsive-custom">
        <asp:GridView ID="gvCitas" runat="server" CssClass="table table-custom" AutoGenerateColumns="False" DataKeyNames="IDCita" OnRowCommand="gvCitas_RowCommand">
            <Columns>
                <asp:BoundField DataField="IDCita" HeaderText="ID" ReadOnly="True" />
                <asp:BoundField DataField="NombrePaciente" HeaderText="Paciente" />
                <asp:BoundField DataField="Fecha" HeaderText="Fecha" DataFormatString="{0:dd/MM/yyyy}" />
                <asp:BoundField DataField="Hora" HeaderText="Hora" />
                <asp:BoundField DataField="DuracionTotal" HeaderText="Duración (min)" />
                <asp:BoundField DataField="Servicios" HeaderText="Servicios" />  
                <asp:BoundField DataField="Total" HeaderText="Total" DataFormatString="{0:C}" />
                <asp:BoundField DataField="Estado" HeaderText="Estado" />
                <asp:TemplateField HeaderText="Acciones">
                    <ItemTemplate>
                        <div class="table-actions">
                            <asp:LinkButton ID="btnEditar" runat="server" CommandName="Editar" CommandArgument='<%# Eval("IDCita") %>' CssClass="btn-action btn-action-edit">
                                <i class="bi bi-pencil"></i> Editar
                            </asp:LinkButton>
                            
                            <!-- NUEVO: Botón para crear un tratamiento directamente desde la cita realizada -->
                            <asp:LinkButton ID="btnRegistrarTratamiento" runat="server" CommandName="RegistrarTratamiento" CommandArgument='<%# Eval("IDCita") %>' CssClass="btn-action btn-action-tratamiento" Visible='<%# Eval("Estado").ToString() == "Realizada" %>'>
                                <i class="bi bi-clipboard-plus"></i> Registrar Tratamiento
                            </asp:LinkButton>

                            <asp:LinkButton ID="btnMarcarRealizada" runat="server" CommandName="MarcarRealizada" CommandArgument='<%# Eval("IDCita") %>' CssClass="btn-action btn-action-complete" Visible='<%# Eval("Estado").ToString() == "Confirmada" && DateTime.Parse(Eval("Fecha").ToString()).Date <= DateTime.Today.Date %>'>
                                <i class="bi bi-check-circle"></i> Realizada
                            </asp:LinkButton>
                            
                            <asp:LinkButton ID="btnEliminar" runat="server" CommandName="Eliminar" CommandArgument='<%# Eval("IDCita") %>' CssClass="btn-action btn-action-delete" OnClientClick="return confirm('¿Estás seguro de eliminar este registro?');">
                                <i class="bi bi-trash"></i> Eliminar
                            </asp:LinkButton>
                        </div>
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
            <EmptyDataTemplate>
                <div class="alert-custom alert-info">
                    No hay citas registradas que coincidan con los filtros.
                </div>
            </EmptyDataTemplate>
        </asp:GridView>
    </div>
   
    <!-- JavaScript para validaciones en el cliente -->
    <script type="text/javascript">
        // NUEVO: Función para mostrar notificaciones amigables (toast)
        function mostrarNotificacion(mensaje, tipo) {
            const container = document.getElementById('notification-container');
            const notification = document.createElement('div');
            notification.className = `notification-item ${tipo}`;

            const iconMap = {
                success: 'bi-check-circle-fill',
                error: 'bi-x-circle-fill',
                warning: 'bi-exclamation-triangle-fill',
                info: 'bi-info-circle-fill'
            };

            notification.innerHTML = `
                <div class="icon">
                    <i class="bi ${iconMap[tipo] || 'bi-info-circle-fill'}"></i>
                </div>
                <div class="message">${mensaje}</div>
            `;

            container.appendChild(notification);

            // Animación de entrada
            setTimeout(() => {
                notification.classList.add('show');
            }, 100);

            // Animación de salida y eliminación
            setTimeout(() => {
                notification.classList.remove('show');
                setTimeout(() => {
                    container.removeChild(notification);
                }, 400); // Coincide con la duración de la transición CSS
            }, 5000); // La notificación dura 5 segundos
        }

        function pageLoad() {
            var cblServicios = document.getElementById('<%= cblServicios.ClientID %>');
            if (cblServicios) {
                var checkboxes = cblServicios.getElementsByTagName('input');
                for (var i = 0; i < checkboxes.length; i++) {
                    checkboxes[i].onclick = calcularTotalYDuracion;
                }
            }

            // Establecer fecha mínima como HOY
            var txtFecha = document.getElementById('<%= txtFecha.ClientID %>');
            if (txtFecha) {
                var today = new Date();
                var minDate = today.toISOString().split('T')[0];
                txtFecha.setAttribute('min', minDate);

                // Establecer fecha máxima como un año desde hoy
                var maxDate = new Date(today);
                maxDate.setFullYear(today.getFullYear() + 1);
                var maxDateStr = maxDate.toISOString().split('T')[0];
                txtFecha.setAttribute('max', maxDateStr);
            }

            // Calcular total y duración inicial
            calcularTotalYDuracion();
        }

        function calcularTotalYDuracion() {
            var total = 0;
            var duracionTotal = 0;
            var serviciosSeleccionados = 0;
            var checkboxes = document.getElementById('<%= cblServicios.ClientID %>').getElementsByTagName('input');
            var labels = document.getElementById('<%= cblServicios.ClientID %>').getElementsByTagName('label');

            for (var i = 0; i < checkboxes.length; i++) {
                if (checkboxes[i].checked) {
                    serviciosSeleccionados++;
                    var text = labels[i].innerText;
                    var precioStr = text.split('$')[1];
                    var precio = parseFloat(precioStr);
                    if (!isNaN(precio)) {
                        total += precio;
                    }
                    
                    // Extraer duración del texto (asumiendo formato: "Nombre Servicio - $XX.XX (X min)")
                    var duracionMatch = text.match(/\((\d+)\s*min\)/);
                    if (duracionMatch) {
                        duracionTotal += parseInt(duracionMatch[1]);
                    }
                }
            }
            
            document.getElementById('<%= lblTotalPrecio.ClientID %>').innerText = '$' + total.toFixed(2);
            document.getElementById('<%= lblDuracionTotal.ClientID %>').innerText = duracionTotal + ' minutos';
            
            // Mostrar advertencias de límites
            mostrarAdvertenciasLimites(serviciosSeleccionados, duracionTotal);
        }

        function mostrarAdvertenciasLimites(serviciosSeleccionados, duracionTotal) {
            var advertenciaDiv = document.getElementById('advertenciaLimites');
            if (!advertenciaDiv) {
                advertenciaDiv = document.createElement('div');
                advertenciaDiv.id = 'advertenciaLimites';
                advertenciaDiv.className = 'alert alert-warning mt-2';
                document.getElementById('<%= lblDuracionTotal.ClientID %>').parentNode.parentNode.appendChild(advertenciaDiv);
            }
            
            var mensajes = [];
            
            // Límite de servicios
            if (serviciosSeleccionados > 4) {
                mensajes.push(' MÁXIMO 4 SERVICIOS: Ha seleccionado ' + serviciosSeleccionados + ' servicios');
            } else if (serviciosSeleccionados === 4) {
                mensajes.push(' Límite máximo de servicios alcanzado');
            }
            
            // Límite de tiempo
            if (duracionTotal > 180) {
                mensajes.push(' TIEMPO EXCEDIDO: Duración total de ' + duracionTotal + ' minutos (máximo 180 min)');
            } else if (duracionTotal > 120) {
                mensajes.push(' Cita de larga duración - Verificar disponibilidad');
            }
            
            if (mensajes.length > 0) {
                advertenciaDiv.innerHTML = '<strong>Validación:</strong> ' + mensajes.join(' | ');
                advertenciaDiv.style.display = 'block';
            } else {
                advertenciaDiv.style.display = 'none';
            }
        }

        // Validación de fecha y hora antes de enviar el formulario
        function validarCita() {
            var fecha = document.getElementById('<%= txtFecha.ClientID %>').value;
            var hora = document.getElementById('<%= txtHora.ClientID %>').value;
            var paciente = document.getElementById('<%= ddlPaciente.ClientID %>').value;
            var servicios = document.querySelectorAll('#<%= cblServicios.ClientID %> input[type="checkbox"]:checked').length;
            var duracionTotal = parseInt(document.getElementById('<%= lblDuracionTotal.ClientID %>').innerText) || 0;

            // Validar que se haya seleccionado un paciente
            if (paciente === "0") {
                mostrarNotificacion('Debe seleccionar un paciente para la cita.', 'warning');
                return false;
            }

            // Validar que se haya seleccionado al menos un servicio
            if (servicios === 0) {
                mostrarNotificacion('Debe seleccionar al menos un servicio para la cita.', 'warning');
                return false;
            }

            // *** NUEVA VALIDACIÓN: Límite de servicios por cita ***
            if (servicios > 4) {
                mostrarNotificacion('No se pueden seleccionar más de 4 servicios por cita por seguridad del paciente.', 'error');
                return false;
            }

            // Validar que la fecha no sea vacía
            if (!fecha) {
                mostrarNotificacion('Debe seleccionar una fecha para la cita.', 'warning');
                return false;
            }

            // Validar que la fecha no sea en el pasado
            var selectedDate = new Date(fecha + "T00:00:00");
            var today = new Date();
            today.setHours(0, 0, 0, 0);

            if (selectedDate < today) {
                mostrarNotificacion('La fecha de la cita no puede ser en el pasado.', 'error');
                return false;
            }

            // Validar que la fecha no sea mayor a un año
            var maxDate = new Date();
            maxDate.setFullYear(today.getFullYear() + 1);
            if (selectedDate > maxDate) {
                mostrarNotificacion('La fecha de la cita no puede ser mayor a un año desde hoy.', 'error');
                return false;
            }

            // Validar que la hora no sea vacía
            if (!hora) {
                mostrarNotificacion('Debe seleccionar una hora para la cita.', 'warning');
                return false;
            }

            // Validar que la hora esté dentro del horario de atención (7:00 AM - 8:00 PM)
            var [hours, minutes] = hora.split(':').map(Number);
            var horaFin = new Date(selectedDate);
            horaFin.setHours(hours, minutes + duracionTotal, 0, 0);

            if (hours < 7 || hours > 20 || (hours === 20 && minutes > 0)) {
                mostrarNotificacion('La hora de la cita debe estar dentro del horario de atención (7:00 AM - 8:00 PM).', 'error');
                return false;
            }

            // Validar que la cita no exceda el horario de atención
            var horaCierre = new Date(selectedDate);
            horaCierre.setHours(20, 0, 0, 0); // 8:00 PM

            if (horaFin > horaCierre) {
                mostrarNotificacion('La cita excede el horario de atención. Duración total: ' + duracionTotal + ' minutos. Hora de fin estimada: ' + horaFin.getHours() + ':' + (horaFin.getMinutes() < 10 ? '0' : '') + horaFin.getMinutes(), 'error');
                return false;
            }

            return true;
        }
    </script>
</asp:Content>