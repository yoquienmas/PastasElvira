using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using CapaEntidad;

namespace CapaDatos
{
    public class CD_MateriaPrima
    {
        public List<MateriaPrima> Listar()
        {
            List<MateriaPrima> lista = new List<MateriaPrima>();

            using (SqlConnection conexion = new SqlConnection(Conexion.cadena))
            {
                string query = "SELECT IdMateria, Nombre, Unidad, CantidadDisponible FROM MateriaPrima";
                SqlCommand cmd = new SqlCommand(query, conexion);
                cmd.CommandType = CommandType.Text;

                conexion.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new MateriaPrima
                        {
                            IdMateria = Convert.ToInt32(dr["IdMateria"]),
                            Nombre = dr["Nombre"].ToString(),
                            Unidad = dr["Unidad"].ToString(),
                            CantidadDisponible = Convert.ToSingle(dr["CantidadDisponible"])
                        });
                    }
                }
            }

            return lista;
        }

        public int Registrar(MateriaPrima materia, out string mensaje)
        {
            mensaje = "";

            try
            {
                using (SqlConnection conexion = new SqlConnection(Conexion.cadena))
                {
                    string query = "INSERT INTO MateriaPrima (Nombre, Unidad, CantidadDisponible) " +
                                   "VALUES (@Nombre, @Unidad, @Cantidad); SELECT SCOPE_IDENTITY();";

                    SqlCommand cmd = new SqlCommand(query, conexion);
                    cmd.Parameters.AddWithValue("@Nombre", materia.Nombre);
                    cmd.Parameters.AddWithValue("@Unidad", materia.Unidad);
                    cmd.Parameters.AddWithValue("@Cantidad", materia.CantidadDisponible);

                    conexion.Open();
                    int idGenerado = Convert.ToInt32(cmd.ExecuteScalar());
                    return idGenerado;
                }
            }
            catch (Exception ex)
            {
                mensaje = "Error al registrar materia prima: " + ex.Message;
                return 0;
            }
        }

        public bool Editar(MateriaPrima materia, out string mensaje)
        {
            mensaje = "";

            try
            {
                using (SqlConnection conexion = new SqlConnection(Conexion.cadena))
                {
                    string query = "UPDATE MateriaPrima SET Nombre = @Nombre, Unidad = @Unidad, CantidadDisponible = @Cantidad " +
                                   "WHERE IdMateria = @Id";

                    SqlCommand cmd = new SqlCommand(query, conexion);
                    cmd.Parameters.AddWithValue("@Id", materia.IdMateria);
                    cmd.Parameters.AddWithValue("@Nombre", materia.Nombre);
                    cmd.Parameters.AddWithValue("@Unidad", materia.Unidad);
                    cmd.Parameters.AddWithValue("@Cantidad", materia.CantidadDisponible);

                    conexion.Open();
                    int filas = cmd.ExecuteNonQuery();
                    return filas > 0;
                }
            }
            catch (Exception ex)
            {
                mensaje = "Error al editar materia prima: " + ex.Message;
                return false;
            }
        }

        public bool Eliminar(int id, out string mensaje)
        {
            mensaje = "";

            try
            {
                using (SqlConnection conexion = new SqlConnection(Conexion.cadena))
                {
                    string query = "DELETE FROM MateriaPrima WHERE IdMateria = @Id";

                    SqlCommand cmd = new SqlCommand(query, conexion);
                    cmd.Parameters.AddWithValue("@Id", id);

                    conexion.Open();
                    int filas = cmd.ExecuteNonQuery();
                    return filas > 0;
                }
            }
            catch (Exception ex)
            {
                mensaje = "Error al eliminar materia prima: " + ex.Message;
                return false;
            }
        }
    }
}
