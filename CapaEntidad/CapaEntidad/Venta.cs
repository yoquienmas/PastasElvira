using System;
using System.Collections.Generic;
using System.Linq;

namespace CapaEntidad
{
    public class Venta
    {
        public int IdVenta { get; set; }
        public DateTime Fecha { get; set; } = DateTime.Now;
        public List<ItemVenta> Items { get; set; } = new List<ItemVenta>();
        public float Total => Items.Sum(i => i.Cantidad * i.PrecioUnitario);
    }
}
