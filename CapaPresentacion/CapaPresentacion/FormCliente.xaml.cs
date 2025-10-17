using CapaEntidad;
using CapaNegocio;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

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
            listaClientes = cnCliente.ListarClientes();
            dgvClientes.ItemsSource = listaClientes;
        }

        private void LimpiarFormulario()
        {
            clienteSeleccionado = null;
            txtNombre.Text = "";
            txtApellido.Text = "";
            txtDocumento.Text = "";
            txtTelefono.Text = "";
            txtEmail.Text = "";
            txtCuil.Text = "";
            txtDireccion.Text = "";
            btnGuardar.Content = "Guardar";

            // NUEVO: Habilitar botón Guardar a "Actualizar"
            btnGuardar.IsEnabled = true;

            // NUEVO: Deshabilitar botones Editar y Eliminar
            btnEditar.IsEnabled = false;

            // Limpiar estilos de validación
            LimpiarEstilosValidacion();
        }

        private void LimpiarEstilosValidacion()
        {
            txtNombre.BorderBrush = System.Windows.Media.Brushes.Gray;
            txtApellido.BorderBrush = System.Windows.Media.Brushes.Gray;
            txtDocumento.BorderBrush = System.Windows.Media.Brushes.Gray;
            txtTelefono.BorderBrush = System.Windows.Media.Brushes.Gray;
            txtEmail.BorderBrush = System.Windows.Media.Brushes.Gray;
            txtCuil.BorderBrush = System.Windows.Media.Brushes.Gray;
        }

        private void btnNuevo_Click(object sender, RoutedEventArgs e)
        {
            LimpiarFormulario();
        }

        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidarFormulario())
                return;

            // NUEVO: Validar si ya existe cliente con el mismo DNI o CUIL
            if (!ValidarDocumentoYCuilUnicos())
                return;

            // Crear objeto cliente
            Cliente cliente = new Cliente
            {
                Nombre = txtNombre.Text.Trim(),
                Apellido = txtApellido.Text.Trim(),
                Documento = txtDocumento.Text.Trim(),
                Telefono = string.IsNullOrWhiteSpace(txtTelefono.Text) ? null : txtTelefono.Text.Trim(),
                Email = string.IsNullOrWhiteSpace(txtEmail.Text) ? null : txtEmail.Text.Trim(),
                Cuil = string.IsNullOrWhiteSpace(txtCuil.Text) ? "" : txtCuil.Text.Replace("-", "").Replace(" ", "").Trim(),
                Direccion = string.IsNullOrWhiteSpace(txtDireccion.Text) ? null : txtDireccion.Text.Trim()
            };

            string mensaje;
            bool resultado;

            if (clienteSeleccionado == null)
            {
                resultado = cnCliente.Registrar(cliente, out mensaje);
            }
            else
            {
                cliente.IdCliente = clienteSeleccionado.IdCliente;
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

        private void btnEditar_Click(object sender, RoutedEventArgs e)
        {
            if (clienteSeleccionado == null)
            {
                MessageBox.Show("Seleccione un cliente para editar", "Error",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!ValidarFormulario())
                return;

            // NUEVO: Validar si ya existe cliente con el mismo DNI o CUIL (excluyendo el actual)
            if (!ValidarDocumentoYCuilUnicos(clienteSeleccionado.IdCliente))
                return;

            // Crear objeto cliente con los datos actualizados (CORREGIDO)
            Cliente cliente = new Cliente
            {
                IdCliente = clienteSeleccionado.IdCliente, // ← AÑADIR ESTA LÍNEA
                Nombre = txtNombre.Text.Trim(),
                Apellido = txtApellido.Text.Trim(),
                Documento = txtDocumento.Text.Trim(),
                Telefono = string.IsNullOrWhiteSpace(txtTelefono.Text) ? null : txtTelefono.Text.Trim(),
                Email = string.IsNullOrWhiteSpace(txtEmail.Text) ? null : txtEmail.Text.Trim(),
                Cuil = string.IsNullOrWhiteSpace(txtCuil.Text) ? null : txtCuil.Text.Replace("-", "").Replace(" ", "").Trim(),
                Direccion = string.IsNullOrWhiteSpace(txtDireccion.Text) ? null : txtDireccion.Text.Trim()
            };

            string mensaje;
            bool resultado = cnCliente.Editar(cliente, out mensaje);

            MessageBox.Show(mensaje, resultado ? "Éxito" : "Error",
                MessageBoxButton.OK,
                resultado ? MessageBoxImage.Information : MessageBoxImage.Error);

            if (resultado)
            {
                CargarClientes();
                LimpiarFormulario();
            }
        }

        // NUEVO: Método para validar que el DNI y CUIL sean únicos
        private bool ValidarDocumentoYCuilUnicos(int idClienteActual = 0)
        {
            string documento = txtDocumento.Text.Trim();
            string cuil = string.IsNullOrWhiteSpace(txtCuil.Text) ? "" : txtCuil.Text.Replace("-", "").Replace(" ", "").Trim();

            bool existeDocumento = cnCliente.ExisteDocumento(documento, idClienteActual);
            bool existeCuil = false;

            if (!string.IsNullOrWhiteSpace(cuil))
            {
                existeCuil = cnCliente.ExisteCuil(cuil, idClienteActual);
            }

            if (existeDocumento && existeCuil)
            {
                MessageBox.Show("Error: Ya existe un cliente registrado con ese DNI y CUIL.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            else if (existeDocumento)
            {
                MessageBox.Show("Error: Ya existe un cliente registrado con ese DNI.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            else if (existeCuil)
            {
                MessageBox.Show("Error: Ya existe un cliente registrado con ese CUIL.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }

        private bool ValidarFormulario()
        {
            bool esValido = true;
            LimpiarEstilosValidacion();

            // Validar Nombre
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                txtNombre.BorderBrush = System.Windows.Media.Brushes.Red;
                MessageBox.Show("El nombre es obligatorio", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                esValido = false;
            }
            else if (txtNombre.Text.Trim().Length < 3)
            {
                txtNombre.BorderBrush = System.Windows.Media.Brushes.Red;
                MessageBox.Show("El nombre debe tener al menos 3 caracteres", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                esValido = false;
            }

            if (string.IsNullOrWhiteSpace(txtApellido.Text))
            {
                txtApellido.BorderBrush = System.Windows.Media.Brushes.Red;
                MessageBox.Show("El apellido es obligatorio", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                esValido = false;
            }

            else if (txtApellido.Text.Trim().Length < 3)
            {
                txtApellido.BorderBrush = System.Windows.Media.Brushes.Red;
                MessageBox.Show("El nombre debe tener al menos 3 caracteres", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                esValido = false;
            }
            // Validar Documento
            if (string.IsNullOrWhiteSpace(txtDocumento.Text))
            {
                txtDocumento.BorderBrush = System.Windows.Media.Brushes.Red;
                MessageBox.Show("El documento es obligatorio", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                esValido = false;
            }
            else if (!EsDocumentoValido(txtDocumento.Text.Trim()))
            {
                txtDocumento.BorderBrush = System.Windows.Media.Brushes.Red;
                MessageBox.Show("El documento no tiene un formato válido", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                esValido = false;
            }

            // Validar Teléfono (si está completado)
            if (!string.IsNullOrWhiteSpace(txtTelefono.Text) && !EsTelefonoValido(txtTelefono.Text.Trim()))
            {
                txtTelefono.BorderBrush = System.Windows.Media.Brushes.Red;
                MessageBox.Show("El teléfono no tiene un formato válido", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                esValido = false;
            }

            // Validar Email (si está completado)
            if (!string.IsNullOrWhiteSpace(txtEmail.Text) && !EsEmailValido(txtEmail.Text.Trim()))
            {
                txtEmail.BorderBrush = System.Windows.Media.Brushes.Red;
                MessageBox.Show("El email no tiene un formato válido", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                esValido = false;
            }

            // Validar CUIL (si está completado)
            if (!string.IsNullOrWhiteSpace(txtCuil.Text) && !EsCuilValido(txtCuil.Text.Trim()))
            {
                txtCuil.BorderBrush = System.Windows.Media.Brushes.Red;
                MessageBox.Show("El CUIL debe tener exactamente 11 dígitos numéricos", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                esValido = false;
            }

            return esValido;
        }

        private bool EsDocumentoValido(string documento)
        {
            // Permitir solo números y guiones, entre 7 y 15 caracteres
            return Regex.IsMatch(documento, @"^[0-9\-]{7,15}$");
        }

        private bool EsTelefonoValido(string telefono)
        {
            // Validar formato de teléfono (números, paréntesis, guiones, espacios)
            return Regex.IsMatch(telefono, @"^[0-9\s\(\)\-]{8,20}$");
        }

        private bool EsEmailValido(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private bool EsCuilValido(string cuil)
        {
            // Validar que tenga exactamente 11 dígitos numéricos (ignorando guiones y espacios)
            string cuilLimpio = cuil.Replace("-", "").Replace(" ", "").Trim();

            // Debe tener exactamente 11 dígitos y ser numérico
            return cuilLimpio.Length == 11 && long.TryParse(cuilLimpio, out _);
        }

        private void dgvClientes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgvClientes.SelectedItem is Cliente cliente)
            {
                clienteSeleccionado = cliente;
                txtNombre.Text = cliente.Nombre;
                txtApellido.Text = cliente.Apellido;
                txtDocumento.Text = cliente.Documento;
                txtTelefono.Text = cliente.Telefono;
                txtEmail.Text = cliente.Email;
                txtCuil.Text = cliente.Cuil;
                txtDireccion.Text = cliente.Direccion;

                // NUEVO: Deshabilitar botón Guardar a "Actualizar"
                btnGuardar.IsEnabled = false;

                // NUEVO: Habilitar botón Editar
                btnEditar.IsEnabled = true;

                LimpiarEstilosValidacion();
            }
        }

        // Eventos para validación en tiempo real
        private void txtNombre_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtNombre.Text))
                txtNombre.BorderBrush = System.Windows.Media.Brushes.Gray;
        }

        // NUEVO: validación no admite un dni ya existente 
        private void txtDocumento_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtDocumento.Text) && EsDocumentoValido(txtDocumento.Text))
            {
                int idActual = clienteSeleccionado?.IdCliente ?? 0;
                if (cnCliente.ExisteDocumento(txtDocumento.Text, idActual))
                {
                    txtDocumento.BorderBrush = Brushes.Red;
                    ToolTipService.SetToolTip(txtDocumento, "Ya existe un cliente con este DNI.");
                }
                else
                {
                    txtDocumento.BorderBrush = Brushes.Gray;
                    ToolTipService.SetToolTip(txtDocumento, null);
                }
            }
        }

        private void txtTelefono_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtTelefono.Text) || EsTelefonoValido(txtTelefono.Text))
                txtTelefono.BorderBrush = System.Windows.Media.Brushes.Gray;
        }

        private void txtEmail_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtEmail.Text) || EsEmailValido(txtEmail.Text))
                txtEmail.BorderBrush = System.Windows.Media.Brushes.Gray;
        }

        // NUEVO: validación no admite un cuil ya existente 
        private void txtCuil_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtCuil.Text) && EsCuilValido(txtCuil.Text))
            {
                int idActual = clienteSeleccionado?.IdCliente ?? 0;
                if (cnCliente.ExisteCuil(txtCuil.Text, idActual))
                {
                    txtCuil.BorderBrush = Brushes.Red;
                    ToolTipService.SetToolTip(txtCuil, "Ya existe un cliente con este CUIL.");
                }
                else
                {
                    txtCuil.BorderBrush = Brushes.Gray;
                    ToolTipService.SetToolTip(txtCuil, null);
                }
            }
        }

        // Validación de entrada para solo números
        private void SoloNumeros_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (sender == txtDocumento || sender == txtTelefono)
            {
                e.Handled = !EsNumero(e.Text);
            }
        }

        private bool EsNumero(string texto)
        {
            return int.TryParse(texto, out _);
        }

        // Formato automático para CUIL
        // Limpiar formato al perder el foco (opcional, para mostrar en la UI)
        private void txtCuil_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtCuil.Text))
            {
                string cuilLimpio = txtCuil.Text.Replace("-", "").Replace(" ", "").Trim();

                if (cuilLimpio.Length == 11 && long.TryParse(cuilLimpio, out _))
                {
                    // Opcional: Mostrar formateado en UI pero guardar sin formato
                    txtCuil.Text = $"{cuilLimpio.Substring(0, 2)}-{cuilLimpio.Substring(2, 8)}-{cuilLimpio.Substring(10, 1)}";
                }
            }
        }
    }
}