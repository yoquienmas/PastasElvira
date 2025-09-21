using CapaDatos;
using CapaEntidad;
using CapaNegocio;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Linq;

namespace CapaPresentacion
{
    public partial class MenuAdmin : Window
    {
        private CN_Reporte cnReporte = new CN_Reporte();

        public MenuAdmin()
        {
            InitializeComponent();
            CargarDashboardAdmin();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
        }
        private void CargarDashboardAdmin()
        {
            try
            {
                // Cargar últimas ventas
                var ventas = cnReporte.ObtenerVentasPorFecha(DateTime.Today.AddDays(-30), DateTime.Today);
                dgvVentasAdmin.ItemsSource = ventas.Take(20); // Mostrar últimas 20 ventas
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar dashboard admin: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region Event Handlers - Admin
        private void btnMateriasPrimas_Click(object sender, RoutedEventArgs e)
        {
            FormMateria formMateria = new FormMateria();
            formMateria.Show();
        }

        private void btnGestionUsuarios_Click(object sender, RoutedEventArgs e)
        {
            FormGestionUsuarios formgestionusuarios = new FormGestionUsuarios();
            formgestionusuarios.Show();
        }

        private void btnProductosTerminados_Click(object sender, RoutedEventArgs e)
        {
            FormProducto formProducto = new FormProducto();
            formProducto.Show();
        }

        private void btnAlertasStock_Click(object sender, RoutedEventArgs e)
        {
            FormAlertas formAlerts = new FormAlertas();
            formAlerts.Show();
        }

        private void btnProgramarProduccion_Click(object sender, RoutedEventArgs e)
        {
            FormProduccion formProduccion = new FormProduccion();
            formProduccion.Show();
        }

        private void btnHistorialProduccion_Click(object sender, RoutedEventArgs e)
        {
            FormHistorialProduccion formHistorialProduccion = new FormHistorialProduccion();
            formHistorialProduccion.Show();
        }

        private void btnDetalleProduccion_Click(object sender, RoutedEventArgs e)
        {
            FormDetalleProduccion formDetalleProduccion = new FormDetalleProduccion();
            formDetalleProduccion.Show();
        }

        private void btnConsumoPorVenta_Click(object sender, RoutedEventArgs e)
        {
            FormConsumoPorVenta formConsumoPorVenta = new FormConsumoPorVenta();
            formConsumoPorVenta.Show();
        }

        private void btnVerDetalleVenta_Click(object sender, RoutedEventArgs e)
        {
            if (dgvVentasAdmin.SelectedItem != null)
            {
                MessageBox.Show("Mostrando detalle de venta seleccionada", "Detalle de Venta", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Por favor, seleccione una venta para ver su detalle", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnCerrarSesion_Click(object sender, RoutedEventArgs e)
        {
            // Buscar y mostrar la ventana de login principal
            var mainWindow = Application.Current.Windows.OfType<MenuPrincipal>().FirstOrDefault();
            if (mainWindow != null)
            {
                mainWindow.Show();
            }

            this.Close(); // Cerrar esta ventana
        }
        #endregion

        private void btnProduccionReceta_Click(object sender, RoutedEventArgs e)
        {
            FormReceta formreceta = new FormReceta();
            formreceta.Show();
        }

        private void btnCostosFijos_Click(object sender, RoutedEventArgs e)
        {
            FormCostoFijo formCostoFijo = new FormCostoFijo();
            formCostoFijo.Show();
        }
    }
}