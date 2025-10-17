using CapaEntidad;
using CapaNegocio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace CapaPresentacion
{
    public partial class FormBuscarCliente : Window
    {
        public Cliente ClienteSeleccionado { get; private set; }
        private CN_Cliente cnCliente = new CN_Cliente();
        private List<Cliente> listaClientes;

        public FormBuscarCliente()
        {
            InitializeComponent();
            CargarClientes();
        }

        private void CargarClientes()
        {
            try
            {
                listaClientes = cnCliente.ListarClientes();
                dgClientes.ItemsSource = listaClientes;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar clientes: " + ex.Message, "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnBuscar_Click(object sender, RoutedEventArgs e)
        {
            BuscarClientes();
        }

        private void txtDocumento_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                BuscarClientes();
            }
        }

        private void BuscarClientes()
        {
            string documento = txtDocumento.Text.Trim();

            if (string.IsNullOrEmpty(documento))
            {
                dgClientes.ItemsSource = listaClientes;
                return;
            }

            var clientesFiltrados = listaClientes
                .Where(c => c.Documento.Contains(documento) ||
                           c.NombreCompleto.ToLower().Contains(documento.ToLower()))
                .ToList();

            dgClientes.ItemsSource = clientesFiltrados;
        }

        private void btnSeleccionar_Click(object sender, RoutedEventArgs e)
        {
            if (dgClientes.SelectedItem is Cliente cliente)
            {
                ClienteSeleccionado = cliente;
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                MessageBox.Show("Por favor seleccione un cliente de la lista.", "Advertencia",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void dgClientes_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (dgClientes.SelectedItem is Cliente cliente)
            {
                ClienteSeleccionado = cliente;
                this.DialogResult = true;
                this.Close();
            }
        }
    }
}