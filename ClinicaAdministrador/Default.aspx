<%@ Page Title="Panel de Control" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="ClinicaAdministrador.Default" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <!-- =========================================== -->
    <!-- VINCULAMOS EL ESTILO ESPECÍFICO DEL DASHBOARD -->
    <link href="css/estiloDefault.css" rel="stylesheet" type="text/css" />
    <!-- =========================================== -->
</asp:Content>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    
    <div class="dashboard-container">
        <!-- MENSAJE DE BIENVENIDA DINÁMICO -->
        <h1>¡Bienvenido, <asp:Label ID="lblNombreUsuarioDashboard" runat="server" Text="Dr. Ibarra"></asp:Label>!</h1>
        <p>Resumen general de la clínica al instante.</p>
        
        <!-- INICIO DE LA FILA DE TARJETAS -->
        <div class="row">
            
           <!-- Tarjeta de Pacientes (Enlace a Pacientes.aspx) -->
<div class="col-md-3 mb-4">
    <!-- MODIFICADO: El href ahora apunta a Pacientes.aspx -->
    <a href="Pacientes.aspx" class="text-decoration-none">
        <div class="dashboard-card dashboard-pacientes">
            <div class="card-body">
                <i class="bi bi-people-fill" style="font-size: 2rem; opacity: 0.7;"></i>
                <h5 class="card-title">Total Pacientes</h5>
                <p class="card-text"><asp:Label ID="lblTotalPacientes" runat="server" Text="0"></asp:Label></p>
            </div>
        </div>
    </a>
</div>

<!-- Tarjeta de Citas (Enlace a Citas de la CLinica) -->
<div class="col-md-3 mb-4">
    <a href="Citas.aspx" class="text-decoration-none">
        <div class="dashboard-card dashboard-citas">
            <div class="card-body">
                <i class="bi bi-calendar-check-fill" style="font-size: 2rem; opacity: 0.7;"></i>
                <h5 class="card-title">Citas</h5>
                <p class="card-text"><asp:Label ID="lblCitasHoy" runat="server" Text="0"></asp:Label></p>
            </div>
        </div>
    </a>
</div>

<!-- Tarjeta de Ingresos (Enlace a Reporte de Ingresos) -->
<div class="col-md-3 mb-4">
    <a href="Reportes.aspx?type=ingresos&month=<%= DateTime.Now.Month %>&year=<%= DateTime.Now.Year %>" class="text-decoration-none">
        <div class="dashboard-card dashboard-ingresos">
            <div class="card-body">
                <i class="bi bi-currency-dollar" style="font-size: 2rem; opacity: 0.7;"></i>
                <h5 class="card-title">Ingresos del Mes (Reportes)</h5>
                <p class="card-text"><asp:Label ID="lblIngresosMes" runat="server" Text="$0.00"></asp:Label></p>
            </div>
        </div>
    </a>
</div>

<!-- Tarjeta de Alertas (Enlace a Inventario.aspx) -->
<div class="col-md-3 mb-4">
    <!-- MODIFICADO: El href ahora apunta a Inventario.aspx -->
    <a href="Inventario.aspx" class="text-decoration-none">
        <div class="dashboard-card dashboard-alertas">
            <div class="card-body">
                <i class="bi bi-exclamation-triangle-fill" style="font-size: 2rem; opacity: 0.7;"></i>
                <h5 class="card-title">Alertas de Inventario</h5>
                <p class="card-text"><asp:Label ID="lblAlertasInventario" runat="server" Text="0"></asp:Label></p>
            </div>
        </div>
    </a>
</div>

</div>
        <!-- FIN DE LA FILA DE TARJETAS -->
        
        <div class="row info-section mt-4">
            <div class="col-md-6">
                <div class="card">
                    <div class="card-header">
                        <h5 class="mb-0"><i class="bi bi-calendar-week"></i> Próximas Citas</h5>
                    </div>
                    <div class="card-body p-0">
                        <div id="proximas_citas" runat="server" class="custom-list-group"></div>
                    </div>
                </div>
            </div>

            <div class="col-md-6">
                <div class="card">
                    <div class="card-header">
                        <h5 class="mb-0"><i class="bi bi-graph-up"></i> Servicios Más Solicitados</h5>
                    </div>
                    <div class="card-body p-0">
                        <div id="servicios_populares" runat="server" class="custom-list-group"></div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>