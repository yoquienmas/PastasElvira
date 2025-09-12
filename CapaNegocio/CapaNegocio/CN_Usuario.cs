using CapaDatos;
using CapaEntidad;
using System;
using System.Collections.Generic;

namespace CapaNegocio
{
    public class CN_Usuario
    {
        private CD_Usuario cd_usuarios = new CD_Usuario(); 

        public bool Login(string nombre, string clave)
        {
            if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(clave))
                throw new Exception("Usuario y contraseña son obligatorios");

            return cd_usuarios.ValidarLogin(nombre, clave);
        }
    }


}

