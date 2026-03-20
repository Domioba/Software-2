using System;
using System.Web.UI;
using ClinicaAdministrador.BLL;

namespace ClinicaAdministrador
{
    public partial class Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // --- LÓGICA PARA EL MENSAJE DE BIENVENIDA ---
                // Verificamos si el usuario ha iniciado sesión
                if (Session["NombreAdmin"] != null)
                {
                    // Si la sesión existe, asignamos el nombre del usuario a la etiqueta
                    lblNombreUsuarioDashboard.Text = Session["NombreAdmin"].ToString();
                }
                else
                {
                    // Si no hay sesión, redirigimos al login por seguridad
                    Response.Redirect("Login.aspx");
                }
                // --- FIN DE LA LÓGICA DE BIENVENIDA ---

                // El resto de tu lógica para cargar los datos del dashboard sigue igual
                var resumen = Dashboard.ObtenerResumen();

                lblTotalPacientes.Text = resumen.totalPacientes.ToString();
                lblCitasHoy.Text = resumen.citasHoy.ToString();
                lblIngresosMes.Text = resumen.ingresosMes.ToString("C");
                lblAlertasInventario.Text = resumen.alertasInventario.ToString();

                // Estas líneas ahora funcionarán porque los div tienen runat="server"
                proximas_citas.InnerHtml = Dashboard.ObtenerHtmlProximasCitas();
                servicios_populares.InnerHtml = Dashboard.ObtenerHtmlServiciosPopulares();
            }
        }
    }
}