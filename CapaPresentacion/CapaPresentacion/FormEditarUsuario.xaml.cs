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
                        string nombreUsuario = txtNombreUsuario.Text;
                        string nombre = txtNombre.Text;
                        string apellido = txtApellido.Text;
                        string documento = txtDocumento.Text;
                        string telefono = txtTelefono.Text;
                        string email = txtEmail.Text;
                        string cuil = txtCuil.Text;
                        string direccion = txtDireccion.Text;
                        string clave = txtPassword.Password;
                        string rol = ((ComboBoxItem)cmbRol.SelectedItem).Tag.ToString();
                        bool activo = Convert.ToBoolean(((ComboBoxItem)cmbEstado.SelectedItem).Tag);

                        if (isEditMode)
                        {
                            // Editar usuario existente
                            bool resultado = objetoCN.EditarUsuario(
                                idUsuario, nombreUsuario, nombre, apellido, documento,
                                telefono, email, cuil, direccion, clave, rol, activo
                            );

                            if (resultado)
                            {
                                MessageBox.Show("Usuario actualizado correctamente", "Éxito",
                                              MessageBoxButton.OK, MessageBoxImage.Information);
                                this.DialogResult = true;
                                this.Close();
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
                if (string.IsNullOrWhiteSpace(txtNombreUsuario.Text))
                {
                    MostrarMensaje("El nombre de usuario es obligatorio");
                    return false;
                }

                if (string.IsNullOrWhiteSpace(txtNombre.Text))
                {
                    MostrarMensaje("El nombre es obligatorio");
                    return false;
                }

                if (string.IsNullOrWhiteSpace(txtApellido.Text))
                {
                    MostrarMensaje("El apellido es obligatorio");
                    return false;
                }

                if (!isEditMode && string.IsNullOrWhiteSpace(txtPassword.Password))
                {
                    MostrarMensaje("La contraseña es obligatoria para nuevos usuarios");
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
        }
    }