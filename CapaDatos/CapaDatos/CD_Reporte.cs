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

        // En CD_Reporte.cs - actualiza el método si es necesario
        public List<ReporteVenta> ObtenerVentasPorFecha(DateTime fechaInicio, DateTime fechaFin)
        {
            List<ReporteVenta> lista = new List<ReporteVenta>();

            using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
            {
                try
                {
                    oconexion.Open();

                    // ✅ CORREGIDO: Nombres exactos de las tablas
                    string query = @"
        SELECT 
            v.IdVenta, 
            v.Fecha, 
            c.Nombre + ' ' + ISNULL(c.Apellido, '') as Cliente,
            c.Documento as DNI,
            u.NombreUsuario as Usuario,
            v.Total,
            v.IdUsuario,
            STUFF((
                SELECT ', ' + p.Nombre + ' (' + CAST(iv.Cantidad AS VARCHAR) + ')'
                FROM ItemVenta iv  -- ✅ CAMBIADO: DetalleVenta → ItemVenta
                INNER JOIN Producto p ON iv.IdProducto = p.IdProducto  -- ✅ Producto (singular)
                WHERE iv.IdVenta = v.IdVenta
                FOR XML PATH('')
            ), 1, 2, '') as Productos,
            (SELECT COUNT(*) FROM ItemVenta iv WHERE iv.IdVenta = v.IdVenta) as CantidadProductos
        FROM Venta v  -- ✅ CORREGIDO: Ventas → Venta
        INNER JOIN Cliente c ON v.IdCliente = c.IdCliente  -- ✅ CORREGIDO: Clientes → Cliente
        INNER JOIN Usuario u ON v.IdUsuario = u.IdUsuario  -- ✅ CORREGIDO: Usuarios → Usuario
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

        // NUEVO MÉTODO: Obtener ventas por vendedor con información de DNI y productos
        // El error está probablemente en un método como este:
        public List<ReporteVenta> ObtenerVentasPorVendedor(int idVendedor, DateTime fechaInicio, DateTime fechaFin)
        {
            List<ReporteVenta> lista = new List<ReporteVenta>();

            using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
            {
                try
                {
                    oconexion.Open();
                    SqlCommand comando = new SqlCommand(@"
                SELECT 
                    v.IdVenta, 
                    v.Fecha, 
                    c.Nombre + ' ' + c.Apellido as Cliente,
                    c.Documento as DNI,
                    u.NombreUsuario as Usuario,
                    v.Total,
                    v.IdUsuario,
                    STUFF((
                        SELECT ', ' + p.Nombre + ' (' + CAST(dv.Cantidad AS VARCHAR) + ')'
                        FROM DetalleVenta dv
                        INNER JOIN Producto p ON dv.IdProducto = p.IdProducto
                        WHERE dv.IdVenta = v.IdVenta
                        FOR XML PATH('')
                    ), 1, 2, '') as Productos,
                    (SELECT COUNT(*) FROM DetalleVenta dv WHERE dv.IdVenta = v.IdVenta) as CantidadProductos
                FROM Venta v
                INNER JOIN Cliente c ON v.IdCliente = c.IdCliente
                INNER JOIN Usuario u ON v.IdUsuario = u.IdUsuario
                WHERE v.Fecha BETWEEN @FechaInicio AND @FechaFin
                AND v.IdUsuario = @IdVendedor  -- ✅ Aquí debe usar el parámetro correcto
                ORDER BY v.Fecha DESC", oconexion);

                    comando.Parameters.AddWithValue("@FechaInicio", fechaInicio);
                    comando.Parameters.AddWithValue("@FechaFin", fechaFin);
                    comando.Parameters.AddWithValue("@IdVendedor", idVendedor);  // ✅ Usar el parámetro del método

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
                                Productos = reader["Productos"] != DBNull.Value ? reader["Productos"].ToString() : "Sin productos",
                                CantidadProductos = (int)reader["CantidadProductos"]
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Error al obtener ventas por vendedor: " + ex.Message);
                }
            }

            return lista;
        }

        // NUEVO MÉTODO: Obtener ventas por vendedor con filtros adicionales
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
                                DNI = dr["DniCliente"] != DBNull.Value ? dr["DniCliente"].ToString() : "",
                                Usuario = dr["Usuario"].ToString(),
                                Total = Convert.ToDecimal(dr["Total"]),
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
                    SqlCommand cmd = new SqlCommand("sp_VentasPorCliente", oconexion);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@IdCliente", idCliente);

                    oconexion.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lista.Add(new ReporteVenta()
                            {
                                IdVenta = Convert.ToInt32(dr["IdVenta"]),
                                Fecha = Convert.ToDateTime(dr["Fecha"]),
                                Total = Convert.ToDecimal(dr["Total"])
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

        public List<ReporteStock> ObtenerProductosStockBajo()
        {
            List<ReporteStock> lista = new List<ReporteStock>();

            using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("sp_ProductosStockBajo", oconexion);
                    cmd.CommandType = CommandType.StoredProcedure;

                    oconexion.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            var producto = new ReporteStock()
                            {
                                IdProducto = Convert.ToInt32(dr["IdProducto"]),
                                Nombre = dr["Nombre"].ToString(),
                                StockActual = Convert.ToInt32(dr["StockActual"]),
                                StockMinimo = Convert.ToInt32(dr["StockMinimo"]),
                                Estado = "CRÍTICO"
                            };

                            if (producto.StockActual == producto.StockMinimo)
                                producto.Estado = "ALERTA";
                            else if (producto.StockActual < producto.StockMinimo)
                                producto.Estado = "CRÍTICO";

                            lista.Add(producto);
                        }
                    }
                }
                catch (Exception ex)
                {
                    lista = new List<ReporteStock>();
                }
            }
            return lista;
        }

        public List<ReporteStock> ObtenerTodosProductosConStock()
        {
            List<ReporteStock> lista = new List<ReporteStock>();

            using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(@"
                        SELECT p.IdProducto, p.Nombre, p.Tipo, p.StockActual, p.StockMinimo, p.PrecioVenta,
                               CASE 
                                   WHEN p.StockActual <= p.StockMinimo THEN 'CRÍTICO'
                                   WHEN p.StockActual <= p.StockMinimo * 1.5 THEN 'ALERTA' 
                                   ELSE 'NORMAL'
                               END as Estado
                        FROM Producto p
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
                    lista = new List<ReporteStock>();
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
        public List<ReporteVentaPorTipo> ObtenerVentasPorTipo(DateTime fechaInicio, DateTime fechaFin)
        {
            List<ReporteVentaPorTipo> lista = new List<ReporteVentaPorTipo>();

            using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
            {
                try
                {
                    string query = @"
                SELECT 
                    p.Tipo,
                    SUM(dv.Cantidad) as Cantidad,
                    SUM(dv.Cantidad * dv.PrecioVenta) as Total
                FROM DetalleVenta dv
                INNER JOIN Venta v ON dv.IdVenta = v.IdVenta
                INNER JOIN Producto p ON dv.IdProducto = p.IdProducto
                WHERE v.Fecha BETWEEN @FechaInicio AND @FechaFin
                GROUP BY p.Tipo
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
                    lista = new List<ReporteVentaPorTipo>();
                }
            }
            return lista;
        }

        public List<ReporteTopCliente> ObtenerTopClientes(DateTime fechaInicio, DateTime fechaFin, int top = 5)
        {
            List<ReporteTopCliente> lista = new List<ReporteTopCliente>();

            using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
            {
                try
                {
                    string query = @"
                SELECT TOP (@Top) 
                    c.Nombre + ' ' + ISNULL(c.Apellido, '') as Nombre,
                    COUNT(v.IdVenta) as CantidadCompras,
                    SUM(v.Total) as TotalGastado
                FROM Venta v
                INNER JOIN Cliente c ON v.IdCliente = c.IdCliente
                WHERE v.Fecha BETWEEN @FechaInicio AND @FechaFin
                GROUP BY c.Nombre, c.Apellido
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
                    lista = new List<ReporteTopCliente>();
                }
            }
            return lista;
        }

        public List<ReporteProductoVendido> ObtenerProductosMasVendidos(DateTime fechaInicio, DateTime fechaFin, int top = 5)
        {
            List<ReporteProductoVendido> lista = new List<ReporteProductoVendido>();

            using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
            {
                try
                {
                    string query = @"
                SELECT TOP (@Top) 
                    p.Nombre as NombreProducto,
                    SUM(dv.Cantidad) as CantidadVendida,
                    SUM(dv.Cantidad * dv.PrecioVenta) as TotalVendido
                FROM DetalleVenta dv
                INNER JOIN Venta v ON dv.IdVenta = v.IdVenta
                INNER JOIN Producto p ON dv.IdProducto = p.IdProducto
                WHERE v.Fecha BETWEEN @FechaInicio AND @FechaFin
                GROUP BY p.Nombre
                ORDER BY CantidadVendida DESC";

                    SqlCommand cmd = new SqlCommand(query, oconexion);
                    cmd.Parameters.AddWithValue("@FechaInicio", fechaInicio);
                    cmd.Parameters.AddWithValue("@FechaFin", fechaFin);
                    cmd.Parameters.AddWithValue("@Top", top);

                    oconexion.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lista.Add(new ReporteProductoVendido()
                            {
                                NombreProducto = dr["NombreProducto"].ToString(),
                                CantidadVendida = Convert.ToInt32(dr["CantidadVendida"]),
                                TotalVendido = Convert.ToDecimal(dr["TotalVendido"])
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    lista = new List<ReporteProductoVendido>();
                }
            }
            return lista;
        }
    }
}