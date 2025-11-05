using CapaEntidad;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;

namespace CapaDatos
{
    public class CD_Sabor
    {
        public List<Sabor> Listar()
        {
            List<Sabor> lista = new List<Sabor>();

            using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
            {
                try
                {
                    string query = "SELECT IdSabor, Descripcion, Activo FROM Sabor WHERE Activo = 1";

                    SqlCommand cmd = new SqlCommand(query, oconexion);
                    cmd.CommandType = CommandType.Text;

                    oconexion.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lista.Add(new Sabor()
                            {
                                IdSabor = Convert.ToInt32(dr["IdSabor"]),
                                Descripcion = dr["Descripcion"].ToString(),
                                Activo = Convert.ToBoolean(dr["Activo"])
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    lista = new List<Sabor>();
                    throw new Exception($"Error en capa de datos: {ex.Message}");
                }
            }
            return lista;
        }

        public bool Registrar(Sabor sabor, out string mensaje)
        {
            mensaje = string.Empty;
            bool resultado = false;

            using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("sp_RegistrarSabor", oconexion);
                    cmd.Parameters.AddWithValue("Descripcion", sabor.Descripcion);
                    cmd.Parameters.AddWithValue("Activo", sabor.Activo);
                    cmd.Parameters.Add("Resultado", SqlDbType.Bit).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("Mensaje", SqlDbType.VarChar, 500).Direction = ParameterDirection.Output;
                    cmd.CommandType = CommandType.StoredProcedure;

                    oconexion.Open();
                    cmd.ExecuteNonQuery();

                    resultado = Convert.ToBoolean(cmd.Parameters["Resultado"].Value);
                    mensaje = cmd.Parameters["Mensaje"].Value.ToString();
                }
                catch (Exception ex)
                {
                    resultado = false;
                    mensaje = $"Error al registrar sabor: {ex.Message}";
                }
            }
            return resultado;
        }

        public bool Editar(Sabor sabor, out string mensaje)
        {
            mensaje = string.Empty;
            bool resultado = false;

            using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("sp_EditarSabor", oconexion);
                    cmd.Parameters.AddWithValue("IdSabor", sabor.IdSabor);
                    cmd.Parameters.AddWithValue("Descripcion", sabor.Descripcion);
                    cmd.Parameters.AddWithValue("Activo", sabor.Activo);
                    cmd.Parameters.Add("Resultado", SqlDbType.Bit).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("Mensaje", SqlDbType.VarChar, 500).Direction = ParameterDirection.Output;
                    cmd.CommandType = CommandType.StoredProcedure;

                    oconexion.Open();
                    cmd.ExecuteNonQuery();

                    resultado = Convert.ToBoolean(cmd.Parameters["Resultado"].Value);
                    mensaje = cmd.Parameters["Mensaje"].Value.ToString();
                }
                catch (Exception ex)
                {
                    resultado = false;
                    mensaje = $"Error al editar sabor: {ex.Message}";
                }
            }
            return resultado;
        }

        public bool Eliminar(int idSabor, out string mensaje)
        {
            mensaje = string.Empty;
            bool resultado = false;

            using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("sp_EliminarSabor", oconexion);
                    cmd.Parameters.AddWithValue("IdSabor", idSabor);
                    cmd.Parameters.Add("Resultado", SqlDbType.Bit).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("Mensaje", SqlDbType.VarChar, 500).Direction = ParameterDirection.Output;
                    cmd.CommandType = CommandType.StoredProcedure;

                    oconexion.Open();
                    cmd.ExecuteNonQuery();

                    resultado = Convert.ToBoolean(cmd.Parameters["Resultado"].Value);
                    mensaje = cmd.Parameters["Mensaje"].Value.ToString();
                }
                catch (Exception ex)
                {
                    resultado = false;
                    mensaje = $"Error al eliminar sabor: {ex.Message}";
                }
            }
            return resultado;
        }
    }
}