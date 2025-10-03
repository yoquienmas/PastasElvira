using System.Collections.Generic;
using CapaEntidad;
using CapaDatos;
using System;

namespace CapaNegocio
{
    public class CN_MateriaPrima
    {
        private CD_MateriaPrima cdMateriaPrima = new CD_MateriaPrima();

        // MÉTODO NUEVO: Verificar si existe materia prima con el mismo nombre
        public bool ExisteMateriaPrima(string nombre, int idExcluir = 0)
        {
            return cdMateriaPrima.ExisteMateriaPrima(nombre, idExcluir);
        }

        // MÉTODOS BÁSICOS CORREGIDOS
        public List<MateriaPrima> Listar()
        {
            return cdMateriaPrima.Listar();
        }

        public int Registrar(MateriaPrima materia, out string mensaje)
        {
            // Validar si ya existe una materia prima con el mismo nombre
            if (ExisteMateriaPrima(materia.Nombre))
            {
                mensaje = "Ya existe una materia prima con el mismo nombre";
                return 0;
            }

            return cdMateriaPrima.Registrar(materia, out mensaje);
        }

        public bool Editar(MateriaPrima materia, out string mensaje)
        {
            // Validar si ya existe otra materia prima con el mismo nombre (excluyendo la actual)
            if (ExisteMateriaPrima(materia.Nombre, materia.IdMateria))
            {
                mensaje = "Ya existe otra materia prima con el mismo nombre";
                return false;
            }

            bool resultado = cdMateriaPrima.Editar(materia, out mensaje);

            if (resultado)
            {
                EventAggregator.Publish(new MateriaPrimaActualizadaEvent());
                ActualizarCostosProductosConMateria(materia.IdMateria);
            }

            return resultado;
        }

        public bool Eliminar(int idMateria, out string mensaje)
        {
            return cdMateriaPrima.Eliminar(idMateria, out mensaje);
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
                CN_Producto cnProducto = new CN_Producto();
                cnProducto.ActualizarCostoProducto(idProducto);
                EventAggregator.Publish(new ProductoActualizadoEvent());
            }
        }
    }
}