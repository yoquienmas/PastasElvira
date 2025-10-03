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
            List<int> productoIds = new List<int>();

            try
            {
                // Obtener todos los IDs de productos visibles
                using (SqlCommand cmdSelectIds = new SqlCommand("SELECT IdProducto FROM Producto WHERE Visible = 1", conexion))
                {
                    using (SqlDataReader dr = cmdSelectIds.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            productoIds.Add(Convert.ToInt32(dr["IdProducto"]));
                        }
                    }
                }

                // Recalcular cada producto individualmente
                foreach (int idProducto in productoIds)
                {
                    RecalcularProductoIndividual(conexion, idProducto);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error en RecalcularTodosLosProductos: {ex.Message}");
            }
        }

        // En CD_CostoFijo.cs, modifica los métodos de cálculo
        private decimal CalcularCostosFijosUnitarios(SqlConnection conexion)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand(@"
            -- Calcular total de costos fijos activos
            DECLARE @TotalCostosFijos DECIMAL(10,2) = (
                SELECT ISNULL(SUM(Monto), 0) 
                FROM CostoFijoProduccion 
                WHERE Activo = 1
            )
            
            -- Calcular total de inversión en materias primas
            DECLARE @TotalMateriasPrimas DECIMAL(10,2) = (
                SELECT ISNULL(SUM(PrecioUnitario * CantidadDisponible), 0)
                FROM MateriaPrima
            )
            
            -- Sumar totales y dividir entre 2500
            DECLARE @TotalInversion DECIMAL(10,2) = @TotalCostosFijos + @TotalMateriasPrimas
            DECLARE @CostoVentaUnitario DECIMAL(10,2) = @TotalInversion / 2500
            
            SELECT ISNULL(@CostoVentaUnitario, 0)", conexion))
                {
                    var result = cmd.ExecuteScalar();
                    return result != DBNull.Value ? Convert.ToDecimal(result) : 0;
                }
            }
            catch
            {
                return 0;
            }
        }

        private void RecalcularProductoIndividual(SqlConnection conexion, int idProducto)
        {
            try
            {
                // 1. Obtener el costo de venta unitario (que ya incluye la división entre 2500)
                decimal costosFijosUnitarios = CalcularCostosFijosUnitarios(conexion);

                // El costo final es directamente el costo unitario calculado
                decimal nuevoCosto = costosFijosUnitarios;

                // 2. Obtener margen de ganancia (usar 30% si no existe)
                decimal margenGanancia = ObtenerMargenGanancia(conexion, idProducto);
                decimal nuevoPrecio = nuevoCosto * (1 + (margenGanancia / 100));

                // 3. Actualizar producto
                ActualizarCostoYPrecioProducto(conexion, idProducto, nuevoCosto, nuevoPrecio);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al recalcular producto {idProducto}: {ex.Message}");
            }
        }

        private decimal CalcularCostoMateriasPrimas(SqlConnection conexion, int idProducto)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand(@"
            SELECT ISNULL(SUM(mp.PrecioUnitario * dr.CantidadNecesaria), 0) as CostoMaterias
            FROM DetalleReceta dr
            INNER JOIN MateriaPrima mp ON dr.IdMateria = mp.IdMateria
            WHERE dr.IdProducto = @IdProducto", conexion))
                {
                    cmd.Parameters.AddWithValue("@IdProducto", idProducto);
                    var result = cmd.ExecuteScalar();
                    return result != DBNull.Value ? Convert.ToDecimal(result) : 0;
                }
            }
            catch
            {
                return 0; // Si hay error, retornar 0
            }
        }

       

        private decimal ObtenerMargenGanancia(SqlConnection conexion, int idProducto)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand(
                    "SELECT MargenGanancia FROM Producto WHERE IdProducto = @IdProducto", conexion))
                {
                    cmd.Parameters.AddWithValue("@IdProducto", idProducto);
                    var result = cmd.ExecuteScalar();
                    return result != DBNull.Value ? Convert.ToDecimal(result) : 30; // 30% por defecto
                }
            }
            catch
            {
                return 30; // 30% por defecto si hay error
            }
        }

        private void ActualizarCostoYPrecioProducto(SqlConnection conexion, int idProducto, decimal costo, decimal precio)
        {
            using (SqlCommand cmd = new SqlCommand(@"
        UPDATE Producto 
        SET CostoProduccion = @Costo, PrecioVenta = @Precio 
        WHERE IdProducto = @IdProducto", conexion))
            {
                cmd.Parameters.AddWithValue("@Costo", costo);
                cmd.Parameters.AddWithValue("@Precio", precio);
                cmd.Parameters.AddWithValue("@IdProducto", idProducto);
                cmd.ExecuteNonQuery();
            }
        }
    }
}