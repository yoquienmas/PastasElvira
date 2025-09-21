using CapaDatos;
using CapaEntidad;
using System;
using System.Collections.Generic;

    namespace CapaNegocio
    {
        public class CN_Cliente
        {
            private CD_Cliente cdCliente = new CD_Cliente();

            public List<Cliente> ListarClientes()
            {
                return cdCliente.ListarClientes();
            }

            public bool Registrar(Cliente cliente, out string mensaje)
            {
                return cdCliente.Registrar(cliente, out mensaje);
            }

            public bool Editar(Cliente cliente, out string mensaje)
            {
                return cdCliente.Editar(cliente, out mensaje);
            }

            public bool Eliminar(int idCliente, out string mensaje)
            {
                return cdCliente.Eliminar(idCliente, out mensaje);
            }
        }
    }