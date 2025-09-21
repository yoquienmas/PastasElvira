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
                            INSERT INTO Ventas (FechaVenta, Total)
                            VALUES (@FechaVenta, @Total);
                            SELECT SCOPE_IDENTITY();";
                        command.Parameters.AddWithValue("@FechaVenta", venta.FechaVenta);
                        command.Parameters.AddWithValue("@Total", venta.Total);

                        venta.IdVenta = Convert.ToInt32(command.ExecuteScalar());

                        // Registrar detalles de venta
                        foreach (var item in venta.Items)
                        {
                            using (var commandDetalle = new SqlCommand())
                            {
                                commandDetalle.Connection = oconexion;
                                commandDetalle.CommandText = @"
                                    INSERT INTO DetalleVenta (IdVenta, IdProducto, Cantidad, PrecioUnitario, Subtotal)
                                    VALUES (@IdVenta, @IdProducto, @Cantidad, @PrecioUnitario, @Subtotal)";
                                commandDetalle.Parameters.AddWithValue("@IdVenta", venta.IdVenta);
                                commandDetalle.Parameters.AddWithValue("@IdProducto", item.IdProducto);
                                commandDetalle.Parameters.AddWithValue("@Cantidad", item.Cantidad);
                                commandDetalle.Parameters.AddWithValue("@PrecioUnitario", item.PrecioUnitario);
                                commandDetalle.Parameters.AddWithValue("@Subtotal", item.Subtotal);

                                commandDetalle.ExecuteNonQuery();
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
                SELECT dv.IdProducto, p.Nombre as NombreProducto, 
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
    }
}