using CapaDatos;
using CapaEntidad;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;

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

        // MÉTODOS NUEVOS QUE FALTAN:
        public List<ReporteStock> ObtenerTodosProductosConStock()
        {
            return cdReporte.ObtenerTodosProductosConStock();
        }

        public List<ReporteVenta> ObtenerVentasPorCliente(int idCliente)
        {
            return cdReporte.ObtenerVentasPorCliente(idCliente);
        }
        private string conexion = "TuCadenaDeConexion"; // Reemplaza con tu cadena de conexión

        public List<ReporteVenta> ObtenerVentasPorVendedor(int idVendedor, DateTime fechaInicio, DateTime fechaFin,
                                                         string dniCliente = "", string nombreProducto = "")
        {
            List<ReporteVenta> lista = new List<ReporteVenta>();

            using (SqlConnection oconexion = new SqlConnection(conexion))
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
                                WHERE dv.IdVenta = v.IdVenta AND p.Nombre LIKE @NombreProducto
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
                                DNI = dr["DNI"].ToString(),
                                Usuario = dr["Usuario"].ToString(),
                                Total = Convert.ToDecimal(dr["Total"]),
                                Productos = dr["Productos"].ToString()
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Puedes loggear el error aquí
                    Console.WriteLine($"Error: {ex.Message}");
                    lista = new List<ReporteVenta>();
                }
            }
            return lista;

        }

        // Otros métodos que puedas necesitar...
        public List<ReporteVenta> ObtenerTodasLasVentas(DateTime fechaInicio, DateTime fechaFin)
        {
            // Implementación similar para obtener todas las ventas
            return new List<ReporteVenta>();
        }
        public List<ConsumoMateriaPrima> ObtenerConsumoMateriaPrimaPorVenta(int idVenta)
        {
            return cdReporte.ObtenerConsumoMateriaPrimaPorVenta(idVenta);
        }
    }
}