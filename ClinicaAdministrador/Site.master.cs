using System;
using System.Web.UI;

namespace ClinicaAdministrador
{
    public partial class SiteMaster : MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Si no es un postback (como un clic en un botón)
            if (!IsPostBack)
            {
                // Si no hay sesión de usuario Y no estamos ya en la página de login
                if (Session["NombreAdmin"] == null && !Request.Url.AbsolutePath.EndsWith("Login.aspx"))
                {
                    // Redirige al login
                    Response.Redirect("Login.aspx");
                }
                // Si hay sesión, actualiza el nombre en la barra
                else if (Session["NombreAdmin"] != null)
                {
                    lblNombreUsuario.Text = Session["NombreAdmin"].ToString();
                }
            }
        }

        protected void btnLogout_Click(object sender, EventArgs e)
        {
            // Cerrar sesión y redirigir al login
            Session.Clear();
            Session.Abandon();
            Response.Redirect("Login.aspx");
        }
    }
}