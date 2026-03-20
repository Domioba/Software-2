using System;
using System.Web.UI;
using System.Data.SqlClient;
using ClinicaAdministrador.DAL; // Asegúrate de tener esta referencia

namespace ClinicaAdministrador
{
    public partial class Configuracion : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Verificamos que el usuario haya iniciado sesión
            if (Session["Usuario"] == null)
            {
                Response.Redirect("Login.aspx");
            }
        }

        protected void btnCambiarPassword_Click(object sender, EventArgs e)
        {
            string usuarioActual = Session["Usuario"].ToString();
            string contrasenaActual = txtContrasenaActual.Text;
            string nuevaContrasena = txtNuevaContrasena.Text;
            string confirmarContrasena = txtConfirmarContrasena.Text;

            // Validación simple en el frontend
            if (nuevaContrasena != confirmarContrasena)
            {
                MostrarMensaje("Las nuevas contraseñas no coinciden.", "error");
                return;
            }

            using (SqlConnection con = DatabaseHelper.GetConnection())
            {
                // 1. Verificar que la contraseña actual es correcta
                string queryVerificar = "SELECT IDAdmin FROM Administrador WHERE Usuario = @Usuario AND ContraseñaHash = @Password";
                using (SqlCommand cmdVerificar = new SqlCommand(queryVerificar, con))
                {
                    cmdVerificar.Parameters.AddWithValue("@Usuario", usuarioActual);
                    cmdVerificar.Parameters.AddWithValue("@Password", contrasenaActual); // ¡Temporal! Sin hash.

                    con.Open();
                    object result = cmdVerificar.ExecuteScalar();

                    if (result == null) // No se encontró el usuario con esa contraseña
                    {
                        MostrarMensaje("La contraseña actual es incorrecta.", "error");
                        return;
                    }

                    // 2. Si la contraseña es correcta, actualizarla
                    string queryActualizar = "UPDATE Administrador SET ContraseñaHash = @NuevaPassword WHERE IDAdmin = @IDAdmin";
                    using (SqlCommand cmdActualizar = new SqlCommand(queryActualizar, con))
                    {
                        cmdActualizar.Parameters.AddWithValue("@NuevaPassword", nuevaContrasena); // ¡Temporal! Sin hash.
                        cmdActualizar.Parameters.AddWithValue("@IDAdmin", result);

                        cmdActualizar.ExecuteNonQuery();
                    }
                }
            }

            MostrarMensaje("¡Contraseña cambiada exitosamente!", "success");
            // Limpiar campos
            txtContrasenaActual.Text = "";
            txtNuevaContrasena.Text = "";
            txtConfirmarContrasena.Text = "";
        }

        private void MostrarMensaje(string mensaje, string tipo)
        {
            lblMensaje.Text = mensaje;
            lblMensaje.Visible = true;
            if (tipo == "success")
            {
                lblMensaje.CssClass = "alert-custom alert-success d-block mb-3";
            }
            else
            {
                lblMensaje.CssClass = "alert-custom alert-error d-block mb-3";
            }
        }
    }
}