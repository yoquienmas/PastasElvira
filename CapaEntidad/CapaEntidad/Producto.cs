using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaEntidad
{
    public class Producto
    {
        public int IdProducto { get; set; }
        public string Nombre { get; set; }
        public string Tipo { get; set; }
        public float PrecioVenta { get; set; }
        public int StockActual { get; set; }
        public int StockMinimo { get; set; }
        public bool Visible { get; set; }
    }

}

