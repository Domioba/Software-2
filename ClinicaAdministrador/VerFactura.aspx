<%@ Page Title="Ver Factura" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="VerFactura.aspx.cs" Inherits="ClinicaAdministrador.VerFactura" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <!-- =========================================== -->
    <!-- VINCULAMOS EL ESTILO ESPECÍFICO DE ESTA PÁGINA -->
    <link href="css/estiloVerFactura.css" rel="stylesheet" type="text/css" />
    <!-- =========================================== -->
</asp:Content>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    
    <div class="invoice-container">
        <!-- Cabecera de la Factura -->
        <header class="invoice-header">
            <h3>Factura</h3>
            <div class="invoice-id">
                <asp:Label ID="lblIDFactura" runat="server" Text="#"></asp:Label>
            </div>
        </header>

        <!-- Detalles del Paciente y Pago -->
        <section class="invoice-details">
            <div class="row">
                <div class="col-md-6">
                    <strong>Paciente:</strong>
                    <asp:Label ID="lblPaciente" runat="server" Text=""></asp:Label>
                </div>
                <div class="col-md-6">
                    <strong>Fecha de Emisión:</strong>
                    <asp:Label ID="lblFecha" runat="server" Text=""></asp:Label>
                </div>
            </div>
            <div class="row">
                <div class="col-md-6">
                    <strong>Método de Pago:</strong>
                    <asp:Label ID="lblMetodoPago" runat="server" Text=""></asp:Label>
                </div>
                <div class="col-md-6">
                    <strong>Estado de Pago:</strong>
                    <div class="mt-1">
                        <asp:Label ID="lblEstadoPago" runat="server" CssClass="badge-payment-status" Text=""></asp:Label>
                    </div>
                </div>
            </div>
        </section>

        <!-- Sección de Servicios -->
        <section class="invoice-section">
            <h5>Servicios Realizados</h5>
            <div class="invoice-services-list">
                <p><asp:Label ID="lblServicios" runat="server" Text=""></asp:Label></p>
            </div>
        </section>

        <!-- Sección del Total -->
        <section class="invoice-section invoice-total">
            <h4>Total a Pagar: <span class="total-amount"><asp:Label ID="lblTotal" runat="server" Text=""></asp:Label></span></h4>
        </section>

        <!-- Botón de Acción -->
        <div class="text-center mt-4">
            <asp:Button ID="btnVolver" runat="server" Text="Volver a Facturación" CssClass="btn-back" PostBackUrl="~/Facturacion.aspx" />
        </div>
    </div>

</asp:Content>