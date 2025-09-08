using System;
using System.Collections.Generic;
using System.Data;
using CapaEntidad;
using Microsoft.Data.SqlClient;


namespace CapaDatos
{
    public class CD_Alerta
    {
        public List<AlertaStock> ListarAlertas()
        {
            List<AlertaStock> lista = new List<AlertaStock>();

            using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
            {
                SqlCommand cmd = new SqlCommand(@"
                    SELECT a.IdAlerta, a.IdProducto, p.Nombre as NombreProducto, 
                           a.FechaAlerta, a.Mensaje, p.StockActual, p.StockMinimo, 'Producto' as Tipo
                    FROM AlertaStock a
                    INNER JOIN Producto p ON a.IdProducto = p.IdProducto
                    ORDER BY a.FechaAlerta DESC", oconexion);

                oconexion.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new AlertaStock()
                        {
                            IdAlerta = Convert.ToInt32(dr["IdAlerta"]),
                            IdProducto = Convert.ToInt32(dr["IdProducto"]),
                            NombreProducto = dr["NombreProducto"].ToString(),
                            FechaAlerta = Convert.ToDateTime(dr["FechaAlerta"]),
                            Mensaje = dr["Mensaje"].ToString(),
                            StockActual = Convert.ToInt32(dr["StockActual"]),
                            StockMinimo = Convert.ToInt32(dr["StockMinimo"]),
                            Tipo = dr["Tipo"].ToString()
                        });
                    }
                }
            }
            return lista;
        }

        public bool RegistrarAlerta(int idProducto, string mensaje)
        {
            try
            {
                using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
                {
                    SqlCommand cmd = new SqlCommand("INSERT INTO AlertaStock (IdProducto, FechaAlerta, Mensaje) VALUES (@idProducto, GETDATE(), @mensaje)", oconexion);
                    cmd.Parameters.AddWithValue("@idProducto", idProducto);
                    cmd.Parameters.AddWithValue("@mensaje", mensaje);

                    oconexion.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch
            {
                return false;
            }
        }

        public bool EliminarAlerta(int idAlerta)
        {
            try
            {
                using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
                {
                    SqlCommand cmd = new SqlCommand("DELETE FROM AlertaStock WHERE IdAlerta = @id", oconexion);
                    cmd.Parameters.AddWithValue("@id", idAlerta);

                    oconexion.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch
            {
                return false;
            }
        }

        public void VerificarYGenerarAlertas()
        {
            using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
            {
                oconexion.Open();

                // Verificar productos con stock bajo
                SqlCommand cmdProductos = new SqlCommand(@"
                    SELECT p.IdProducto, p.Nombre, p.StockActual, p.StockMinimo
                    FROM Producto p
                    WHERE p.StockActual <= p.StockMinimo 
                    AND p.Visible = 1
                    AND NOT EXISTS (
                        SELECT 1 FROM AlertaStock a 
                        WHERE a.IdProducto = p.IdProducto 
                        AND CAST(a.FechaAlerta AS DATE) = CAST(GETDATE() AS DATE)
                    )", oconexion);

                using (SqlDataReader dr = cmdProductos.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        int idProducto = Convert.ToInt32(dr["IdProducto"]);
                        string nombre = dr["Nombre"].ToString();
                        int stockActual = Convert.ToInt32(dr["StockActual"]);
                        int stockMinimo = Convert.ToInt32(dr["StockMinimo"]);

                        string mensaje = $"ALERTA: Producto '{nombre}' con stock bajo. Actual: {stockActual}, Mínimo: {stockMinimo}";

                        // Registrar alerta
                        RegistrarAlerta(idProducto, mensaje);
                    }
                }

                // Verificar materias primas con stock bajo (opcional)
                SqlCommand cmdMaterias = new SqlCommand(@"
                    SELECT m.IdMateria, m.Nombre, m.CantidadDisponible, m.StockMinimo
                    FROM MateriaPrima m
                    WHERE m.CantidadDisponible <= m.StockMinimo 
                    AND NOT EXISTS (
                        SELECT 1 FROM AlertaStock a 
                        WHERE a.IdProducto = m.IdMateria 
                        AND CAST(a.FechaAlerta AS DATE) = CAST(GETDATE() AS DATE)
                        AND a.Mensaje LIKE '%materia prima%'
                    )", oconexion);

                using (SqlDataReader dr = cmdMaterias.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        int idMateria = Convert.ToInt32(dr["IdMateria"]);
                        string nombre = dr["Nombre"].ToString();
                        float stockActual = Convert.ToSingle(dr["CantidadDisponible"]);
                        int stockMinimo = Convert.ToInt32(dr["StockMinimo"]);

                        string mensaje = $"ALERTA: Materia prima '{nombre}' con stock bajo. Actual: {stockActual}, Mínimo: {stockMinimo}";

                        // Registrar alerta (usamos IdProducto = -idMateria para diferenciar)
                        RegistrarAlerta(-idMateria, mensaje);
                    }
                }
            }
        }

        public int ObtenerCantidadAlertasPendientes()
        {
            using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
            {
                SqlCommand cmd = new SqlCommand(@"
                    SELECT COUNT(*) 
                    FROM AlertaStock a
                    INNER JOIN Producto p ON a.IdProducto = p.IdProducto
                    WHERE CAST(a.FechaAlerta AS DATE) = CAST(GETDATE() AS DATE)
                    AND p.StockActual <= p.StockMinimo", oconexion);

                oconexion.Open();
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }
    }
}