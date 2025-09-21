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

            // Método para obtener usuario por ID
            public DataTable ObtenerUsuarioPorId(int idUsuario)
            {
                return objetoCD.ObtenerUsuarioPorId(idUsuario);
            }

            // Mostrar todos los usuarios
            public DataTable MostrarUsuarios()
            {
                return objetoCD.MostrarUsuarios();
            }

            // Insertar usuario completo
            public bool InsertarUsuario(string nombreUsuario, string nombre, string apellido, string documento,
                                      string telefono, string email, string cuil, string direccion,
                                      string clave, string rol, bool activo)
            {
                return objetoCD.InsertarUsuario(nombreUsuario, nombre, apellido, documento, telefono,
                                              email, cuil, direccion, clave, rol, activo);
            }

            // Editar usuario completo
            public bool EditarUsuario(int idUsuario, string nombreUsuario, string nombre, string apellido,
                                    string documento, string telefono, string email, string cuil,
                                    string direccion, string clave, string rol, bool activo)
            {
                return objetoCD.EditarUsuario(idUsuario, nombreUsuario, nombre, apellido, documento,
                                            telefono, email, cuil, direccion, clave, rol, activo);
            }

            // Eliminar usuario
            public bool EliminarUsuario(int idUsuario)
            {
                return objetoCD.EliminarUsuario(idUsuario);
            }
        }
    }