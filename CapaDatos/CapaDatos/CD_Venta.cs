using CapaEntidad;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;

namespace CapaDatos
{
    public class CD_Venta
    {
        
        public bool Registrar(Venta objVenta, out string mensaje)
        {
            bool registroExitoso = false;
            mensaje = string.Empty;
            int idVentaGenerado = 0;

            
            try
            {
                
                using (SqlConnection con = new SqlConnection(Conexion.cadena))
                {
                    con.Open();
          
                    SqlTransaction transaction = con.BeginTransaction();

                    try
                    {
                       
                        SqlCommand cmdVenta = new SqlCommand("SP_REGISTRAR_VENTA", con, transaction);
                        cmdVenta.CommandType = CommandType.StoredProcedure;

                        cmdVenta.Parameters.AddWithValue("@Fecha", objVenta.Fecha);
                        cmdVenta.Parameters.AddWithValue("@Total", objVenta.Total);
                        cmdVenta.Parameters.Add("@IdVentaResultado", SqlDbType.Int).Direction = ParameterDirection.Output;
                        cmdVenta.Parameters.Add("@Mensaje", SqlDbType.NVarChar, 200).Direction = ParameterDirection.Output;

                        cmdVenta.ExecuteNonQuery();

                        idVentaGenerado = Convert.ToInt32(cmdVenta.Parameters["@IdVentaResultado"].Value);
                        mensaje = cmdVenta.Parameters["@Mensaje"].Value.ToString();

                        if (idVentaGenerado > 0)
                        {
                            foreach (ItemVenta item in objVenta.Items)
                            {
                                SqlCommand cmdItem = new SqlCommand("SP_REGISTRAR_ITEM_VENTA", con, transaction);
                                cmdItem.CommandType = CommandType.StoredProcedure;
                                cmdItem.Parameters.AddWithValue("@IdVenta", idVentaGenerado);
                                cmdItem.Parameters.AddWithValue("@IdProducto", item.IdProducto);
                                cmdItem.Parameters.AddWithValue("@Cantidad", item.Cantidad);
                                cmdItem.Parameters.AddWithValue("@PrecioUnitario", item.PrecioUnitario);
                                cmdItem.Parameters.Add("@Respuesta", SqlDbType.Bit).Direction = ParameterDirection.Output;

                                cmdItem.ExecuteNonQuery();

                               
                                SqlCommand cmdStock = new SqlCommand("SP_ACTUALIZAR_STOCK", con, transaction);
                                cmdStock.CommandType = CommandType.StoredProcedure;
                                cmdStock.Parameters.AddWithValue("@IdProducto", item.IdProducto);
                                cmdStock.Parameters.AddWithValue("@CantidadVendida", item.Cantidad);
                                cmdStock.ExecuteNonQuery();
                            }

                            
                            transaction.Commit();
                            registroExitoso = true;
                            mensaje = "Venta registrada exitosamente.";
                        }
                        else
                        {
                           
                            transaction.Rollback();
                            registroExitoso = false;
                        }
                    }
                    catch (Exception ex)
                    {
                       
                        transaction.Rollback();
                        registroExitoso = false;
                        mensaje = "Error al registrar la venta: " + ex.Message;
                    }
                }
            }
            catch (Exception ex)
            {
               
                registroExitoso = false;
                mensaje = "Error de conexión con la base de datos: " + ex.Message;
            }

            return registroExitoso;
        }

       
    }
}
