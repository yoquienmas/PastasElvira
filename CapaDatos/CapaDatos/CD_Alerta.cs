using CapaDatos;
using CapaEntidad;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;


    namespace CapaDatos
    {
        public class CD_Alerta
        {
            public List<AlertaStock> ListarAlertas()
            {
                List<AlertaStock> alertas = new List<AlertaStock>();

                using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
                {
                    oconexion.Open();
                    using (var command = new SqlCommand())
                    {
                        command.Connection = oconexion;
                        command.CommandText = "SELECT * FROM AlertaStock ORDER BY FechaAlerta DESC";
                        command.CommandType = CommandType.Text;

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                alertas.Add(new AlertaStock
                                {
                                    IdAlerta = (int)reader["IdAlerta"],
                                    IdProducto = (int)reader["IdProducto"],
                                    FechaAlerta = (DateTime)reader["FechaAlerta"],
                                    Mensaje = reader["Mensaje"].ToString()
                                });
                            }
                        }
                    }
                }

                return alertas;
            }

            public int ObtenerCantidadAlertasPendientes()
            {
                using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
                {
                    oconexion.Open();
                    using (var command = new SqlCommand())
                    {
                        command.Connection = oconexion;
                        command.CommandText = "SELECT COUNT(*) FROM AlertaStock WHERE CAST(FechaAlerta AS DATE) = CAST(GETDATE() AS DATE)";
                        command.CommandType = CommandType.Text;

                        return Convert.ToInt32(command.ExecuteScalar());
                    }
                }
            }

            public bool VerificarYGenerarAlertas()
            {
                try
                {
                    // Lógica para verificar y generar alertas
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            public bool EliminarAlerta(int idAlerta)
            {
                using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
                {
                    oconexion.Open();
                    using (var command = new SqlCommand())
                    {
                        command.Connection = oconexion;
                        command.CommandText = "DELETE FROM AlertaStock WHERE IdAlerta = @IdAlerta";
                        command.Parameters.AddWithValue("@IdAlerta", idAlerta);
                        command.CommandType = CommandType.Text;

                        return command.ExecuteNonQuery() > 0;
                    }
                }
            }

            public bool LimpiarAlertasAntiguas()
            {
                using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
                {
                    oconexion.Open();
                    using (var command = new SqlCommand())
                    {
                        command.Connection = oconexion;
                        command.CommandText = "DELETE FROM AlertaStock WHERE FechaAlerta < DATEADD(day, -7, GETDATE())";
                        command.CommandType = CommandType.Text;

                        return command.ExecuteNonQuery() > 0;
                    }
                }
            }

            public bool GenerarAlertaProducto(int idProducto, string nombre, int stockActual, int stockMinimo)
            {
                using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
                {
                    oconexion.Open();
                    using (var command = new SqlCommand())
                    {
                        command.Connection = oconexion;
                        command.CommandText = @"
                        INSERT INTO AlertaStock (IdProducto, FechaAlerta, Mensaje)
                        VALUES (@IdProducto, GETDATE(), 
                                'Stock bajo del producto: ' + @Nombre + '. Actual: ' + CAST(@StockActual AS VARCHAR) + ', Mínimo: ' + CAST(@StockMinimo AS VARCHAR))";
                        command.Parameters.AddWithValue("@IdProducto", idProducto);
                        command.Parameters.AddWithValue("@Nombre", nombre);
                        command.Parameters.AddWithValue("@StockActual", stockActual);
                        command.Parameters.AddWithValue("@StockMinimo", stockMinimo);
                        command.CommandType = CommandType.Text;

                        return command.ExecuteNonQuery() > 0;
                    }
                }
            }

            public bool GenerarAlertaMateriaPrima(int idMateria, string nombre, float cantidadActual, int stockMinimo)
            {
                using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
                {
                    oconexion.Open();
                    using (var command = new SqlCommand())
                    {
                        command.Connection = oconexion;
                        command.CommandText = @"
                        INSERT INTO AlertaStock (IdProducto, FechaAlerta, Mensaje)
                        VALUES (0, GETDATE(), 
                                'Stock bajo de materia prima: ' + @Nombre + '. Actual: ' + CAST(@CantidadActual AS VARCHAR) + ', Mínimo: ' + CAST(@StockMinimo AS VARCHAR))";
                        command.Parameters.AddWithValue("@Nombre", nombre);
                        command.Parameters.AddWithValue("@CantidadActual", cantidadActual);
                        command.Parameters.AddWithValue("@StockMinimo", stockMinimo);
                        command.CommandType = CommandType.Text;

                        return command.ExecuteNonQuery() > 0;
                    }
                }
            }
        }
    }