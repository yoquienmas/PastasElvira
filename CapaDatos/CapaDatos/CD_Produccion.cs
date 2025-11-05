using CapaEntidad;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;

namespace CapaDatos
{
    public class CD_Produccion
    {
        // ✅ PRODUCCIÓN DIRECTA - SIN VALIDACIÓN DE MATERIAS PRIMAS
        public int Registrar(Produccion produccion, out string mensaje)
        {
            mensaje = string.Empty;
            int idProduccion = 0;

            try
            {
                using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
                {
                    oconexion.Open();

                    using (SqlTransaction transaction = oconexion.BeginTransaction())
                    {
                        try
                        {
                            // 1. INSERTAR EN PRODUCCION
                            using (var command = new SqlCommand())
                            {
                                command.Connection = oconexion;
                                command.Transaction = transaction;

                                command.CommandText = @"
                                    INSERT INTO Produccion (IdProducto, CantidadProducida, FechaProduccion, Estado)
                                    VALUES (@IdProducto, @CantidadProducida, GETDATE(), 1);
                                    SELECT SCOPE_IDENTITY();";

                                command.Parameters.AddWithValue("@IdProducto", produccion.IdProducto);
                                command.Parameters.AddWithValue("@CantidadProducida", produccion.CantidadProducida);

                                // Ejecutar y obtener el ID
                                idProduccion = Convert.ToInt32(command.ExecuteScalar());
                            }

                            // 2. ACTUALIZAR STOCK DEL PRODUCTO
                            using (var command = new SqlCommand())
                            {
                                command.Connection = oconexion;
                                command.Transaction = transaction;

                                command.CommandText = @"
                                    UPDATE Producto 
                                    SET StockActual = StockActual + @CantidadProducida 
                                    WHERE IdProducto = @IdProducto";

                                command.Parameters.AddWithValue("@IdProducto", produccion.IdProducto);
                                command.Parameters.AddWithValue("@CantidadProducida", produccion.CantidadProducida);

                                int filasAfectadas = command.ExecuteNonQuery();

                                if (filasAfectadas == 0)
                                {
                                    throw new Exception("No se pudo actualizar el stock del producto");
                                }
                            }

                            // 3. CONFIRMAR TRANSACCIÓN
                            transaction.Commit();

                            mensaje = $"✅ Producción registrada exitosamente. ID: {idProduccion}";
                            return idProduccion;
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            throw new Exception($"Error en transacción: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                mensaje = $"❌ Error al registrar producción: {ex.Message}";
                return 0;
            }
        }

        public bool Actualizar(Produccion produccion, out string mensaje)
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

                        // Obtener la cantidad anterior
                        int cantidadAnterior = 0;
                        var cmdSelect = new SqlCommand(
                            "SELECT CantidadProducida FROM Produccion WHERE IdProduccion = @IdProduccion",
                            oconexion);
                        cmdSelect.Parameters.AddWithValue("@IdProduccion", produccion.IdProduccion);
                        var resultado = cmdSelect.ExecuteScalar();
                        if (resultado != null)
                            cantidadAnterior = Convert.ToInt32(resultado);

                        // Actualizar producción
                        command.CommandText = @"
                            UPDATE Produccion 
                            SET IdProducto = @IdProducto, 
                                CantidadProducida = @CantidadProducida,
                                FechaProduccion = GETDATE()
                            WHERE IdProduccion = @IdProduccion AND Estado = 1";

                        command.Parameters.AddWithValue("@IdProduccion", produccion.IdProduccion);
                        command.Parameters.AddWithValue("@IdProducto", produccion.IdProducto);
                        command.Parameters.AddWithValue("@CantidadProducida", produccion.CantidadProducida);

                        int result = command.ExecuteNonQuery();

                        if (result > 0)
                        {
                            // ✅ ACTUALIZAR STOCK (nueva cantidad - anterior)
                            int diferencia = produccion.CantidadProducida - cantidadAnterior;
                            if (diferencia != 0)
                            {
                                command.CommandText = @"
                                    UPDATE Producto 
                                    SET StockActual = StockActual + @Diferencia 
                                    WHERE IdProducto = @IdProducto";
                                command.Parameters.Clear();
                                command.Parameters.AddWithValue("@IdProducto", produccion.IdProducto);
                                command.Parameters.AddWithValue("@Diferencia", diferencia);
                                command.ExecuteNonQuery();
                            }

                            mensaje = "Producción actualizada exitosamente";
                            return true;
                        }
                        else
                        {
                            mensaje = "No se pudo actualizar la producción";
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                mensaje = ex.Message;
                return false;
            }
        }

        public bool Eliminar(int idProduccion, out string mensaje)
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

                        // Obtener datos antes de eliminar
                        var cmdSelect = new SqlCommand(
                            "SELECT IdProducto, CantidadProducida FROM Produccion WHERE IdProduccion = @IdProduccion",
                            oconexion);
                        cmdSelect.Parameters.AddWithValue("@IdProduccion", idProduccion);
                        var reader = cmdSelect.ExecuteReader();

                        int idProducto = 0;
                        int cantidad = 0;
                        if (reader.Read())
                        {
                            idProducto = reader.GetInt32(0);
                            cantidad = reader.GetInt32(1);
                        }
                        reader.Close();

                        // Eliminar (desactivar) producción
                        command.CommandText = "UPDATE Produccion SET Estado = 0 WHERE IdProduccion = @IdProduccion";
                        command.Parameters.AddWithValue("@IdProduccion", idProduccion);

                        int result = command.ExecuteNonQuery();

                        if (result > 0)
                        {
                            // ✅ REVERTIR STOCK
                            command.CommandText = @"
                                UPDATE Producto 
                                SET StockActual = StockActual - @Cantidad 
                                WHERE IdProducto = @IdProducto";
                            command.Parameters.Clear();
                            command.Parameters.AddWithValue("@IdProducto", idProducto);
                            command.Parameters.AddWithValue("@Cantidad", cantidad);
                            command.ExecuteNonQuery();

                            mensaje = "Producción eliminada y stock revertido exitosamente";
                            return true;
                        }
                        else
                        {
                            mensaje = "No se pudo eliminar la producción";
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                mensaje = ex.Message;
                return false;
            }
        }

        public List<Produccion> Listar()
        {
            List<Produccion> producciones = new List<Produccion>();

            using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
            {
                oconexion.Open();
                using (var command = new SqlCommand())
                {
                    command.Connection = oconexion;
                    command.CommandText = @"
                        SELECT 
                            p.IdProduccion,
                            p.IdProducto,
                            p.CantidadProducida,
                            p.FechaProduccion,
                            p.Estado,
                            pr.Nombre
                        FROM Produccion p
                        INNER JOIN Producto pr ON p.IdProducto = pr.IdProducto
                        WHERE p.Estado = 1
                        ORDER BY p.FechaProduccion DESC";

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
                                Estado = true,
                                NombreProducto = reader["Nombre"].ToString() ?? "Producto sin nombre"
                            });
                        }
                    }
                }
            }
            return producciones;
        }
    }
}