using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaEntidad
{
    public class ReporteVentaPorTipo
    {
        public string Tipo { get; set; }
        public int Cantidad { get; set; }
        public decimal Total { get; set; }
        public decimal Porcentaje { get; set; }
    }


}
