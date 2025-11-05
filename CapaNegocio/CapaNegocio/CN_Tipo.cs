using CapaDatos;
using CapaEntidad;
using System;
using System.Collections.Generic;

namespace CapaNegocio
{
    public class CN_Tipo
    {
        private CD_Tipo cdTipo = new CD_Tipo();

        public List<Tipo> Listar()
        {
            try
            {
                return cdTipo.Listar();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al listar tipos: {ex.Message}");
            }
        }

        public bool Registrar(Tipo tipo, out string mensaje)
        {
            mensaje = string.Empty;

            if (string.IsNullOrWhiteSpace(tipo.Descripcion))
            {
                mensaje = "La descripción del tipo no puede estar vacía";
                return false;
            }

            try
            {
                return cdTipo.Registrar(tipo, out mensaje);
            }
            catch (Exception ex)
            {
                mensaje = $"Error al registrar tipo: {ex.Message}";
                return false;
            }
        }

        public bool Editar(Tipo tipo, out string mensaje)
        {
            mensaje = string.Empty;

            if (string.IsNullOrWhiteSpace(tipo.Descripcion))
            {
                mensaje = "La descripción del tipo no puede estar vacía";
                return false;
            }

            if (tipo.IdTipo == 0)
            {
                mensaje = "El ID del tipo es inválido";
                return false;
            }

            try
            {
                return cdTipo.Editar(tipo, out mensaje);
            }
            catch (Exception ex)
            {
                mensaje = $"Error al editar tipo: {ex.Message}";
                return false;
            }
        }

        public bool Eliminar(int idTipo, out string mensaje)
        {
            mensaje = string.Empty;

            if (idTipo == 0)
            {
                mensaje = "El ID del tipo es inválido";
                return false;
            }

            try
            {
                return cdTipo.Eliminar(idTipo, out mensaje);
            }
            catch (Exception ex)
            {
                mensaje = $"Error al eliminar tipo: {ex.Message}";
                return false;
            }
        }
    }
}