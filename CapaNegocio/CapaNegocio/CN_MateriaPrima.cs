using System.Collections.Generic;
using CapaEntidad;
using CapaDatos;
using System;

namespace CapaNegocio
{
    public class CN_MateriaPrima
    {
        private CD_MateriaPrima cdMateriaPrima = new CD_MateriaPrima();

        // MÉTODOS BÁSICOS CORREGIDOS
        public List<MateriaPrima> Listar()
        {
            return cdMateriaPrima.Listar(); // CORREGIDO: llamar al método de capa de datos
        }

        public int Registrar(MateriaPrima materia, out string mensaje)
        {
            return cdMateriaPrima.Registrar(materia, out mensaje); // CORREGIDO
        }

        public bool Editar(MateriaPrima materia, out string mensaje)
        {
            bool resultado = cdMateriaPrima.Editar(materia, out mensaje); // CORREGIDO

            if (resultado)
            {
                // Publicar evento de actualización
                EventAggregator.Publish(new MateriaPrimaActualizadaEvent());

                // Actualizar costos de productos que usan esta materia prima
                ActualizarCostosProductosConMateria(materia.IdMateria);
            }

            return resultado;
        }

        public bool Eliminar(int idMateria, out string mensaje)
        {
            return cdMateriaPrima.Eliminar(idMateria, out mensaje); // CORREGIDO
        }

        // MÉTODOS NUEVOS (sin cambios)
        public List<MateriaPrima> ObtenerMateriasPrimasParaVerificar()
        {
            return cdMateriaPrima.ObtenerMateriasPrimasParaVerificar();
        }

        public List<int> ObtenerProductosConMateriaPrima(int idMateria)
        {
            return cdMateriaPrima.ObtenerProductosConMateriaPrima(idMateria);
        }

        public bool ActualizarStock(int idMateria, float cantidad)
        {
            return cdMateriaPrima.ActualizarStock(idMateria, cantidad);
        }

        private void ActualizarCostosProductosConMateria(int idMateria)
        {
            var productos = ObtenerProductosConMateriaPrima(idMateria);

            foreach (var idProducto in productos)
            {
                // Actualizar costo de cada producto
                CN_Producto cnProducto = new CN_Producto();
                cnProducto.ActualizarCostoProducto(idProducto);

                // Publicar evento
                EventAggregator.Publish(new ProductoActualizadoEvent());
            }
        }
    }
}