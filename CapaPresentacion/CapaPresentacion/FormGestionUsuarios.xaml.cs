using System;
using System.Windows;
using System.Windows.Controls;
using CapaNegocio;
using System.Data;

    namespace CapaPresentacion
    {
        public partial class FormGestionUsuarios : Window
        {
            private CN_Usuario objetoCN = new CN_Usuario();
            private string idUsuarioSeleccionado = "";
            private int _idUsuarioLogueado;
            private string _rolUsuarioLogueado;

            public bool SeRealizaronCambios { get; private set; }

            public FormGestionUsuarios()
            {
                InitializeComponent();
                MostrarUsuarios();
            }

            private void Window_Loaded(object sender, RoutedEventArgs e)
            {
                ActualizarContador();
            }

            public FormGestionUsuarios(int idUsuario, string rolUsuario) : this()
            {
                _idUsuarioLogueado = idUsuario;
                _rolUsuarioLogueado = rolUsuario;
            }

            private void BtnAgregarUsuario_Click(object sender, RoutedEventArgs e)
            {
                try
                {
                    FormEditarUsuario formEditar = new FormEditarUsuario();
                    if (formEditar.ShowDialog() == true)
                    {
                        MostrarUsuarios();
                        SeRealizaronCambios = true;
                        txtEstado.Text = "Usuario agregado correctamente";
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al abrir formulario: " + ex.Message, "Error",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            private void dgvUsuarios_SelectionChanged(object sender, SelectionChangedEventArgs e)
            {
                if (dgvUsuarios.SelectedItem != null)
                {
                    DataRowView row = (DataRowView)dgvUsuarios.SelectedItem;
                    idUsuarioSeleccionado = row["IdUsuario"].ToString();
                    txtEstado.Text = $"Usuario seleccionado: {row["NombreUsuario"]}";
                }
            }

            private void BtnCerrar_Click(object sender, RoutedEventArgs e)
            {
                this.DialogResult = true;
                this.Close();
            }

            private void BtnActualizar_Click(object sender, RoutedEventArgs e)
            {
                MostrarUsuarios();
                txtEstado.Text = "Lista actualizada";
            }

            private void MostrarUsuarios()
            {
                try
                {
                    txtEstado.Text = "Cargando usuarios...";
                    DataTable dt = objetoCN.MostrarUsuarios();
                    dgvUsuarios.ItemsSource = dt.DefaultView;
                    ActualizarContador();
                    txtEstado.Text = "Usuarios cargados correctamente";
                }
                catch (Exception ex)
                {
                    txtEstado.Text = "Error al cargar usuarios";
                    MessageBox.Show("Error al cargar usuarios: " + ex.Message, "Error",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            private void ActualizarContador()
            {
                if (dgvUsuarios.ItemsSource != null)
                {
                    int count = ((DataView)dgvUsuarios.ItemsSource).Count;
                    txtContador.Text = $"{count} usuario{(count != 1 ? "s" : "")}";
                }
                else
                {
                    txtContador.Text = "0 usuarios";
                }
            }

            private void BtnEditarUsuario_Click(object sender, RoutedEventArgs e)
            {
                if (!string.IsNullOrEmpty(idUsuarioSeleccionado))
                {
                    try
                    {
                        FormEditarUsuario formEditar = new FormEditarUsuario(idUsuarioSeleccionado);
                        if (formEditar.ShowDialog() == true)
                        {
                            MostrarUsuarios();
                            SeRealizaronCambios = true;
                            txtEstado.Text = "Usuario actualizado correctamente";
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error al editar usuario: " + ex.Message, "Error",
                                      MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Seleccione un usuario para editar", "Advertencia",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }

            private void BtnEliminarUsuario_Click(object sender, RoutedEventArgs e)
            {
                if (!string.IsNullOrEmpty(idUsuarioSeleccionado))
                {
                    if (MessageBox.Show("¿Está seguro de eliminar este usuario?\nEsta acción no se puede deshacer.",
                        "Confirmar Eliminación", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                    {
                        try
                        {
                            bool resultado = objetoCN.EliminarUsuario(Convert.ToInt32(idUsuarioSeleccionado));

                            if (resultado)
                            {
                                MessageBox.Show("Usuario eliminado correctamente", "Éxito",
                                              MessageBoxButton.OK, MessageBoxImage.Information);
                                MostrarUsuarios();
                                SeRealizaronCambios = true;
                                txtEstado.Text = "Usuario eliminado correctamente";
                            }
                            else
                            {
                                MessageBox.Show("No se pudo eliminar el usuario", "Error",
                                              MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Error al eliminar usuario: " + ex.Message, "Error",
                                          MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Seleccione un usuario para eliminar", "Advertencia",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }

            // Evento para manejar doble clic en el grid
            private void dgvUsuarios_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
            {
                BtnEditarUsuario_Click(sender, e);
            }
        }
    }