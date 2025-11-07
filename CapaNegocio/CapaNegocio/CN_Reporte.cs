using CapaDatos;
using CapaEntidad;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;

namespace CapaNegocio
{
    public class CN_Reporte
    {
        private CD_Reporte cdReporte = new CD_Reporte();

        public List<ReporteVenta> ObtenerVentasPorFecha(DateTime fechaInicio, DateTime fechaFin)
        {
            return cdReporte.ObtenerVentasPorFecha(fechaInicio, fechaFin);
        }

        public decimal ObtenerTotalVentasPeriodo(DateTime fechaInicio, DateTime fechaFin)
        {
            return cdReporte.ObtenerTotalVentasPeriodo(fechaInicio, fechaFin);
        }

        public List<ReporteStock> ObtenerProductosStockBajo()
        {
            return cdReporte.ObtenerProductosStockBajo();
        }

        public int ObtenerCantidadVentasPeriodo(DateTime fechaInicio, DateTime fechaFin)
        {
            return cdReporte.ObtenerCantidadVentasPeriodo(fechaInicio, fechaFin);
        }

        public List<ReporteConsumo> ObtenerConsumoMateriaPrima(DateTime fechaInicio, DateTime fechaFin)
        {
            return cdReporte.ObtenerConsumoMateriaPrima(fechaInicio, fechaFin);
        }

        public List<ReporteStock> ObtenerTodosProductosConStock()
        {
            return cdReporte.ObtenerTodosProductosConStock();
        }

        public List<ReporteVenta> ObtenerVentasPorCliente(int idCliente)
        {
            return cdReporte.ObtenerVentasPorCliente(idCliente);
        }
        public List<ReporteVenta> ObtenerVentasPorVendedor(int idUsuario, DateTime fechaInicio, DateTime fechaFin)
        {
            return cdReporte.ObtenerVentasPorVendedor(idUsuario, fechaInicio, fechaFin);
        }

        
        public DataTable DiagnosticarVentasEnBD(int idUsuario, DateTime fechaInicio, DateTime fechaFin)
        {
            DataTable resultado = new DataTable();
            resultado.Columns.Add("Tipo", typeof(string));
            resultado.Columns.Add("Valor", typeof(string));

            using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
            {
                try
                {
                    oconexion.Open();

                    SqlCommand cmdCount = new SqlCommand("SELECT COUNT(*) FROM Venta", oconexion);
                    int totalVentas = (int)cmdCount.ExecuteScalar();
                    resultado.Rows.Add("Total ventas en BD", totalVentas.ToString());

                    SqlCommand cmdVendedor = new SqlCommand(@"
                        SELECT COUNT(*) FROM Venta 
                        WHERE IdUsuario = @IdUsuario 
                        AND Fecha BETWEEN @FechaInicio AND @FechaFin", oconexion);

                    cmdVendedor.Parameters.AddWithValue("@IdUsuario", idUsuario);
                    cmdVendedor.Parameters.AddWithValue("@FechaInicio", fechaInicio);
                    cmdVendedor.Parameters.AddWithValue("@FechaFin", fechaFin);

                    int ventasVendedor = (int)cmdVendedor.ExecuteScalar();
                    resultado.Rows.Add($"Ventas del vendedor {idUsuario}", ventasVendedor.ToString());

                    SqlCommand cmdEstructura = new SqlCommand(@"
                        SELECT TABLE_NAME, COLUMN_NAME, DATA_TYPE 
                        FROM INFORMATION_SCHEMA.COLUMNS 
                        WHERE TABLE_NAME IN ('Venta', 'Cliente', 'Usuario') 
                        ORDER BY TABLE_NAME, ORDINAL_POSITION", oconexion);

                    using (SqlDataReader reader = cmdEstructura.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string columna = $"{reader["TABLE_NAME"]}.{reader["COLUMN_NAME"]} ({reader["DATA_TYPE"]})";
                            resultado.Rows.Add("Estructura", columna);
                        }
                    }

                    SqlCommand cmdEjemplo = new SqlCommand(@"
                        SELECT TOP 5 v.IdVenta, v.Fecha, c.Nombre, u.NombreUsuario, v.Total 
                        FROM Venta v 
                        LEFT JOIN Cliente c ON v.IdCliente = c.IdCliente 
                        LEFT JOIN Usuario u ON v.IdUsuario = u.IdUsuario 
                        ORDER BY v.Fecha DESC", oconexion);

                    using (SqlDataReader reader = cmdEjemplo.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string ventaEjemplo = $"ID: {reader["IdVenta"]}, Fecha: {reader["Fecha"]}, Cliente: {reader["Nombre"]}, Vendedor: {reader["NombreUsuario"]}, Total: {reader["Total"]}";
                            resultado.Rows.Add("Venta ejemplo", ventaEjemplo);
                        }
                    }
                }
                catch (Exception ex)
                {
                    resultado.Rows.Add("Error diagnóstico", ex.Message);
                }
            }
            return resultado;
        }

        // Método simplificado para el dashboard
        public List<ReporteVenta> ObtenerVentasPorVendedor(int idUsuario)
        {
            return ObtenerVentasPorVendedor(idUsuario, DateTime.Now.AddMonths(-1), DateTime.Now);
        }

        public List<ReporteVenta> ObtenerTodasLasVentas(DateTime fechaInicio, DateTime fechaFin)
        {
            return cdReporte.ObtenerVentasPorFecha(fechaInicio, fechaFin);
        }

        public List<ConsumoMateriaPrima> ObtenerConsumoMateriaPrimaPorVenta(int idVenta)
        {
            return cdReporte.ObtenerConsumoMateriaPrimaPorVenta(idVenta);
        }

        // ✅ MÉTODOS CORREGIDOS - SOLO UNA VERSIÓN DE CADA UNO

        public List<ReporteVentaPorTipo> ObtenerVentasPorTipo(DateTime inicio, DateTime fin)
        {
            return cdReporte.ObtenerVentasPorTipo(inicio, fin);
        }

        public List<ReporteTopCliente> ObtenerTopClientes(DateTime inicio, DateTime fin, int cantidad)
        {
            return cdReporte.ObtenerTopClientes(inicio, fin, cantidad);
        }

        // EN CN_Reporte.cs - ACTUALIZAR el método
        public List<ReporteProductoVendido> ObtenerProductosMasVendidos(DateTime fechaInicio, DateTime fechaFin, int top = 5, string tipoProducto = null)
        {
            return cdReporte.ObtenerProductosMasVendidos(fechaInicio, fechaFin, top, tipoProducto);
        }

        public ReporteVenta ObtenerVentaPorId(int idVenta)
        {
            return cdReporte.ObtenerVentaPorId(idVenta);
        }

        public List<string> ObtenerTiposProducto()
        {
            return cdReporte.ObtenerTiposProducto();
        }
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

    }
}