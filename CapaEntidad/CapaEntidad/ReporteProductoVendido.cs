using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaEntidad
{
    public class ReporteProductoVendido
    {
        public string NombreProducto { get; set; }
        public int CantidadVendida { get; set; }
        public decimal TotalVendido { get; set; }
        public decimal Porcentaje { get; set; }
    }
}
