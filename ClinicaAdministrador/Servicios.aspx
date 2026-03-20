<%@ Page Title="Gestión de Servicios" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Servicios.aspx.cs" Inherits="ClinicaAdministrador.Servicios" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <!-- =========================================== -->
    <!-- VINCULAMOS EL ESTILO ESPECÍFICO DE ESTA PÁGINA -->
    <link href="css/estiloServicios.css" rel="stylesheet" type="text/css" />
    <!-- =========================================== -->
</asp:Content>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    
    <div class="d-flex justify-content-between align-items-center mb-4">
        <h2 class="fw-bold">Gestión de Servicios</h2>
        <asp:Button ID="btnNuevoServicio" runat="server" Text="Nuevo Servicio" CssClass="btn-add-new" OnClick="btnNuevoServicio_Click" />
    </div>

    <!-- LABELS DE MENSAJE PARA EL USUARIO -->
    <asp:Label ID="lblMensajeError" runat="server" Visible="False" CssClass="alert-custom alert-error d-block mb-3" role="alert"></asp:Label>
    <asp:Label ID="lblMensajeExito" runat="server" Visible="False" CssClass="alert-custom alert-success d-block mb-3" role="alert"></asp:Label>

    <!-- PANEL PARA EL FORMULARIO DE CREAR/EDITAR SERVICIO -->
    <asp:Panel ID="pnlFormularioServicio" runat="server" CssClass="card card-custom mb-4 shadow" Visible="false">
        <div class="card-header">
            <h4 id="tituloFormularioServicio" runat="server" class="mb-0">Nuevo Servicio</h4>
        </div>
        <div class="card-body">
            <!-- Campo oculto para almacenar el ID del servicio en caso de edición -->
            <asp:HiddenField ID="hfIDServicio" runat="server" />

            <div class="row">
                <div class="col-md-6 mb-3">
                    <label for="txtNombreServicio" class="form-label fw-bold">Nombre del Servicio</label>
                    <asp:TextBox ID="txtNombreServicio" runat="server" CssClass="form-control" required></asp:TextBox>
                </div>
                <div class="col-md-3 mb-3">
                    <label for="ddlCategoria" class="form-label fw-bold">Categoría</label>
                    <asp:DropDownList ID="ddlCategoria" runat="server" CssClass="form-select">
                        <asp:ListItem Text="Facial" Value="Facial" />
                        <asp:ListItem Text="Corporal" Value="Corporal" />
                        <asp:ListItem Text="Médico" Value="Médico" />
                        <asp:ListItem Text="Capilar" Value="Capilar" />
                    </asp:DropDownList>
                </div>
                <div class="col-md-3 mb-3">
                    <label for="ddlEstado" class="form-label fw-bold">Estado</label>
                    <asp:DropDownList ID="ddlEstado" runat="server" CssClass="form-select">
                        <asp:ListItem Text="Activo" Value="Activo" />
                        <asp:ListItem Text="Inactivo" Value="Inactivo" />
                    </asp:DropDownList>
                </div>
            </div>
            <div class="mb-3">
                <label for="txtDescripcion" class="form-label fw-bold">Descripción</label>
                <asp:TextBox ID="txtDescripcion" runat="server" TextMode="MultiLine" Rows="3" CssClass="form-control"></asp:TextBox>
            </div>
            <div class="row">
                <div class="col-md-4 mb-3">
                    <label for="txtPrecio" class="form-label fw-bold">Precio ($)</label>
                    <asp:TextBox ID="txtPrecio" runat="server" TextMode="Number" step="0.01" CssClass="form-control" required></asp:TextBox>
                </div>
                <div class="col-md-4 mb-3">
                    <label for="txtDuracion" class="form-label fw-bold">Duración (minutos)</label>
                    <asp:TextBox ID="txtDuracion" runat="server" TextMode="Number" CssClass="form-control" required></asp:TextBox>
                </div>
                
            </div>
            
            <div class="d-flex gap-2">
                <asp:Button ID="btnGuardarServicio" runat="server" Text="Guardar Servicio" CssClass="btn btn-success-custom" OnClick="btnGuardarServicio_Click" />
                <!-- Se añade UseSubmitBehavior="false" para evitar la validación del formulario al cancelar -->
                <asp:Button ID="btnCancelarServicio" runat="server" Text="Cancelar" CssClass="btn btn-secondary-custom" OnClick="btnCancelarServicio_Click" UseSubmitBehavior="false" />
            </div>
        </div>
    </asp:Panel>

    <!-- GRIDVIEW PARA MOSTRAR LA LISTA DE SERVICIOS -->
    <div class="table-responsive-custom">
        <asp:GridView ID="gvServicios" runat="server" CssClass="table table-custom" AutoGenerateColumns="False" DataKeyNames="IDServicio" OnRowCommand="gvServicios_RowCommand">
            <Columns>
                <asp:BoundField DataField="IDServicio" HeaderText="ID" ReadOnly="True" />
                <asp:BoundField DataField="NombreServicio" HeaderText="Nombre" />
                <asp:TemplateField HeaderText="Categoría">
                    <ItemTemplate>
                        <span class="badge-category <%# Eval("Categoria").ToString().ToLower() %>">
                            <%# Eval("Categoria") %>
                        </span>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:BoundField DataField="Precio" HeaderText="Precio" DataFormatString="{0:C}" />
                <asp:BoundField DataField="Duracion" HeaderText="Duración (min)" />
                <asp:TemplateField HeaderText="Estado">
                    <ItemTemplate>
                        <span class="badge-status <%# Eval("Estado").ToString().ToLower() %>">
                            <%# Eval("Estado") %>
                        </span>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Acciones">
                    <ItemTemplate>
                        <div class="table-actions">
                            <asp:LinkButton ID="btnEditar" runat="server" CommandName="Editar" CommandArgument='<%# Eval("IDServicio") %>' CssClass="btn-action btn-action-edit">
                                <i class="bi bi-pencil"></i> Editar
                            </asp:LinkButton>
                            <!-- Se cambia el texto y la confirmación para reflejar la acción de "Desactivar" -->
                            <asp:LinkButton ID="btnDesactivar" runat="server" CommandName="Eliminar" CommandArgument='<%# Eval("IDServicio") %>' CssClass="btn-action btn-action-delete" OnClientClick="return confirm('¿Estás seguro de DESACTIVAR este servicio? No se eliminará permanentemente.');">
                                <i class="bi bi-x-circle"></i> Desactivar
                            </asp:LinkButton>
                        </div>
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
            <EmptyDataTemplate>
                <div class="alert-custom alert-info">
                    No hay servicios registrados.
                </div>
            </EmptyDataTemplate>
        </asp:GridView>
    </div>
</asp:Content>