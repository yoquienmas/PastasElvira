using System;
using System.Collections.Generic;
using System.Data;
using System.Linq; // Agrega esta línea para usar LINQ
using CapaEntidad;
using Microsoft.Data.SqlClient;

namespace CapaDatos
{
    public class CD_Producto
    {
        public List<Producto> Listar()
        {
            List<Producto> productos = new List<Producto>();

            using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
            {
                try
                {
                    oconexion.Open();
                    SqlCommand comando = new SqlCommand("SELECT IdProducto, Nombre, PrecioVenta, StockActual FROM Producto", oconexion);

                    using (SqlDataReader reader = comando.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            productos.Add(new Producto
                            {
                                IdProducto = (int)reader["IdProducto"],
                                Nombre = reader["Nombre"].ToString(),
                                PrecioVenta = SafeConvertToDecimal(reader["PrecioVenta"]),
                                StockActual = (int)reader["StockActual"]
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Error al listar productos: " + ex.Message);
                }
            }

            return productos;
        }

        // Agrega este método helper en la clase CD_Producto
        private decimal SafeConvertToDecimal(object value)
        {
            if (value == null || value == DBNull.Value)
                return 0;

            try
            {
                return Convert.ToDecimal(value);
            }
            catch
            {
                return 0;
            }
        }

        public int Registrar(Producto producto, out string mensaje)
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
                    INSERT INTO Producto (Nombre, Tipo, PrecioVenta, CostoProduccion, 
                                        MargenGanancia, StockActual, StockMinimo, Visible)
                    VALUES (@Nombre, @Tipo, @PrecioVenta, @CostoProduccion, 
                            @MargenGanancia, @StockActual, @StockMinimo, @Visible);
                    SELECT SCOPE_IDENTITY();";

                        command.Parameters.AddWithValue("@Nombre", producto.Nombre);
                        command.Parameters.AddWithValue("@Tipo", producto.Tipo);
                        command.Parameters.AddWithValue("@PrecioVenta", producto.PrecioVenta);
                        command.Parameters.AddWithValue("@CostoProduccion", producto.CostoProduccion);
                        command.Parameters.AddWithValue("@MargenGanancia", producto.MargenGanancia);
                        command.Parameters.AddWithValue("@StockActual", producto.StockActual);
                        command.Parameters.AddWithValue("@StockMinimo", producto.StockMinimo);
                        command.Parameters.AddWithValue("@Visible", producto.Visible);

                        // Ejecutar y obtener el ID del nuevo producto
                        int idGenerado = Convert.ToInt32(command.ExecuteScalar());

                        mensaje = "Producto registrado correctamente";
                        return idGenerado;
                    }
                }
            }
            catch (Exception ex)
            {
                mensaje = ex.Message;
                return 0; // Devuelve 0 en caso de error
            }
        }
        public bool Editar(Producto producto, out string mensaje)
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
                            UPDATE Producto 
                            SET Nombre = @Nombre, Tipo = @Tipo, PrecioVenta = @PrecioVenta, 
                                CostoProduccion = @CostoProduccion, MargenGanancia = @MargenGanancia,
                                StockActual = @StockActual, StockMinimo = @StockMinimo, Visible = @Visible
                            WHERE IdProducto = @IdProducto";
                            command.Parameters.AddWithValue("@IdProducto", producto.IdProducto);
                            command.Parameters.AddWithValue("@Nombre", producto.Nombre);
                            command.Parameters.AddWithValue("@Tipo", producto.Tipo);
                            command.Parameters.AddWithValue("@PrecioVenta", producto.PrecioVenta);
                            command.Parameters.AddWithValue("@CostoProduccion", producto.CostoProduccion);
                            command.Parameters.AddWithValue("@MargenGanancia", producto.MargenGanancia);
                            command.Parameters.AddWithValue("@StockActual", producto.StockActual);
                            command.Parameters.AddWithValue("@StockMinimo", producto.StockMinimo);
                            command.Parameters.AddWithValue("@Visible", producto.Visible);

                            int result = command.ExecuteNonQuery();
                            mensaje = "Producto actualizado correctamente";
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

            public bool Eliminar(int idProducto, out string mensaje)
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
                            command.CommandText = "DELETE FROM Producto WHERE IdProducto = @IdProducto";
                            command.Parameters.AddWithValue("@IdProducto", idProducto);

                            int result = command.ExecuteNonQuery();
                            mensaje = "Producto eliminado correctamente";
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

