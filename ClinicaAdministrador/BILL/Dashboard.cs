// BLL/Dashboard.cs
using System.Linq;
using ClinicaAdministrador.DAL;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace ClinicaAdministrador.BLL
{
    public static class Dashboard
    {
        // MÉTODO PARA OBTENER EL RESUMEN DEL DASHBOARD
        public static (int totalPacientes, int citasHoy, decimal ingresosMes, int alertasInventario) ObtenerResumen()
        {
            int totalPacientes = 0;
            int citasHoy = 0;
            decimal ingresosMes = 0;
            int alertasInventario = 0;

            // 1. Total de Pacientes
            using (SqlConnection con = DatabaseHelper.GetConnection())
            {
                using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Pacientes", con))
                {
                    con.Open();
                    totalPacientes = Convert.ToInt32(cmd.ExecuteScalar());
                }
            }

            // 2. Citas de Hoy
            // 2. Citas de Hoy
            using (SqlConnection con = DatabaseHelper.GetConnection())
            {
                using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Citas WHERE CAST(Fecha AS DATE) = CAST(GETDATE() AS DATE)", con))
                {
                    con.Open();
                    citasHoy = Convert.ToInt32(cmd.ExecuteScalar());
                }
            }


            // 3. Ingresos del Mes
            using (SqlConnection con = DatabaseHelper.GetConnection())
            {
                using (SqlCommand cmd = new SqlCommand("SELECT ISNULL(SUM(Total), 0) FROM Facturacion WHERE MONTH(Fecha) = MONTH(GETDATE()) AND YEAR(Fecha) = YEAR(GETDATE())", con))
                {
                    con.Open();
                    ingresosMes = Convert.ToDecimal(cmd.ExecuteScalar());
                }
            }

            // 4. Alertas de Inventario
            using (SqlConnection con = DatabaseHelper.GetConnection())
            {
                string query = @"SELECT COUNT(*) FROM Inventario 
                                 WHERE CantidadDisponible <= 10 OR (DATEDIFF(day, FechaVencimiento, GETDATE()) BETWEEN 0 AND 30)";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    con.Open();
                    alertasInventario = Convert.ToInt32(cmd.ExecuteScalar());
                }
            }

            return (totalPacientes, citasHoy, ingresosMes, alertasInventario);
        }

        // MÉTODO PARA OBTENER LAS PRÓXIMAS CITAS COMO HTML
        public static string ObtenerHtmlProximasCitas()
        {
            StringBuilder sb = new StringBuilder();
            using (SqlConnection con = DatabaseHelper.GetConnection())
            {
                string query = @"SELECT TOP 5 c.IDCita, p.NombreCompleto, c.Fecha, c.Hora 
                                 FROM Citas c
                                 INNER JOIN Pacientes p ON c.IDPaciente = p.IDPaciente
                                 WHERE c.Fecha >= CAST(GETDATE() AS DATE) AND c.Estado <> 'Realizada'
                                 ORDER BY c.Fecha, c.Hora";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (!reader.HasRows)
                        {
                            sb.Append("<p class='text-muted'>No hay próximas citas.</p>");
                        }
                        else
                        {
                            while (reader.Read())
                            {
                                sb.Append("<div class='list-group-item'>");
                                sb.Append($"<div class='d-flex w-100 justify-content-between'>");
                                sb.Append($"<h6 class='mb-1'>{reader["NombreCompleto"]}</h6>");
                                sb.Append($"<small>{Convert.ToDateTime(reader["Fecha"]).ToString("dd/MM")} a las {reader["Hora"]}</small>");
                                sb.Append($"</div></div>");
                            }
                        }
                    }
                }
            }
            return sb.ToString();
        }

        // MÉTODO PARA OBTENER LOS SERVICIOS POPULARES COMO HTML
        // MÉTODO PARA OBTENER LOS SERVICIOS POPULARES COMO HTML (VERSIÓN COMPATIBLE)
        // MÉTODO PARA OBTENER LOS SERVICIOS POPULARES (VERSIÓN RÁPIDA Y EFICIENTE)
        // DENTRO de BLL/Dashboard.cs, reemplaza solo este método:
        public static string ObtenerHtmlServiciosPopulares()
        {
            StringBuilder sb = new StringBuilder();
            using (SqlConnection con = DatabaseHelper.GetConnection())
            {
                // La consulta ahora es simple y eficiente gracias a la normalización
                string query = @"
            SELECT TOP 5 s.NombreServicio, COUNT(fs.IDFactura) AS VecesSolicitado
            FROM Facturacion_Servicios fs
            INNER JOIN Servicios s ON fs.IDServicio = s.IDServicio
            INNER JOIN Facturacion f ON fs.IDFactura = f.IDFactura
            WHERE MONTH(f.Fecha) = MONTH(GETDATE()) AND YEAR(f.Fecha) = YEAR(GETDATE())
            GROUP BY s.NombreServicio
            ORDER BY VecesSolicitado DESC";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (!reader.HasRows)
                        {
                            sb.Append("<p class='text-muted'>No hay servicios solicitados este mes.</p>");
                        }
                        else
                        {
                            while (reader.Read())
                            {
                                sb.Append("<div class='list-group-item'>");
                                sb.Append($"<div class='d-flex w-100 justify-content-between'>");
                                sb.Append($"<h6 class='mb-1'>{reader["NombreServicio"]}</h6>");
                                sb.Append($"<small class='text-muted'>{reader["VecesSolicitado"]} veces</small>");
                                sb.Append($"</div></div>");
                            }
                        }
                    }
                }
            }
            return sb.ToString();
        }
    }
}