using System.Collections.Generic;
using CapaDatos;
using CapaEntidad;

namespace CapaNegocio
{
    public class CN_Producto
    {
        private CD_Producto cdProducto = new CD_Producto();

        // Listar todos los productos
        public List<Producto> Listar()
        {
            return cdProducto.Listar();
        }

        // Registrar un producto
        public int Registrar(Producto obj, out string mensaje)
        {
            mensaje = string.Empty;

            if (string.IsNullOrWhiteSpace(obj.Nombre))
                mensaje = "El nombre del producto es obligatorio.";
            else if (string.IsNullOrWhiteSpace(obj.Tipo))
                mensaje = "El tipo de producto es obligatorio.";
            else if (obj.CostoProduccion <= 0)
                mensaje = "El costo de producción debe ser mayor a 0.";
            else if (obj.MargenGanancia < 0)
                mensaje = "El margen de ganancia no puede ser negativo.";
            else if (obj.StockActual < 0)
                mensaje = "El stock no puede ser negativo.";
            else if (obj.StockMinimo < 0)
                mensaje = "El stock mínimo no puede ser negativo.";

            // Calcular precio automáticamente si no se ingresó
            if (obj.PrecioVenta <= 0 && obj.CostoProduccion > 0)
                obj.PrecioVenta = obj.CostoProduccion * (1 + (obj.MargenGanancia / 100));

            if (!string.IsNullOrEmpty(mensaje))
                return 0;

            return cdProducto.Registrar(obj, out mensaje);
        }

        // Editar un producto
        public bool Editar(Producto obj, out string mensaje)
        {
            mensaje = string.Empty;

            if (string.IsNullOrWhiteSpace(obj.Nombre))
                mensaje = "El nombre del producto es obligatorio.";
            else if (string.IsNullOrWhiteSpace(obj.Tipo))
                mensaje = "El tipo de producto es obligatorio.";
            else if (obj.CostoProduccion <= 0)
                mensaje = "El costo de producción debe ser mayor a 0.";
            else if (obj.MargenGanancia < 0)
                mensaje = "El margen de ganancia no puede ser negativo.";
            else if (obj.StockActual < 0)
                mensaje = "El stock no puede ser negativo.";
            else if (obj.StockMinimo < 0)
                mensaje = "El stock mínimo no puede ser negativo.";

            // Calcular precio automáticamente si no se ingresó
            if (obj.PrecioVenta <= 0 && obj.CostoProduccion > 0)
                obj.PrecioVenta = obj.CostoProduccion * (1 + (obj.MargenGanancia / 100));

            if (!string.IsNullOrEmpty(mensaje))
                return false;

            return cdProducto.Editar(obj, out mensaje);
        }

        // Eliminar un producto
        public bool Eliminar(int idProducto, out string mensaje)
        {
            return cdProducto.Eliminar(idProducto, out mensaje);
        }
    }
}
