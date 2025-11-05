using CapaEntidad;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;

namespace CapaDatos
{
    public class CD_Tipo
    {
        public List<Tipo> Listar()
        {
            List<Tipo> lista = new List<Tipo>();

            using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
            {
                try
                {
                    string query = "SELECT IdTipo, Descripcion, Activo FROM Tipo WHERE Activo = 1";

                    SqlCommand cmd = new SqlCommand(query, oconexion);
                    cmd.CommandType = CommandType.Text;

                    oconexion.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lista.Add(new Tipo()
                            {
                                IdTipo = Convert.ToInt32(dr["IdTipo"]),
                                Descripcion = dr["Descripcion"].ToString(),
                                Activo = Convert.ToBoolean(dr["Activo"])
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    lista = new List<Tipo>();
                    throw new Exception($"Error en capa de datos: {ex.Message}");
                }
            }
            return lista;
        }

        public bool Registrar(Tipo tipo, out string mensaje)
        {
            mensaje = string.Empty;
            bool resultado = false;

            using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("sp_RegistrarTipo", oconexion);
                    cmd.Parameters.AddWithValue("Descripcion", tipo.Descripcion);
                    cmd.Parameters.AddWithValue("Activo", tipo.Activo);
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
                    mensaje = $"Error al registrar tipo: {ex.Message}";
                }
            }
            return resultado;
        }

        public bool Editar(Tipo tipo, out string mensaje)
        {
            mensaje = string.Empty;
            bool resultado = false;

            using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("sp_EditarTipo", oconexion);
                    cmd.Parameters.AddWithValue("IdTipo", tipo.IdTipo);
                    cmd.Parameters.AddWithValue("Descripcion", tipo.Descripcion);
                    cmd.Parameters.AddWithValue("Activo", tipo.Activo);
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
                    mensaje = $"Error al editar tipo: {ex.Message}";
                }
            }
            return resultado;
        }

        public bool Eliminar(int idTipo, out string mensaje)
        {
            mensaje = string.Empty;
            bool resultado = false;

            using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("sp_EliminarTipo", oconexion);
                    cmd.Parameters.AddWithValue("IdTipo", idTipo);
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
                    mensaje = $"Error al eliminar tipo: {ex.Message}";
                }
            }
            return resultado;
        }
    }
}