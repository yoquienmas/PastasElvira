using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaEntidad
{
    public class MateriaPrima
    {
        public int IdMateria { get; set; }
        public string Nombre { get; set; }
        public string Unidad { get; set; } // Kg, unidades, etc.
        public float CantidadDisponible { get; set; }
    }
}
