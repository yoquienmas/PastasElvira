using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaEntidad
{
    public class CostoFijo
    {
        public int IdCosto { get; set; }
        public string Concepto { get; set; }
        public decimal Monto { get; set; }
        public bool Activo { get; set; }
    }
}