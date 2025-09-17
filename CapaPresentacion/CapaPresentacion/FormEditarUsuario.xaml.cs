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
        private string idUsuario = "";
        private bool isEditMode = false;

        public FormEditarUsuario()
        {
            InitializeComponent();
            // Modo nuevo usuario
            Title = "Nuevo Usuario";
        }

        public FormEditarUsuario(string id)
        {
            InitializeComponent();
            idUsuario = id;
            isEditMode = true;
            Title = "Editar Usuario";
            CargarDatosUsuario();
        }

        private void CargarDatosUsuario()
        {
            try
            {
                DataTable dt = objetoCN.MostrarUsuarioPorId(idUsuario);

                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    txtId.Text = row["IdUsuario"].ToString();
                    txtNombre.Text = row["NombreUsuario"].ToString();
                    txtUsuario.Text = row["Usuario"].ToString();

                    // Seleccionar rol
                    foreach (ComboBoxItem item in cmbRol.Items)
                    {
                        if (item.Tag.ToString() == row["Rol"].ToString())
                        {
                            cmbRol.SelectedItem = item;
                            break;
                        }
                    }

                    // Seleccionar estado
                    foreach (ComboBoxItem item in cmbEstado.Items)
                    {
                        if (item.Tag.ToString() == row["Activo"].ToString())
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
                    string usuario = txtUsuario.Text;
                    string clave = txtPassword.Password;
                    string rol = ((ComboBoxItem)cmbRol.SelectedItem).Tag.ToString();
                    string activo = ((ComboBoxItem)cmbEstado.SelectedItem).Tag.ToString();

                    if (isEditMode)
                    {
                        // Editar usuario existente
                        objetoCN.EditarUsuario(idUsuario, usuario, clave, rol, activo);
                        MessageBox.Show("Usuario actualizado correctamente", "Éxito",
                                      MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        // Insertar nuevo usuario
                        objetoCN.InsertarUsuario(usuario, clave, rol, activo);
                        MessageBox.Show("Usuario creado correctamente", "Éxito",
                                      MessageBoxButton.OK, MessageBoxImage.Information);
                    }

                    this.DialogResult = true;
                    this.Close();
                }
                catch (Exception ex)
                {
                    MostrarMensaje("Error al guardar: " + ex.Message);
                }
            }
        }

        private bool ValidarCampos()
        {

            if (string.IsNullOrWhiteSpace(txtUsuario.Text))
            {
                MostrarMensaje("El usuario es obligatorio");
                return false;
            }

            if (!isEditMode && string.IsNullOrWhiteSpace(txtPassword.Password))
            {
                MostrarMensaje("La contraseña es obligatoria");
                return false;
            }

            if (cmbRol.SelectedItem == null)
            {
                MostrarMensaje("Seleccione un rol");
                return false;
            }

            if (cmbEstado.SelectedItem == null)
            {
                MostrarMensaje("Seleccione un estado");
                return false;
            }

            txtMensaje.Visibility = Visibility.Collapsed;
            return true;
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

        private void txtNombre_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}