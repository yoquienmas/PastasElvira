using System;
using System.Collections.Generic;
using System.Data;
using CapaEntidad;
using Microsoft.Data.SqlClient;

namespace CapaDatos
{
    public class CD_Producto
    {
        public List<Producto> Listar()
        {
            List<Producto> lista = new List<Producto>();

            try
            {
                using (SqlConnection conexion = new SqlConnection(Conexion.cadena))
                {
                    string query = "SELECT IdProducto, Nombre, Tipo, CostoProduccion, MargenGanancia, PrecioVenta, StockActual, StockMinimo, Visible FROM Producto";

                    SqlCommand cmd = new SqlCommand(query, conexion);
                    cmd.CommandType = CommandType.Text;

                    conexion.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lista.Add(new Producto()
                            {
                                IdProducto = Convert.ToInt32(dr["IdProducto"]),
                                Nombre = dr["Nombre"].ToString(),
                                Tipo = dr["Tipo"].ToString(),
                                CostoProduccion = Convert.ToDecimal(dr["CostoProduccion"]),
                                MargenGanancia = Convert.ToDecimal(dr["MargenGanancia"]),
                                PrecioVenta = Convert.ToDecimal(dr["PrecioVenta"]),
                                StockActual = Convert.ToInt32(dr["StockActual"]),
                                StockMinimo = Convert.ToInt32(dr["StockMinimo"]),
                                Visible = Convert.ToBoolean(dr["Visible"])
                            });
                        }
                    }
                }
            }
            catch (Exception)
            {
                lista = new List<Producto>();
            }

