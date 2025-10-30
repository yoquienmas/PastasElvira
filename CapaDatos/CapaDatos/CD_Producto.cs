using CapaEntidad;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;

namespace CapaDatos
{
    public class CD_Producto
    {
        public List<Producto> Listar()
        {
            List<Producto> lista = new List<Producto>();

            using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
            {
                try
                {
                    // QUERY CORREGIDA - sin errores de sintaxis
                    string query = @"SELECT 
                    p.IdProducto, 
                    p.Nombre,
                    t.Descripcion AS Tipo, 
                    s.Descripcion AS Sabor, 
                    p.PrecioVenta, 
                    p.Visible, 
                    p.CostoProduccion, 
                    p.MargenGanancia, 
                    p.StockActual, 
                    p.StockMinimo, 
                    p.EsProductoBase, 
                    p.IdTipo, 
                    p.IdSabor
                FROM Producto p
                INNER JOIN Tipo t ON p.IdTipo = t.IdTipo
                INNER JOIN Sabor s ON p.IdSabor = s.IdSabor
                WHERE p.Visible = 1";

                    SqlCommand cmd = new SqlCommand(query, oconexion);
                    cmd.CommandType = CommandType.Text;

                    oconexion.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            try
                            {
                                string nombreReal = reader["Nombre"] != DBNull.Value ? reader["Nombre"].ToString() : "";
                                string tipo = reader["Tipo"] != DBNull.Value ? reader["Tipo"].ToString() : "";
                                string sabor = reader["Sabor"] != DBNull.Value ? reader["Sabor"].ToString() : "";

                                // Si el nombre está vacío, generar uno automático
                                if (string.IsNullOrEmpty(nombreReal))
                                {
                                    nombreReal = $"{sabor} {tipo}".Trim();
                                }

                                lista.Add(new Producto()
                                {
                                    IdProducto = reader["IdProducto"] != DBNull.Value ? Convert.ToInt32(reader["IdProducto"]) : 0,
                                    Nombre = nombreReal,
                                    Tipo = tipo,
                                    Sabor = sabor,
                                    PrecioVenta = reader["PrecioVenta"] != DBNull.Value ? Convert.ToDecimal(reader["PrecioVenta"]) : 0,
                                    Visible = reader["Visible"] != DBNull.Value ? Convert.ToBoolean(reader["Visible"]) : true,
                                    CostoProduccion = reader["CostoProduccion"] != DBNull.Value ? Convert.ToDecimal(reader["CostoProduccion"]) : 0,
                                    MargenGanancia = reader["MargenGanancia"] != DBNull.Value ? Convert.ToDecimal(reader["MargenGanancia"]) : 0,
                                    StockActual = reader["StockActual"] != DBNull.Value ? Convert.ToInt32(reader["StockActual"]) : 0,
                                    StockMinimo = reader["StockMinimo"] != DBNull.Value ? Convert.ToInt32(reader["StockMinimo"]) : 0,
                                    EsProductoBase = reader["EsProductoBase"] != DBNull.Value ? Convert.ToBoolean(reader["EsProductoBase"]) : true,
                                    IdTipo = reader["IdTipo"] != DBNull.Value ? Convert.ToInt32(reader["IdTipo"]) : 0,
                                    IdSabor = reader["IdSabor"] != DBNull.Value ? Convert.ToInt32(reader["IdSabor"]) : 0
                                });
                            }
                            catch (Exception exLinea)
                            {
                                Console.WriteLine($"Error procesando fila: {exLinea.Message}");
                            }
                        }
                    }

                    Console.WriteLine($"Productos cargados exitosamente: {lista.Count}");
                }
                catch (Exception ex)
                {
                    lista = new List<Producto>();
                    Console.WriteLine($"Error en Listar: {ex.Message}");
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                    MessageBox.Show($"Error al cargar productos: {ex.Message}\n\nDetalles: {ex.StackTrace}");
                }
            }
            return lista;
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
                        // Query actualizada para incluir el campo Nombre
                        command.CommandText = @"
                    INSERT INTO Producto (IdTipo, IdSabor, Nombre, PrecioVenta, CostoProduccion, 
                                        MargenGanancia, StockActual, StockMinimo, Visible)
                    VALUES (@IdTipo, @IdSabor, @Nombre, @PrecioVenta, @CostoProduccion, 
                            @MargenGanancia, @StockActual, @StockMinimo, @Visible);
                    SELECT SCOPE_IDENTITY();";

