using CapaEntidad;
using CapaNegocio;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace CapaPresentacion
{
    public partial class FormGestionarSabores : Window
    {
        private CN_Sabor cnSabor = new CN_Sabor();
        private Sabor saborSeleccionado = null;

        public FormGestionarSabores()
        {
            InitializeComponent();
            CargarSabores();
        }

        private void CargarSabores()
        {
            try
            {
                List<Sabor> sabores = cnSabor.Listar();
                dgvSabores.ItemsSource = sabores;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar sabores: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnAgregar_Click(object sender, RoutedEventArgs e)
        {
            GuardarSabor(false);
        }

        private void btnEditarSabor_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Sabor sabor)
            {
                saborSeleccionado = sabor;
                txtDescripcion.Text = sabor.Descripcion;
                chkActivo.IsChecked = sabor.Activo;
                btnAgregar.Content = "💾 Guardar";
            }
        }

        private void btnEliminarSabor_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Sabor sabor)
            {
                var resultado = MessageBox.Show(
                    $"¿Está seguro de eliminar el sabor '{sabor.Descripcion}'?",
                    "Confirmar eliminación",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (resultado == MessageBoxResult.Yes)
                {
                    string mensaje;
                    bool exito = cnSabor.Eliminar(sabor.IdSabor, out mensaje);

                    if (exito)
                    {
                        MessageBox.Show(mensaje, "Éxito",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        CargarSabores();
                        LimpiarFormulario();
                    }
                    else
                    {
                        MessageBox.Show(mensaje, "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void GuardarSabor(bool esEdicion)
        {
            if (string.IsNullOrWhiteSpace(txtDescripcion.Text))
            {
                MessageBox.Show("La descripción es obligatoria", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Sabor sabor = new Sabor
            {
                Descripcion = txtDescripcion.Text.Trim(),
                Activo = chkActivo.IsChecked ?? true
            };

            if (esEdicion && saborSeleccionado != null)
            {
                sabor.IdSabor = saborSeleccionado.IdSabor;
            }

            string mensaje;
            bool resultado;

            if (esEdicion)
            {
                resultado = cnSabor.Editar(sabor, out mensaje);
            }
            else
            {
                resultado = cnSabor.Registrar(sabor, out mensaje);
            }

            if (resultado)
            {
                MessageBox.Show(mensaje, "Éxito",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                CargarSabores();
                LimpiarFormulario();
            }
            else
            {
                MessageBox.Show(mensaje, "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void dgvSabores_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgvSabores.SelectedItem is Sabor sabor)
            {
                saborSeleccionado = sabor;
            }
        }

        private void btnLimpiar_Click(object sender, RoutedEventArgs e)
        {
            LimpiarFormulario();
        }

        private void LimpiarFormulario()
        {
            saborSeleccionado = null;
            txtDescripcion.Text = "";
            chkActivo.IsChecked = true;
            btnAgregar.Content = "➕ Agregar";
            dgvSabores.SelectedItem = null;
        }

        private void btnCerrar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}