using CapaEntidad;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;

namespace CapaDatos
{
    public class CD_Venta
    {
        private SqlConnection conexion = new SqlConnection(Conexion.cadena);

        public bool VerificarStockDisponible(int idProducto, int cantidad)
        {
            using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
            {
                oconexion.Open();
                using (var command = new SqlCommand())
                {
                    command.Connection = oconexion;
                    command.CommandText = "SELECT StockActual FROM Producto WHERE IdProducto = @IdProducto";
                    command.Parameters.AddWithValue("@IdProducto", idProducto);

                    int stockActual = Convert.ToInt32(command.ExecuteScalar());
                    return stockActual >= cantidad;
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

                    return command.ExecuteNonQuery() > 0;
                }
            }
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

        public bool Registrar(Venta venta, out string mensaje)
        {
            try
            {
                using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
                {
                    oconexion.Open();
                    using (var command = new SqlCommand())
                    {
                        command.Connection = oconexion;
                        command.CommandText = @"
                    INSERT INTO Venta (Fecha, IdCliente, IdUsuario, Total, MetodoPago)
                    VALUES (@Fecha, @IdCliente, @IdUsuario, @Total, @MetodoPago);
                    SELECT SCOPE_IDENTITY();";

                        command.Parameters.AddWithValue("@Fecha", venta.FechaVenta);
                        command.Parameters.AddWithValue("@IdCliente", venta.IdCliente);
                        command.Parameters.AddWithValue("@IdUsuario", venta.IdVendedor);
                        command.Parameters.AddWithValue("@Total", venta.Total);
                        command.Parameters.AddWithValue("@MetodoPago", venta.MetodoPagoId); // ✅ USAR MetodoPagoId

                        venta.IdVenta = Convert.ToInt32(command.ExecuteScalar());

                        // Registrar detalles de venta
                        foreach (var item in venta.Items)
                        {
                            using (var commandDetalle = new SqlCommand())
                            {
                                commandDetalle.Connection = oconexion;
                                commandDetalle.CommandText = @"
                            INSERT INTO DetalleVenta (IdVenta, IdProducto, Cantidad, PrecioUnitario)
                            VALUES (@IdVenta, @IdProducto, @Cantidad, @PrecioUnitario)";

                                commandDetalle.Parameters.AddWithValue("@IdVenta", venta.IdVenta);
                                commandDetalle.Parameters.AddWithValue("@IdProducto", item.IdProducto);
                                commandDetalle.Parameters.AddWithValue("@Cantidad", item.Cantidad);
                                commandDetalle.Parameters.AddWithValue("@PrecioUnitario", item.PrecioUnitario);

                                commandDetalle.ExecuteNonQuery();

                                // Actualizar stock del producto
                                using (var commandStock = new SqlCommand())
                                {
                                    commandStock.Connection = oconexion;
                                    commandStock.CommandText = "UPDATE Producto SET StockActual = StockActual - @Cantidad WHERE IdProducto = @IdProducto";
                                    commandStock.Parameters.AddWithValue("@Cantidad", item.Cantidad);
                                    commandStock.Parameters.AddWithValue("@IdProducto", item.IdProducto);
                                    commandStock.ExecuteNonQuery();
                                }
                            }
                        }

                        mensaje = "Venta registrada exitosamente";
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                mensaje = $"Error al registrar venta: {ex.Message}";
                return false;
            }
        }

        public List<ItemVenta> ObtenerDetallesVenta(int idVenta)
        {
            List<ItemVenta> detalles = new List<ItemVenta>();

            using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
            {
                try
                {
                    oconexion.Open();
                    SqlCommand comando = new SqlCommand(@"
                SELECT dv.IdProducto, 
                       CASE 
                           WHEN p.Tipo IS NULL OR p.Tipo = '' OR p.Tipo = 'NULL' 
                           THEN p.Nombre
                           ELSE p.Tipo + ' - ' + p.Nombre 
                       END as NombreProducto,
                       dv.Cantidad, dv.PrecioUnitario, dv.Subtotal
                FROM DetalleVenta dv
                INNER JOIN Producto p ON dv.IdProducto = p.IdProducto
                WHERE dv.IdVenta = @IdVenta", oconexion);

                    comando.Parameters.AddWithValue("@IdVenta", idVenta);

                    using (SqlDataReader reader = comando.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            detalles.Add(new ItemVenta
                            {
                                IdProducto = (int)reader["IdProducto"],
                                NombreProducto = reader["NombreProducto"].ToString(),
                                Cantidad = (int)reader["Cantidad"],
                                PrecioUnitario = Convert.ToDecimal(reader["PrecioUnitario"]),
                                // Subtotal se calcula automáticamente en la propiedad
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Error al obtener detalles de venta: " + ex.Message);
                }
            }

            return detalles;
        }

        // En CD_Reporte o donde obtengas las ventas para el historial
        public List<ReporteVenta> ObtenerVentasPorFecha(DateTime fechaInicio, DateTime fechaFin)
        {
            List<ReporteVenta> ventas = new List<ReporteVenta>();

            using (SqlConnection conexion = new SqlConnection(Conexion.cadena))
            {
                try
                {
                    conexion.Open();
                    string query = @"
                SELECT 
                    v.IdVenta, 
                    v.Fecha, 
                    v.Total, 
                    v.IdUsuario, 
                    mp.Nombre as MetodoPago, -- ✅ OBTENER NOMBRE
                    c.Nombre as Cliente, 
                    c.DNI, 
                    u.Nombre as Usuario
                FROM Venta v 
                LEFT JOIN Clientes c ON v.IdCliente = c.IdCliente
                LEFT JOIN Usuarios u ON v.IdUsuario = u.IdUsuario
                LEFT JOIN MetodoPago mp ON v.MetodoPago = mp.IdMetodoPago -- ✅ JOIN NUEVO
                WHERE v.Fecha BETWEEN @FechaInicio AND @FechaFin
                ORDER BY v.Fecha DESC";

                    using (SqlCommand cmd = new SqlCommand(query, conexion))
                    {
                        cmd.Parameters.AddWithValue("@FechaInicio", fechaInicio);
                        cmd.Parameters.AddWithValue("@FechaFin", fechaFin);

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
                                    MetodoPago = Convert.ToInt32(reader["MetodoPago"]),
                                    Cliente = reader["Cliente"]?.ToString() ?? "CONSUMIDOR FINAL",
                                    DNI = reader["DNI"]?.ToString() ?? "",
                                    Usuario = reader["Usuario"]?.ToString() ?? ""
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

    }
}