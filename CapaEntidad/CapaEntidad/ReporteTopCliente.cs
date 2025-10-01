using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaEntidad
{
    public class ReporteTopCliente
    {
        public string Nombre { get; set; }
        public int CantidadCompras { get; set; }
        public decimal TotalGastado { get; set; }
        public decimal Porcentaje { get; set; }
    }

}