                        command.Parameters.AddWithValue("@IdTipo", producto.IdTipo);
                        command.Parameters.AddWithValue("@IdSabor", producto.IdSabor);
                        // AGREGAR ESTE PARÁMETRO
                        command.Parameters.AddWithValue("@Nombre", producto.Nombre ?? "Producto Sin Nombre");
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
                return 0;
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
                        // Query actualizada para incluir el campo Nombre
                        command.CommandText = @"
                            UPDATE Producto 
                            SET IdTipo = @IdTipo, IdSabor = @IdSabor, Nombre = @Nombre, 
                                PrecioVenta = @PrecioVenta, CostoProduccion = @CostoProduccion, 
                                MargenGanancia = @MargenGanancia, StockActual = @StockActual, 
                                StockMinimo = @StockMinimo, Visible = @Visible
                            WHERE IdProducto = @IdProducto";

                        command.Parameters.AddWithValue("@IdProducto", producto.IdProducto);
                        command.Parameters.AddWithValue("@IdTipo", producto.IdTipo);
                        command.Parameters.AddWithValue("@IdSabor", producto.IdSabor);
                        // AGREGAR ESTE PARÁMETRO
                        command.Parameters.AddWithValue("@Nombre", producto.Nombre ?? "Producto Sin Nombre");
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

