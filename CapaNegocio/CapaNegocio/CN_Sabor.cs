using CapaDatos;
using CapaEntidad;
using System;
using System.Collections.Generic;

namespace CapaNegocio
{
    public class CN_Sabor
    {
        private CD_Sabor cdSabor = new CD_Sabor();

        public List<Sabor> Listar()
        {
            try
            {
                return cdSabor.Listar();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al listar sabores: {ex.Message}");
            }
        }

        public bool Registrar(Sabor sabor, out string mensaje)
        {
            mensaje = string.Empty;

            if (string.IsNullOrWhiteSpace(sabor.Descripcion))
            {
                mensaje = "La descripción del sabor no puede estar vacía";
                return false;
            }

            try
            {
                return cdSabor.Registrar(sabor, out mensaje);
            }
            catch (Exception ex)
            {
                mensaje = $"Error al registrar sabor: {ex.Message}";
                return false;
            }
        }

        public bool Editar(Sabor sabor, out string mensaje)
        {
            mensaje = string.Empty;

            if (string.IsNullOrWhiteSpace(sabor.Descripcion))
            {
                mensaje = "La descripción del sabor no puede estar vacía";
                return false;
            }

            if (sabor.IdSabor == 0)
            {
                mensaje = "El ID del sabor es inválido";
                return false;
            }

            try
            {
                return cdSabor.Editar(sabor, out mensaje);
            }
            catch (Exception ex)
            {
                mensaje = $"Error al editar sabor: {ex.Message}";
                return false;
            }
        }

        public bool Eliminar(int idSabor, out string mensaje)
        {
            mensaje = string.Empty;

            if (idSabor == 0)
            {
                mensaje = "El ID del sabor es inválido";
                return false;
            }

            try
            {
                return cdSabor.Eliminar(idSabor, out mensaje);
            }
            catch (Exception ex)
            {
                mensaje = $"Error al eliminar sabor: {ex.Message}";
                return false;
            }
        }
    }
}