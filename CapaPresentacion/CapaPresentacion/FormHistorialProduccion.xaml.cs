using CapaEntidad;
using CapaNegocio;
using System;
using System.Windows;

namespace CapaPresentacion
{
    public partial class FormHistorialProduccion : Window
    {
        private CN_Produccion cnProduccion = new CN_Produccion();

        public FormHistorialProduccion()
        {
            InitializeComponent();
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CargarHistorialProduccion();
        }

        private void CargarHistorialProduccion()
        {
            try
            {
                var historial = cnProduccion.Listar();
                dgvProducciones.ItemsSource = historial;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar historial: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}