        // MÉTODO ACTUALIZADO: Ahora obtiene los tipos desde la tabla Tipo usando Descripcion
        public List<string> ListarTiposProducto()
        {
            List<string> listaTipos = new List<string>();

            try
            {
                using (SqlConnection conexion = new SqlConnection(Conexion.cadena))
                {
                    // CONSULTA CORREGIDA: Usar Descripcion en lugar de Nombre
                    string query = "SELECT Descripcion FROM Tipo WHERE Activo = 1 ORDER BY Descripcion";

                    SqlCommand cmd = new SqlCommand(query, conexion);
                    cmd.CommandType = CommandType.Text;

                    conexion.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            listaTipos.Add(dr["Descripcion"].ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                listaTipos = new List<string>();
                Console.WriteLine($"Error en ListarTiposProducto: {ex.Message}");
            }

            return listaTipos;
        }

        // MÉTODO CORREGIDO: Listar sabores desde la tabla Sabor usando Descripcion
        public List<string> ListarSaboresProducto()
        {
            List<string> listaSabores = new List<string>();

            try
            {
                using (SqlConnection conexion = new SqlConnection(Conexion.cadena))
                {
                    // CONSULTA CORREGIDA: Usar Descripcion en lugar de Nombre
                    string query = "SELECT Descripcion FROM Sabor WHERE Activo = 1 ORDER BY Descripcion";

                    SqlCommand cmd = new SqlCommand(query, conexion);
                    cmd.CommandType = CommandType.Text;

                    conexion.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            listaSabores.Add(dr["Descripcion"].ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                listaSabores = new List<string>();
                Console.WriteLine($"Error en ListarSaboresProducto: {ex.Message}");
            }

            return listaSabores;
        }

        // MÉTODO CORREGIDO: Obtener ID de Tipo por descripcion
        public int ObtenerIdTipoPorNombre(string descripcionTipo)
        {
            try
            {
                using (SqlConnection conexion = new SqlConnection(Conexion.cadena))
                {
                    // CONSULTA CORREGIDA: Buscar por Descripcion
                    string query = "SELECT IdTipo FROM Tipo WHERE Descripcion = @Descripcion AND Activo = 1";

                    SqlCommand cmd = new SqlCommand(query, conexion);
                    cmd.Parameters.AddWithValue("@Descripcion", descripcionTipo);

                    conexion.Open();
                    var result = cmd.ExecuteScalar();
                    return result != null ? Convert.ToInt32(result) : 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en ObtenerIdTipoPorNombre: {ex.Message}");
                return 0;
            }
        }

        // MÉTODO CORREGIDO: Obtener ID de Sabor por descripcion
        public int ObtenerIdSaborPorNombre(string descripcionSabor)
        {
            try
            {
                using (SqlConnection conexion = new SqlConnection(Conexion.cadena))
                {
                    // CONSULTA CORREGIDA: Buscar por Descripcion
                    string query = "SELECT IdSabor FROM Sabor WHERE Descripcion = @Descripcion AND Activo = 1";

                    SqlCommand cmd = new SqlCommand(query, conexion);
                    cmd.Parameters.AddWithValue("@Descripcion", descripcionSabor);

                    conexion.Open();
                    var result = cmd.ExecuteScalar();
                    return result != null ? Convert.ToInt32(result) : 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en ObtenerIdSaborPorNombre: {ex.Message}");
                return 0;
            }
        }

        // NUEVO MÉTODO: Verificar si existe combinación Tipo-Sabor
        public bool ExisteCombinacionTipoSabor(int idTipo, int idSabor, int idProductoExcluir = 0)
        {
            try
            {
                using (SqlConnection conexion = new SqlConnection(Conexion.cadena))
                {
                    string query = @"SELECT COUNT(*) FROM Producto 
                                  WHERE IdTipo = @IdTipo AND IdSabor = @IdSabor 
                                  AND Visible = 1";

                    if (idProductoExcluir > 0)
                    {
                        query += " AND IdProducto != @IdProductoExcluir";
                    }

                    SqlCommand cmd = new SqlCommand(query, conexion);
                    cmd.Parameters.AddWithValue("@IdTipo", idTipo);
                    cmd.Parameters.AddWithValue("@IdSabor", idSabor);

                    if (idProductoExcluir > 0)
                    {
                        cmd.Parameters.AddWithValue("@IdProductoExcluir", idProductoExcluir);
                    }

                    conexion.Open();
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en ExisteCombinacionTipoSabor: {ex.Message}");
                return false;
            }
        }

        // MÉTODO ACTUALIZADO: ObtenerProductosParaVerificar
        public List<Producto> ObtenerProductosParaVerificar()
        {
            List<Producto> productos = new List<Producto>();

            using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
            {
                oconexion.Open();
                using (var command = new SqlCommand())
                {
                    command.Connection = oconexion;
                    command.CommandText = @"SELECT 
                            p.IdProducto, 
                            t.Descripcion AS Tipo, 
                            s.Descripcion AS Sabor, 
                            p.StockActual, 
                            p.StockMinimo
                        FROM Producto p
                        INNER JOIN Tipo t ON p.IdTipo = t.IdTipo
                        INNER JOIN Sabor s ON p.IdSabor = s.IdSabor
                        WHERE p.Visible = 1";
                    command.CommandType = CommandType.Text;

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string tipo = reader["Tipo"].ToString();
                            string sabor = reader["Sabor"].ToString();

                            productos.Add(new Producto
                            {
                                IdProducto = (int)reader["IdProducto"],
                                Nombre = $"{sabor} {tipo}",
                                Tipo = tipo,
                                Sabor = sabor,
                                StockActual = (int)reader["StockActual"],
                                StockMinimo = (int)reader["StockMinimo"]
                            });
                        }
                    }
                }
            }
            return productos;
        }

        // MÉTODO ACTUALIZADO: ObtenerProductoPorId
        public Producto ObtenerProductoPorId(int idProducto)
        {
            using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
            {
                oconexion.Open();
                using (var command = new SqlCommand())
                {
                    command.Connection = oconexion;
                    command.CommandText = @"SELECT 
                            p.*, 
                            t.Descripcion AS TipoDescripcion, 
                            s.Descripcion AS SaborDescripcion
                        FROM Producto p
                        INNER JOIN Tipo t ON p.IdTipo = t.IdTipo
                        INNER JOIN Sabor s ON p.IdSabor = s.IdSabor
                        WHERE p.IdProducto = @IdProducto";
                    command.Parameters.AddWithValue("@IdProducto", idProducto);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string tipo = reader["TipoDescripcion"].ToString();
                            string sabor = reader["SaborDescripcion"].ToString();

                            return new Producto
                            {
                                IdProducto = (int)reader["IdProducto"],
                                Nombre = $"{sabor} {tipo}",
                                Tipo = tipo,
                                Sabor = sabor,
                                PrecioVenta = Convert.ToDecimal(reader["PrecioVenta"]),
                                CostoProduccion = Convert.ToDecimal(reader["CostoProduccion"]),
                                MargenGanancia = Convert.ToDecimal(reader["MargenGanancia"]),
                                StockActual = (int)reader["StockActual"],
                                StockMinimo = (int)reader["StockMinimo"],
                                Visible = (bool)reader["Visible"],
                                IdTipo = (int)reader["IdTipo"],
                                IdSabor = (int)reader["IdSabor"]
                            };
                        }
                    }
                }
            }
            return null;
        }

        // Los siguientes métodos se mantienen igual
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
                        command.CommandText = "UPDATE Producto SET Visible = 0 WHERE IdProducto = @IdProducto";
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

        public decimal CalcularCostosFijosUnitarios()
        {
            using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
            {
                oconexion.Open();
                using (var command = new SqlCommand())
                {
                    command.Connection = oconexion;
                    command.CommandText = @"
                DECLARE @TotalCostosFijos DECIMAL(10,2) = (
                    SELECT ISNULL(SUM(Monto), 0) 
                    FROM CostoFijoProduccion 
                    WHERE Activo = 1
                )
                
                DECLARE @TotalMateriasPrimas DECIMAL(10,2) = (
                    SELECT ISNULL(SUM(PrecioUnitario * CantidadDisponible), 0)
                    FROM MateriaPrima
                )
                
                DECLARE @TotalInversion DECIMAL(10,2) = @TotalCostosFijos + @TotalMateriasPrimas
                DECLARE @CostoVentaUnitario DECIMAL(10,2) = @TotalInversion / 2500
                
                SELECT ISNULL(@CostoVentaUnitario, 0)";

                    var result = command.ExecuteScalar();
                    return result != DBNull.Value ? Convert.ToDecimal(result) : 0;
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

        public bool ActualizarCostoYPrecioProducto(int idProducto, decimal nuevoCosto, decimal nuevoPrecio)
        {
            using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
            {
                oconexion.Open();
                using (var command = new SqlCommand())
                {
                    command.Connection = oconexion;
                    command.CommandText = @"UPDATE Producto 
                                  SET CostoProduccion = @Costo, PrecioVenta = @Precio 
                                  WHERE IdProducto = @IdProducto";
                    command.Parameters.AddWithValue("@Costo", nuevoCosto);
                    command.Parameters.AddWithValue("@Precio", nuevoPrecio);
                    command.Parameters.AddWithValue("@IdProducto", idProducto);

                    return command.ExecuteNonQuery() > 0;
                }
            }
        }

        public int ContarProductosEnBD()
        {
            try
            {
                using (SqlConnection conexion = new SqlConnection(Conexion.cadena))
                {
                    string query = "SELECT COUNT(*) FROM Producto WHERE Visible = 1";
                    SqlCommand cmd = new SqlCommand(query, conexion);

                    conexion.Open();
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al contar productos: {ex.Message}");
                return 0;
            }
        }
    }
}