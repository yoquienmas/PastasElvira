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
                    SqlCommand cmd = new SqlCommand("sp_ReporteVentasPorFecha", oconexion);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@FechaInicio", fechaInicio);
                    cmd.Parameters.AddWithValue("@FechaFin", fechaFin);

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
                                Usuario = dr["Usuario"].ToString(),
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

        // NUEVO MÉTODO: Obtener ventas por vendedor con información de DNI y productos
        public List<ReporteVenta> ObtenerVentasPorVendedor(int idVendedor, DateTime fechaInicio, DateTime fechaFin)
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
                            c.DNI as DniCliente,
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
                        AND v.IdUsuario = @IdVendedor
                        ORDER BY v.Fecha DESC";

                    SqlCommand cmd = new SqlCommand(query, oconexion);
                    cmd.Parameters.AddWithValue("@FechaInicio", fechaInicio.Date);
                    cmd.Parameters.AddWithValue("@FechaFin", fechaFin.Date);
                    cmd.Parameters.AddWithValue("@IdVendedor", idVendedor);

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
                            c.DNI as DniCliente,
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
    }
}