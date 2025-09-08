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
                    SqlCommand cmd = new SqlCommand("SELECT p.IdProduccion, p.IdProducto, prod.Nombre as NombreProducto, p.CantidadProducida, p.FechaProduccion FROM Produccion p INNER JOIN Producto prod ON p.IdProducto = prod.IdProducto ORDER BY p.FechaProduccion DESC", conexion);

                    conexion.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lista.Add(new Produccion()
                            {
                                IdProduccion = Convert.ToInt32(dr["IdProduccion"]),
                                IdProducto = Convert.ToInt32(dr["IdProducto"]),
                                NombreProducto = dr["NombreProducto"].ToString(),
                                CantidadProducida = Convert.ToInt32(dr["CantidadProducida"]),
                                FechaProduccion = Convert.ToDateTime(dr["FechaProduccion"])
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    lista = new List<Produccion>();
                }
            }
            return lista;
        }

        public bool Registrar(Produccion produccion, out string mensaje)
        {
            bool resultado = false;
            mensaje = string.Empty;

            try
            {
                using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
                {
                    SqlCommand cmd = new SqlCommand("SP_RegistrarProduccion", oconexion);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@IdProducto", produccion.IdProducto);
                    cmd.Parameters.AddWithValue("@CantidadProducida", produccion.CantidadProducida);

                    // Parámetro de salida para el resultado
                    SqlParameter outputParam = new SqlParameter("@Resultado", SqlDbType.NVarChar, 200);
                    outputParam.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(outputParam);

                    oconexion.Open();
                    cmd.ExecuteNonQuery();

                    mensaje = outputParam.Value.ToString();
                    resultado = !mensaje.Contains("Error") && !mensaje.Contains("No hay stock");
                }
            }
            catch (Exception ex)
            {
                resultado = false;
                mensaje = ex.Message;
            }

            return resultado;
        }

        public string ValidarDisponibilidadMateriasPrimas(int idProducto, int cantidad)
        {
            using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
            {
                SqlCommand cmd = new SqlCommand(@"
                    SELECT m.Nombre, m.CantidadDisponible, pm.CantidadNecesaria * @cantidad as CantidadRequerida
                    FROM ProductoMateriaPrima pm
                    INNER JOIN MateriaPrima m ON pm.IdMateria = m.IdMateria
                    WHERE pm.IdProducto = @idProducto
                    AND m.CantidadDisponible < (pm.CantidadNecesaria * @cantidad)", oconexion);

                cmd.Parameters.AddWithValue("@idProducto", idProducto);
                cmd.Parameters.AddWithValue("@cantidad", cantidad);

                oconexion.Open();
                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.HasRows)
                {
                    dr.Read();
                    return $"Stock insuficiente de {dr["Nombre"]}. Disponible: {dr["CantidadDisponible"]}, Requerido: {dr["CantidadRequerida"]}";
                }
            }
            return null;
        }

        public List<DetalleReceta> ListarRecetaPorProducto(int idProducto)
        {
            List<DetalleReceta> lista = new List<DetalleReceta>();

            using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
            {
                SqlCommand cmd = new SqlCommand("sp_ListarRecetaProducto", oconexion);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@IdProducto", idProducto);

                oconexion.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new DetalleReceta()
                        {
                            IdProductoMateriaPrima = Convert.ToInt32(dr["IdProductoMateriaPrima"]),
                            IdProducto = idProducto,
                            IdMateria = Convert.ToInt32(dr["IdMateria"]),
                            NombreMateria = dr["MateriaPrima"].ToString(),
                            CantidadNecesaria = Convert.ToSingle(dr["CantidadNecesaria"]),
                            Unidad = dr["Unidad"].ToString()
                        });
                    }
                }
            }
            return lista;
        }

        public bool AgregarMateriaPrimaAReceta(int idProducto, int idMateria, float cantidad)
        {
            try
            {
                using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
                {
                    oconexion.Open();

                    // 1. Agregar materia prima a la receta
                    SqlCommand cmd = new SqlCommand("sp_AgregarProductoMateriaPrima", oconexion);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@IdProducto", idProducto);
                    cmd.Parameters.AddWithValue("@IdMateria", idMateria);
                    cmd.Parameters.AddWithValue("@CantidadNecesaria", cantidad);
                    cmd.ExecuteNonQuery();

                    // 2. Recalcular costo del producto
                    RecalcularCostoProducto(oconexion, idProducto);

                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public bool EliminarMateriaPrimaDeReceta(int idProductoMateria)
        {
            try
            {
                using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
                {
                    oconexion.Open();

                    // 1. Obtener el idProducto antes de eliminar
                    SqlCommand cmdGet = new SqlCommand("SELECT IdProducto FROM ProductoMateriaPrima WHERE IdProductoMateriaPrima = @id", oconexion);
                    cmdGet.Parameters.AddWithValue("@id", idProductoMateria);
                    int idProducto = Convert.ToInt32(cmdGet.ExecuteScalar());

                    // 2. Eliminar la materia prima de la receta
                    SqlCommand cmdDelete = new SqlCommand("DELETE FROM ProductoMateriaPrima WHERE IdProductoMateriaPrima = @id", oconexion);
                    cmdDelete.Parameters.AddWithValue("@id", idProductoMateria);
                    cmdDelete.ExecuteNonQuery();

                    // 3. Recalcular costo del producto
                    RecalcularCostoProducto(oconexion, idProducto);

                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        private void RecalcularCostoProducto(SqlConnection conexion, int idProducto)
        {
            // Recalcular costo usando el SP
            SqlCommand cmd = new SqlCommand("CalcularCostoProducto", conexion);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdProducto", idProducto);
            cmd.Parameters.Add("@CostoTotal", SqlDbType.Decimal).Direction = ParameterDirection.Output;
            cmd.Parameters.Add("@PrecioSugerido", SqlDbType.Decimal).Direction = ParameterDirection.Output;
            cmd.ExecuteNonQuery();
        }

    }
}