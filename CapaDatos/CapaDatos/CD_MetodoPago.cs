using CapaEntidad;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;

namespace CapaDatos
{
    public class CD_MetodoPago
    {
        public List<MetodoPago> Listar()
        {
            List<MetodoPago> lista = new List<MetodoPago>();

            try
            {
                using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
                {
                    string query = "SELECT IdMetodoPago, Nombre, Descripcion, Activo FROM MetodoPago WHERE Activo = 1 ORDER BY IdMetodoPago";

                    SqlCommand cmd = new SqlCommand(query, oconexion);
                    cmd.CommandType = CommandType.Text;

                    oconexion.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lista.Add(new MetodoPago()
                            {
                                IdMetodoPago = Convert.ToInt32(dr["IdMetodoPago"]),
                                Nombre = dr["Nombre"].ToString(),
                                Descripcion = dr["Descripcion"] != DBNull.Value ? dr["Descripcion"].ToString() : "",
                                Activo = Convert.ToBoolean(dr["Activo"])
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                lista = new List<MetodoPago>();
                // Puedes loggear el error aquí si es necesario
            }

            return lista;
        }
    }
}