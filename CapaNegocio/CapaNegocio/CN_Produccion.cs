using System.Collections.Generic;
using CapaDatos;
using CapaEntidad;

namespace CapaNegocio
{
    public class CN_Produccion
    {
        private CD_Produccion cdProduccion = new CD_Produccion();

        public List<Produccion> Listar()
        {
            return cdProduccion.Listar();
        }

        public int Registrar(Produccion produccion, out string mensaje)
        {
            return cdProduccion.Registrar(produccion, out mensaje);
        }
    }
}