            return lista;
        }

        public int Registrar(Producto obj, out string mensaje)
        {
            int idProductoGenerado = 0;
            mensaje = string.Empty;

            try
            {
                using (SqlConnection conexion = new SqlConnection(Conexion.cadena))
                {
                    string query = @"INSERT INTO Producto (Nombre, Tipo, CostoProduccion, MargenGanancia, PrecioVenta, StockActual, StockMinimo, Visible)
                                     VALUES (@Nombre, @Tipo, @CostoProduccion, @MargenGanancia, @PrecioVenta, @StockActual, @StockMinimo, @Visible);
                                     SELECT SCOPE_IDENTITY();";

                    SqlCommand cmd = new SqlCommand(query, conexion);
                    cmd.Parameters.AddWithValue("@Nombre", obj.Nombre);
                    cmd.Parameters.AddWithValue("@Tipo", obj.Tipo);
                    cmd.Parameters.AddWithValue("@CostoProduccion", obj.CostoProduccion);
                    cmd.Parameters.AddWithValue("@MargenGanancia", obj.MargenGanancia);
                    // ✅ CONVERSIÓN EXPLÍCITA de double a decimal
                    cmd.Parameters.AddWithValue("@PrecioVenta", Convert.ToDecimal(obj.PrecioVenta));
                    cmd.Parameters.AddWithValue("@StockActual", obj.StockActual);
                    cmd.Parameters.AddWithValue("@StockMinimo", obj.StockMinimo);
                    cmd.Parameters.AddWithValue("@Visible", obj.Visible);

                    conexion.Open();
                    idProductoGenerado = Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
            catch (Exception ex)
            {
                idProductoGenerado = 0;
                mensaje = "Error al registrar producto: " + ex.Message;
            }

            return idProductoGenerado;
        }

        public bool Editar(Producto obj, out string mensaje)
        {
            bool resultado = false;
            mensaje = string.Empty;

            try
            {
                using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
                {
                    string query = @"UPDATE Producto 
                                     SET Nombre = @Nombre, 
                                         Tipo = @Tipo,
                                         CostoProduccion = @CostoProduccion,
                                         MargenGanancia = @MargenGanancia,
                                         PrecioVenta = @PrecioVenta,
                                         StockActual = @StockActual,
                                         StockMinimo = @StockMinimo,
                                         Visible = @Visible
                                     WHERE IdProducto = @IdProducto";

                    SqlCommand cmd = new SqlCommand(query, oconexion);
                    cmd.Parameters.AddWithValue("@IdProducto", obj.IdProducto);
                    cmd.Parameters.AddWithValue("@Nombre", obj.Nombre);
                    cmd.Parameters.AddWithValue("@Tipo", obj.Tipo);
                    cmd.Parameters.AddWithValue("@CostoProduccion", obj.CostoProduccion);
                    cmd.Parameters.AddWithValue("@MargenGanancia", obj.MargenGanancia);
                    // ✅ CONVERSIÓN EXPLÍCITA de double a decimal
                    cmd.Parameters.AddWithValue("@PrecioVenta", Convert.ToDecimal(obj.PrecioVenta));
                    cmd.Parameters.AddWithValue("@StockActual", obj.StockActual);
                    cmd.Parameters.AddWithValue("@StockMinimo", obj.StockMinimo);
                    cmd.Parameters.AddWithValue("@Visible", obj.Visible);

                    oconexion.Open();
                    resultado = cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                resultado = false;
                mensaje = "Error al editar producto: " + ex.Message;
            }

            return resultado;
        }

        public bool Eliminar(int idProducto, out string mensaje)
        {
            bool resultado = false;
            mensaje = string.Empty;

            try
            {
                using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
                {
                    string query = "DELETE FROM Producto WHERE IdProducto = @IdProducto";

                    SqlCommand cmd = new SqlCommand(query, oconexion);
                    cmd.Parameters.AddWithValue("@IdProducto", idProducto);

                    oconexion.Open();
                    resultado = cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                resultado = false;
                mensaje = "Error al eliminar producto: " + ex.Message;
            }

            return resultado;
        }

        public Producto ObtenerProducto(int idProducto)
        {
            Producto producto = new Producto();

            using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
            {
                SqlCommand cmd = new SqlCommand("SELECT * FROM Producto WHERE IdProducto = @id", oconexion);
                cmd.Parameters.AddWithValue("@id", idProducto);

                oconexion.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        producto = new Producto()
                        {
                            IdProducto = Convert.ToInt32(dr["IdProducto"]),
                            Nombre = dr["Nombre"].ToString(),
                            Tipo = dr["Tipo"].ToString(),
                            CostoProduccion = Convert.ToDecimal(dr["CostoProduccion"]),
                            MargenGanancia = Convert.ToDecimal(dr["MargenGanancia"]),
                            PrecioVenta = Convert.ToDecimal(dr["PrecioVenta"]),
                            StockActual = Convert.ToInt32(dr["StockActual"]),
                            StockMinimo = Convert.ToInt32(dr["StockMinimo"]),
                            Visible = Convert.ToBoolean(dr["Visible"])
                        };
                    }
                }
            }
            return producto;
        }

        public bool CalcularCostoProducto(int idProducto, out decimal costoTotal, out decimal precioSugerido, out string mensaje)
        {
            costoTotal = 0;
            precioSugerido = 0;
            mensaje = string.Empty;

            try
            {
                using (SqlConnection conexion = new SqlConnection(Conexion.cadena))
                {
                    SqlCommand cmd = new SqlCommand("CalcularCostoProducto", conexion);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@IdProducto", idProducto);

                    // Parámetros de salida
                    cmd.Parameters.Add("@CostoTotal", SqlDbType.Decimal).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("@PrecioSugerido", SqlDbType.Decimal).Direction = ParameterDirection.Output;

                    conexion.Open();
                    cmd.ExecuteNonQuery();

                    costoTotal = Convert.ToDecimal(cmd.Parameters["@CostoTotal"].Value);
                    precioSugerido = Convert.ToDecimal(cmd.Parameters["@PrecioSugerido"].Value);
                    mensaje = "Costo calculado correctamente";

                    return true;
                }
            }
            catch (Exception ex)
            {
                mensaje = ex.Message;
                return false;
            }
        }
    }
}