<%@ Page Title="Gestión de Facturación" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Facturacion.aspx.cs" Inherits="ClinicaAdministrador.Facturacion" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link href="css/estiloFacturacion.css" rel="stylesheet" type="text/css" />
</asp:Content>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    
    <div class="d-flex justify-content-between align-items-center mb-4">
        <h2 class="fw-bold">Gestión de Facturación</h2>
        <asp:Button ID="btnNuevaFactura" runat="server" Text="Nueva Factura" CssClass="btn-add-new" OnClick="btnNuevaFactura_Click" />
    </div>

    <!-- PANEL PARA EL FORMULARIO DE CREAR/EDITAR FACTURA -->
    <asp:Panel ID="pnlFormularioFactura" runat="server" CssClass="card card-custom mb-4 shadow" Visible="false">
        <div class="card-header">
            <h4 id="tituloFormularioFactura" runat="server" class="mb-0">Nueva Factura</h4>
        </div>
        <div class="card-body">
            <asp:HiddenField ID="hfIDFactura" runat="server" />
            
            <!-- CAMPO OCULTO CLAVE PARA GUARDAR EL ID DEL PACIENTE -->
            <asp:HiddenField ID="hfIDPaciente" runat="server" />
            
            <!-- NUEVO PASO 1: SELECCIONAR CITA -->
            <div class="row">
                <div class="col-md-12 mb-3">
                    <label for="ddlCita" class="form-label fw-bold">1. Seleccionar Cita para Facturar</label>
                    <asp:DropDownList ID="ddlCita" runat="server" CssClass="form-select" onchange="cargarDatosDesdeCita();"></asp:DropDownList>
                    <small class="form-text text-muted">Al seleccionar una cita, los datos del paciente y los servicios se cargarán automáticamente.</small>
                </div>
            </div>

            <!-- PASO 2: DATOS DEL PACIENTE (SE CARGARÁN AUTOMÁTICAMENTE) -->
            <div class="row">
                <div class="col-md-6 mb-3">
                    <label for="ddlPaciente" class="form-label fw-bold">2. Paciente Asociado</label>
                    <asp:DropDownList ID="ddlPaciente" runat="server" CssClass="form-select" disabled="true"></asp:DropDownList>
                </div>
                <div class="col-md-6 mb-3">
                    <label for="txtFechaFactura" class="form-label fw-bold">Fecha de Factura</label>
                    <asp:TextBox ID="txtFechaFactura" runat="server" TextMode="Date" CssClass="form-control" required></asp:TextBox>
                </div>
            </div>

            <!-- PASO 3: SERVICIOS (SE CARGARÁN AUTOMÁTICAMENTE) -->
            <div class="row">
                <div class="col-md-12 mb-3">
                    <label class="form-label fw-bold">3. Servicios de la Cita</label>
                    <ul id="cblServicios" class="service-checkbox-list"></ul>
                </div>
            </div>

            <div class="row">
                <div class="col-md-6 mb-3">
                    <label for="lblTotalPrecio" class="form-label fw-bold">Total a Pagar:</label>
                    <asp:Label ID="lblTotalPrecio" runat="server" Text="$0.00" CssClass="form-control-plaintext fs-4" style="color: var(--verde-whatsapp); font-weight: 700;"></asp:Label>
                </div>
                <div class="col-md-6 mb-3">
                    <label for="ddlMetodoPago" class="form-label fw-bold">Método de Pago</label>
                    <asp:DropDownList ID="ddlMetodoPago" runat="server" CssClass="form-select">
                        <asp:ListItem Text="-- Seleccione --" Value="0" />
                        <asp:ListItem Text="Efectivo" Value="Efectivo" />
                        <asp:ListItem Text="Tarjeta de Débito" Value="Tarjeta de Débito" />
                        <asp:ListItem Text="Tarjeta de Crédito" Value="Tarjeta de Crédito" />
                        <asp:ListItem Text="Transferencia Bancaria" Value="Transferencia" />
                    </asp:DropDownList>
                </div>
            </div>

            <div class="row">
                <div class="col-md-6 mb-3">
                    <label for="ddlEstadoPago" class="form-label fw-bold">Estado de Pago</label>
                    <asp:DropDownList ID="ddlEstadoPago" runat="server" CssClass="form-select">
                        <asp:ListItem Text="-- Seleccione --" Value="0" />
                        <asp:ListItem Text="Pagado" Value="Pagado" />
                        <asp:ListItem Text="Pendiente" Value="Pendiente" />
                    </asp:DropDownList>
                </div>
            </div>
            
            <div class="d-flex gap-2">
                <asp:Button ID="btnGuardarFactura" runat="server" Text="Guardar Factura" CssClass="btn btn-success-custom" OnClick="btnGuardarFactura_Click" />
                <asp:Button ID="btnCancelarFactura" runat="server" Text="Cancelar" CssClass="btn btn-secondary-custom" OnClick="btnCancelarFactura_Click" />
            </div>
        </div>
    </asp:Panel>

    <!-- LABEL DE ERROR CON ESTILO PERSONALIZADO -->
    <asp:Label ID="lblMensajeError" runat="server" Visible="False" CssClass="alert-custom alert-error d-block mb-3" role="alert"></asp:Label>

    <!-- GRIDVIEW PARA MOSTRAR LA LISTA DE FACTURAS -->
    <div class="table-responsive-custom">
        <asp:GridView ID="gvFacturas" runat="server" CssClass="table table-custom" AutoGenerateColumns="False" DataKeyNames="IDFactura" OnRowCommand="gvFacturas_RowCommand">
            <Columns>
                <asp:BoundField DataField="IDFactura" HeaderText="ID Factura" ReadOnly="True" />
                <asp:BoundField DataField="NombrePaciente" HeaderText="Paciente" />
                <asp:BoundField DataField="Fecha" HeaderText="Fecha" DataFormatString="{0:dd/MM/yyyy}" />
                <asp:BoundField DataField="Servicio" HeaderText="Servicios" />
                <asp:BoundField DataField="Total" HeaderText="Total" DataFormatString="{0:C}" />
                <asp:BoundField DataField="MetodoPago" HeaderText="Método de Pago" />
                <asp:TemplateField HeaderText="Estado Pago">
                    <ItemTemplate>
                        <span class="badge-payment-status <%# Eval("EstadoPago").ToString() == "Pagado" ? "pagado" : "pendiente" %>">
                            <%# Eval("EstadoPago") %>
                        </span>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Acciones">
                    <ItemTemplate>
                        <div class="table-actions">
                            <asp:LinkButton ID="btnVer" runat="server" CommandName="VerFactura" CommandArgument='<%# Eval("IDFactura") %>' CssClass="btn-action btn-action-view">
                                <i class="bi bi-eye"></i> Ver
                            </asp:LinkButton>
                            <asp:LinkButton ID="btnEliminar" runat="server" CommandName="Eliminar" CommandArgument='<%# Eval("IDFactura") %>' CssClass="btn-action btn-action-delete" OnClientClick="return confirm('¿Estás seguro de eliminar este registro?');">
                                <i class="bi bi-trash"></i> Eliminar
                            </asp:LinkButton>
                        </div>
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
            <EmptyDataTemplate>
                <div class="alert-custom alert-info">
                    No hay facturas registradas.
                </div>
            </EmptyDataTemplate>
        </asp:GridView>
    </div>
   
    <!-- BLOQUE DE JAVASCRIPT FINAL -->
    <script type="text/javascript">
        function pageLoad() {
            var checkboxes = document.querySelectorAll('#cblServicios input[type="checkbox"]');
            for (var i = 0; i < checkboxes.length; i++) {
                checkboxes[i].onclick = calcularTotal;
            }
        }

        function cargarDatosDesdeCita() {
            var ddlCita = document.getElementById('<%= ddlCita.ClientID %>');
            var idCita = ddlCita.value;

            limpiarFormulario();

            if (idCita === "0") {
                return;
            }

            PageMethods.ObtenerDatosPorCita(idCita, onDatosCitaRecibidos, onError);
        }

        function onDatosCitaRecibidos(resultados) {
            if (typeof resultados === 'string') {
                resultados = JSON.parse(resultados);
            }

            if (resultados.error) {
                alert('Error del servidor: ' + resultados.error);
                return;
            }

            if (!resultados.paciente) {
                alert('Error: No se encontraron datos del paciente para esta cita.');
                limpiarFormulario();
                return;
            }

            var ddlPaciente = document.getElementById('<%= ddlPaciente.ClientID %>');
            ddlPaciente.innerHTML = '';
            var option = new Option(resultados.paciente.nombre, resultados.paciente.id);
            ddlPaciente.add(option);

            document.getElementById('<%= hfIDPaciente.ClientID %>').value = resultados.paciente.id;

            var txtFecha = document.getElementById('<%= txtFechaFactura.ClientID %>');
            txtFecha.value = resultados.paciente.fechaCita;

            var contenedorServicios = document.getElementById('cblServicios');
            contenedorServicios.innerHTML = '';

            if (resultados.servicios.length === 0) {
                contenedorServicios.innerHTML = '<li class="text-muted">Esta cita no tiene servicios asociados. No se puede facturar.</li>';
                return;
            }

            for (var i = 0; i < resultados.servicios.length; i++) {
                var servicio = resultados.servicios[i];
                
                var li = document.createElement('li');
                
                var input = document.createElement('input');
                input.type = 'checkbox';
                input.id = 'servicio_' + servicio.id;
                input.name = 'cblServicios';
                input.value = servicio.precio.replace(',', '.');
                input.checked = true;
                input.onclick = calcularTotal;

                var label = document.createElement('label');
                label.htmlFor = 'servicio_' + servicio.id;
                label.appendChild(document.createTextNode(servicio.nombre + ' - $' + servicio.precio));
                
                li.appendChild(input);
                li.appendChild(label);
                contenedorServicios.appendChild(li);
            }

            calcularTotal();
        }

        function limpiarFormulario() {
            document.getElementById('<%= ddlPaciente.ClientID %>').innerHTML = '';
            document.getElementById('<%= txtFechaFactura.ClientID %>').value = '';
            document.getElementById('cblServicios').innerHTML = '';
            document.getElementById('<%= lblTotalPrecio.ClientID %>').innerText = '$0.00';
        }

        function onError(error) {
            alert('Error al cargar los datos de la cita: ' + error.get_message());
        }

        function calcularTotal() {
            var total = 0;
            var checkboxes = document.querySelectorAll('#cblServicios input[type="checkbox"]:checked');
            
            for (var i = 0; i < checkboxes.length; i++) {
                var precio = parseFloat(checkboxes[i].value);
                if (!isNaN(precio)) {
                    total += precio;
                }
            }
            
            document.getElementById('<%= lblTotalPrecio.ClientID %>').innerText = '$' + total.toFixed(2).replace('.', ',');
        }
    </script>
</asp:Content>