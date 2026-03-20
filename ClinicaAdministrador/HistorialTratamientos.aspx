<%@ Page Title="Historial de Tratamientos" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="HistorialTratamientos.aspx.cs" Inherits="ClinicaAdministrador.HistorialTratamientos" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <!-- =========================================== -->
    <!-- VINCULAMOS EL ESTILO ESPECÍFICO DE ESTA PÁGINA -->
    <link href="css/estiloHistorial.css" rel="stylesheet" type="text/css" />
    <!-- =========================================== -->
</asp:Content>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    
    <div class="d-flex justify-content-between align-items-center mb-4">
        <h2 class="fw-bold">Historial de Tratamientos por Paciente</h2>
    </div>

    <div class="card card-custom mb-4 shadow">
        <div class="card-header">
            <h4 class="mb-0">Seleccionar Paciente</h4>
        </div>
        <div class="card-body">
            <div class="row">
                <div class="col-md-8 mb-3">
                    <label for="ddlPaciente" class="form-label fw-bold">Paciente</label>
                    <asp:DropDownList ID="ddlPaciente" runat="server" CssClass="form-select" required></asp:DropDownList>
                </div>
                <div class="col-md-4 mb-3 d-flex align-items-end">
                    <asp:Button ID="btnConsultar" runat="server" Text="Consultar Historial" CssClass="btn-consult" OnClick="btnConsultar_Click" />
                </div>
            </div>
        </div>
    </div>

    <!-- GRIDVIEW PARA MOSTRAR EL HISTORIAL -->
    <div class="table-responsive-custom">
        <asp:GridView ID="gvHistorial" runat="server" CssClass="table table-custom" AutoGenerateColumns="False">
            <Columns>
                <asp:BoundField DataField="FechaTratamiento" HeaderText="Fecha" DataFormatString="{0:dd/MM/yyyy}" />
                <asp:BoundField DataField="NombreServicio" HeaderText="Servicio" />
                <asp:BoundField DataField="NombreProducto" HeaderText="Producto Utilizado" />
                <asp:BoundField DataField="Descripcion" HeaderText="Descripción" />
                <asp:BoundField DataField="Dosis" HeaderText="Dosis" />
                <asp:BoundField DataField="Observaciones" HeaderText="Observaciones" />
                <asp:BoundField DataField="NombreAdmin" HeaderText="Registrado por" />
            </Columns>
            <EmptyDataTemplate>
                <div class="alert-custom alert-info">
                    No se encontraron tratamientos para este paciente, o no ha sido seleccionado.
                </div>
            </EmptyDataTemplate>
        </asp:GridView>
    </div>

</asp:Content>