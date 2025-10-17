using CapaEntidad;
using CapaDatos;
using System.Collections.Generic;

namespace CapaNegocio
{
    public class CN_MetodoPago
    {
        private CD_MetodoPago cdMetodoPago = new CD_MetodoPago();

        public List<MetodoPago> Listar()
        {
            return cdMetodoPago.Listar();
        }
    }
}