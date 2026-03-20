<%@ Page Title="Configuración" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Configuracion.aspx.cs" Inherits="ClinicaAdministrador.Configuracion" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <!-- =========================================== -->
    <!-- VINCULAMOS EL ESTILO ESPECÍFICO DE ESTA PÁGINA -->
    <link href="css/estiloConfiguracion.css" rel="stylesheet" type="text/css" />
    <!-- =========================================== -->
</asp:Content>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    
    <div class="d-flex justify-content-between align-items-center mb-4">
        <h2 class="fw-bold">Configuración de Cuenta</h2>
    </div>

    <div class="card card-custom mb-4 shadow">
        <div class="card-header">
            <h4 class="mb-0">Cambiar Contraseña</h4>
        </div>
        <div class="card-body">
            <p class="text-muted">Utiliza este formulario para cambiar tu contraseña de acceso al sistema.</p>
            
            <!-- Mensaje de Éxito o Error -->
            <asp:Label ID="lblMensaje" runat="server" Visible="False" CssClass="alert-custom d-block mb-3" role="alert"></asp:Label>

            <div class="row">
                <div class="col-md-4 mb-3">
                    <label for="txtContrasenaActual" class="form-label fw-bold">Contraseña Actual</label>
                    <asp:TextBox ID="txtContrasenaActual" runat="server" TextMode="Password" CssClass="form-control" required></asp:TextBox>
                </div>
            </div>
            <div class="row">
                <div class="col-md-4 mb-3">
                    <label for="txtNuevaContrasena" class="form-label fw-bold">Nueva Contraseña</label>
                    <asp:TextBox ID="txtNuevaContrasena" runat="server" TextMode="Password" CssClass="form-control" required></asp:TextBox>
                </div>
                <div class="col-md-4 mb-3">
                    <label for="txtConfirmarContrasena" class="form-label fw-bold">Confirmar Nueva Contraseña</label>
                    <asp:TextBox ID="txtConfirmarContrasena" runat="server" TextMode="Password" CssClass="form-control" required></asp:TextBox>
                </div>
            </div>
            
            <asp:Button ID="btnCambiarPassword" runat="server" Text="Cambiar Contraseña" CssClass="btn btn-primary-custom" OnClick="btnCambiarPassword_Click" />
        </div>
    </div>

</asp:Content>