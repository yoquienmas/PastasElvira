using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaEntidad
{
    public class Sabor
    {
        public int IdSabor { get; set; }
        public string Descripcion { get; set; }
        public bool Activo { get; set; }

        public Sabor()
        {
            Descripcion = string.Empty;
            Activo = true;
        }
    }
}
