using System;
using System.Data.SqlClient;
using System.Web.UI;
using ClinicaAdministrador.DAL;
using BCrypt.Net;

namespace ClinicaAdministrador
{
    public partial class Login : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Si el usuario ya está logueado, lo redirigimos al panel principal
                if (Session["Usuario"] != null)
                {
                    Response.Redirect("Default.aspx");
                }
            }
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            string usuario = txtUsuario.Text.Trim();
            string password = txtPassword.Text.Trim();

            using (SqlConnection con = DatabaseHelper.GetConnection())
            {
                // 2. LA CONSULTA AHORA SOLO BUSCA POR USUARIO Y TRAE EL HASH ALMACENADO
                
                string query = "SELECT IDAdmin, NombreCompleto, ContraseñaHash FROM Administrador WHERE Usuario = @Usuario";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Usuario", usuario);
                    // Ya no se necesita el parámetro @Password aquí

                    try
                    {
                        con.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read()) // Si se encontró un usuario con ese nombre
                            {
                                // 3. OBTENEMOS EL HASH GUARDADO EN LA BASE DE DATOS
                                string storedHash = reader["ContraseñaHash"].ToString();

                                // 4. VERIFICAMOS SI LA CONTRASEÑA ESCRITA COINCIDE CON EL HASH
                                // BCrypt.Verify se encarga de todo el proceso de comparación de forma segura.
                                if (BCrypt.Net.BCrypt.Verify(password, storedHash))
                                {
                                    // Las credenciales son correctas, iniciamos sesión
                                    Session["IDAdmin"] = reader["IDAdmin"];
                                    Session["NombreAdmin"] = reader["NombreCompleto"];
                                    Session["Usuario"] = usuario;
                                    Response.Redirect("Default.aspx");
                                }
                                else
                                {
                                    // La contraseña no coincide con el hash almacenado
                                    lblError.Text = "Usuario o contraseña incorrectos.";
                                    lblError.Visible = true;
                                }
                            }
                            else
                            {
                               
                                lblError.Text = "Usuario o contraseña incorrectos.";
                                lblError.Visible = true;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                      
                        lblError.Text = "Ocurrió un error en el servidor. Inténtelo más tarde.";
                        lblError.Visible = true;
                    }
                }
            }
        }
    }
}