            // MÉTODOS CORREGIDOS (SQL y nombre)
            public List<string> ListarTiposProducto()
            {
                List<string> listaTipos = new List<string>();

                try
                {
                    using (SqlConnection conexion = new SqlConnection(Conexion.cadena))
                    {
                        // Consulta para obtener los tipos únicos de la tabla Producto, filtrando por la columna 'Visible'
                        string query = "SELECT DISTINCT Tipo FROM Producto WHERE Visible = 1";

                        SqlCommand cmd = new SqlCommand(query, conexion);
                        cmd.CommandType = CommandType.Text;

                        conexion.Open();
                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                listaTipos.Add(dr["Tipo"].ToString());
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    listaTipos = new List<string>();
                }

                return listaTipos;
            }

            // MÉTODOS NUEVOS (ya implementados anteriormente)
            public List<Producto> ObtenerProductosParaVerificar()
            {
                List<Producto> productos = new List<Producto>();

                using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
                {
                    oconexion.Open();
                    using (var command = new SqlCommand())
                    {
                        command.Connection = oconexion;
                        command.CommandText = "SELECT * FROM Producto WHERE Visible = 1";
                        command.CommandType = CommandType.Text;

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                productos.Add(new Producto
                                {
                                    IdProducto = (int)reader["IdProducto"],
                                    Nombre = reader["Nombre"].ToString(),
                                    Tipo = reader["Tipo"].ToString(),
                                    StockActual = (int)reader["StockActual"],
                                    StockMinimo = (int)reader["StockMinimo"]
                                });
                            }
                        }
                    }
                }
                return productos;
            }

            public bool ActualizarCostoProducto(int idProducto, decimal nuevoCosto)
            {
                using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
                {
                    oconexion.Open();
                    using (var command = new SqlCommand())
                    {
                        command.Connection = oconexion;
                        command.CommandText = "UPDATE Producto SET CostoProduccion = @Costo WHERE IdProducto = @IdProducto";
                        command.Parameters.AddWithValue("@Costo", nuevoCosto);
                        command.Parameters.AddWithValue("@IdProducto", idProducto);
                        command.CommandType = CommandType.Text;

                        return command.ExecuteNonQuery() > 0;
                    }
                }
            }

            public bool ActualizarStockProducto(int idProducto, int cantidad)
            {
                using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
                {
                    oconexion.Open();
                    using (var command = new SqlCommand())
                    {
                        command.Connection = oconexion;
                        command.CommandText = "UPDATE Producto SET StockActual = StockActual + @Cantidad WHERE IdProducto = @IdProducto";
                        command.Parameters.AddWithValue("@Cantidad", cantidad);
                        command.Parameters.AddWithValue("@IdProducto", idProducto);
                        command.CommandType = CommandType.Text;

                        return command.ExecuteNonQuery() > 0;
                    }
                }
            }

            public decimal CalcularCostoMateriasPrimas(int idProducto)
            {
                decimal costoTotal = 0;

                using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
                {
                    oconexion.Open();
                    using (var command = new SqlCommand())
                    {
                        command.Connection = oconexion;
                        command.CommandText = @"
                        SELECT SUM(mp.PrecioUnitario * dr.CantidadNecesaria) as CostoTotal
                        FROM DetalleReceta dr
                        INNER JOIN MateriaPrima mp ON dr.IdMateria = mp.IdMateria
                        WHERE dr.IdProducto = @IdProducto";
                        command.Parameters.AddWithValue("@IdProducto", idProducto);

                        var result = command.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            costoTotal = Convert.ToDecimal(result);
                        }
                    }
                }
                return costoTotal;
            }

            public decimal CalcularCostosFijosUnitarios()
            {
                using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
                {
                    oconexion.Open();
                    using (var command = new SqlCommand())
                    {
                        command.Connection = oconexion;
                        command.CommandText = @"
                        DECLARE @TotalCostosFijos DECIMAL(10,2) = (SELECT ISNULL(SUM(Monto), 0) FROM CostosFijos WHERE Activo = 1)
                        DECLARE @TotalProductos INT = (SELECT COUNT(*) FROM Producto WHERE Visible = 1)
                        SELECT CASE WHEN @TotalProductos > 0 THEN @TotalCostosFijos / @TotalProductos ELSE 0 END";
                        return Convert.ToDecimal(command.ExecuteScalar());
                    }
                }
            }

            public Producto ObtenerProductoPorId(int idProducto)
            {
                using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
                {
                    oconexion.Open();
                    using (var command = new SqlCommand())
                    {
                        command.Connection = oconexion;
                        command.CommandText = "SELECT * FROM Producto WHERE IdProducto = @IdProducto";
                        command.Parameters.AddWithValue("@IdProducto", idProducto);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new Producto
                                {
                                    IdProducto = (int)reader["IdProducto"],
                                    Nombre = reader["Nombre"].ToString(),
                                    Tipo = reader["Tipo"].ToString(),
                                    PrecioVenta = Convert.ToDecimal(reader["PrecioVenta"]),
                                    CostoProduccion = Convert.ToDecimal(reader["CostoProduccion"]),
                                    MargenGanancia = Convert.ToDecimal(reader["MargenGanancia"]),
                                    StockActual = (int)reader["StockActual"],
                                    StockMinimo = (int)reader["StockMinimo"],
                                    Visible = (bool)reader["Visible"]
                                };
                            }
                        }
                    }
                }
                return null;
            }
        }
    }