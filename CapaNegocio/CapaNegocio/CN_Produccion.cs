using System.Collections.Generic;
using CapaDatos;
using CapaEntidad;

namespace CapaNegocio
{
    public class CN_Produccion
    {
        private CD_Produccion cd_produccion = new CD_Produccion(); // ✅ Instanciar

        public List<Produccion> Listar()
        {
            return cd_produccion.Listar();
        }

        public bool Registrar(Produccion produccion, out string mensaje)
        {
            return cd_produccion.Registrar(produccion, out mensaje);
        }

        public string ValidarDisponibilidadMateriasPrimas(int idProducto, int cantidad)
        {
            return cd_produccion.ValidarDisponibilidadMateriasPrimas(idProducto, cantidad);
        }

        public List<DetalleReceta> ListarRecetaPorProducto(int idProducto)
        {
            return cd_produccion.ListarRecetaPorProducto(idProducto);
        }

        public bool AgregarMateriaPrimaAReceta(int idProducto, int idMateria, float cantidad)
        {
            return cd_produccion.AgregarMateriaPrimaAReceta(idProducto, idMateria, cantidad);
        }

        public bool EliminarMateriaPrimaDeReceta(int idProductoMateria)
        {
            return cd_produccion.EliminarMateriaPrimaDeReceta(idProductoMateria);
        }
    }
}