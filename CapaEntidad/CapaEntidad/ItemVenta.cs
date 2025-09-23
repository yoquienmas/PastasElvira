using System;
using System.Collections.Generic;
namespace CapaEntidad
{
    public class ItemVenta
    {
        public int IdProducto { get; set; }
        public string NombreProducto { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal
        {
            get { return Cantidad * PrecioUnitario; }
        }
    }
}
