using CapaDatos;
using CapaEntidad;
using System;
using System.Collections.Generic;

namespace CapaNegocio
{
    public class CN_Cliente
    {
        private CD_Cliente cd_cliente = new CD_Cliente();

        public List<Cliente> Listar()
        {
            return cd_cliente.Listar();
        }

        public bool Registrar(Cliente cliente, out string mensaje)
        {
            return cd_cliente.Registrar(cliente, out mensaje);
        }

        public bool Editar(Cliente cliente, out string mensaje)
        {
            return cd_cliente.Editar(cliente, out mensaje);
        }

        public bool Eliminar(int idCliente, out string mensaje)
        {
            return cd_cliente.Eliminar(idCliente, out mensaje);
        }
    }
}