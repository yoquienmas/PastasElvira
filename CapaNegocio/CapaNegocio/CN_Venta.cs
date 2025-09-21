using CapaEntidad;
using CapaDatos;
using System.Collections.Generic;
using System;

namespace CapaNegocio
{
    public class CN_Venta
    {
        private CD_Venta cdVenta = new CD_Venta();

        public bool Registrar(Venta venta, out string mensaje)
        {
            try
            {
                // Calcular el total de la venta
                venta.Total = 0;
                foreach (var item in venta.Items)
                {
                    venta.Total += item.Subtotal;
                }

                return cdVenta.Registrar(venta, out mensaje);
            }
            catch (Exception ex)
            {
                mensaje = $"Error al registrar venta: {ex.Message}";
                return false;
            }
        }
        public List<ItemVenta> ObtenerDetallesVenta(int idVenta)
        {
            return cdVenta.ObtenerDetallesVenta(idVenta);
        }
    }
}