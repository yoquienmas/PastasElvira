using System;
using System.Data;
using CapaDatos;

namespace CapaNegocio
{
    public class CN_Usuario
    {
        private CD_Usuario objetoCD = new CD_Usuario();

        // Método para login
        public bool Login(string NombreUsuario, string clave)
        {
            return objetoCD.Login(NombreUsuario, clave);
        }

        // Método para obtener usuario por nombre
        public DataTable ObtenerUsuarioPorNombre(string NombreUsuario)
        {
            return objetoCD.ObtenerUsuarioPorNombre(NombreUsuario);
        }

        // Métodos existentes
        public DataTable MostrarUsuarios()
        {
            return objetoCD.MostrarUsuarios();
        }

        public DataTable MostrarUsuarioPorId(string idUsuario)
        {
            return objetoCD.MostrarUsuarioPorId(idUsuario);
        }

        public void InsertarUsuario(string NombreUsuario, string clave, string rol, string activo)
        {
            objetoCD.InsertarUsuario(NombreUsuario, clave, rol, activo);
        }

        public void EditarUsuario(string idUsuario, string NombreUsuario, string clave, string rol, string activo)
        {
            objetoCD.EditarUsuario(idUsuario, NombreUsuario, clave, rol, activo);
        }

        public void EliminarUsuario(string idUsuario)
        {
            objetoCD.EliminarUsuario(idUsuario);
        }
    }
}