<%@ Page Title="Iniciar Sesión" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="ClinicaAdministrador.Login" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <!-- Google Fonts -->
    <link href="https://fonts.googleapis.com/css2?family=Poppins:wght@300;400;500;600;700&family=Playfair+Display:wght@600;700&display=swap" rel="stylesheet">
    
    <!-- Font Awesome -->
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css">
    
    <!-- CSS del login -->
    <link href="css/login.css" rel="stylesheet" type="text/css" />
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="login-page">
        <!-- Columna Izquierda - Formulario -->
        <div class="login-left">
            <div class="login-form-container">
                <!-- Logo -->
                <div class="login-logo">
                    <div class="logo-icon">
                        <i class="fas fa-stethoscope"></i>
                    </div>
                    <h1>Consultorio Médico</h1>
                </div>

                <!-- Encabezado -->
                <div class="login-header">
                    <h2>Bienvenido Dr. Jorge Ibarra</h2>
                    <p>Ingrese sus credenciales para acceder al sistema</p>
                </div>

                <!-- Formulario -->
                <div class="login-form">
                    <!-- Mensaje de error -->
                    <asp:Label ID="lblError" runat="server" CssClass="error-message" Visible="false"></asp:Label>

                    <!-- Campo Usuario -->
                    <div class="form-group">
                        <label class="form-label">Usuario</label>
                        <div class="input-with-icon">
                            <i class="fas fa-user input-icon"></i>
                            <asp:TextBox ID="txtUsuario" runat="server" CssClass="form-input" placeholder="Usuario o Correo Electrónico"></asp:TextBox>
                        </div>
                    </div>

                    <!-- Campo Contraseña -->
                    <div class="form-group">
                        <label class="form-label">Contraseña</label>
                        <div class="input-with-icon">
                            <i class="fas fa-lock input-icon"></i>
                            <asp:TextBox ID="txtPassword" runat="server" TextMode="Password" CssClass="form-input" placeholder="Contraseña"></asp:TextBox>
                            <i class="fas fa-eye-slash password-toggle" id="passwordToggle"></i>
                        </div>
                    </div>

                   
                        

                    <!-- Botón de login -->
                    <asp:Button ID="btnLogin" runat="server" Text="Iniciar Sesión" CssClass="login-btn" OnClick="btnLogin_Click" />
                </div>
            </div>
        </div>

        <!-- Columna Derecha - Información -->
        <div class="login-right">
            <!-- Patrón de fondo -->
            <div class="background-pattern"></div>
            
            <!-- Contenido -->
            <div class="right-content">
                <h1>Clinica Medica y Estetica</h1>
                <p class="subtitle">Gestione su consulta de manera eficiente con nuestra plataforma diseñada para profesionales de la salud.</p>
                
                <!-- Características -->
                <div class="features-grid">
                    <div class="feature-card">
                        <div class="feature-icon">
                            <i class="fas fa-chart-line"></i>
                        </div>
                        <div class="feature-title">Historiales</div>
                        <div class="feature-desc">Acceso completo al historial de pacientes</div>
                    </div>
                    
                    <div class="feature-card">
                        <div class="feature-icon">
                            <i class="fas fa-calendar-alt"></i>
                        </div>
                        <div class="feature-title">Citas</div>
                        <div class="feature-desc">Gestión inteligente de agenda</div>
                    </div>
                    
                    <div class="feature-card">
                        <div class="feature-icon">
                            <i class="fas fa-prescription"></i>
                        </div>
                        <div class="feature-title">Recetas</div>
                        <div class="feature-desc">Prescripciones digitales</div>
                    </div>
                    
                    <div class="feature-card">
                        <div class="feature-icon">
                            <i class="fas fa-shield-alt"></i>
                        </div>
                        <div class="feature-title">Seguridad</div>
                        <div class="feature-desc">Datos protegidos y encriptados</div>
                    </div>
                </div>
            </div>
        </div>

        <!-- Footer -->
        <footer class="login-footer">
            <p>&copy; 2025 Clinica Medica y Estetica Dr. Jorge Ibarra. Todos los derechos reservados.</p>
        </footer>
    </div>

    <!-- JavaScript -->
    <script>
        document.addEventListener('DOMContentLoaded', function () {
            // Toggle de contraseña
            const passwordToggle = document.getElementById('passwordToggle');
            const passwordInput = document.getElementById('<%= txtPassword.ClientID %>');

            if (passwordToggle && passwordInput) {
                passwordToggle.addEventListener('click', function () {
                    if (passwordInput.type === 'password') {
                        passwordInput.type = 'text';
                        passwordToggle.classList.remove('fa-eye-slash');
                        passwordToggle.classList.add('fa-eye');
                    } else {
                        passwordInput.type = 'password';
                        passwordToggle.classList.remove('fa-eye');
                        passwordToggle.classList.add('fa-eye-slash');
                    }
                });
            }

            // Checkbox "Recuérdame"
            const rememberMe = document.getElementById('rememberMe');
            if (rememberMe) {
                rememberMe.addEventListener('click', function () {
                    const checkbox = this.querySelector('.remember-checkbox');
                    checkbox.classList.toggle('checked');
                });
            }

            // Limpiar error al escribir
            const usuarioInput = document.getElementById('<%= txtUsuario.ClientID %>');
            const errorLabel = document.getElementById('<%= lblError.ClientID %>');
            
            if (usuarioInput && errorLabel) {
                usuarioInput.addEventListener('input', function() {
                    errorLabel.style.display = 'none';
                });
            }

            const passwordInputField = document.getElementById('<%= txtPassword.ClientID %>');
            if (passwordInputField && errorLabel) {
                passwordInputField.addEventListener('input', function () {
                    errorLabel.style.display = 'none';
                });
            }
        });
    </script>
</asp:Content>