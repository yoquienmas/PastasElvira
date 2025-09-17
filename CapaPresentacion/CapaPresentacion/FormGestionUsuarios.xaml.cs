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
        }

        public FormGestionUsuarios(int idUsuario, string rolUsuario) : this()
        {
            _idUsuarioLogueado = idUsuario;
            _rolUsuarioLogueado = rolUsuario;
        }

        // Métodos para los eventos
        private void BtnAgregarUsuario_Click(object sender, RoutedEventArgs e)
        {
            FormEditarUsuario formEditar = new FormEditarUsuario();
            if (formEditar.ShowDialog() == true)
            {
                MostrarUsuarios();
                SeRealizaronCambios = true;
            }
        }

        private void dgvUsuarios_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgvUsuarios.SelectedItem != null)
            {
                DataRowView row = (DataRowView)dgvUsuarios.SelectedItem;
                idUsuarioSeleccionado = row["IdUsuario"].ToString();
            }
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void txtNombre_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Lógica de búsqueda/filtrado si es necesario
        }

        private void MostrarUsuarios()
        {
            try
            {
                DataTable dt = objetoCN.MostrarUsuarios();
                dgvUsuarios.ItemsSource = dt.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar usuarios: " + ex.Message);
            }
        }

        private void BtnEditarUsuario_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(idUsuarioSeleccionado))
            {
                FormEditarUsuario formEditar = new FormEditarUsuario(idUsuarioSeleccionado);
                if (formEditar.ShowDialog() == true)
                {
                    MostrarUsuarios();
                    SeRealizaronCambios = true;
                }
            }
            else
            {
                MessageBox.Show("Seleccione un usuario para editar");
            }
        }

        private void BtnEliminarUsuario_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(idUsuarioSeleccionado))
            {
                if (MessageBox.Show("¿Está seguro de eliminar este usuario?", "Confirmar",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    try
                    {
                        objetoCN.EliminarUsuario(idUsuarioSeleccionado);
                        MessageBox.Show("Usuario eliminado correctamente");
                        MostrarUsuarios();
                        SeRealizaronCambios = true;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error al eliminar usuario: " + ex.Message);
                    }
                }
            }
            else
            {
                MessageBox.Show("Seleccione un usuario para eliminar");
            }
        }
    }
}