<%@ Page Title="Gestión de Inventario" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Inventario.aspx.cs" Inherits="ClinicaAdministrador.Inventario" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <!-- =========================================== -->
    <!-- VINCULAMOS EL ESTILO ESPECÍFICO DE ESTA PÁGINA -->
    <link href="css/estiloInventario.css" rel="stylesheet" type="text/css" />
    <!-- =========================================== -->
</asp:Content>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    
    <div class="d-flex justify-content-between align-items-center mb-4">
        <h2 class="fw-bold">Gestión de Inventario</h2>
        <asp:Button ID="btnNuevoProducto" runat="server" Text="Nuevo Producto" CssClass="btn-add-new" OnClick="btnNuevoProducto_Click" />
    </div>

    <!-- MENSAJES DE ALERTA -->
    <asp:Label ID="lblMensajeError" runat="server" Visible="False" CssClass="alert-custom alert-error d-block mb-3" role="alert"></asp:Label>
    <asp:Label ID="lblMensajeExito" runat="server" Visible="False" CssClass="alert-custom alert-success d-block mb-3" role="alert"></asp:Label>

    <!-- PANEL PARA EL FORMULARIO DE CREAR/EDITAR PRODUCTO -->
    <asp:Panel ID="pnlFormularioProducto" runat="server" CssClass="card card-custom mb-4 shadow" Visible="false">
        <div class="card-header">
            <h4 id="tituloFormularioProducto" runat="server" class="mb-0">Nuevo Producto</h4>
        </div>
        <div class="card-body">
            <asp:HiddenField ID="hfIDProducto" runat="server" />

            <div class="row">
                <div class="col-md-6 mb-3">
                    <label for="txtNombreProducto" class="form-label fw-bold">Nombre del Producto</label>
                    <asp:TextBox ID="txtNombreProducto" runat="server" CssClass="form-control" required></asp:TextBox>
                </div>
                <div class="col-md-6 mb-3">
                    <label for="txtCategoria" class="form-label fw-bold">Categoría</label>
                    <asp:TextBox ID="txtCategoria" runat="server" CssClass="form-control" required></asp:TextBox>
                </div>
            </div>
            <div class="row">
                <div class="col-md-3 mb-3">
                    <label for="txtCantidad" class="form-label fw-bold">Cantidad Disponible</label>
                    <asp:TextBox ID="txtCantidad" runat="server" TextMode="Number" step="0.01" min="0" max="100"
                        CssClass="form-control" placeholder="0.00" required
                        onchange="validarCantidad(this)" onkeyup="validarCantidad(this)"></asp:TextBox>
                    <small class="form-text text-muted cantidad-mensaje" style="display: none; color: #dc3545;"></small>
                    <small class="form-text text-muted">Máximo: 100 unidades</small>
                </div>
                <div class="col-md-3 mb-3">
                    <label for="txtUnidadMedida" class="form-label fw-bold">Unidad de Medida</label>
                    <asp:DropDownList ID="ddlUnidadMedida" runat="server" CssClass="form-select">
                        <asp:ListItem Text="Seleccione unidad" Value="" />
                        <asp:ListItem Text="ml" Value="ml" />
                        <asp:ListItem Text="gr" Value="gr" />
                        <asp:ListItem Text="unidades" Value="unidades" />
                        <asp:ListItem Text="frascos" Value="frascos" />
                        <asp:ListItem Text="jeringas" Value="jeringas" />
                        <asp:ListItem Text="ampollas" Value="ampollas" />
                        <asp:ListItem Text="viales" Value="viales" />
                        <asp:ListItem Text="kit" Value="kit" />
                    </asp:DropDownList>
                </div>
                <div class="col-md-3 mb-3">
                    <label for="txtFechaVencimiento" class="form-label fw-bold">Fecha de Vencimiento</label>
                    <asp:TextBox ID="txtFechaVencimiento" runat="server" TextMode="Date" CssClass="form-control"></asp:TextBox>
                    <small class="form-text text-muted">Opcional - Si se especifica, no puede ser una fecha pasada</small>
                </div>
                <div class="col-md-3 mb-3">
                    <label for="txtCostoUnitario" class="form-label fw-bold">Costo Unitario ($)</label>
                    <asp:TextBox ID="txtCostoUnitario" runat="server" TextMode="Number" step="0.01" min="0" max="10000"
                        CssClass="form-control" placeholder="0.00" required
                        onchange="validarCosto(this)" onkeyup="validarCosto(this)"></asp:TextBox>
                    <small class="form-text text-muted costo-mensaje" style="display: none; color: #dc3545;"></small>
                    <small class="form-text text-muted">Máximo: $10,000</small>
                </div>
            </div>
            <div class="row">
                <div class="col-md-6 mb-3">
                    <label for="ddlEstado" class="form-label fw-bold">Estado</label>
                    <asp:DropDownList ID="ddlEstado" runat="server" CssClass="form-select">
                        <asp:ListItem Text="Activo" Value="Activo" />
                        <asp:ListItem Text="Inactivo" Value="Inactivo" />
                    </asp:DropDownList>
                </div>
            </div>
            
            <div class="d-flex gap-2">
                <asp:Button ID="btnGuardarProducto" runat="server" Text="Guardar" CssClass="btn btn-success-custom" 
                    OnClick="btnGuardarProducto_Click" OnClientClick="return validarFormularioProducto();" />
                <asp:Button ID="btnCancelarProducto" runat="server" Text="Cancelar" CssClass="btn btn-secondary-custom" 
                    OnClick="btnCancelarProducto_Click" UseSubmitBehavior="false" />
            </div>
        </div>
    </asp:Panel>

    <!-- GRIDVIEW PARA MOSTRAR LA LISTA DE PRODUCTOS CON ALERTAS -->
    <div class="table-responsive-custom">
        <asp:GridView ID="gvInventario" runat="server" CssClass="table table-custom" AutoGenerateColumns="False" DataKeyNames="IDProducto" OnRowDataBound="gvInventario_RowDataBound" OnRowCommand="gvInventario_RowCommand">
            <Columns>
                <asp:BoundField DataField="IDProducto" HeaderText="ID" ReadOnly="True" />
                <asp:BoundField DataField="NombreProducto" HeaderText="Producto" />
                <asp:BoundField DataField="Categoria" HeaderText="Categoría" />
                <asp:BoundField DataField="CantidadDisponible" HeaderText="Cantidad" DataFormatString="{0:N2}" />
                <asp:BoundField DataField="UnidadMedida" HeaderText="Unidad" />
                <asp:BoundField DataField="FechaVencimiento" HeaderText="Vencimiento" DataFormatString="{0:dd/MM/yyyy}" NullDisplayText="-" />
                <asp:BoundField DataField="CostoUnitario" HeaderText="Costo Unitario" DataFormatString="{0:C}" />
                <asp:TemplateField HeaderText="Estado">
                    <ItemTemplate>
                        <span class="badge-status <%# Eval("Estado").ToString().ToLower() %>">
                            <%# Eval("Estado") %>
                        </span>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Alertas">
                    <ItemTemplate>
                        <asp:Label ID="lblAlerta" runat="server" CssClass="badge-inventory"></asp:Label>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Acciones">
                    <ItemTemplate>
                        <div class="table-actions">
                            <asp:LinkButton ID="btnEditar" runat="server" CommandName="Editar" CommandArgument='<%# Eval("IDProducto") %>' CssClass="btn-action btn-action-edit">
                                <i class="bi bi-pencil"></i> Editar
                            </asp:LinkButton>
                            <asp:LinkButton ID="btnEliminar" runat="server" CommandName="Eliminar" CommandArgument='<%# Eval("IDProducto") %>' CssClass="btn-action btn-action-delete" 
                                OnClientClick='<%# "return confirmEliminarProducto(\"" + Eval("NombreProducto") + "\", \"" + Eval("Estado") + "\");" %>'>
                                <i class="bi bi-trash"></i> 
                                <asp:Literal ID="ltTextoBoton" runat="server" Text='<%# Eval("Estado").ToString() == "Inactivo" ? "Eliminar" : "Desactivar" %>' />
                            </asp:LinkButton>
                        </div>
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
            <EmptyDataTemplate>
                <div class="alert-custom alert-info">
                    No hay productos en el inventario.
                </div>
            </EmptyDataTemplate>
        </asp:GridView>
    </div>

    <!-- JavaScript para validaciones -->
    <script type="text/javascript">
        // Límites realistas para una clínica estética
        const MAX_CANTIDAD = 100;
        const MAX_COSTO = 10000;

        function pageLoad() {
            // Asignar eventos a los campos numéricos
            var cantidadInputs = document.querySelectorAll('input[type="number"]');
            cantidadInputs.forEach(function (input) {
                input.addEventListener('change', function () {
                    if (this.id.includes('Cantidad')) {
                        validarCantidad(this);
                    } else if (this.id.includes('Costo')) {
                        validarCosto(this);
                    }
                });
                input.addEventListener('input', function () {
                    if (this.id.includes('Cantidad')) {
                        validarCantidad(this);
                    } else if (this.id.includes('Costo')) {
                        validarCosto(this);
                    }
                });
            });

            // Validar fecha de vencimiento
            var fechaVencimiento = document.getElementById('<%= txtFechaVencimiento.ClientID %>');
            if (fechaVencimiento) {
                fechaVencimiento.addEventListener('change', validarFechaVencimiento);
            }

            // Validar unidad de medida
            var ddlUnidad = document.getElementById('<%= ddlUnidadMedida.ClientID %>');
            if (ddlUnidad) {
                ddlUnidad.addEventListener('change', validarUnidadMedida);
            }
        }

        function validarCantidad(input) {
            var cantidad = parseFloat(input.value) || 0;
            var mensajeElement = input.parentNode.querySelector('.cantidad-mensaje');

            if (cantidad < 0) {
                mensajeElement.textContent = 'La cantidad no puede ser negativa';
                mensajeElement.style.display = 'block';
                input.style.borderColor = '#dc3545';
                return false;
            }

            if (cantidad > MAX_CANTIDAD) {
                mensajeElement.textContent = 'La cantidad máxima permitida es ' + MAX_CANTIDAD;
                mensajeElement.style.display = 'block';
                input.style.borderColor = '#dc3545';
                return false;
            }

            // Validar decimales para unidades específicas
            var ddlUnidad = document.getElementById('<%= ddlUnidadMedida.ClientID %>');
            if (ddlUnidad && ddlUnidad.value === 'unidades') {
                if (!Number.isInteger(cantidad)) {
                    mensajeElement.textContent = 'Para unidades, la cantidad debe ser un número entero';
                    mensajeElement.style.display = 'block';
                    input.style.borderColor = '#dc3545';
                    return false;
                }
            }

            mensajeElement.style.display = 'none';
            input.style.borderColor = cantidad > 0 ? '#28a745' : '#6c757d';
            return true;
        }

        function validarCosto(input) {
            var costo = parseFloat(input.value) || 0;
            var mensajeElement = input.parentNode.querySelector('.costo-mensaje');

            if (costo < 0) {
                mensajeElement.textContent = 'El costo no puede ser negativo';
                mensajeElement.style.display = 'block';
                input.style.borderColor = '#dc3545';
                return false;
            }

            if (costo > MAX_COSTO) {
                mensajeElement.textContent = 'El costo máximo permitido es $' + MAX_COSTO.toLocaleString();
                mensajeElement.style.display = 'block';
                input.style.borderColor = '#dc3545';
                return false;
            }

            mensajeElement.style.display = 'none';
            input.style.borderColor = costo > 0 ? '#28a745' : '#6c757d';
            return true;
        }

        function validarFechaVencimiento() {
            var fechaInput = document.getElementById('<%= txtFechaVencimiento.ClientID %>');
            if (!fechaInput.value) return true; // Fecha opcional

            var fechaSeleccionada = new Date(fechaInput.value);
            var hoy = new Date();
            hoy.setHours(0, 0, 0, 0); // Ignorar la hora

            if (fechaSeleccionada < hoy) {
                alert('La fecha de vencimiento no puede ser en el pasado');
                fechaInput.value = '';
                fechaInput.focus();
                return false;
            }

            return true;
        }

        function validarUnidadMedida() {
            var ddlUnidad = document.getElementById('<%= ddlUnidadMedida.ClientID %>');
            if (ddlUnidad.value === "") {
                alert('Por favor, seleccione una unidad de medida');
                ddlUnidad.focus();
                return false;
            }
            return true;
        }

        function validarFormularioProducto() {
            var txtCantidad = document.getElementById('<%= txtCantidad.ClientID %>');
            var txtCosto = document.getElementById('<%= txtCostoUnitario.ClientID %>');
            var txtFecha = document.getElementById('<%= txtFechaVencimiento.ClientID %>');
            var ddlUnidad = document.getElementById('<%= ddlUnidadMedida.ClientID %>');

            // Validar unidad de medida
            if (!validarUnidadMedida()) {
                return false;
            }

            // Validar cantidad
            if (!validarCantidad(txtCantidad)) {
                alert('Por favor, corrija el campo de cantidad antes de guardar.');
                txtCantidad.focus();
                return false;
            }

            // Validar costo
            if (!validarCosto(txtCosto)) {
                alert('Por favor, corrija el campo de costo antes de guardar.');
                txtCosto.focus();
                return false;
            }

            // Validar fecha de vencimiento
            if (!validarFechaVencimiento()) {
                return false;
            }

            // Validar que la cantidad no sea cero para productos activos
            var ddlEstado = document.getElementById('<%= ddlEstado.ClientID %>');
            var cantidad = parseFloat(txtCantidad.value) || 0;

            if (ddlEstado.value === 'Activo' && cantidad === 0) {
                if (!confirm('El producto está activo pero la cantidad es 0. ¿Desea continuar?')) {
                    return false;
                }
            }

            return confirm('¿Está seguro de guardar el producto?');
        }

        function confirmEliminarProducto(nombreProducto, estado) {
            if (estado === 'Inactivo') {
                return confirm('¿Está seguro de ELIMINAR PERMANENTEMENTE el producto \"' + nombreProducto + '\"? Esta acción no se puede deshacer.');
            } else {
                return confirm('¿Está seguro de DESACTIVAR el producto \"' + nombreProducto + '\"? El producto se marcará como inactivo pero no se eliminará.');
            }
        }
    </script>
</asp:Content>