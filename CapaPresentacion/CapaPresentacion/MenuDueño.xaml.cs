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
    public partial class MenuDueño : Window
    {
        private CN_Reporte cnReporte = new CN_Reporte();
        private CN_Cliente cnCliente = new CN_Cliente();

        public MenuDueño()
        {
            InitializeComponent();
            CargarDashboardDueño();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        { 
        }
          private void CargarDashboardDueño()
         {
            try
            {
                // Obtener fecha actual para cálculos
                DateTime inicioMes = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                DateTime finMes = inicioMes.AddMonths(1).AddDays(-1);

                // 1. Ventas del mes
                decimal totalVentasMes = cnReporte.ObtenerTotalVentasPeriodo(inicioMes, finMes);
                txtVentasMes.Text = totalVentasMes.ToString("C");

                // 2. Total clientes
                var clientes = cnCliente.ListarClientes();
                txtTotalClientes.Text = clientes.Count.ToString();

                // 3. Productos vendidos (aproximado)
                var ventasMes = cnReporte.ObtenerVentasPorFecha(inicioMes, finMes);
                int totalProductos = ventasMes.Sum(v => v.CantidadProductos);
                txtProductosVendidos.Text = totalProductos.ToString();

                // 4. Stock crítico
                var stockCritico = cnReporte.ObtenerProductosStockBajo();
                txtStockCritico.Text = $"{stockCritico.Count} productos";

                // 5. Ventas por tipo
                CargarVentasPorTipo(inicioMes, finMes);

                // 6. Top 5 clientes
                CargarTopClientes(inicioMes, finMes);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar dashboard: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CargarVentasPorTipo(DateTime inicio, DateTime fin)
        {
            try
            {
                var ventasPorTipo = cnReporte.ObtenerVentasPorTipo(inicio, fin);
                dgvVentasPorTipo.ItemsSource = ventasPorTipo;
            }
            catch (Exception ex)
            {
                // Simulación si no hay conexión a BD
                var ventasPorTipo = new List<dynamic>
                {
                    new { Tipo = "Ñoquis", Cantidad = 150, Total = 75000m },
                    new { Tipo = "Ravioles", Cantidad = 200, Total = 45000m },
                    new { Tipo = "Tallarines", Cantidad = 80, Total = 12000m },
                    new { Tipo = "Canelones", Cantidad = 50, Total = 8000m }
                };
                dgvVentasPorTipo.ItemsSource = ventasPorTipo;
            }
        }

        private void CargarTopClientes(DateTime inicio, DateTime fin)
        {
            try
            {
                var topClientes = cnReporte.ObtenerTopClientes(inicio, fin, 5);
                dgvTopClientes.ItemsSource = topClientes;
            }
            catch (Exception ex)
            {
                // Simulación si no hay conexión a BD
                var topClientes = new List<dynamic>
                {
                    new { Nombre = "María González", CantidadCompras = 8, TotalGastado = 18500m },
                    new { Nombre = "Carlos Rodríguez", CantidadCompras = 6, TotalGastado = 14200m },
                    new { Nombre = "Restaurant Don José", CantidadCompras = 5, TotalGastado = 23500m },
                    new { Nombre = "Ana Martínez", CantidadCompras = 4, TotalGastado = 8600m },
                    new { Nombre = "Juan Pérez", CantidadCompras = 3, TotalGastado = 7200m }
                };
                dgvTopClientes.ItemsSource = topClientes;
            }
        }

        #region Event Handlers - Dueño
        private void btnReportesGenerales_Click(object sender, RoutedEventArgs e)
        {
            FormReportes formReportes = new FormReportes();
            formReportes.Show();
        }

        private void btnGestionUsuarios_Click(object sender, RoutedEventArgs e)
        {
            FormGestionUsuarios formgestionusuarios = new FormGestionUsuarios();
            formgestionusuarios.Show();
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

        private void dgvTopClientes_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // Puedes implementar funcionalidad adicional aquí si es necesario
        }
        #endregion
    }
}

