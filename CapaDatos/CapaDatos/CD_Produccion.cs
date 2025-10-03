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
                        // ✅ CORREGIDO: Cambiar "NombreProducto" por "Nombre"
                        errorMessage += $"{reader["Nombre"]} (Necesita: {reader["CantidadRequerida"]}, Disponible: {reader["CantidadDisponible"]}), ";
                    }
                    return errorMessage.TrimEnd(',', ' ');
                }
            }
        }
    }
    return string.Empty;
}

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

                        // ✅ USAR SCOPE_IDENTITY()
                        command.CommandText = @"
                    INSERT INTO Produccion (IdProducto, CantidadProducida, FechaProduccion, Estado)
                    VALUES (@IdProducto, @CantidadProducida, GETDATE(), 1);
                    SELECT SCOPE_IDENTITY();";

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
                        command.CommandText = "UPDATE Produccion SET Estado = 0 WHERE IdProduccion = @IdProduccion";
                        command.Parameters.AddWithValue("@IdProduccion", idProduccion);

                        int result = command.ExecuteNonQuery();
                        if (result > 0)
                        {
                            mensaje = "Producción eliminada exitosamente";
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

        public Produccion ObtenerPorId(int idProduccion)
        {
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
                    pr.Nombre,
                    pr.Tipo
                FROM Produccion p
                INNER JOIN Producto pr ON p.IdProducto = pr.IdProducto
                WHERE p.IdProduccion = @IdProduccion AND p.Estado = 1";

                    command.Parameters.AddWithValue("@IdProduccion", idProduccion);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var produccion = new Produccion
                            {
                                IdProduccion = (int)reader["IdProduccion"],
                                IdProducto = (int)reader["IdProducto"],
                                CantidadProducida = (int)reader["CantidadProducida"],
                                FechaProduccion = (DateTime)reader["FechaProduccion"],
                                Estado = true
                            };

                            // Construir nombre del producto
                            string nombre = reader["Nombre"].ToString();
                            string tipo = reader["Tipo"].ToString();

                            if (string.IsNullOrEmpty(nombre))
                                produccion.NombreProducto = "Producto sin nombre";
                            else if (string.IsNullOrEmpty(tipo) || tipo == "NULL")
                                produccion.NombreProducto = nombre;
                            else
                                produccion.NombreProducto = $"{tipo} - {nombre}";

                            return produccion;
                        }
                    }
                }
            }
            return null;
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
                    // ✅ INCLUIR ESTADO PERO CON MANEJO SEGURO
                    command.CommandText = @"
                SELECT 
                    p.IdProduccion,
                    p.IdProducto,
                    p.CantidadProducida,
                    p.FechaProduccion,
                    p.Estado,
                    pr.Nombre,
                    pr.Tipo
                FROM Produccion p
                INNER JOIN Producto pr ON p.IdProducto = pr.IdProducto
                ORDER BY p.FechaProduccion DESC";

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var produccion = new Produccion
                            {
                                IdProduccion = (int)reader["IdProduccion"],
                                IdProducto = (int)reader["IdProducto"],
                                CantidadProducida = (int)reader["CantidadProducida"],
                                FechaProduccion = (DateTime)reader["FechaProduccion"]
                            };

                            // ✅ MANEJO SEGURO DEL CAMPO ESTADO
                            var estadoValue = reader["Estado"];
                            if (estadoValue != DBNull.Value)
                            {
                                if (estadoValue is bool)
                                    produccion.Estado = (bool)estadoValue;
                                else if (estadoValue is string estadoStr)
                                    produccion.Estado = estadoStr == "1" || estadoStr.ToLower() == "true";
                                else if (estadoValue is int estadoInt)
                                    produccion.Estado = estadoInt == 1;
                            }

                            // ✅ CONSTRUIR EL NombreProducto
                            string nombre = reader["Nombre"].ToString();
                            string tipo = reader["Tipo"].ToString();

                            if (string.IsNullOrEmpty(nombre))
                                produccion.NombreProducto = "Producto sin nombre";
                            else if (string.IsNullOrEmpty(tipo) || tipo == "NULL")
                                produccion.NombreProducto = nombre;
                            else
                                produccion.NombreProducto = $"{tipo} - {nombre}";

                            producciones.Add(produccion);
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