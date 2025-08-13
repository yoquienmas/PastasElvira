using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaEntidad
{
    public class ItemVenta
    {
        public Producto Producto { get; set; }
        public int Cantidad { get; set; }
        public float PrecioUnitario { get; set; }
    }
}
