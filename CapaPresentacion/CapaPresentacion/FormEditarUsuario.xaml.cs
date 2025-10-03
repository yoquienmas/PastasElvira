using CapaDatos;
using CapaNegocio;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace CapaPresentacion
{
    public partial class FormEditarUsuario : Window
    {
        private CN_Usuario objetoCN = new CN_Usuario();
        private int idUsuario = 0;
        private bool isEditMode = false;

        public FormEditarUsuario()
        {
            InitializeComponent();
            Title = "Nuevo Usuario";
            txtId.Text = "Nuevo";
        }

        public FormEditarUsuario(string id)
        {
            InitializeComponent();
            if (int.TryParse(id, out idUsuario))
            {
                isEditMode = true;
                Title = "Editar Usuario";
                CargarDatosUsuario();
            }
        }

        private void CargarDatosUsuario()
        {
            try
            {
                DataTable dt = objetoCN.ObtenerUsuarioPorId(idUsuario);

                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    txtId.Text = row["IdUsuario"].ToString();
                    txtNombreUsuario.Text = row["NombreUsuario"].ToString();
                    txtNombre.Text = row["Nombre"].ToString();
                    txtApellido.Text = row["Apellido"].ToString();
                    txtDocumento.Text = row["Documento"].ToString();
                    txtTelefono.Text = row["Telefono"].ToString();
                    txtEmail.Text = row["Email"].ToString();
                    txtCuil.Text = row["Cuil"].ToString();
                    txtDireccion.Text = row["Direccion"].ToString();

                    // Seleccionar rol
                    string rol = row["Rol"].ToString();
                    foreach (ComboBoxItem item in cmbRol.Items)
                    {
                        if (item.Tag.ToString() == rol)
                        {
                            cmbRol.SelectedItem = item;
                            break;
                        }
                    }

                    // Seleccionar estado
                    bool estado = Convert.ToBoolean(row["Activo"]);
                    foreach (ComboBoxItem item in cmbEstado.Items)
                    {
                        if (Convert.ToBoolean(item.Tag) == estado)
                        {
                            cmbEstado.SelectedItem = item;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MostrarMensaje("Error al cargar datos: " + ex.Message);
            }
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (ValidarCampos())
            {
                try
                {
                    string nombreUsuario = txtNombreUsuario.Text.Trim();
                    string nombre = txtNombre.Text.Trim();
                    string apellido = txtApellido.Text.Trim();
                    string documento = txtDocumento.Text.Trim();
                    string telefono = txtTelefono.Text.Trim();
                    string email = txtEmail.Text.Trim();
                    string cuil = txtCuil.Text.Trim();
                    string direccion = txtDireccion.Text.Trim();
                    string clave = txtPassword.Password;
                    string rol = ((ComboBoxItem)cmbRol.SelectedItem).Tag.ToString();
                    bool activo = Convert.ToBoolean(((ComboBoxItem)cmbEstado.SelectedItem).Tag);

                    if (isEditMode)
                    {
                        // Para edición, si no se ingresa nueva contraseña, mantener la actual
                        string claveParaEditar = string.IsNullOrWhiteSpace(clave) ? null : clave;

                        // Editar usuario existente
                        bool resultado = objetoCN.EditarUsuario(
                            idUsuario, nombreUsuario, nombre, apellido, documento,
                            telefono, email, cuil, direccion, claveParaEditar, rol, activo
                        );

                        if (resultado)
                        {
                            MessageBox.Show("Usuario actualizado correctamente", "Éxito",
                                          MessageBoxButton.OK, MessageBoxImage.Information);
                            this.DialogResult = true;
                            this.Close();
                        }
                        else
                        {
                            MostrarMensaje("No se pudo actualizar el usuario");
                        }
                    }
                    else
                    {
                        // Insertar nuevo usuario
                        bool resultado = objetoCN.InsertarUsuario(
                            nombreUsuario, nombre, apellido, documento, telefono,
                            email, cuil, direccion, clave, rol, activo
                        );

                        if (resultado)
                        {
                            MessageBox.Show("Usuario creado correctamente", "Éxito",
                                          MessageBoxButton.OK, MessageBoxImage.Information);
                            this.DialogResult = true;
                            this.Close();
                        }
                        else
                        {
                            MostrarMensaje("No se pudo crear el usuario");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MostrarMensaje("Error al guardar: " + ex.Message);
                }
            }
        }

        private bool ValidarCampos()
        {
            // Validar campos obligatorios
            if (string.IsNullOrWhiteSpace(txtNombreUsuario.Text))
            {
                MostrarMensaje("El nombre de usuario es obligatorio");
                txtNombreUsuario.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MostrarMensaje("El nombre es obligatorio");
                txtNombre.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtApellido.Text))
            {
                MostrarMensaje("El apellido es obligatorio");
                txtApellido.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtDocumento.Text))
            {
                MostrarMensaje("El documento es obligatorio");
                txtDocumento.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                MostrarMensaje("El email es obligatorio");
                txtEmail.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtCuil.Text))
            {
                MostrarMensaje("El CUIL es obligatorio");
                txtCuil.Focus();
                return false;
            }

            if (!isEditMode && string.IsNullOrWhiteSpace(txtPassword.Password))
            {
                MostrarMensaje("La contraseña es obligatoria para nuevos usuarios");
                txtPassword.Focus();
                return false;
            }

            if (cmbRol.SelectedItem == null)
            {
                MostrarMensaje("Seleccione un rol");
                cmbRol.Focus();
                return false;
            }

            if (cmbEstado.SelectedItem == null)
            {
                MostrarMensaje("Seleccione un estado");
                cmbEstado.Focus();
                return false;
            }

            // Validar formato de email
            if (!ValidarEmail(txtEmail.Text))
            {
                MostrarMensaje("El formato del email no es válido");
                txtEmail.Focus();
                return false;
            }

            // Validar duplicados
            if (!ValidarDuplicados())
            {
                return false;
            }

            txtMensaje.Visibility = Visibility.Collapsed;
            return true;
        }

        private bool ValidarEmail(string email)
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

        private bool ValidarDuplicados()
        {
            try
            {
                // Verificar nombre de usuario duplicado
                if (objetoCN.ExisteNombreUsuario(txtNombreUsuario.Text.Trim(), isEditMode ? idUsuario : (int?)null))
                {
                    MostrarMensaje("El nombre de usuario ya existe");
                    txtNombreUsuario.Focus();
                    return false;
                }

                // Verificar documento duplicado
                if (objetoCN.ExisteDocumento(txtDocumento.Text.Trim(), isEditMode ? idUsuario : (int?)null))
                {
                    MostrarMensaje("El documento ya existe");
                    txtDocumento.Focus();
                    return false;
                }

                // Verificar CUIL duplicado
                if (objetoCN.ExisteCuil(txtCuil.Text.Trim(), isEditMode ? idUsuario : (int?)null))
                {
                    MostrarMensaje("El CUIL ya existe");
                    txtCuil.Focus();
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                MostrarMensaje("Error al validar datos: " + ex.Message);
                return false;
            }
        }

        private void MostrarMensaje(string mensaje)
        {
            txtMensaje.Text = mensaje;
            txtMensaje.Visibility = Visibility.Visible;
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Ocultar mensaje de error cuando el usuario comience a escribir
            if (txtMensaje.Visibility == Visibility.Visible)
            {
                txtMensaje.Visibility = Visibility.Collapsed;
            }

            // Validar formato de email en tiempo real
            if (sender == txtEmail && !string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                if (!ValidarEmail(txtEmail.Text))
                {
                    txtEmail.BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red);
                }
                else
                {
                    txtEmail.BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0xA3, 0x6B, 0x00));
                }
            }
        }
    }
}