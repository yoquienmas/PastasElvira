using System;
using System.Collections.Generic;
using CapaDatos;
using CapaEntidad;

namespace CapaNegocio
{
    public class CN_Produccion
    {
        private CD_Produccion cdProduccion = new CD_Produccion();

        public int Registrar(Produccion produccion, out string mensaje)
        {
            return cdProduccion.Registrar(produccion, out mensaje);
        }

        public bool Actualizar(Produccion produccion, out string mensaje)
        {
            return cdProduccion.Actualizar(produccion, out mensaje);
        }

        public bool Eliminar(int idProduccion, out string mensaje)
        {
            return cdProduccion.Eliminar(idProduccion, out mensaje);
        }

     

        public List<Produccion> Listar()
        {
            return cdProduccion.Listar();
        }
    }
}