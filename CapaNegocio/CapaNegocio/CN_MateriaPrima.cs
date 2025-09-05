using System.Collections.Generic;
using CapaEntidad;
using CapaDatos;

namespace CapaNegocio
{
    public class CN_MateriaPrima
    {
        private CD_MateriaPrima objcd_materia = new CD_MateriaPrima();

        public List<MateriaPrima> Listar()
        {
            return objcd_materia.Listar();
        }

        public int Registrar(MateriaPrima obj, out string mensaje)
        {
            mensaje = string.Empty;
            if (string.IsNullOrWhiteSpace(obj.Nombre))
            {
                mensaje = "El nombre de la materia prima no puede estar vacío.";
                return 0;
            }

            return objcd_materia.Registrar(obj, out mensaje);
        }

        public bool Editar(MateriaPrima obj, out string mensaje)
        {
            mensaje = string.Empty;
            if (obj.IdMateria == 0)
            {
                mensaje = "Debe seleccionar una materia prima.";
                return false;
            }

            return objcd_materia.Editar(obj, out mensaje);
        }

        public bool Eliminar(int id, out string mensaje)
        {
            return objcd_materia.Eliminar(id, out mensaje);
        }
    }
}
