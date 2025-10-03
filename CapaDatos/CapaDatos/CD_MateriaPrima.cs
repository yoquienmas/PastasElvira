using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using CapaEntidad;

namespace CapaDatos
{
    public class CD_MateriaPrima
    {
        private SqlConnection conexion = new SqlConnection(Conexion.cadena);

        // MÉTODO NUEVO: Verificar si existe materia prima con el mismo nombre
        public bool ExisteMateriaPrima(string nombre, int idExcluir = 0)
        {
            using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
            {
                oconexion.Open();
                using (var command = new SqlCommand())
                {
                    command.Connection = oconexion;

                    if (idExcluir == 0)
                    {
                        command.CommandText = "SELECT COUNT(*) FROM MateriaPrima WHERE Nombre = @Nombre";
                        command.Parameters.AddWithValue("@Nombre", nombre);
                    }
                    else
                    {
                        command.CommandText = "SELECT COUNT(*) FROM MateriaPrima WHERE Nombre = @Nombre AND IdMateria != @IdExcluir";
                        command.Parameters.AddWithValue("@Nombre", nombre);
                        command.Parameters.AddWithValue("@IdExcluir", idExcluir);
                    }

                    int count = Convert.ToInt32(command.ExecuteScalar());
                    return count > 0;
                }
            }
        }

        // MÉTODOS BÁSICOS
        public List<MateriaPrima> Listar()
        {
            List<MateriaPrima> materias = new List<MateriaPrima>();

            using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
            {
                oconexion.Open();
                using (var command = new SqlCommand())
                {
                    command.Connection = oconexion;
                    command.CommandText = "SELECT * FROM MateriaPrima";
                    command.CommandType = CommandType.Text;

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            materias.Add(new MateriaPrima
                            {
                                IdMateria = (int)reader["IdMateria"],
                                Nombre = reader["Nombre"].ToString(),
                                Unidad = reader["Unidad"].ToString(),
                                CantidadDisponible = Convert.ToSingle(reader["CantidadDisponible"]),
                                StockMinimo = (int)reader["StockMinimo"],
                                PrecioUnitario = Convert.ToDecimal(reader["PrecioUnitario"])
                            });
                        }
                    }
                }
            }

            return materias;
        }

        public int Registrar(MateriaPrima materia, out string mensaje)
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
                            INSERT INTO MateriaPrima (Nombre, Unidad, CantidadDisponible, StockMinimo, PrecioUnitario)
                            VALUES (@Nombre, @Unidad, @CantidadDisponible, @StockMinimo, @PrecioUnitario);
                            SELECT SCOPE_IDENTITY();";
                        command.Parameters.AddWithValue("@Nombre", materia.Nombre);
                        command.Parameters.AddWithValue("@Unidad", materia.Unidad);
                        command.Parameters.AddWithValue("@CantidadDisponible", materia.CantidadDisponible);
                        command.Parameters.AddWithValue("@StockMinimo", materia.StockMinimo);
                        command.Parameters.AddWithValue("@PrecioUnitario", materia.PrecioUnitario);

                        int id = Convert.ToInt32(command.ExecuteScalar());
                        mensaje = "Materia prima registrada correctamente";
                        return id;
                    }
                }
            }
            catch (Exception ex)
            {
                mensaje = ex.Message;
                return 0;
            }
        }

        public bool Editar(MateriaPrima materia, out string mensaje)
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
                            UPDATE MateriaPrima 
                            SET Nombre = @Nombre, Unidad = @Unidad, 
                                CantidadDisponible = @CantidadDisponible, 
                                StockMinimo = @StockMinimo, 
                                PrecioUnitario = @PrecioUnitario
                            WHERE IdMateria = @IdMateria";
                        command.Parameters.AddWithValue("@IdMateria", materia.IdMateria);
                        command.Parameters.AddWithValue("@Nombre", materia.Nombre);
                        command.Parameters.AddWithValue("@Unidad", materia.Unidad);
                        command.Parameters.AddWithValue("@CantidadDisponible", materia.CantidadDisponible);
                        command.Parameters.AddWithValue("@StockMinimo", materia.StockMinimo);
                        command.Parameters.AddWithValue("@PrecioUnitario", materia.PrecioUnitario);

                        int result = command.ExecuteNonQuery();
                        mensaje = "Materia prima actualizada correctamente";
                        return result > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                mensaje = ex.Message;
                return false;
            }
        }

        public bool Eliminar(int idMateria, out string mensaje)
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
                        command.CommandText = "DELETE FROM MateriaPrima WHERE IdMateria = @IdMateria";
                        command.Parameters.AddWithValue("@IdMateria", idMateria);

                        int result = command.ExecuteNonQuery();
                        mensaje = "Materia prima eliminada correctamente";
                        return result > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                mensaje = ex.Message;
                return false;
            }
        }

        // MÉTODOS ADICIONALES
        public List<MateriaPrima> ObtenerMateriasPrimasParaVerificar()
        {
            List<MateriaPrima> materias = new List<MateriaPrima>();

            using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
            {
                oconexion.Open();
                using (var command = new SqlCommand())
                {
                    command.Connection = oconexion;
                    command.CommandText = "SELECT * FROM MateriaPrima WHERE CantidadDisponible <= StockMinimo";

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            materias.Add(new MateriaPrima
                            {
                                IdMateria = (int)reader["IdMateria"],
                                Nombre = reader["Nombre"].ToString(),
                                Unidad = reader["Unidad"].ToString(),
                                CantidadDisponible = Convert.ToSingle(reader["CantidadDisponible"]),
                                StockMinimo = (int)reader["StockMinimo"],
                                PrecioUnitario = Convert.ToDecimal(reader["PrecioUnitario"])
                            });
                        }
                    }
                }
            }
            return materias;
        }

        public bool ActualizarStock(int idMateria, float cantidad)
        {
            using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
            {
                oconexion.Open();
                using (var command = new SqlCommand())
                {
                    command.Connection = oconexion;
                    command.CommandText = "UPDATE MateriaPrima SET CantidadDisponible = CantidadDisponible + @Cantidad WHERE IdMateria = @IdMateria";
                    command.Parameters.AddWithValue("@IdMateria", idMateria);
                    command.Parameters.AddWithValue("@Cantidad", cantidad);

                    return command.ExecuteNonQuery() > 0;
                }
            }
        }

        public List<int> ObtenerProductosConMateriaPrima(int idMateria)
        {
            List<int> productos = new List<int>();

            using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
            {
                oconexion.Open();
                using (var command = new SqlCommand())
                {
                    command.Connection = oconexion;
                    command.CommandText = "SELECT DISTINCT IdProducto FROM DetalleReceta WHERE IdMateria = @IdMateria";
                    command.Parameters.AddWithValue("@IdMateria", idMateria);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            productos.Add((int)reader["IdProducto"]);
                        }
                    }
                }
            }
            return productos;
        }
    }
}