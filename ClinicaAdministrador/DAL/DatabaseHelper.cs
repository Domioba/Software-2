using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Data.SqlClient;

namespace ClinicaAdministrador.DAL
{
    public static class DatabaseHelper
    {
        // Este método obtiene la conexión a la base de datos usando el nombre que definimos en Web.config
        public static SqlConnection GetConnection()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            return new SqlConnection(connectionString);
        }
    }
}