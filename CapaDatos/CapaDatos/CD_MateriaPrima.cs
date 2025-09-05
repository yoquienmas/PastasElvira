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

            try
            {
                using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
                {
                    string query = @"SELECT IdMateria, Nombre, Unidad, CantidadDisponible, StockMinimo, PrecioUnitario
                                     FROM MateriaPrima";

                    SqlCommand cmd = new SqlCommand(query, oconexion);
                    cmd.CommandType = CommandType.Text;

                    oconexion.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lista.Add(new MateriaPrima()
                            {
                                IdMateria = Convert.ToInt32(dr["IdMateria"]),
                                Nombre = dr["Nombre"].ToString(),
                                Unidad = dr["Unidad"].ToString(),
                                CantidadDisponible = Convert.ToInt32(dr["CantidadDisponible"]),
                                StockMinimo = Convert.ToInt32(dr["StockMinimo"]),
                                PrecioUnitario = Convert.ToDecimal(dr["PrecioUnitario"])
                            });
                        }
                    }
                }
            }
            catch (Exception)
            {
                lista = new List<MateriaPrima>();
            }

            return lista;
        }

        public int Registrar(MateriaPrima obj, out string mensaje)
        {
            int id = 0;
            mensaje = string.Empty;

            try
            {
                using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
                {
                    SqlCommand cmd = new SqlCommand("INSERT INTO MateriaPrima(Nombre, Unidad, CantidadDisponible, StockMinimo, PrecioUnitario) " +
                                                    "OUTPUT INSERTED.IdMateria VALUES(@Nombre,@Unidad,@CantidadDisponible,@StockMinimo,@PrecioUnitario)", oconexion);

                    cmd.Parameters.AddWithValue("@Nombre", obj.Nombre);
                    cmd.Parameters.AddWithValue("@Unidad", obj.Unidad);
                    cmd.Parameters.AddWithValue("@CantidadDisponible", obj.CantidadDisponible);
                    cmd.Parameters.AddWithValue("@StockMinimo", obj.StockMinimo);
                    cmd.Parameters.AddWithValue("@PrecioUnitario", obj.PrecioUnitario);

                    oconexion.Open();
                    id = Convert.ToInt32(cmd.ExecuteScalar());
                }
                mensaje = "Materia prima registrada correctamente.";
            }
            catch (Exception ex)
            {
                id = 0;
                mensaje = "Error al registrar materia prima: " + ex.Message;
            }

            return id;
        }

        public bool Editar(MateriaPrima obj, out string mensaje)
        {
            bool resultado = false;
            mensaje = string.Empty;

            try
            {
                using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
                {
                    SqlCommand cmd = new SqlCommand(@"UPDATE MateriaPrima SET 
                                                      Nombre=@Nombre, 
                                                      Unidad=@Unidad, 
                                                      CantidadDisponible=@CantidadDisponible, 
                                                      StockMinimo=@StockMinimo, 
                                                      PrecioUnitario=@PrecioUnitario
                                                      WHERE IdMateria=@IdMateria", oconexion);

                    cmd.Parameters.AddWithValue("@IdMateria", obj.IdMateria);
                    cmd.Parameters.AddWithValue("@Nombre", obj.Nombre);
                    cmd.Parameters.AddWithValue("@Unidad", obj.Unidad);
                    cmd.Parameters.AddWithValue("@CantidadDisponible", obj.CantidadDisponible);
                    cmd.Parameters.AddWithValue("@StockMinimo", obj.StockMinimo);
                    cmd.Parameters.AddWithValue("@PrecioUnitario", obj.PrecioUnitario);

                    oconexion.Open();
                    resultado = cmd.ExecuteNonQuery() > 0;
                }
                mensaje = resultado ? "Materia prima editada correctamente." : "No se pudo editar.";
            }
            catch (Exception ex)
            {
                resultado = false;
                mensaje = "Error al editar materia prima: " + ex.Message;
            }

            return resultado;
        }

        public bool Eliminar(int id, out string mensaje)
        {
            bool resultado = false;
            mensaje = string.Empty;

            try
            {
                using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
                {
                    SqlCommand cmd = new SqlCommand("DELETE FROM MateriaPrima WHERE IdMateria=@IdMateria", oconexion);
                    cmd.Parameters.AddWithValue("@IdMateria", id);

                    oconexion.Open();
                    resultado = cmd.ExecuteNonQuery() > 0;
                }
                mensaje = resultado ? "Materia prima eliminada correctamente." : "No se pudo eliminar.";
            }
            catch (Exception ex)
            {
                resultado = false;
                mensaje = "Error al eliminar materia prima: " + ex.Message;
            }

            return resultado;
        }
    }
}
