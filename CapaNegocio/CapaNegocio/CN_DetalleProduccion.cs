using CapaDatos;
using CapaEntidad;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;

namespace CapaNegocio
{
    public class CN_DetalleProduccion
    {
        private CD_DetalleProduccion objcd = new CD_DetalleProduccion();

        public List<DetalleProduccion> ListarPorProduccion(int idProduccion)
        {
            return objcd.ListarPorProduccion(idProduccion);
        }

        public int Registrar(DetalleProduccion obj, out string mensaje)
        {
            int idAutogenerado = 0;
            mensaje = string.Empty;

            try
            {
                using (SqlConnection conexion = new SqlConnection(Conexion.cadena))
                {
                    conexion.Open();
                    using (SqlTransaction transaccion = conexion.BeginTransaction())
                    {
                        idAutogenerado = objcd.Registrar(obj, conexion, transaccion);

                        if (idAutogenerado > 0)
                        {
                            transaccion.Commit();
                        }
                        else
                        {
                            transaccion.Rollback();
                            mensaje = "No se pudo registrar el detalle de producción.";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                idAutogenerado = 0;
                mensaje = ex.Message;
            }

            return idAutogenerado;
        }

        public bool Eliminar(int idDetalleProduccion, out string mensaje)
        {
            return objcd.Eliminar(idDetalleProduccion, out mensaje);
        }
    }
}
