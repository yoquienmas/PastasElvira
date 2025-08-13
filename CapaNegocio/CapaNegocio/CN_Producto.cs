using CapaDatos;
using CapaEntidad;

namespace CapaNegocio
{
    public class CN_Producto
    {
        private CD_Producto cdProducto = new CD_Producto();

        public List<Producto> Listar()
        {
            return cdProducto.Listar();
        }

        public int Registrar(Producto obj, out string mensaje)
        {
            return cdProducto.Registrar(obj, out mensaje);
        }

        public bool Editar(Producto obj, out string mensaje)
        {
            return cdProducto.Editar(obj, out mensaje);
        }

        public bool Eliminar(int idProducto, out string mensaje)
        {
            return cdProducto.Eliminar(idProducto, out mensaje);
        }
    }
}
