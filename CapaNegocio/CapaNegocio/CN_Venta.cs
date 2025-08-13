using CapaEntidad;
using CapaDatos;
using System.Collections.Generic;

namespace CapaNegocio
{
    public class CN_Venta
    {
        private CD_Venta cd_venta = new CD_Venta();


        public bool Registrar(Venta objVenta, out string mensaje)
        {
            mensaje = string.Empty;

            if (objVenta.Items == null || objVenta.Items.Count == 0)
            {
                mensaje = "La venta debe contener al menos un producto.";
                return false;
            }

            return cd_venta.Registrar(objVenta, out mensaje);
        }

    }
}
