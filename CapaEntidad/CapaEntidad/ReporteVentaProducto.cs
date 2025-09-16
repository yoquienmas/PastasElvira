using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaEntidad
{
    public class ReporteVentaProducto
    {
        public int IdProducto { get; set; }
        public string NombreProducto { get; set; }
        public int CantidadVendida { get; set; }
        public decimal TotalVendido { get; set; }
        public decimal PrecioPromedio { get; set; }
        public DateTime Fecha { get; set; }
        public int Cantidad { get; set; }
        public decimal Subtotal { get; set; }
    }
}