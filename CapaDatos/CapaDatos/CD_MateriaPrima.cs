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

            using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
            {
                SqlCommand cmd = new SqlCommand("SELECT * FROM MateriaPrima", oconexion);
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
                            CantidadDisponible = Convert.ToSingle(dr["CantidadDisponible"]),
                            StockMinimo = Convert.ToInt32(dr["StockMinimo"]),
                            PrecioUnitario = Convert.ToDecimal(dr["PrecioUnitario"])
                        });
                    }
                }
            }
            return lista;
        }

        public int Registrar(MateriaPrima materia, out string mensaje)
        {
            int idGenerado = 0;
            mensaje = string.Empty;

            try
            {
                using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
                {
                    SqlCommand cmd = new SqlCommand("INSERT INTO MateriaPrima (Nombre, Unidad, CantidadDisponible, StockMinimo, PrecioUnitario) OUTPUT INSERTED.IdMateria VALUES (@nombre, @unidad, @cantidad, @stockMinimo, @precio)", oconexion);

                    cmd.Parameters.AddWithValue("@nombre", materia.Nombre);
                    cmd.Parameters.AddWithValue("@unidad", materia.Unidad);
                    cmd.Parameters.AddWithValue("@cantidad", materia.CantidadDisponible);
                    cmd.Parameters.AddWithValue("@stockMinimo", materia.StockMinimo);
                    cmd.Parameters.AddWithValue("@precio", materia.PrecioUnitario);

                    oconexion.Open();
                    idGenerado = Convert.ToInt32(cmd.ExecuteScalar());
                    mensaje = "Materia prima registrada correctamente";
                }
            }
            catch (Exception ex)
            {
                idGenerado = 0;
                mensaje = ex.Message;
            }
            return idGenerado;
        }

        // ✅ SOLO DEBE HABER UN MÉTODO EDITAR - ELIMINA CUALQUIER DUPLICADO
        public bool Editar(MateriaPrima materia, out string mensaje)
        {
            mensaje = string.Empty;

            try
            {
                using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
                {
                    oconexion.Open();

                    // 1. Guardar el precio anterior para comparar
                    SqlCommand cmdPrecioAnterior = new SqlCommand("SELECT PrecioUnitario FROM MateriaPrima WHERE IdMateria = @id", oconexion);
                    cmdPrecioAnterior.Parameters.AddWithValue("@id", materia.IdMateria);
                    decimal precioAnterior = Convert.ToDecimal(cmdPrecioAnterior.ExecuteScalar());

                    // 2. Actualizar la materia prima
                    SqlCommand cmd = new SqlCommand("UPDATE MateriaPrima SET Nombre = @nombre, Unidad = @unidad, CantidadDisponible = @cantidad, StockMinimo = @stockMinimo, PrecioUnitario = @precio WHERE IdMateria = @id", oconexion);
                    cmd.Parameters.AddWithValue("@id", materia.IdMateria);
                    cmd.Parameters.AddWithValue("@nombre", materia.Nombre);
                    cmd.Parameters.AddWithValue("@unidad", materia.Unidad);
                    cmd.Parameters.AddWithValue("@cantidad", materia.CantidadDisponible);
                    cmd.Parameters.AddWithValue("@stockMinimo", materia.StockMinimo);
                    cmd.Parameters.AddWithValue("@precio", materia.PrecioUnitario);

                    bool resultado = cmd.ExecuteNonQuery() > 0;
                    mensaje = resultado ? "Materia prima actualizada correctamente" : "No se pudo actualizar la materia prima";

                    // 3. Si cambió el precio, recalcular costos de productos afectados
                    if (resultado && precioAnterior != materia.PrecioUnitario)
                    {
                        RecalcularProductosConMateria(oconexion, materia.IdMateria);
                    }

                    return resultado;
                }
            }
            catch (Exception ex)
            {
                mensaje = ex.Message;
                return false;
            }
        }

        public bool Eliminar(int idMateria, out string mensaje)
        {
            mensaje = string.Empty;

            try
            {
                using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
                {
                    SqlCommand cmd = new SqlCommand("DELETE FROM MateriaPrima WHERE IdMateria = @id", oconexion);
                    cmd.Parameters.AddWithValue("@id", idMateria);

                    oconexion.Open();
                    bool resultado = cmd.ExecuteNonQuery() > 0;
                    mensaje = resultado ? "Materia prima eliminada correctamente" : "No se pudo eliminar la materia prima";
                    return resultado;
                }
            }
            catch (Exception ex)
            {
                mensaje = ex.Message;
                return false;
            }
        }

        private void RecalcularProductosConMateria(SqlConnection conexion, int idMateria)
        {
            // Obtener todos los productos que usan esta materia prima
            SqlCommand cmd = new SqlCommand("SELECT DISTINCT IdProducto FROM ProductoMateriaPrima WHERE IdMateria = @idMateria", conexion);
            cmd.Parameters.AddWithValue("@idMateria", idMateria);

            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    int idProducto = Convert.ToInt32(dr["IdProducto"]);

                    // Recalcular costo para cada producto
                    SqlCommand cmdRecalcular = new SqlCommand("CalcularCostoProducto", conexion);
                    cmdRecalcular.CommandType = CommandType.StoredProcedure;
                    cmdRecalcular.Parameters.AddWithValue("@IdProducto", idProducto);
                    cmdRecalcular.Parameters.Add("@CostoTotal", SqlDbType.Decimal).Direction = ParameterDirection.Output;
                    cmdRecalcular.Parameters.Add("@PrecioSugerido", SqlDbType.Decimal).Direction = ParameterDirection.Output;
                    cmdRecalcular.ExecuteNonQuery();
                }
            }
        }
    }
}

