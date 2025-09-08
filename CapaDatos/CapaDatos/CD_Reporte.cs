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