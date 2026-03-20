<%@ Page Title="Reportes" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Reportes.aspx.cs" Inherits="ClinicaAdministrador.Reportes" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <!-- La referencia a nuestro CSS principal -->
    <link href="css/estiloReportes.css" rel="stylesheet" type="text/css" />
</asp:Content>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    
    <!-- Contenedor principal con el estado de carga -->
    <div class="loading-overlay" id="loading-overlay" style="display: none;">
        <div class="loading-content">
            <div class="spinner"></div>
            <p class="loading-text">Generando reporte, por favor, espera un momento...</p>
        </div>
    </div>

    <!-- Contenedor principal con el formulario y el resultado -->
    <main class="main-container">
        <!-- Formulario de filtros -->
        <div class="filters-card">
            <h4 class="card-title">Filtros del Reporte</h4>
            <div class="row g-3">
                <div class="col-md-4 mb-3">
                    <label for="ddlTipoReporte" class="form-label fw-bold">Tipo de Reporte</label>
                    <asp:DropDownList ID="ddlTipoReporte" runat="server" CssClass="form-select">
                        <asp:ListItem Text="Ingresos Mensuales" Value="ingresos" />
                        <asp:ListItem Text="Servicios Más Solicitados" Value="servicios" />
                        <asp:ListItem Text="Clientes Atendidos" Value="clientes" />
                    </asp:DropDownList>
                </div>
                <div class="col-md-4 mb-3">
                    <label for="ddlMes" class="form-label fw-bold">Mes</label>
                    <asp:DropDownList ID="ddlMes" runat="server" CssClass="form-select">
                        <asp:ListItem Text="Enero" Value="1" />
                        <asp:ListItem Text="Febrero" Value="2" />
                        <asp:ListItem Text="Marzo" Value="3" />
                        <asp:ListItem Text="Abril" Value="4" />
                        <asp:ListItem Text="Mayo" Value="5" />
                        <asp:ListItem Text="Junio" Value="6" />
                        <asp:ListItem Text="Julio" Value="7" />
                        <asp:ListItem Text="Agosto" Value="8" />
                        <asp:ListItem Text="Septiembre" Value="9" />
                        <asp:ListItem Text="Octubre" Value="10" />
                        <asp:ListItem Text="Noviembre" Value="11" />
                        <asp:ListItem Text="Diciembre" Value="12" />
                    </asp:DropDownList>
                </div>
                <div class="col-md-4 mb-3">
                    <label for="ddlAnio" class="form-label fw-bold">Año</label>
                    <asp:DropDownList ID="ddlAnio" runat="server" CssClass="form-select">
                   
                    </asp:DropDownList>
                </div>
                <div class="col-12">
                    <asp:Button ID="btnGenerar" runat="server" Text="Generar Reporte" CssClass="btn btn-primary w-100" OnClick="btnGenerar_Click" OnClientClick="showLoading();" />
                </div>
            </div>
        </div>

       
        <asp:Panel ID="pnlResultado" runat="server" CssClass="card card-custom shadow" Visible="false">
            <div class="card-header">
                <h4 id="tituloResultado" runat="server" class="mb-0">Resultado del Reporte</h4>
            </div>
            <div class="card-body">
            
                <div class="table-responsive-custom">
                    <asp:GridView ID="gvReporte" runat="server" CssClass="table table-custom" AutoGenerateColumns="False">
                        <EmptyDataTemplate>
                            <div class="alert-custom alert-info">
                                No hay registros que coincidan con los filtros.
                            </div>
                        </EmptyDataTemplate>
                    </asp:GridView>
                </div>
                
           
                <div class="mt-3 text-end">
                    <asp:Label ID="lblTotal" runat="server" CssClass="fw-bold fs-5"></asp:Label>
                </div>
            </div>
        </asp:Panel>

        <div class="alert-custom alert-success d-block mt-3" id="success-message" style="display: none;">
            <i class="bi bi-check-circle-fill"></i>
            <span id="success-text">¡Reporte generado con éxito!</span>
        </div>

     
        <div class="alert-custom alert-error d-block mt-3" id="error-message" style="display: none;">
            <i class="bi bi-exclamation-triangle-fill"></i>
            <span id="error-text"></span>
        </div>
    </main>

    <!-- JavaScript para el estado de carga y manejo del PDF -->
    <script type="text/javascript">
        function showLoading() {
            document.getElementById('loading-overlay').style.display = 'flex';
            document.getElementById('success-message').style.display = 'none'; 
            document.getElementById('error-message').style.display = 'none'; 
        }

        function hideLoading() {
            document.getElementById('loading-overlay').style.display = 'none';
        }

        function showSuccess() {
            document.getElementById('success-message').style.display = 'flex';
            setTimeout(() => { hideSuccess(); }, 3000); 
        }

        function showError(message) {
            document.getElementById('error-text').innerText = message;
            document.getElementById('error-message').style.display = 'flex';
            document.getElementById('loading-overlay').style.display = 'none'; 
        }
    </script>
</asp:Content>