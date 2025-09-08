using System.Collections.Generic;
using CapaEntidad;
using CapaDatos;

namespace CapaNegocio
{
    public class CN_MateriaPrima
    {
        private CD_MateriaPrima cd_materiaPrima = new CD_MateriaPrima();

        public List<MateriaPrima> Listar()
        {
            return cd_materiaPrima.Listar();
        }

        public int Registrar(MateriaPrima materia, out string mensaje)
        {
            return cd_materiaPrima.Registrar(materia, out mensaje);
        }

        public bool Editar(MateriaPrima materia, out string mensaje)
        {
            return cd_materiaPrima.Editar(materia, out mensaje);
        }

        public bool Eliminar(int idMateria, out string mensaje)
        {
            return cd_materiaPrima.Eliminar(idMateria, out mensaje);
        }
    }
}