using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using CapaEntidad;

namespace CapaDatos
{
    public class CD_Producto
    {
        public List<Producto> Listar()
        {
            List<Producto> lista = new List<Producto>();

            using (SqlConnection con = new SqlConnection(Conexion.cadena))
            {
                string query = "SELECT IdProducto, Nombre, Tipo, PrecioVenta, StockActual FROM Producto";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.CommandType = CommandType.Text;

                con.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new Producto
                        {
                            IdProducto = Convert.ToInt32(dr["IdProducto"]),
                            Nombre = dr["Nombre"].ToString(),
                            Tipo = dr["Tipo"].ToString(),
                            PrecioVenta = Convert.ToSingle(dr["PrecioVenta"]),
                            StockActual = Convert.ToInt32(dr["StockActual"])
                        });
                    }
                }
            }

            return lista;
        }

        public int Registrar(Producto obj, out string mensaje)
        {
            mensaje = "";
            int idGenerado = 0;

            using (SqlConnection con = new SqlConnection(Conexion.cadena))
            {
                string query = "INSERT INTO Producto(Nombre, Tipo, PrecioVenta, StockActual) " +
                               "VALUES(@Nombre, @Tipo, @PrecioVenta, @StockActual); SELECT SCOPE_IDENTITY();";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@Nombre", obj.Nombre);
                cmd.Parameters.AddWithValue("@Tipo", obj.Tipo);
                cmd.Parameters.AddWithValue("@PrecioVenta", obj.PrecioVenta);
                cmd.Parameters.AddWithValue("@StockActual", obj.StockActual);

                try
                {
                    con.Open();
                    idGenerado = Convert.ToInt32(cmd.ExecuteScalar());
                    mensaje = "Producto registrado correctamente.";
                }
                catch (Exception ex)
                {
                    mensaje = "Error: " + ex.Message;
                }
            }

            return idGenerado;
        }

        public bool Editar(Producto obj, out string mensaje)
        {
            mensaje = "";
            bool resultado = false;

            using (SqlConnection con = new SqlConnection(Conexion.cadena))
            {
                string query = "UPDATE Producto SET Nombre=@Nombre, Tipo=@Tipo, PrecioVenta=@PrecioVenta, StockActual=@StockActual " +
                               "WHERE IdProducto=@IdProducto";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@IdProducto", obj.IdProducto);
                cmd.Parameters.AddWithValue("@Nombre", obj.Nombre);
                cmd.Parameters.AddWithValue("@Tipo", obj.Tipo);
                cmd.Parameters.AddWithValue("@PrecioVenta", obj.PrecioVenta);
                cmd.Parameters.AddWithValue("@StockActual", obj.StockActual);

                try
                {
                    con.Open();
                    resultado = cmd.ExecuteNonQuery() > 0;
                    mensaje = "Producto editado correctamente.";
                }
                catch (Exception ex)
                {
                    mensaje = "Error: " + ex.Message;
                }
            }

            return resultado;
        }

        public bool Eliminar(int idProducto, out string mensaje)
        {
            mensaje = "";
            bool resultado = false;

            using (SqlConnection con = new SqlConnection(Conexion.cadena))
            {
                string query = "DELETE FROM Producto WHERE IdProducto=@IdProducto";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@IdProducto", idProducto);

                try
                {
                    con.Open();
                    resultado = cmd.ExecuteNonQuery() > 0;
                    mensaje = "Producto eliminado correctamente.";
                }
                catch (Exception ex)
                {
                    mensaje = "Error: " + ex.Message;
                }
            }

            return resultado;
        }
    }
}
