using System.Collections.Generic;
using CapaDatos;
using CapaEntidad;

namespace CapaNegocio
{
    public class CN_MateriaPrima
    {
        private CD_MateriaPrima cdMateria = new CD_MateriaPrima();

        public List<MateriaPrima> Listar()
        {
            return cdMateria.Listar();
        }

        public int Registrar(MateriaPrima materia, out string mensaje)
        {
            mensaje = "";

            if (string.IsNullOrWhiteSpace(materia.Nombre))
                mensaje = "El nombre de la materia prima es obligatorio.";
            else if (string.IsNullOrWhiteSpace(materia.Unidad))
                mensaje = "Debe especificar la unidad (Kg, L, etc.).";
            else if (materia.CantidadDisponible < 0)
                mensaje = "La cantidad disponible no puede ser negativa.";

            if (!string.IsNullOrWhiteSpace(mensaje))
                return 0;

            return cdMateria.Registrar(materia, out mensaje);
        }

        public bool Editar(MateriaPrima materia, out string mensaje)
        {
            mensaje = "";

            if (string.IsNullOrWhiteSpace(materia.Nombre))
                mensaje = "El nombre de la materia prima es obligatorio.";
            else if (string.IsNullOrWhiteSpace(materia.Unidad))
                mensaje = "Debe especificar la unidad (Kg, L, etc.).";
            else if (materia.CantidadDisponible < 0)
                mensaje = "La cantidad disponible no puede ser negativa.";

            if (!string.IsNullOrWhiteSpace(mensaje))
                return false;

            return cdMateria.Editar(materia, out mensaje);
        }

        public bool Eliminar(int id, out string mensaje)
        {
            return cdMateria.Eliminar(id, out mensaje);
        }
    }
}
