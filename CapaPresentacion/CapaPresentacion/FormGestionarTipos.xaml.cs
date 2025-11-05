using CapaEntidad;
using CapaNegocio;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace CapaPresentacion
{
    public partial class FormGestionarTipos : Window
    {
        private CN_Tipo cnTipo = new CN_Tipo();
        private Tipo tipoSeleccionado = null;

        public FormGestionarTipos()
        {
            InitializeComponent();
            CargarTipos();
        }

        private void CargarTipos()
        {
            try
            {
                List<Tipo> tipos = cnTipo.Listar();
                dgvTipos.ItemsSource = tipos;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar tipos: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnAgregar_Click(object sender, RoutedEventArgs e)
        {
            GuardarTipo(false);
        }

        private void btnEditarTipo_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Tipo tipo)
            {
                tipoSeleccionado = tipo;
                txtDescripcion.Text = tipo.Descripcion;
                chkActivo.IsChecked = tipo.Activo;
                btnAgregar.Content = "💾 Guardar";
            }
        }

        private void btnEliminarTipo_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Tipo tipo)
            {
                var resultado = MessageBox.Show(
                    $"¿Está seguro de eliminar el tipo '{tipo.Descripcion}'?",
                    "Confirmar eliminación",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (resultado == MessageBoxResult.Yes)
                {
                    string mensaje;
                    bool exito = cnTipo.Eliminar(tipo.IdTipo, out mensaje);

                    if (exito)
                    {
                        MessageBox.Show(mensaje, "Éxito",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        CargarTipos();
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

        private void GuardarTipo(bool esEdicion)
        {
            if (string.IsNullOrWhiteSpace(txtDescripcion.Text))
            {
                MessageBox.Show("La descripción es obligatoria", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Tipo tipo = new Tipo
            {
                Descripcion = txtDescripcion.Text.Trim(),
                Activo = chkActivo.IsChecked ?? true
            };

            if (esEdicion && tipoSeleccionado != null)
            {
                tipo.IdTipo = tipoSeleccionado.IdTipo;
            }

            string mensaje;
            bool resultado;

            if (esEdicion)
            {
                resultado = cnTipo.Editar(tipo, out mensaje);
            }
            else
            {
                resultado = cnTipo.Registrar(tipo, out mensaje);
            }

            if (resultado)
            {
                MessageBox.Show(mensaje, "Éxito",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                CargarTipos();
                LimpiarFormulario();
            }
            else
            {
                MessageBox.Show(mensaje, "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void dgvTipos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgvTipos.SelectedItem is Tipo tipo)
            {
                tipoSeleccionado = tipo;
            }
        }

        private void btnLimpiar_Click(object sender, RoutedEventArgs e)
        {
            LimpiarFormulario();
        }

        private void LimpiarFormulario()
        {
            tipoSeleccionado = null;
            txtDescripcion.Text = "";
            chkActivo.IsChecked = true;
            btnAgregar.Content = "➕ Agregar";
            dgvTipos.SelectedItem = null;
        }

        private void btnCerrar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}