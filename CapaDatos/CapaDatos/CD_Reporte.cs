using System;
using System.Collections.Generic;
using System.Data;
using CapaEntidad;
using Microsoft.Data.SqlClient;

namespace CapaDatos
{
    public class CD_Reporte
    {
        public List<ReporteConsumo> ObtenerConsumoMateriaPrima(DateTime fechaInicio, DateTime fechaFin)
        {
            List<ReporteConsumo> lista = new List<ReporteConsumo>();

            using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("sp_ConsumoMateriaPrimaPorPeriodo", oconexion);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@FechaInicio", fechaInicio);
                    cmd.Parameters.AddWithValue("@FechaFin", fechaFin);

                    oconexion.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lista.Add(new ReporteConsumo()
                            {
                                MateriaPrima = dr["MateriaPrima"].ToString(),
                                CantidadConsumida = Convert.ToDecimal(dr["CantidadConsumida"]),
                                Unidad = dr["Unidad"].ToString(),
                                CostoTotal = dr["CostoTotal"] != DBNull.Value ? Convert.ToDecimal(dr["CostoTotal"]) : 0
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    lista = new List<ReporteConsumo>();
                }
            }

            return lista;
        }


        public List<ReporteVenta> ObtenerVentasPorFecha(DateTime fechaInicio, DateTime fechaFin)
        {
            List<ReporteVenta> lista = new List<ReporteVenta>();

            using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
            {
                try
                {
                    oconexion.Open();

                    // ✅ QUERY ACTUALIZADA con MétodoPago
                    string query = @"
        SELECT 
            v.IdVenta, 
            v.Fecha, 
            ISNULL(c.Nombre + ' ' + ISNULL(c.Apellido, ''), 'CONSUMIDOR FINAL') as Cliente,
            ISNULL(c.Documento, '') as DNI,
            u.NombreUsuario as Usuario,
            v.Total,
            v.IdUsuario,
            v.MetodoPago, -- ✅ AGREGADO: Método de pago
            STUFF((
                SELECT ', ' + p.Nombre + ' (' + CAST(dv.Cantidad AS VARCHAR) + ')'
                FROM DetalleVenta dv
                INNER JOIN Producto p ON dv.IdProducto = p.IdProducto
                WHERE dv.IdVenta = v.IdVenta
                FOR XML PATH('')
            ), 1, 2, '') as Productos,
            (SELECT COUNT(*) FROM DetalleVenta dv WHERE dv.IdVenta = v.IdVenta) as CantidadProductos
        FROM Venta v
        LEFT JOIN Cliente c ON v.IdCliente = c.IdCliente
        INNER JOIN Usuario u ON v.IdUsuario = u.IdUsuario
        WHERE v.Fecha BETWEEN @FechaInicio AND @FechaFin
        ORDER BY v.Fecha DESC";

                    SqlCommand comando = new SqlCommand(query, oconexion);

                    // ✅ Ajustar fechas para incluir todo el día
                    comando.Parameters.AddWithValue("@FechaInicio", fechaInicio.Date);
                    comando.Parameters.AddWithValue("@FechaFin", fechaFin.Date.AddDays(1).AddSeconds(-1));

                    using (SqlDataReader reader = comando.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(new ReporteVenta
                            {
                                IdVenta = (int)reader["IdVenta"],
                                Fecha = (DateTime)reader["Fecha"],
                                Cliente = reader["Cliente"].ToString(),
                                DNI = reader["DNI"].ToString(),
                                Usuario = reader["Usuario"].ToString(),
                                Total = (decimal)reader["Total"],
                                IdUsuario = (int)reader["IdUsuario"],
                                MetodoPago = reader["MetodoPago"] != DBNull.Value ? (int)reader["MetodoPago"] : 1, // ✅ AGREGADO
                                Productos = reader["Productos"] != DBNull.Value ? reader["Productos"].ToString() : "Sin productos",
                                CantidadProductos = (int)reader["CantidadProductos"]
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Error al obtener ventas por fecha: " + ex.Message);
                }
            }

            return lista;
        }

        public List<ReporteVenta> ObtenerVentasPorVendedor(int idVendedor, DateTime fechaInicio, DateTime fechaFin)
        {
            List<ReporteVenta> lista = new List<ReporteVenta>();

            using (SqlConnection oconexion = new SqlConnection(Conexion.cadena)) // ✅ USAR Conexion.cadena
            {
                try
                {
                    oconexion.Open();

                    // ✅ QUERY SIMPLIFICADA Y CORREGIDA
                    string query = @"
                SELECT 
                    v.IdVenta, 
                    v.Fecha, 
                    ISNULL(c.Nombre + ' ' + ISNULL(c.Apellido, ''), 'CONSUMIDOR FINAL') as Cliente,
                    ISNULL(c.Documento, '') as DNI,
                    u.NombreUsuario as Usuario,
                    v.Total,
                    v.IdUsuario,
                    v.MetodoPago,
                    STUFF((
                        SELECT ', ' + p.Nombre + ' (' + CAST(dv.Cantidad AS VARCHAR) + ')'
                        FROM DetalleVenta dv
                        INNER JOIN Producto p ON dv.IdProducto = p.IdProducto
                        WHERE dv.IdVenta = v.IdVenta
                        FOR XML PATH('')
                    ), 1, 2, '') as Productos,
                    (SELECT COUNT(*) FROM DetalleVenta dv WHERE dv.IdVenta = v.IdVenta) as CantidadProductos
                FROM Venta v
                LEFT JOIN Cliente c ON v.IdCliente = c.IdCliente
                INNER JOIN Usuario u ON v.IdUsuario = u.IdUsuario
                WHERE v.Fecha BETWEEN @FechaInicio AND @FechaFin
                AND v.IdUsuario = @IdVendedor
                ORDER BY v.Fecha DESC";

                    SqlCommand comando = new SqlCommand(query, oconexion);

                    // ✅ PARÁMETROS CORRECTOS
                    comando.Parameters.AddWithValue("@FechaInicio", fechaInicio.Date);
                    comando.Parameters.AddWithValue("@FechaFin", fechaFin.Date.AddDays(1).AddSeconds(-1));
                    comando.Parameters.AddWithValue("@IdVendedor", idVendedor);

                    using (SqlDataReader reader = comando.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(new ReporteVenta
                            {
                                IdVenta = reader["IdVenta"] != DBNull.Value ? Convert.ToInt32(reader["IdVenta"]) : 0,
                                Fecha = reader["Fecha"] != DBNull.Value ? Convert.ToDateTime(reader["Fecha"]) : DateTime.MinValue,
                                Cliente = reader["Cliente"]?.ToString() ?? "CONSUMIDOR FINAL",
                                DNI = reader["DNI"]?.ToString() ?? "",
                                Usuario = reader["Usuario"]?.ToString() ?? "",
                                Total = reader["Total"] != DBNull.Value ? Convert.ToDecimal(reader["Total"]) : 0,
                                IdUsuario = reader["IdUsuario"] != DBNull.Value ? Convert.ToInt32(reader["IdUsuario"]) : 0,
                                MetodoPago = reader["MetodoPago"] != DBNull.Value ? Convert.ToInt32(reader["MetodoPago"]) : 1,
                                Productos = reader["Productos"] != DBNull.Value ? reader["Productos"].ToString() : "Sin productos",
                                CantidadProductos = reader["CantidadProductos"] != DBNull.Value ? Convert.ToInt32(reader["CantidadProductos"]) : 0
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    // ✅ MEJOR MANEJO DE ERRORES
                    throw new Exception($"Error al obtener ventas por vendedor {idVendedor}: {ex.Message}");
                }
            }

            return lista;
        }

        public List<ReporteVenta> ObtenerVentasPorVendedorConFiltros(int idVendedor, DateTime fechaInicio, DateTime fechaFin, string dniCliente = null, string nombreProducto = null)
        {
            List<ReporteVenta> lista = new List<ReporteVenta>();

            using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
            {
                try
                {
                    string query = @"
                        SELECT 
                            v.IdVenta, 
                            v.Fecha, 
                            c.NombreCompleto as Cliente,
                            c.DNI as DNI,
                            u.NombreCompleto as Usuario,
                            v.Total,
                            v.MetodoPago, -- ✅ AGREGADO: Método de pago
                            STUFF((
                                SELECT ', ' + p.Nombre + ' (' + CAST(dv.Cantidad AS VARCHAR) + ')'
                                FROM DetalleVenta dv
                                INNER JOIN Producto p ON dv.IdProducto = p.IdProducto
                                WHERE dv.IdVenta = v.IdVenta
                                FOR XML PATH('')
                            ), 1, 2, '') as Productos
                        FROM Venta v 
                        INNER JOIN Cliente c ON v.IdCliente = c.IdCliente
                        INNER JOIN Usuario u ON v.IdUsuario = u.IdUsuario
                        WHERE CONVERT(DATE, v.Fecha) BETWEEN @FechaInicio AND @FechaFin
                        AND v.IdUsuario = @IdVendedor";

                    // Agregar filtros opcionales
                    if (!string.IsNullOrEmpty(dniCliente))
                    {
                        query += " AND c.DNI LIKE @DniCliente";
                    }

                    if (!string.IsNullOrEmpty(nombreProducto))
                    {
                        query += @" AND EXISTS (
                            SELECT 1 FROM DetalleVenta dv
                            INNER JOIN Producto p ON dv.IdProducto = p.IdProducto
                            WHERE dv.IdVenta = v.IdVenta
                            AND p.Nombre LIKE @NombreProducto
                        )";
                    }

                    query += " ORDER BY v.Fecha DESC";

                    SqlCommand cmd = new SqlCommand(query, oconexion);
                    cmd.Parameters.AddWithValue("@FechaInicio", fechaInicio.Date);
                    cmd.Parameters.AddWithValue("@FechaFin", fechaFin.Date);
                    cmd.Parameters.AddWithValue("@IdVendedor", idVendedor);

                    if (!string.IsNullOrEmpty(dniCliente))
                    {
                        cmd.Parameters.AddWithValue("@DniCliente", "%" + dniCliente + "%");
                    }

                    if (!string.IsNullOrEmpty(nombreProducto))
                    {
                        cmd.Parameters.AddWithValue("@NombreProducto", "%" + nombreProducto + "%");
                    }

                    oconexion.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lista.Add(new ReporteVenta()
                            {
                                IdVenta = Convert.ToInt32(dr["IdVenta"]),
                                Fecha = Convert.ToDateTime(dr["Fecha"]),
                                Cliente = dr["Cliente"].ToString(),
                                DNI = dr["DNI"] != DBNull.Value ? dr["DNI"].ToString() : "",
                                Usuario = dr["Usuario"].ToString(),
                                Total = Convert.ToDecimal(dr["Total"]),
                                MetodoPago = dr["MetodoPago"] != DBNull.Value ? Convert.ToInt32(dr["MetodoPago"]) : 1, // ✅ AGREGADO
                                Productos = dr["Productos"] != DBNull.Value ? dr["Productos"].ToString() : ""
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    lista = new List<ReporteVenta>();
                }
            }
            return lista;
        }

        public List<ReporteVenta> ObtenerVentasPorCliente(int idCliente)
        {
            List<ReporteVenta> lista = new List<ReporteVenta>();

            using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
            {
                try
                {
                    oconexion.Open();

                    // ✅ QUERY ACTUALIZADA con MétodoPago
                    string query = @"
                SELECT 
                    v.IdVenta, 
                    v.Fecha, 
                    ISNULL(c.Nombre + ' ' + ISNULL(c.Apellido, ''), 'CONSUMIDOR FINAL') as Cliente,
                    ISNULL(c.Documento, '') as DNI,
                    u.NombreUsuario as Usuario,
                    v.Total,
                    v.IdUsuario,
                    v.MetodoPago, -- ✅ AGREGADO: Método de pago
                    STUFF((
                        SELECT ', ' + p.Nombre + ' (' + CAST(dv.Cantidad AS VARCHAR) + ')'
                        FROM DetalleVenta dv
                        INNER JOIN Producto p ON dv.IdProducto = p.IdProducto
                        WHERE dv.IdVenta = v.IdVenta
                        FOR XML PATH('')
                    ), 1, 2, '') as Productos,
                    (SELECT COUNT(*) FROM DetalleVenta dv WHERE dv.IdVenta = v.IdVenta) as CantidadProductos
                FROM Venta v
                LEFT JOIN Cliente c ON v.IdCliente = c.IdCliente
                INNER JOIN Usuario u ON v.IdUsuario = u.IdUsuario
                WHERE c.IdCliente = @IdCliente
                ORDER BY v.Fecha DESC";

                    SqlCommand comando = new SqlCommand(query, oconexion);
                    comando.Parameters.AddWithValue("@IdCliente", idCliente);

                    using (SqlDataReader reader = comando.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(new ReporteVenta
                            {
                                IdVenta = (int)reader["IdVenta"],
                                Fecha = (DateTime)reader["Fecha"],
                                Cliente = reader["Cliente"].ToString(),
                                DNI = reader["DNI"].ToString(),
                                Usuario = reader["Usuario"].ToString(),
                                Total = (decimal)reader["Total"],
                                IdUsuario = (int)reader["IdUsuario"],
                                MetodoPago = reader["MetodoPago"] != DBNull.Value ? (int)reader["MetodoPago"] : 1, // ✅ AGREGADO
                                Productos = reader["Productos"] != DBNull.Value ? reader["Productos"].ToString() : "Sin productos",
                                CantidadProductos = (int)reader["CantidadProductos"]
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Error al obtener ventas por cliente: " + ex.Message);
                }
            }
            return lista;
        }

        // En CD_Reporte.cs - REEMPLAZAR el método ObtenerProductosStockBajo
        public List<ReporteStock> ObtenerProductosStockBajo()
        {
            List<ReporteStock> lista = new List<ReporteStock>();

            using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(@"
                SELECT 
                    p.IdProducto, 
                    COALESCE(p.Nombre, CONCAT(s.Descripcion, ' ', t.Descripcion)) as Nombre,
                    t.Descripcion as Tipo,  -- ✅ COLUMNA CORREGIDA
                    p.StockActual, 
                    p.StockMinimo,
                    CASE 
                        WHEN p.StockActual <= p.StockMinimo THEN 'CRÍTICO'
                        WHEN p.StockActual <= p.StockMinimo * 1.5 THEN 'ALERTA' 
                        ELSE 'NORMAL'
                    END as Estado
                FROM Producto p
                INNER JOIN Tipo t ON p.IdTipo = t.IdTipo  -- ✅ JOIN CORREGIDO
                INNER JOIN Sabor s ON p.IdSabor = s.IdSabor  -- ✅ JOIN CORREGIDO
                WHERE p.Visible = 1
                AND p.StockActual <= (p.StockMinimo * 1.5)  -- Stock bajo o crítico
                ORDER BY p.StockActual ASC", oconexion);

                    oconexion.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lista.Add(new ReporteStock()
                            {
                                IdProducto = Convert.ToInt32(dr["IdProducto"]),
                                Nombre = dr["Nombre"].ToString(),
                                Tipo = dr["Tipo"].ToString(),
                                StockActual = Convert.ToInt32(dr["StockActual"]),
                                StockMinimo = Convert.ToInt32(dr["StockMinimo"]),
                                Estado = dr["Estado"].ToString()
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error en ObtenerProductosStockBajo: {ex.Message}");
                }
            }
            return lista;
        }

        // En CD_Reporte.cs - REEMPLAZAR el método ObtenerTodosProductosConStock
        public List<ReporteStock> ObtenerTodosProductosConStock()
        {
            List<ReporteStock> lista = new List<ReporteStock>();

            using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(@"
                SELECT 
                    p.IdProducto, 
                    COALESCE(p.Nombre, CONCAT(s.Descripcion, ' ', t.Descripcion)) as Nombre,
                    t.Descripcion as Tipo,  -- ✅ COLUMNA CORREGIDA
                    p.StockActual, 
                    p.StockMinimo, 
                    p.PrecioVenta,
                    CASE 
                        WHEN p.StockActual <= p.StockMinimo THEN 'CRÍTICO'
                        WHEN p.StockActual <= p.StockMinimo * 1.5 THEN 'ALERTA' 
                        ELSE 'NORMAL'
                    END as Estado
                FROM Producto p
                INNER JOIN Tipo t ON p.IdTipo = t.IdTipo  -- ✅ JOIN CORREGIDO
                INNER JOIN Sabor s ON p.IdSabor = s.IdSabor  -- ✅ JOIN CORREGIDO
                WHERE p.Visible = 1
                ORDER BY p.StockActual ASC", oconexion);

                    oconexion.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lista.Add(new ReporteStock()
                            {
                                IdProducto = Convert.ToInt32(dr["IdProducto"]),
                                Nombre = dr["Nombre"].ToString(),
                                Tipo = dr["Tipo"].ToString(),
                                StockActual = Convert.ToInt32(dr["StockActual"]),
                                StockMinimo = Convert.ToInt32(dr["StockMinimo"]),
                                PrecioVenta = Convert.ToDecimal(dr["PrecioVenta"]),
                                Estado = dr["Estado"].ToString()
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error en ObtenerTodosProductosConStock: {ex.Message}");
                }
            }
            return lista;
        }

        public decimal ObtenerTotalVentasPeriodo(DateTime fechaInicio, DateTime fechaFin)
        {
            using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(@"
                        SELECT ISNULL(SUM(Total), 0) 
                        FROM Venta 
                        WHERE Fecha BETWEEN @FechaInicio AND @FechaFin", oconexion);

                    cmd.Parameters.AddWithValue("@FechaInicio", fechaInicio);
                    cmd.Parameters.AddWithValue("@FechaFin", fechaFin);

                    oconexion.Open();
                    return Convert.ToDecimal(cmd.ExecuteScalar());
                }
                catch
                {
                    return 0;
                }
            }
        }

        public int ObtenerCantidadVentasPeriodo(DateTime fechaInicio, DateTime fechaFin)
        {
            using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(@"
                        SELECT COUNT(*) 
                        FROM Venta 
                        WHERE Fecha BETWEEN @FechaInicio AND @FechaFin", oconexion);

                    cmd.Parameters.AddWithValue("@FechaInicio", fechaInicio);
                    cmd.Parameters.AddWithValue("@FechaFin", fechaFin);

                    oconexion.Open();
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
                catch
                {
                    return 0;
                }
            }
        }

        public List<ReporteVenta> ObtenerVentasPorVendedor(int idUsuario, DateTime fechaInicio, DateTime fechaFin,
                                                            string dniCliente = "", string nombreProducto = "")
        {
            List<ReporteVenta> lista = new List<ReporteVenta>();

            using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
            {
                try
                {
                    string query = @"
                        SELECT 
                            v.IdVenta, 
                            v.Fecha, 
                            c.Nombre + ' ' + c.Apellido as Cliente,
                            c.Documento as DniCliente,
                            u.NombreUsuario as Vendedor,
                            v.Total,
                            v.IdUsuario,
                            STUFF((
                                SELECT ', ' + p.Nombre + ' (' + CAST(dv.Cantidad AS VARCHAR) + ')'
                                FROM DetalleVenta dv
                                INNER JOIN Producto p ON dv.IdProducto = p.IdProducto
                                WHERE dv.IdVenta = v.IdVenta
                                FOR XML PATH('')
                            ), 1, 2, '') as Productos
                        FROM Venta v 
                        INNER JOIN Cliente c ON v.IdCliente = c.IdCliente
                        INNER JOIN Usuario u ON v.IdUsuario = u.IdUsuario
                        WHERE CONVERT(DATE, v.Fecha) BETWEEN @FechaInicio AND @FechaFin
                        AND v.IdUsuario = @IdVendedor";

                    if (!string.IsNullOrEmpty(dniCliente))
                    {
                        query += " AND c.Documento LIKE @DniCliente";
                    }

                    if (!string.IsNullOrEmpty(nombreProducto))
                    {
                        query += @" AND EXISTS (
                                SELECT 1 FROM DetalleVenta dv
                                INNER JOIN Producto p ON dv.IdProducto = p.IdProducto
                                WHERE dv.IdVenta = v.IdVenta AND p.Nombre LIKE @NombreProducto
                            )";
                    }

                    query += " ORDER BY v.Fecha DESC";

                    SqlCommand cmd = new SqlCommand(query, oconexion);
                    cmd.Parameters.AddWithValue("@FechaInicio", fechaInicio.Date);
                    cmd.Parameters.AddWithValue("@FechaFin", fechaFin.Date);
                    cmd.Parameters.AddWithValue("@IdVendedor", idUsuario);

                    if (!string.IsNullOrEmpty(dniCliente))
                    {
                        cmd.Parameters.AddWithValue("@DniCliente", "%" + dniCliente + "%");
                    }

                    if (!string.IsNullOrEmpty(nombreProducto))
                    {
                        cmd.Parameters.AddWithValue("@NombreProducto", "%" + nombreProducto + "%");
                    }

                    oconexion.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lista.Add(new ReporteVenta()
                            {
                                IdVenta = Convert.ToInt32(dr["IdVenta"]),
                                Fecha = Convert.ToDateTime(dr["Fecha"]),
                                Cliente = dr["Cliente"].ToString(),
                                DNI = dr["DniCliente"].ToString(),
                                Usuario = dr["Vendedor"].ToString(),
                                IdUsuario = Convert.ToInt32(dr["IdUsuario"]),
                                Total = Convert.ToDecimal(dr["Total"]),
                                Productos = dr["Productos"] != DBNull.Value ? dr["Productos"].ToString() : "Sin productos"
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error al obtener ventas por vendedor: {ex.Message}");
                }
            }
            return lista;
        }


        public List<ConsumoMateriaPrima> ObtenerConsumoMateriaPrimaPorVenta(int idVenta)
        {
            List<ConsumoMateriaPrima> lista = new List<ConsumoMateriaPrima>();

            using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("sp_ConsumoMateriaPrimaPorVenta", oconexion);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@IdVenta", idVenta);

                    oconexion.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lista.Add(new ConsumoMateriaPrima()
                            {
                                MateriaPrima = dr["MateriaPrima"].ToString(),
                                CantidadConsumida = Convert.ToDecimal(dr["CantidadConsumida"]),
                                Unidad = dr["Unidad"].ToString()
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    lista = new List<ConsumoMateriaPrima>();
                }
            }
            return lista;
        }
        public List<ReporteVenta> ObtenerTodasLasVentas()
        {
            List<ReporteVenta> ventas = new List<ReporteVenta>();

            using (SqlConnection conexion = new SqlConnection(Conexion.cadena))
            {
                try
                {
                    conexion.Open();
                    string query = @"
                SELECT v.IdVenta, v.Fecha, v.Total, v.IdUsuario, 
                       c.Nombre as Cliente, c.DNI, 
                       u.Nombre as Usuario
                FROM Ventas v 
                LEFT JOIN Clientes c ON v.IdCliente = c.IdCliente
                LEFT JOIN Usuarios u ON v.IdUsuario = u.IdUsuario
                ORDER BY v.Fecha DESC";

                    using (SqlCommand cmd = new SqlCommand(query, conexion))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ventas.Add(new ReporteVenta
                                {
                                    IdVenta = Convert.ToInt32(reader["IdVenta"]),
                                    Fecha = Convert.ToDateTime(reader["Fecha"]),
                                    Total = Convert.ToDecimal(reader["Total"]),
                                    IdUsuario = Convert.ToInt32(reader["IdUsuario"]),
                                    Cliente = reader["Cliente"]?.ToString() ?? "N/A",
                                    DNI = reader["DNI"]?.ToString() ?? "N/A",
                                    Usuario = reader["Usuario"]?.ToString() ?? "N/A"
                                });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Error al obtener ventas: " + ex.Message);
                }
            }

            return ventas;
        }
        // En CD_Reporte.cs - CORREGIR este método
        // En CD_Reporte.cs - REEMPLAZAR el método ObtenerVentasPorTipo
        public List<ReporteVentaPorTipo> ObtenerVentasPorTipo(DateTime fechaInicio, DateTime fechaFin)
        {
            List<ReporteVentaPorTipo> lista = new List<ReporteVentaPorTipo>();

            using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
            {
                try
                {
                    string query = @"
        SELECT 
            COALESCE(t.Descripcion, 'Sin Tipo') as Tipo,
            SUM(dv.Cantidad) as Cantidad,
            SUM(dv.Cantidad * dv.PrecioUnitario) as Total
        FROM DetalleVenta dv
        INNER JOIN Venta v ON dv.IdVenta = v.IdVenta
        INNER JOIN Producto p ON dv.IdProducto = p.IdProducto
        INNER JOIN Tipo t ON p.IdTipo = t.IdTipo  -- ✅ JOIN CORREGIDO
        WHERE v.Fecha BETWEEN @FechaInicio AND @FechaFin
        GROUP BY t.Descripcion
        ORDER BY Total DESC";

                    SqlCommand cmd = new SqlCommand(query, oconexion);
                    cmd.Parameters.AddWithValue("@FechaInicio", fechaInicio);
                    cmd.Parameters.AddWithValue("@FechaFin", fechaFin);

                    oconexion.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lista.Add(new ReporteVentaPorTipo()
                            {
                                Tipo = dr["Tipo"].ToString(),
                                Cantidad = Convert.ToInt32(dr["Cantidad"]),
                                Total = Convert.ToDecimal(dr["Total"])
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error en ObtenerVentasPorTipo: {ex.Message}");
                }
            }
            return lista;
        }
        // En CD_Reporte.cs - CORREGIR este método
        public List<ReporteTopCliente> ObtenerTopClientes(DateTime fechaInicio, DateTime fechaFin, int top = 5)
        {
            List<ReporteTopCliente> lista = new List<ReporteTopCliente>();

            using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
            {
                try
                {
                    string query = @"
        SELECT TOP (@Top) 
            COALESCE(c.Nombre + ' ' + ISNULL(c.Apellido, ''), 'CONSUMIDOR FINAL') as Nombre,
            COUNT(v.IdVenta) as CantidadCompras,
            SUM(v.Total) as TotalGastado
        FROM Venta v
        LEFT JOIN Cliente c ON v.IdCliente = c.IdCliente
        WHERE v.Fecha BETWEEN @FechaInicio AND @FechaFin
        GROUP BY c.Nombre, c.Apellido
        HAVING COUNT(v.IdVenta) > 0
        ORDER BY TotalGastado DESC";

                    SqlCommand cmd = new SqlCommand(query, oconexion);
                    cmd.Parameters.AddWithValue("@FechaInicio", fechaInicio);
                    cmd.Parameters.AddWithValue("@FechaFin", fechaFin);
                    cmd.Parameters.AddWithValue("@Top", top);

                    oconexion.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lista.Add(new ReporteTopCliente()
                            {
                                Nombre = dr["Nombre"].ToString(),
                                CantidadCompras = Convert.ToInt32(dr["CantidadCompras"]),
                                TotalGastado = Convert.ToDecimal(dr["TotalGastado"])
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error en ObtenerTopClientes: {ex.Message}");
                }
            }
            return lista;
        }

        // ✅ NUEVO MÉTODO: Obtener una venta específica por ID
        public ReporteVenta ObtenerVentaPorId(int idVenta)
        {
            using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
            {
                try
                {
                    oconexion.Open();

                    string query = @"
                SELECT 
                    v.IdVenta, 
                    v.Fecha, 
                    ISNULL(c.Nombre + ' ' + ISNULL(c.Apellido, ''), 'CONSUMIDOR FINAL') as Cliente,
                    ISNULL(c.Documento, '') as DNI,
                    u.NombreUsuario as Usuario,
                    v.Total,
                    v.IdUsuario,
                    v.MetodoPago, -- ✅ INCLUIDO: Método de pago
                    STUFF((
                        SELECT ', ' + p.Nombre + ' (' + CAST(dv.Cantidad AS VARCHAR) + ')'
                        FROM DetalleVenta dv
                        INNER JOIN Producto p ON dv.IdProducto = p.IdProducto
                        WHERE dv.IdVenta = v.IdVenta
                        FOR XML PATH('')
                    ), 1, 2, '') as Productos,
                    (SELECT COUNT(*) FROM DetalleVenta dv WHERE dv.IdVenta = v.IdVenta) as CantidadProductos
                FROM Venta v
                LEFT JOIN Cliente c ON v.IdCliente = c.IdCliente
                INNER JOIN Usuario u ON v.IdUsuario = u.IdUsuario
                WHERE v.IdVenta = @IdVenta";

                    SqlCommand comando = new SqlCommand(query, oconexion);
                    comando.Parameters.AddWithValue("@IdVenta", idVenta);

                    using (SqlDataReader reader = comando.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new ReporteVenta
                            {
                                IdVenta = (int)reader["IdVenta"],
                                Fecha = (DateTime)reader["Fecha"],
                                Cliente = reader["Cliente"].ToString(),
                                DNI = reader["DNI"].ToString(),
                                Usuario = reader["Usuario"].ToString(),
                                Total = (decimal)reader["Total"],
                                IdUsuario = (int)reader["IdUsuario"],
                                MetodoPago = reader["MetodoPago"] != DBNull.Value ? (int)reader["MetodoPago"] : 1,
                                Productos = reader["Productos"] != DBNull.Value ? reader["Productos"].ToString() : "Sin productos",
                                CantidadProductos = (int)reader["CantidadProductos"]
                            };
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Error al obtener venta por ID: " + ex.Message);
                }
            }
            return null;
        }

        public List<string> ObtenerTiposProducto()
        {
            List<string> tipos = new List<string>();

            using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
            {
                try
                {
                    string query = @"
                SELECT DISTINCT t.Descripcion as Tipo
                FROM Tipo t
                INNER JOIN Producto p ON t.IdTipo = p.IdTipo
                WHERE p.Visible = 1
                ORDER BY t.Descripcion";

                    SqlCommand cmd = new SqlCommand(query, oconexion);
                    oconexion.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            tipos.Add(dr["Tipo"].ToString());
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al obtener tipos de producto: {ex.Message}");
                    // Devolver tipos básicos en caso de error
                    tipos = new List<string> { "Pastas", "Salsas", "Postres" };
                }
            }

            return tipos;
        }

        // AGREGAR en CD_Reporte.cs
        public int ObtenerClientesDelPeriodo(DateTime fechaInicio, DateTime fechaFin)
        {
            using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
            {
                try
                {
                    string query = @"
                SELECT COUNT(DISTINCT v.IdCliente) as TotalClientes
                FROM Venta v
                WHERE v.Fecha BETWEEN @FechaInicio AND @FechaFin
                AND v.IdCliente IS NOT NULL";

                    SqlCommand cmd = new SqlCommand(query, oconexion);
                    cmd.Parameters.AddWithValue("@FechaInicio", fechaInicio);
                    cmd.Parameters.AddWithValue("@FechaFin", fechaFin);

                    oconexion.Open();
                    var result = cmd.ExecuteScalar();
                    return result != DBNull.Value ? Convert.ToInt32(result) : 0;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al obtener clientes del período: {ex.Message}");
                    return 0;
                }
            }
        }

        // EN CD_Reporte.cs - ACTUALIZAR el método ObtenerProductosMasVendidos
        public List<ReporteProductoVendido> ObtenerProductosMasVendidos(DateTime fechaInicio, DateTime fechaFin, int top = 5, string tipoProducto = null)
        {
            List<ReporteProductoVendido> lista = new List<ReporteProductoVendido>();

            using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
            {
                try
                {
                    string query = @"
        SELECT TOP (@Top) 
            COALESCE(p.Nombre, CONCAT(s.Descripcion, ' ', t.Descripcion)) as NombreProducto,
            t.Descripcion as Tipo,
            SUM(dv.Cantidad) as CantidadVendida,
            SUM(dv.Cantidad * dv.PrecioUnitario) as TotalVendido
        FROM DetalleVenta dv
        INNER JOIN Venta v ON dv.IdVenta = v.IdVenta
        INNER JOIN Producto p ON dv.IdProducto = p.IdProducto
        INNER JOIN Tipo t ON p.IdTipo = t.IdTipo
        INNER JOIN Sabor s ON p.IdSabor = s.IdSabor
        WHERE v.Fecha BETWEEN @FechaInicio AND @FechaFin";

                    // Agregar filtro por tipo si se especifica
                    if (!string.IsNullOrEmpty(tipoProducto) && tipoProducto != "Todos los tipos")
                    {
                        query += " AND t.Descripcion = @TipoProducto";
                    }

                    query += @"
        GROUP BY p.Nombre, s.Descripcion, t.Descripcion
        HAVING SUM(dv.Cantidad) > 0
        ORDER BY CantidadVendida DESC";

                    SqlCommand cmd = new SqlCommand(query, oconexion);
                    cmd.Parameters.AddWithValue("@FechaInicio", fechaInicio);
                    cmd.Parameters.AddWithValue("@FechaFin", fechaFin);
                    cmd.Parameters.AddWithValue("@Top", top);

                    // Agregar parámetro de tipo si se especifica
                    if (!string.IsNullOrEmpty(tipoProducto) && tipoProducto != "Todos los tipos")
                    {
                        cmd.Parameters.AddWithValue("@TipoProducto", tipoProducto);
                    }

                    oconexion.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lista.Add(new ReporteProductoVendido()
                            {
                                NombreProducto = dr["NombreProducto"].ToString(),
                                Tipo = dr["Tipo"].ToString(),
                                CantidadVendida = Convert.ToInt32(dr["CantidadVendida"]),
                                TotalVendido = Convert.ToDecimal(dr["TotalVendido"])
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error en ObtenerProductosMasVendidos: {ex.Message}");
                }
            }
            return lista;
        }

    }
}