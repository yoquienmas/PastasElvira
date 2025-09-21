using CapaEntidad;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;

namespace CapaDatos
{
        public class CD_Produccion
        {
            public string ValidarDisponibilidadMateriasPrimas(int idProducto, int cantidadProducida)
            {
                using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
                {
                    oconexion.Open();
                    using (var command = new SqlCommand())
                    {
                        command.Connection = oconexion;
                        command.CommandText = @"
                        SELECT mp.Nombre, dr.CantidadNecesaria * @Cantidad as CantidadRequerida, mp.CantidadDisponible
                        FROM DetalleReceta dr
                        INNER JOIN MateriaPrima mp ON dr.IdMateria = mp.IdMateria
                        WHERE dr.IdProducto = @IdProducto
                        AND mp.CantidadDisponible < (dr.CantidadNecesaria * @Cantidad)";
                        command.Parameters.AddWithValue("@IdProducto", idProducto);
                        command.Parameters.AddWithValue("@Cantidad", cantidadProducida);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                string errorMessage = "Materias primas insuficientes: ";
                                while (reader.Read())
                                {
                                    errorMessage += $"{reader["Nombre"]} (Necesita: {reader["CantidadRequerida"]}, Disponible: {reader["CantidadDisponible"]}), ";
                                }
                                return errorMessage.TrimEnd(',', ' ');
                            }
                        }
                    }
                }
                return string.Empty;
            }

            // MÉTODO REGISTRAR FALTANTE
            public int Registrar(Produccion produccion, out string mensaje)
            {
                mensaje = string.Empty;
                try
                {
                    using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
                    {
                        oconexion.Open();
                        using (var command = new SqlCommand())
                        {
                            command.Connection = oconexion;
                            command.CommandText = @"
                            INSERT INTO Produccion (IdProducto, CantidadProducida, FechaProduccion, Estado)
                            OUTPUT INSERTED.IdProduccion
                            VALUES (@IdProducto, @CantidadProducida, GETDATE(), 1)";
                            command.Parameters.AddWithValue("@IdProducto", produccion.IdProducto);
                            command.Parameters.AddWithValue("@CantidadProducida", produccion.CantidadProducida);

                            int idProduccion = Convert.ToInt32(command.ExecuteScalar());
                            mensaje = "Producción registrada exitosamente";
                            return idProduccion;
                        }
                    }
                }
                catch (Exception ex)
                {
                    mensaje = ex.Message;
                    return 0;
                }
            }

            // MÉTODO LISTAR FALTANTE
            public List<Produccion> Listar()
            {
                List<Produccion> producciones = new List<Produccion>();

                using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
                {
                    oconexion.Open();
                    using (var command = new SqlCommand())
                    {
                        command.Connection = oconexion;
                        command.CommandText = "SELECT * FROM Produccion ORDER BY FechaProduccion DESC";

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                producciones.Add(new Produccion
                                {
                                    IdProduccion = (int)reader["IdProduccion"],
                                    IdProducto = (int)reader["IdProducto"],
                                    CantidadProducida = (int)reader["CantidadProducida"],
                                    FechaProduccion = (DateTime)reader["FechaProduccion"],
                                });
                            }
                        }
                    }
                }
                return producciones;
            }

            public bool RegistrarDetalleProduccion(int idProduccion, int idMateria, decimal cantidadUtilizada)
            {
                using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
                {
                    oconexion.Open();
                    using (var command = new SqlCommand())
                    {
                        command.Connection = oconexion;
                        command.CommandText = @"
                        INSERT INTO DetalleProduccion (IdProduccion, IdMateria, CantidadUtilizada)
                        VALUES (@IdProduccion, @IdMateria, @CantidadUtilizada)";
                        command.Parameters.AddWithValue("@IdProduccion", idProduccion);
                        command.Parameters.AddWithValue("@IdMateria", idMateria);
                        command.Parameters.AddWithValue("@CantidadUtilizada", cantidadUtilizada);

                        return command.ExecuteNonQuery() > 0;
                    }
                }
            }

            public List<DetalleReceta> ObtenerRecetaProducto(int idProducto)
            {
                List<DetalleReceta> receta = new List<DetalleReceta>();

                using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
                {
                    oconexion.Open();
                    using (var command = new SqlCommand())
                    {
                        command.Connection = oconexion;
                        command.CommandText = "SELECT * FROM DetalleReceta WHERE IdProducto = @IdProducto";
                        command.Parameters.AddWithValue("@IdProducto", idProducto);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                receta.Add(new DetalleReceta
                                {
                                    IdProductoMateriaPrima = (int)reader["IdProductoMateriaPrima"],
                                    IdProducto = (int)reader["IdProducto"],
                                    IdMateria = (int)reader["IdMateria"],
                                    CantidadNecesaria = Convert.ToSingle(reader["CantidadNecesaria"])
                                });
                            }
                        }
                    }
                }
                return receta;
            }

            public bool AgregarMateriaPrimaAReceta(int idProducto, int idMateria, float cantidad)
            {
                using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
                {
                    oconexion.Open();
                    using (var command = new SqlCommand())
                    {
                        command.Connection = oconexion;
                        command.CommandText = @"
                        INSERT INTO DetalleReceta (IdProducto, IdMateria, CantidadNecesaria)
                        VALUES (@IdProducto, @IdMateria, @Cantidad)";
                        command.Parameters.AddWithValue("@IdProducto", idProducto);
                        command.Parameters.AddWithValue("@IdMateria", idMateria);
                        command.Parameters.AddWithValue("@Cantidad", cantidad);

                        return command.ExecuteNonQuery() > 0;
                    }
                }
            }

            public bool EliminarMateriaPrimaDeReceta(int idProductoMateriaPrima)
            {
                using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
                {
                    oconexion.Open();
                    using (var command = new SqlCommand())
                    {
                        command.Connection = oconexion;
                        command.CommandText = "DELETE FROM DetalleReceta WHERE IdProductoMateriaPrima = @Id";
                        command.Parameters.AddWithValue("@Id", idProductoMateriaPrima);

                        return command.ExecuteNonQuery() > 0;
                    }
                }
            }
        }
    }