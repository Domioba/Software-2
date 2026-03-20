<%@ Page Title="Gestión de Tratamientos" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Tratamientos.aspx.cs" Inherits="ClinicaAdministrador.Tratamientos" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link href="css/estiloTratamientos.css" rel="stylesheet" type="text/css" />
</asp:Content>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    
    <div class="d-flex justify-content-between align-items-center mb-4">
        <h2 class="fw-bold">Gestión de Tratamientos</h2>
        <asp:Button ID="btnNuevoTratamiento" runat="server" Text="Registrar Tratamiento Realizado" CssClass="btn-add-new" OnClick="btnNuevoTratamiento_Click" />
    </div>

    <!-- PANEL PARA EL FORMULARIO DE REGISTRO/EDICIÓN DE TRATAMIENTO -->
    <asp:Panel ID="pnlFormularioTratamiento" runat="server" CssClass="card card-custom mb-4 shadow" Visible="false">
        <div class="card-header">
            <h4 id="tituloFormularioTratamiento" runat="server" class="mb-0">Registrar Nuevo Tratamiento</h4>
        </div>
        <div class="card-body">
            
            <!-- CAMPOS OCULTOS PARA GUARDAR IDS -->
            <asp:HiddenField ID="hfIDTratamiento" runat="server" />
            <asp:HiddenField ID="hfIDPaciente" runat="server" />

            <!-- PASO 1: SELECCIONAR CITA -->
            <div class="row">
                <div class="col-md-12 mb-3">
                    <label for="ddlCita" class="form-label fw-bold">1. Seleccionar Cita Realizada</label>
                    <asp:DropDownList ID="ddlCita" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlCita_SelectedIndexChanged"></asp:DropDownList>
                    <small class="form-text text-muted">Al seleccionar una cita, los datos del paciente, la fecha y los servicios se cargarán automáticamente.</small>
                </div>
            </div>

            <!-- PASO 2: DATOS DEL PACIENTE Y FECHA (SE CARGARÁN AUTOMÁTICAMENTE) -->
            <div class="row">
                <div class="col-md-6 mb-3">
                    <label for="ddlPaciente" class="form-label fw-bold">2. Paciente Asociado</label>
                    <asp:DropDownList ID="ddlPaciente" runat="server" CssClass="form-select" disabled="true"></asp:DropDownList>
                </div>
                <div class="col-md-6 mb-3">
                    <label for="txtFechaTratamiento" class="form-label fw-bold">Fecha del Tratamiento</label>
                    <asp:TextBox ID="txtFechaTratamiento" runat="server" TextMode="Date" CssClass="form-control" ReadOnly="true" BackColor="#f8f9fa"></asp:TextBox>
                    <small class="form-text text-muted">La fecha se establece automáticamente según la cita seleccionada.</small>
                </div>
            </div>

            <!-- PASO 3: LISTA DE SERVICIOS Y PRODUCTOS FIJOS -->
            <div class="row">
                <div class="col-12 mb-3">
                    <label class="form-label fw-bold">3. Productos Utilizados por Servicio</label>
                    <small class="form-text text-muted">El sistema muestra el producto asignado a cada servicio. Solo ingrese la cantidad utilizada (mínimo: 1, máximo: inventario disponible).</small>
                </div>
            </div>

            <asp:Repeater ID="rptServiciosProductos" runat="server" OnItemDataBound="rptServiciosProductos_ItemDataBound">
                <ItemTemplate>
                    <div class="row align-items-end mb-3 servicio-detalle">
                        <asp:HiddenField ID="hfIDServicio" runat="server" Value='<%# Eval("IDServicio") %>' />
                        <asp:HiddenField ID="hfIDProducto" runat="server" Value='<%# Eval("IDProducto") %>' />
                        <asp:HiddenField ID="hfInventarioDisponible" runat="server" Value='<%# Eval("InventarioDisponible") %>' />
                        
                        <div class="col-md-3">
                            <label class="form-label">Servicio</label>
                            <asp:TextBox ID="txtNombreServicio" runat="server" Text='<%# Eval("NombreServicio") %>' CssClass="form-control" ReadOnly="true" />
                        </div>
                        
                        <div class="col-md-3">
                            <label class="form-label">Producto Asignado</label>
                            <asp:TextBox ID="txtNombreProducto" runat="server" Text='<%# Eval("NombreProducto") %>' CssClass="form-control" ReadOnly="true" />
                        </div>

                        <div class="col-md-2">
                            <label class="form-label">Disponible</label>
                            <asp:TextBox ID="txtInventarioDisponible" runat="server" Text='<%# Eval("InventarioDisponible") %>' CssClass="form-control" ReadOnly="true" BackColor="#e8f5e8" />
                        </div>
                        
                        <div class="col-md-4">
                            <label class="form-label">Cantidad Utilizada <span class="text-danger">*</span></label>
                            <asp:TextBox ID="txtCantidad" runat="server" TextMode="Number" step="0.01" min="1" 
                                CssClass="form-control cantidad-utilizada" placeholder="Ej: 1.5" 
                                onchange="validarCantidad(this)" required></asp:TextBox>
                            <small class="form-text text-muted cantidad-mensaje" style="display: none; color: #dc3545;"></small>
                        </div>
                    </div>
                </ItemTemplate>
            </asp:Repeater>

            <div class="mb-3">
                <label for="txtObservaciones" class="form-label fw-bold">Observaciones Generales del Tratamiento</label>
                <asp:TextBox ID="txtObservaciones" runat="server" TextMode="MultiLine" Rows="3" CssClass="form-control"></asp:TextBox>
            </div>
            
            <div class="d-flex gap-2">
                <asp:Button ID="btnGuardarTratamiento" runat="server" Text="Guardar Tratamiento y Actualizar Inventario" 
                    CssClass="btn btn-success-custom" OnClick="btnGuardarTratamiento_Click" OnClientClick="return validarFormularioTratamiento();" />
                <asp:Button ID="btnCancelarTratamiento" runat="server" Text="Cancelar" CssClass="btn btn-secondary-custom" 
                    OnClick="btnCancelarTratamiento_Click" UseSubmitBehavior="false" />
            </div>
        </div>
    </asp:Panel>

    <!-- LABEL DE MENSAJE CON ESTILO PERSONALIZADO -->
    <asp:Label ID="lblMensaje" runat="server" Visible="False" CssClass="alert-custom d-block mb-3" role="alert"></asp:Label>

    <!-- GRIDVIEW PARA MOSTRAR LA LISTA DE TRATAMIENTOS REGISTRADOS -->
    <div class="table-responsive-custom">
        <asp:GridView ID="gvTratamientos" runat="server" CssClass="table table-custom" AutoGenerateColumns="False" DataKeyNames="IDTratamiento" OnRowCommand="gvTratamientos_RowCommand">
            <Columns>
                <asp:BoundField DataField="IDTratamiento" HeaderText="ID" ReadOnly="True" />
                <asp:BoundField DataField="NombrePaciente" HeaderText="Paciente" />
                <asp:BoundField DataField="FechaTratamiento" HeaderText="Fecha" DataFormatString="{0:dd/MM/yyyy}" />
                <asp:BoundField DataField="NumProductosUsados" HeaderText="Productos Usados" />
                <asp:TemplateField HeaderText="Acciones">
                    <ItemTemplate>
                        <div class="table-actions">
                            <asp:LinkButton ID="btnEditar" runat="server" CommandName="EditarTratamiento" CommandArgument='<%# Eval("IDTratamiento") %>' CssClass="btn-action btn-action-edit">
                                <i class="bi bi-pencil"></i> Editar
                            </asp:LinkButton>
                            <asp:LinkButton ID="btnEliminar" runat="server" CommandName="EliminarTratamiento" CommandArgument='<%# Eval("IDTratamiento") %>' CssClass="btn-action btn-action-delete" OnClientClick="return confirm('¿Está seguro de eliminar este tratamiento? El inventario será restaurado.');">
                                <i class="bi bi-trash"></i> Eliminar
                            </asp:LinkButton>
                        </div>
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
            <EmptyDataTemplate>
                <div class="alert-custom alert-info">
                    No hay tratamientos registrados.
                </div>
            </EmptyDataTemplate>
        </asp:GridView>
    </div>

    <!-- BLOQUE DE JAVASCRIPT PARA LA LÓGICA AJAX -->
    <script type="text/javascript">
        function pageLoad() {
            // Asignar eventos a los campos de cantidad
            var cantidadInputs = document.querySelectorAll('.cantidad-utilizada');
            cantidadInputs.forEach(function(input) {
                input.addEventListener('change', function() {
                    validarCantidad(this);
                });
                input.addEventListener('input', function() {
                    validarCantidad(this);
                });
            });
        }

        function cargarDatosDesdeCita() {
            // Esta función ahora es un respaldo. La carga principal se hace en el servidor.
            // Se mantiene por si el DDL se cambia manualmente después de la carga inicial.
            var ddlCita = document.getElementById('<%= ddlCita.ClientID %>');
            var idCita = ddlCita.value;

            limpiarFormularioPaciente();

            if (idCita === "0") {
                return;
            }

            PageMethods.ObtenerDatosDeCitaParaTratamiento(idCita, onDatosCitaRecibidos, onError);
        }

        function onDatosCitaRecibidos(resultados) {
            var datos;
            try {
                datos = JSON.parse(resultados);
            } catch (e) {
                mostrarErrorCliente('La respuesta del servidor no es válida.');
                return;
            }

            if (datos.error) {
                mostrarErrorCliente('Error del servidor: ' + datos.error);
                return;
            }

            if (!datos.paciente) {
                mostrarErrorCliente('Error: No se encontraron datos del paciente para esta cita.');
                limpiarFormularioPaciente();
                return;
            }

            var ddlPaciente = document.getElementById('<%= ddlPaciente.ClientID %>');
            ddlPaciente.innerHTML = '';
            var option = new Option(datos.paciente.nombre, datos.paciente.id);
            ddlPaciente.add(option);

            document.getElementById('<%= hfIDPaciente.ClientID %>').value = datos.paciente.id;

            var txtFecha = document.getElementById('<%= txtFechaTratamiento.ClientID %>');
            txtFecha.value = datos.paciente.fechaCita;
        }

        function limpiarFormularioPaciente() {
            document.getElementById('<%= ddlPaciente.ClientID %>').innerHTML = '';
            document.getElementById('<%= hfIDPaciente.ClientID %>').value = '';
            document.getElementById('<%= txtFechaTratamiento.ClientID %>').value = '';
        }

        function onError(error) {
            mostrarErrorCliente('Error de comunicación al cargar los datos de la cita: ' + error.get_message());
        }

        function validarCantidad(input) {
            var row = input.closest('.servicio-detalle');
            var inventarioDisponible = parseFloat(row.querySelector('[id*="hfInventarioDisponible"]').value);
            var cantidad = parseFloat(input.value) || 0;
            var mensajeElement = row.querySelector('.cantidad-mensaje');

            if (cantidad < 1) {
                mensajeElement.textContent = 'La cantidad mínima es 1';
                mensajeElement.style.display = 'block';
                input.style.borderColor = '#dc3545';
                return false;
            }

            if (cantidad > inventarioDisponible) {
                mensajeElement.textContent = 'No hay suficiente inventario. Máximo disponible: ' + inventarioDisponible;
                mensajeElement.style.display = 'block';
                input.style.borderColor = '#dc3545';
                return false;
            }

            if (cantidad <= 0) {
                mensajeElement.textContent = 'La cantidad debe ser mayor a 0';
                mensajeElement.style.display = 'block';
                input.style.borderColor = '#dc3545';
                return false;
            }

            mensajeElement.style.display = 'none';
            input.style.borderColor = '#28a745';
            return true;
        }

        function validarFormularioTratamiento() {
            var ddlCita = document.getElementById('<%= ddlCita.ClientID %>');
            if (ddlCita.value === "0") {
                alert('Debe seleccionar una cita.');
                return false;
            }

            var cantidadInputs = document.querySelectorAll('.cantidad-utilizada');
            var hayCantidadesValidas = false;
            var hayErrores = false;

            cantidadInputs.forEach(function (input) {
                if (!validarCantidad(input)) {
                    hayErrores = true;
                }
                if (input.value && parseFloat(input.value) > 0) {
                    hayCantidadesValidas = true;
                }
            });

            if (hayErrores) {
                alert('Por favor, corrija los errores en las cantidades antes de guardar.');
                return false;
            }

            if (!hayCantidadesValidas) {
                alert('Debe ingresar al menos una cantidad para un producto.');
                return false;
            }

            return confirm('¿Está seguro de guardar el tratamiento? Se actualizará el inventario.');
        }

        function mostrarErrorCliente(mensaje) {
            alert(mensaje);
        }
    </script>
</asp:Content>