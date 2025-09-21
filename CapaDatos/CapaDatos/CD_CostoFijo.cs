using CapaEntidad;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System;

namespace CapaDatos
{
    public class CD_CostoFijo
    {
        public List<CostoFijo> Listar()
        {
            List<CostoFijo> lista = new List<CostoFijo>();

            using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
            {
                SqlCommand cmd = new SqlCommand("SELECT IdCosto, Concepto, Monto, Activo FROM CostoFijoProduccion ORDER BY Concepto", oconexion);
                oconexion.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new CostoFijo()
                        {
                            IdCosto = Convert.ToInt32(dr["IdCosto"]),
                            Concepto = dr["Concepto"].ToString(),
                            Monto = Convert.ToDecimal(dr["Monto"]),
                            Activo = Convert.ToBoolean(dr["Activo"])
                        });
                    }
                }
            }
            return lista;
        }

        public bool Registrar(CostoFijo costo, out string mensaje)
        {
            mensaje = string.Empty;

            try
            {
                using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
                {
                    oconexion.Open(); // Abre la conexión aquí

                    SqlCommand cmd = new SqlCommand("INSERT INTO CostoFijoProduccion (Concepto, Monto, Activo) VALUES (@concepto, @monto, @activo); SELECT SCOPE_IDENTITY();", oconexion); // Obtener el Id insertado si lo necesitas
                    cmd.Parameters.AddWithValue("@concepto", costo.Concepto);
                    cmd.Parameters.AddWithValue("@monto", costo.Monto);
                    cmd.Parameters.AddWithValue("@activo", costo.Activo);

                    int rowsAffected = cmd.ExecuteNonQuery(); // Ejecutar el INSERT
                    bool resultado = rowsAffected > 0;
                    mensaje = resultado ? "Costo fijo registrado correctamente" : "No se pudo registrar el costo fijo";

                    if (resultado && costo.Activo)
                    {
                        RecalcularTodosLosProductos(oconexion); // La conexión ya está abierta
                    }

                    return resultado;
                } // La conexión se cierra automáticamente aquí
            }
            catch (Exception ex)
            {
                mensaje = ex.Message;
                return false;
            }
        }

        public bool Editar(CostoFijo costo, out string mensaje)
        {
            mensaje = string.Empty;

            try
            {
                using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
                {
                    oconexion.Open();

                    // 1. Obtener estado anterior para comparar
                    // Usar un comando separado y cerrarlo antes de la siguiente operación
                    bool estadoAnterior;
                    using (SqlCommand cmdEstadoAnterior = new SqlCommand("SELECT Activo FROM CostoFijoProduccion WHERE IdCosto = @id", oconexion))
                    {
                        cmdEstadoAnterior.Parameters.AddWithValue("@id", costo.IdCosto);
                        estadoAnterior = Convert.ToBoolean(cmdEstadoAnterior.ExecuteScalar());
                    } // cmdEstadoAnterior se libera aquí

                    // 2. Actualizar el costo fijo
                    using (SqlCommand cmd = new SqlCommand("UPDATE CostoFijoProduccion SET Concepto = @concepto, Monto = @monto, Activo = @activo WHERE IdCosto = @id", oconexion))
                    {
                        cmd.Parameters.AddWithValue("@id", costo.IdCosto);
                        cmd.Parameters.AddWithValue("@concepto", costo.Concepto);
                        cmd.Parameters.AddWithValue("@monto", costo.Monto);
                        cmd.Parameters.AddWithValue("@activo", costo.Activo);

                        bool resultado = cmd.ExecuteNonQuery() > 0;
                        mensaje = resultado ? "Costo fijo actualizado correctamente" : "No se pudo actualizar el costo fijo";

                        // 3. Si cambió el estado o monto de un costo activo, recalcular productos
                        if (resultado && (estadoAnterior != costo.Activo || costo.Activo))
                        {
                            RecalcularTodosLosProductos(oconexion);
                        }
                        return resultado;
                    } // cmd se libera aquí
                } // oconexion se cierra y libera aquí
            }
            catch (Exception ex)
            {
                mensaje = ex.Message;
                return false;
            }
        }

        public bool Eliminar(int idCosto, out string mensaje)
        {
            mensaje = string.Empty;

            try
            {
                using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
                {
                    oconexion.Open();

                    // 1. Verificar si el costo estaba activo
                    bool estabaActivo;
                    using (SqlCommand cmdVerificar = new SqlCommand("SELECT Activo FROM CostoFijoProduccion WHERE IdCosto = @id", oconexion))
                    {
                        cmdVerificar.Parameters.AddWithValue("@id", idCosto);
                        estabaActivo = Convert.ToBoolean(cmdVerificar.ExecuteScalar());
                    } // cmdVerificar se libera aquí

                    // 2. Eliminar el costo
                    using (SqlCommand cmd = new SqlCommand("DELETE FROM CostoFijoProduccion WHERE IdCosto = @id", oconexion))
                    {
                        cmd.Parameters.AddWithValue("@id", idCosto);

                        bool resultado = cmd.ExecuteNonQuery() > 0;
                        mensaje = resultado ? "Costo fijo eliminado correctamente" : "No se pudo eliminar el costo fijo";

                        // 3. Si el costo eliminado estaba activo, recalcular productos
                        if (resultado && estabaActivo)
                        {
                            RecalcularTodosLosProductos(oconexion);
                        }
                        return resultado;
                    } // cmd se libera aquí
                } // oconexion se cierra y libera aquí
            }
            catch (Exception ex)
            {
                mensaje = ex.Message;
                return false;
            }
        }

        private void RecalcularTodosLosProductos(SqlConnection conexion)
        {
            // Lista para almacenar los IDs de productos antes de procesarlos
            List<int> productoIds = new List<int>();

            // Primero, obtener todos los IdProducto y cerrar el DataReader
            using (SqlCommand cmdSelectIds = new SqlCommand("SELECT IdProducto FROM Producto WHERE Visible = 1", conexion))
            {
                using (SqlDataReader dr = cmdSelectIds.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        productoIds.Add(Convert.ToInt32(dr["IdProducto"]));
                    }
                } // El dr se cierra y libera aquí. Ahora la conexión está libre.
            } // cmdSelectIds se libera aquí

            // Ahora, iterar sobre los IDs y recalcular cada producto
            foreach (int idProducto in productoIds)
            {
                // Para cada producto, usar un nuevo SqlCommand (la conexión sigue abierta)
                using (SqlCommand cmdRecalcular = new SqlCommand("CalcularCostoProducto", conexion))
                {
                    cmdRecalcular.CommandType = CommandType.StoredProcedure;
                    cmdRecalcular.Parameters.AddWithValue("@IdProducto", idProducto);
                    cmdRecalcular.Parameters.Add("@CostoTotal", SqlDbType.Decimal).Direction = ParameterDirection.Output;
                    cmdRecalcular.Parameters.Add("@PrecioSugerido", SqlDbType.Decimal).Direction = ParameterDirection.Output;
                    cmdRecalcular.ExecuteNonQuery();
                } // cmdRecalcular se libera aquí
            }
        }
    }
}