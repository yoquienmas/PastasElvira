using CapaEntidad;
using CapaNegocio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace CapaPresentacion
{
    public partial class MenuPrincipal : Window
    {
        private CN_Reporte cnReporte = new CN_Reporte();
        private CN_Venta cnVenta = new CN_Venta();
        private CN_Cliente cnCliente = new CN_Cliente();
        private CN_Producto cnProducto = new CN_Producto();
        private CN_Alerta cnAlerta = new CN_Alerta();

        public MenuPrincipal()
        {
            InitializeComponent();
            txtFecha.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            cboRol.SelectedIndex = 0; // Seleccionar DUEÑO por defecto
        }

        private void cboRol_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboRol.SelectedItem == null) return;

            string rol = ((ComboBoxItem)cboRol.SelectedItem).Tag.ToString();
            ActualizarVistaSegunRol(rol);
        }

        private void ActualizarVistaSegunRol(string rol)
        {
            // Habilitar solo la pestaña correspondiente al rol
            tabDueño.IsEnabled = rol == "DUEÑO";
            tabAdmin.IsEnabled = rol == "ADMIN";
            tabVendedor.IsEnabled = rol == "VENDEDOR";

            // Seleccionar la pestaña activa
            if (rol == "DUEÑO")
            {
                tabDueño.IsSelected = true;
                CargarDashboardDueño();
            }
            else if (rol == "ADMIN")
            {
                tabAdmin.IsSelected = true;
                CargarDashboardAdmin();
            }
            else if (rol == "VENDEDOR")
            {
                tabVendedor.IsSelected = true;
                CargarDashboardVendedor();
            }
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
                var clientes = cnCliente.Listar();
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
            // Simulación - En una implementación real, esto vendría de la base de datos
            var ventasPorTipo = new List<dynamic>
            {
                new { Tipo = "Ñoquis", Cantidad = 150, Total = 75000m },
                new { Tipo = "Ravioles", Cantidad = 200, Total = 45000m },
                new { Tipo = "Tallarines", Cantidad = 80, Total = 12000m },
                new { Tipo = "Canelones", Cantidad = 50, Total = 8000m }
            };

            dgvVentasPorTipo.ItemsSource = ventasPorTipo;
        }

        private void CargarTopClientes(DateTime inicio, DateTime fin)
        {
            // Simulación - En una implementación real, esto vendría de la base de datos
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

        private void CargarDashboardVendedor()
        {
            try
            {
                // Simular datos del vendedor actual (en producción vendría del login)
                int idVendedorSimulado = 1;

                // Mis ventas del mes
                DateTime inicioMes = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                DateTime finMes = inicioMes.AddMonths(1).AddDays(-1);

                var misVentas = cnReporte.ObtenerVentasPorFecha(inicioMes, finMes)
                    .Where(v => v.Usuario.Contains("Vendedor")) // Simulación
                    .ToList();

                decimal totalVentas = misVentas.Sum(v => v.Total);
                int cantidadVentas = misVentas.Count;
                decimal ticketPromedio = cantidadVentas > 0 ? totalVentas / cantidadVentas : 0;

                txtMisVentasTotal.Text = totalVentas.ToString("C");
                txtMisVentasCantidad.Text = cantidadVentas.ToString();
                txtMisTicketPromedio.Text = ticketPromedio.ToString("C");

                dgvMisVentas.ItemsSource = misVentas;

                // Productos disponibles
                var productos = cnProducto.Listar();
                foreach (var producto in productos)
                {
                    producto.EstadoStock = producto.StockActual <= producto.StockMinimo ? "CRÍTICO" :
                                          producto.StockActual <= producto.StockMinimo * 2 ? "BAJO" : "NORMAL";
                }
                dgvProductosVendedor.ItemsSource = productos;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar dashboard vendedor: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region Event Handlers - Admin
        private void btnReporteVentas_Click(object sender, RoutedEventArgs e)
        {
            FormReportes formReportes = new FormReportes();
            formReportes.Show();
        }

        private void btnCostosFijos_Click(object sender, RoutedEventArgs e)
        {
            FormCostoFijo formCostoFijo = new FormCostoFijo();
            formCostoFijo.Show();
        }

        private void btnEstadoFinanciero_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Módulo de Estado Financiero en desarrollo", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void btnNuevoProducto_Click(object sender, RoutedEventArgs e)
        {
            FormProducto formProducto = new FormProducto();
            formProducto.Show();

        }

        private void btnListaProductos_Click(object sender, RoutedEventArgs e)
        {
            // Asumiendo que tienes FormProducto para listar productos
            MessageBox.Show("Abrir listado de productos", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void btnGestionRecetas_Click(object sender, RoutedEventArgs e)
        {
            FormReceta formReceta = new FormReceta();
            formReceta.Show();
        }

        private void btnRegistrarUsuario_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Módulo de registro de usuarios en desarrollo", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void btnListaUsuarios_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Módulo de lista de usuarios en desarrollo", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void btnVerDetalleVenta_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Ver detalle de venta seleccionada", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        #endregion

        #region Event Handlers - Vendedor
        private void btnNuevaVenta_Click(object sender, RoutedEventArgs e)
        {
            FormVenta formVenta = new FormVenta();
            formVenta.Show();
        }

        private void btnRegistrarCliente_Click(object sender, RoutedEventArgs e)
        {
            FormCliente formCliente = new FormCliente();
            formCliente.Show();
        }

        private void btnVerProductos_Click(object sender, RoutedEventArgs e)
        {
            // Asumiendo que tienes FormProducto para ver productos
            MessageBox.Show("Abrir listado completo de productos", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        #endregion

        private void dgvTopClientes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}