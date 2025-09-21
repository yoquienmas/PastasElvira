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
    public partial class MenuVendedor : Window
    {
        private int _idVendedor;
        private string _nombreVendedor;
        private CN_Reporte cnReporte = new CN_Reporte();
        private CN_Producto cnProducto = new CN_Producto();

        public MenuVendedor(int idVendedor, string nombreVendedor)
        {
            InitializeComponent();
            _idVendedor = idVendedor;
            _nombreVendedor = nombreVendedor;
            CargarDashboardVendedor();
        }

        private void CargarDashboardVendedor()
        {
            try
            {
                // Obtener fecha actual para cálculos
                DateTime inicioMes = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                DateTime finMes = inicioMes.AddMonths(1).AddDays(-1);

                // 1. Cargar mis ventas del mes
                CargarMisVentasMes(inicioMes, finMes);

                // 2. Cargar productos disponibles
                var productos = cnProducto.Listar();
                dgvProductosDisponibles.ItemsSource = productos;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar dashboard vendedor: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CargarMisVentasMes(DateTime inicio, DateTime fin)
        {
            try
            {
                var misVentas = cnReporte.ObtenerVentasPorVendedor(_idVendedor, inicio, fin);

                // Actualizar métricas
                decimal totalVendido = misVentas.Sum(v => v.Total);
                int cantidadVentas = misVentas.Count;
                decimal ticketPromedio = cantidadVentas > 0 ? totalVendido / cantidadVentas : 0;

                txtMisVentasTotal.Text = totalVendido.ToString("C");
                txtMisVentasCantidad.Text = cantidadVentas.ToString();
                txtMisTicketPromedio.Text = ticketPromedio.ToString("C");

                // Mostrar últimas ventas
                dgvMisVentas.ItemsSource = misVentas.Take(10); // Mostrar últimas 10 ventas
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar ventas: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region Event Handlers - Vendedor
        private void btnNuevaVenta_Click(object sender, RoutedEventArgs e)
        {
            // Reemplaza la llamada a SetVendedor con el constructor adecuado
            FormVenta formVenta = new FormVenta(_idVendedor, _nombreVendedor);
            formVenta.Show();
        }

        private void btnMisVentas_Click(object sender, RoutedEventArgs e)
        {
            // CORRECCIÓN: Pasar ambos parámetros requeridos al constructor
            FormHistorialVentas formhistorialventas = new FormHistorialVentas(_idVendedor, _nombreVendedor);
            formhistorialventas.Show();
        }

        private void btnGestionClientes_Click(object sender, RoutedEventArgs e)
        {
            FormCliente formCliente = new FormCliente();
            formCliente.Show();
        }

        private void btnVerDetalleVenta_Click(object sender, RoutedEventArgs e)
        {
            if (dgvMisVentas.SelectedItem != null)
            {
                // Aquí puedes implementar la lógica para ver el detalle de la venta seleccionada
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
    }
}