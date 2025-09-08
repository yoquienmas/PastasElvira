using CapaEntidad;
using CapaNegocio;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace CapaPresentacion
{
    public partial class FormCliente : Window
    {
        private CN_Cliente cnCliente = new CN_Cliente();
        private List<Cliente> listaClientes;
        private Cliente clienteSeleccionado = null;

        public FormCliente()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CargarClientes();
            LimpiarFormulario();
        }

        private void CargarClientes()
        {
            listaClientes = cnCliente.Listar();
            dgvClientes.ItemsSource = listaClientes;
        }

        private void LimpiarFormulario()
        {
            clienteSeleccionado = null;
            txtNombre.Text = "";
            txtDocumento.Text = "";
            txtTelefono.Text = "";
            txtEmail.Text = "";
            txtDireccion.Text = "";
            btnGuardar.Content = "Guardar";
            btnEliminar.IsEnabled = false;
        }

        private void btnNuevo_Click(object sender, RoutedEventArgs e)
        {
            LimpiarFormulario();
        }

        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            // Validaciones
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("El nombre es obligatorio", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtDocumento.Text))
            {
                MessageBox.Show("El documento es obligatorio", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Crear objeto cliente
            Cliente cliente = new Cliente
            {
                Nombre = txtNombre.Text.Trim(),
                Documento = txtDocumento.Text.Trim(),
                Telefono = string.IsNullOrWhiteSpace(txtTelefono.Text) ? null : txtTelefono.Text.Trim(),
                Email = string.IsNullOrWhiteSpace(txtEmail.Text) ? null : txtEmail.Text.Trim(),
                Direccion = string.IsNullOrWhiteSpace(txtDireccion.Text) ? null : txtDireccion.Text.Trim()
            };

            // Si hay cliente seleccionado, es una edición
            if (clienteSeleccionado != null)
            {
                cliente.IdCliente = clienteSeleccionado.IdCliente;
            }

            string mensaje;
            bool resultado;

            if (clienteSeleccionado == null)
            {
                // Registrar nuevo cliente
                resultado = cnCliente.Registrar(cliente, out mensaje);
            }
            else
            {
                // Editar cliente existente
                resultado = cnCliente.Editar(cliente, out mensaje);
            }

            MessageBox.Show(mensaje, resultado ? "Éxito" : "Error",
                MessageBoxButton.OK,
                resultado ? MessageBoxImage.Information : MessageBoxImage.Error);

            if (resultado)
            {
                CargarClientes();
                LimpiarFormulario();
            }
        }

        private void btnEliminar_Click(object sender, RoutedEventArgs e)
        {
            if (clienteSeleccionado == null)
            {
                MessageBox.Show("Seleccione un cliente para eliminar", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var confirmacion = MessageBox.Show($"¿Está seguro de eliminar al cliente: {clienteSeleccionado.Nombre}?",
                "Confirmar Eliminación", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (confirmacion == MessageBoxResult.Yes)
            {
                string mensaje;
                bool resultado = cnCliente.Eliminar(clienteSeleccionado.IdCliente, out mensaje);

                MessageBox.Show(mensaje, resultado ? "Éxito" : "Error",
                    MessageBoxButton.OK,
                    resultado ? MessageBoxImage.Information : MessageBoxImage.Error);

                if (resultado)
                {
                    CargarClientes();
                    LimpiarFormulario();
                }
            }
        }

        private void dgvClientes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgvClientes.SelectedItem is Cliente cliente)
            {
                clienteSeleccionado = cliente;
                txtNombre.Text = cliente.Nombre;
                txtDocumento.Text = cliente.Documento;
                txtTelefono.Text = cliente.Telefono;
                txtEmail.Text = cliente.Email;
                txtDireccion.Text = cliente.Direccion;
                btnGuardar.Content = "Actualizar";
                btnEliminar.IsEnabled = true;
            }
        }
    }
}