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
        private string conexion = "Server=localhost;Database=PastasElvira;Integrated Security=true;"; // Ajusta tu cadena

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

        public List<ReporteVenta> ObtenerVentasPorVendedor(int idUsuario, DateTime fechaInicio, DateTime fechaFin,
                                                            string dniCliente = "", string nombreProducto = "")
        {
            List<ReporteVenta> lista = new List<ReporteVenta>();

            using (SqlConnection oconexion = new SqlConnection(conexion))
            {
                try
                {
                    // ✅ CONSULTA CORREGIDA
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

                    // Agregar filtros opcionales
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
                    // ✅ CORREGIDO: Usar throw en lugar de MessageBox
                    throw new Exception($"Error al obtener ventas por vendedor: {ex.Message}");
                }
            }
            return lista;
        }

        // ✅ MÉTODO DE DIAGNÓSTICO CORREGIDO (sin MessageBox)
        public DataTable DiagnosticarVentasEnBD(int idUsuario, DateTime fechaInicio, DateTime fechaFin)
        {
            DataTable resultado = new DataTable();
            resultado.Columns.Add("Tipo", typeof(string));
            resultado.Columns.Add("Valor", typeof(string));

            using (SqlConnection oconexion = new SqlConnection(conexion))
            {
                try
                {
                    oconexion.Open();

                    // 1. Verificar si hay ventas en general
                    SqlCommand cmdCount = new SqlCommand("SELECT COUNT(*) FROM Venta", oconexion);
                    int totalVentas = (int)cmdCount.ExecuteScalar();
                    resultado.Rows.Add("Total ventas en BD", totalVentas.ToString());

                    // 2. Verificar ventas específicas del vendedor
                    SqlCommand cmdVendedor = new SqlCommand(@"
                        SELECT COUNT(*) FROM Venta 
                        WHERE IdUsuario = @IdUsuario 
                        AND Fecha BETWEEN @FechaInicio AND @FechaFin", oconexion);

                    cmdVendedor.Parameters.AddWithValue("@IdUsuario", idUsuario);
                    cmdVendedor.Parameters.AddWithValue("@FechaInicio", fechaInicio);
                    cmdVendedor.Parameters.AddWithValue("@FechaFin", fechaFin);

                    int ventasVendedor = (int)cmdVendedor.ExecuteScalar();
                    resultado.Rows.Add($"Ventas del vendedor {idUsuario}", ventasVendedor.ToString());

                    // 3. Verificar estructura de tablas
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

                    // 4. Verificar algunas ventas de ejemplo
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

        // Método simplificado para el dashboard (sin filtros de fecha)
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
        // En tu clase CN_Reporte, agrega estos métodos:
        public List<dynamic> ObtenerVentasPorTipo(DateTime inicio, DateTime fin)
        {
            // Implementar lógica para obtener ventas por tipo
            return new List<dynamic>();
        }

        public List<dynamic> ObtenerTopClientes(DateTime inicio, DateTime fin, int cantidad)
        {
            // Implementar lógica para obtener top clientes
            return new List<dynamic>();
        }

    }
}