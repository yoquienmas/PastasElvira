using CapaDatos;
using CapaEntidad;
using CapaNegocio;
using System;
using System.Collections.Generic;
using System.Windows;

namespace CapaPresentacion
{
    public partial class FormProduccion : Window
    {
        private CN_Produccion cnProduccion = new CN_Produccion();
        private CN_Producto cnProducto = new CN_Producto();

        public FormProduccion()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Listar productos disponibles en el ComboBox
            cmbProducto.ItemsSource = cnProducto.Listar();
        }

        private void btnRegistrar_Click(object sender, RoutedEventArgs e)
        {
            if (cmbProducto.SelectedItem is Producto productoSeleccionado)
            {
                if (!int.TryParse(txtCantidad.Text, out int cantidad) || cantidad <= 0)
                {
                    MessageBox.Show("Ingrese una cantidad válida.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                Produccion produccion = new Produccion
                {
                    IdProducto = productoSeleccionado.IdProducto,
                    CantidadProducida = cantidad

                };

                string mensaje;
                int idGenerado = cnProduccion.Registrar(produccion, out mensaje);

                if (idGenerado > 0)
                {
                    MessageBox.Show($"Producción registrada correctamente.\n{mensaje}", "Éxito",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show($"Error al registrar producción.\n{mensaje}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Seleccione un producto.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
