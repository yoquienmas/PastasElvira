using System.Collections.Generic;
using CapaDatos;
using CapaEntidad;

namespace CapaNegocio
{
    public class CN_Producto
    {
        private CD_Producto cd_producto = new CD_Producto();

        public List<Producto> Listar()
        {
            return cd_producto.Listar();
        }

        public int Registrar(Producto producto, out string mensaje)
        {
            return cd_producto.Registrar(producto, out mensaje);
        }

        public bool Editar(Producto producto, out string mensaje)
        {
            return cd_producto.Editar(producto, out mensaje);
        }

        public bool Eliminar(int idProducto, out string mensaje)
        {
            return cd_producto.Eliminar(idProducto, out mensaje);
        }

        public Producto ObtenerProducto(int idProducto)
        {
            return cd_producto.ObtenerProducto(idProducto);
        }

        public bool CalcularCostoProducto(int idProducto, out decimal costoTotal, out decimal precioSugerido, out string mensaje)
        {
            return cd_producto.CalcularCostoProducto(idProducto, out costoTotal, out precioSugerido, out mensaje);
        }

    }
}
