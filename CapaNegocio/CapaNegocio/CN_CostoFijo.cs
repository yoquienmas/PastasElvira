using CapaDatos;
using CapaEntidad;
using System.Collections.Generic;

namespace CapaNegocio
{
    public class CN_CostoFijo
    {
        private CD_CostoFijo cd_costoFijo = new CD_CostoFijo();

        public List<CostoFijo> Listar()
        {
            return cd_costoFijo.Listar();
        }

        public bool Registrar(CostoFijo costo, out string mensaje)
        {
            return cd_costoFijo.Registrar(costo, out mensaje);
        }

        public bool Editar(CostoFijo costo, out string mensaje)
        {
            return cd_costoFijo.Editar(costo, out mensaje);
        }

        public bool Eliminar(int idCosto, out string mensaje)
        {
            return cd_costoFijo.Eliminar(idCosto, out mensaje);
        }
    }
}