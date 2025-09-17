using CapaEntidad;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;


namespace CapaDatos
{
    public class CD_DetalleProduccion
    {
        // ✅ Listar detalles de una producción específica
        public List<DetalleProduccion> ListarPorProduccion(int idProduccion)
        {
            List<DetalleProduccion> lista = new List<DetalleProduccion>();

            using (SqlConnection conexion = new SqlConnection(Conexion.cadena))
            {
                try
                {
                    string query = @"SELECT dp.IdDetalleProduccion, dp.IdProduccion, dp.IdMateria, 
                                            dp.CantidadUtilizada, mp.Nombre AS NombreMateria
                                     FROM DetalleProduccion dp
                                     INNER JOIN MateriaPrima mp ON mp.IdMateria = dp.IdMateria
                                     WHERE dp.IdProduccion = @IdProduccion";

                    SqlCommand cmd = new SqlCommand(query, conexion);
                    cmd.Parameters.AddWithValue("@IdProduccion", idProduccion);
                    cmd.CommandType = CommandType.Text;

                    conexion.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lista.Add(new DetalleProduccion
                            {
                                IdDetalleProduccion = Convert.ToInt32(dr["IdDetalleProduccion"]),
                                IdProduccion = Convert.ToInt32(dr["IdProduccion"]),
                                IdMateria = Convert.ToInt32(dr["IdMateria"]),
                                CantidadUtilizada = Convert.ToDecimal(dr["CantidadUtilizada"]),
                                Nombre = dr["Nombre"].ToString()
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    lista = new List<DetalleProduccion>();
                }
            }

            return lista;
        }

        // ✅ Registrar detalle de producción dentro de una transacción
        public int Registrar(DetalleProduccion obj, SqlConnection conexion, SqlTransaction transaccion)
        {
            int idAutogenerado = 0;

            try
            {
                SqlCommand cmd = new SqlCommand("SP_REGISTRAR_DETALLE_PRODUCCION", conexion, transaccion);
                cmd.Parameters.AddWithValue("IdProduccion", obj.IdProduccion);
                cmd.Parameters.AddWithValue("IdMateria", obj.IdMateria);
                cmd.Parameters.AddWithValue("CantidadUtilizada", obj.CantidadUtilizada);

                cmd.Parameters.Add("Resultado", SqlDbType.Int).Direction = ParameterDirection.Output;

                cmd.CommandType = CommandType.StoredProcedure;

                cmd.ExecuteNonQuery();

                idAutogenerado = Convert.ToInt32(cmd.Parameters["Resultado"].Value);
            }
            catch
            {
                idAutogenerado = 0;
            }

            return idAutogenerado;
        }

        // ✅ Eliminar un detalle de producción
        public bool Eliminar(int idDetalleProduccion, out string mensaje)
        {
            bool resultado = false;
            mensaje = string.Empty;

            try
            {
                using (SqlConnection conexion = new SqlConnection(Conexion.cadena))
                {
                    SqlCommand cmd = new SqlCommand("SP_ELIMINAR_DETALLE_PRODUCCION", conexion);
                    cmd.Parameters.AddWithValue("IdDetalleProduccion", idDetalleProduccion);

                    cmd.Parameters.Add("Resultado", SqlDbType.Bit).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("Mensaje", SqlDbType.VarChar, 500).Direction = ParameterDirection.Output;

                    cmd.CommandType = CommandType.StoredProcedure;

                    conexion.Open();
                    cmd.ExecuteNonQuery();

                    resultado = Convert.ToBoolean(cmd.Parameters["Resultado"].Value);
                    mensaje = cmd.Parameters["Mensaje"].Value.ToString();
                }
            }
            catch (Exception ex)
            {
                resultado = false;
                mensaje = ex.Message;
            }

            return resultado;
        }
    }
}
