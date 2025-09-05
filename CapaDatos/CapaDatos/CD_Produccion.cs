using CapaEntidad;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;

namespace CapaDatos
{
    public class CD_Produccion
    {
        public List<Produccion> Listar()
        {
            List<Produccion> lista = new List<Produccion>();

            using (SqlConnection conexion = new SqlConnection(Conexion.cadena))
            {
                try
                {
                    string query = @"SELECT p.IdProduccion, p.Fecha, p.CantidadProducida, 
                                            p.IdProducto, pr.Nombre AS NombreProducto
                                     FROM Produccion p
                                     INNER JOIN Producto pr ON pr.IdProducto = p.IdProducto";

                    SqlCommand cmd = new SqlCommand(query, conexion);
                    cmd.CommandType = CommandType.Text;

                    conexion.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lista.Add(new Produccion
                            {
                                IdProduccion = Convert.ToInt32(dr["IdProduccion"]),
                                Fecha = Convert.ToDateTime(dr["Fecha"]),
                                CantidadProducida = Convert.ToInt32(dr["CantidadProducida"]),
                                IdProducto = Convert.ToInt32(dr["IdProducto"]),
                                NombreProducto = dr["NombreProducto"].ToString()
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Manejo de error básico
                    lista = new List<Produccion>();
                }
            }

            return lista;
        }

        public int Registrar(Produccion obj, out string mensaje)
        {
            int idAutogenerado = 0;
            mensaje = string.Empty;

            try
            {
                using (SqlConnection conexion = new SqlConnection(Conexion.cadena))
                {
                    SqlCommand cmd = new SqlCommand("SP_REGISTRAR_PRODUCCION", conexion);
                    cmd.Parameters.AddWithValue("IdProducto", obj.IdProducto);
                    cmd.Parameters.AddWithValue("CantidadProducida", obj.CantidadProducida);
                    cmd.Parameters.AddWithValue("Fecha", obj.Fecha);

                    cmd.Parameters.Add("Resultado", SqlDbType.Int).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("Mensaje", SqlDbType.VarChar, 500).Direction = ParameterDirection.Output;
                    cmd.CommandType = CommandType.StoredProcedure;

                    conexion.Open();
                    cmd.ExecuteNonQuery();

                    idAutogenerado = Convert.ToInt32(cmd.Parameters["Resultado"].Value);
                    mensaje = cmd.Parameters["Mensaje"].Value.ToString();
                }
            }
            catch (Exception ex)
            {
                idAutogenerado = 0;
                mensaje = ex.Message;
            }

            return idAutogenerado;
        }
    }
}
