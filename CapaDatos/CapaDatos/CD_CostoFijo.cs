using CapaEntidad;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;

namespace CapaDatos
{
    public class CD_CostoFijo
    {
        public List<CostoFijo> Listar()
        {
            List<CostoFijo> lista = new List<CostoFijo>();

            using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
            {
                SqlCommand cmd = new SqlCommand("SELECT * FROM CostoFijoProduccion ORDER BY Concepto", oconexion);
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
                    SqlCommand cmd = new SqlCommand("INSERT INTO CostoFijoProduccion (Concepto, Monto, Activo) VALUES (@concepto, @monto, @activo)", oconexion);
                    cmd.Parameters.AddWithValue("@concepto", costo.Concepto);
                    cmd.Parameters.AddWithValue("@monto", costo.Monto);
                    cmd.Parameters.AddWithValue("@activo", costo.Activo);

                    oconexion.Open();
                    bool resultado = cmd.ExecuteNonQuery() > 0;
                    mensaje = resultado ? "Costo fijo registrado correctamente" : "No se pudo registrar el costo fijo";
                    
                    if (resultado && costo.Activo)
                    {
                        RecalcularTodosLosProductos(oconexion);
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

        public bool Editar(CostoFijo costo, out string mensaje)
        {
            mensaje = string.Empty;
            
            try
            {
                using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
                {
                    oconexion.Open();
                    
                    // 1. Obtener estado anterior para comparar
                    SqlCommand cmdEstadoAnterior = new SqlCommand("SELECT Activo FROM CostoFijoProduccion WHERE IdCosto = @id", oconexion);
                    cmdEstadoAnterior.Parameters.AddWithValue("@id", costo.IdCosto);
                    bool estadoAnterior = Convert.ToBoolean(cmdEstadoAnterior.ExecuteScalar());
                    
                    // 2. Actualizar el costo fijo
                    SqlCommand cmd = new SqlCommand("UPDATE CostoFijoProduccion SET Concepto = @concepto, Monto = @monto, Activo = @activo WHERE IdCosto = @id", oconexion);
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
                }
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
                    SqlCommand cmdVerificar = new SqlCommand("SELECT Activo FROM CostoFijoProduccion WHERE IdCosto = @id", oconexion);
                    cmdVerificar.Parameters.AddWithValue("@id", idCosto);
                    bool estabaActivo = Convert.ToBoolean(cmdVerificar.ExecuteScalar());
                    
                    // 2. Eliminar el costo
                    SqlCommand cmd = new SqlCommand("DELETE FROM CostoFijoProduccion WHERE IdCosto = @id", oconexion);
                    cmd.Parameters.AddWithValue("@id", idCosto);
                    
                    bool resultado = cmd.ExecuteNonQuery() > 0;
                    mensaje = resultado ? "Costo fijo eliminado correctamente" : "No se pudo eliminar el costo fijo";
                    
                    // 3. Si el costo eliminado estaba activo, recalcular productos
                    if (resultado && estabaActivo)
                    {
                        RecalcularTodosLosProductos(oconexion);
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

        private void RecalcularTodosLosProductos(SqlConnection conexion)
        {
            // Obtener todos los productos
            SqlCommand cmd = new SqlCommand("SELECT IdProducto FROM Producto WHERE Visible = 1", conexion);
            
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