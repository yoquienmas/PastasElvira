using CapaDatos;
using CapaEntidad;
using CapaNegocio;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using LiveCharts;
using LiveCharts.Wpf;
using System.Globalization;

namespace CapaPresentacion
{
    public partial class MenuDueño : Window
    {
        private CN_Reporte cnReporte = new CN_Reporte();
        private CN_Cliente cnCliente = new CN_Cliente();

        // Propiedades para los gráficos
        public SeriesCollection SeriesVentasPorTipo { get; set; }
        public SeriesCollection SeriesTopClientes { get; set; }
        public SeriesCollection SeriesProductosVendidos { get; set; }
        public SeriesCollection SeriesStockCritico { get; set; }

        public string[] LabelsVentasPorTipo { get; set; }
        public string[] LabelsTopClientes { get; set; }
        public string[] LabelsProductosVendidos { get; set; }
        public string[] LabelsStockCritico { get; set; }

        public Func<double, string> Formatter { get; set; }

        public MenuDueño()
        {
            InitializeComponent();

            // Configurar el formateador de currency
            Formatter = value => value.ToString("C", new CultureInfo("es-AR"));

            // Inicializar las colecciones
            SeriesVentasPorTipo = new SeriesCollection();
            SeriesTopClientes = new SeriesCollection();
            SeriesProductosVendidos = new SeriesCollection();
            SeriesStockCritico = new SeriesCollection();

            // Establecer el DataContext
            DataContext = this;

            CargarDashboardDueño();
            CargarInformacionUsuario();
        }

        private void CargarInformacionUsuario()
        {
            // Aquí puedes cargar información del usuario actual
            txtFechaPrincipal.Text = DateTime.Now.ToString("dd/MM/yyyy");
            txtUsuarioPrincipal.Text = "Dueño";
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CargarDashboardDueño();
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
                txtVentasMes.Text = totalVentasMes.ToString("C", new CultureInfo("es-AR"));

                // 2. Total clientes
                var clientes = cnCliente.ListarClientes();
                txtTotalClientes.Text = clientes.Count.ToString();

                // 3. Productos vendidos
                var ventasMes = cnReporte.ObtenerVentasPorFecha(inicioMes, finMes);
                int totalProductos = ventasMes.Sum(v => v.CantidadProductos);
                txtProductosVendidos.Text = totalProductos.ToString();

                // 4. Stock crítico
                var stockCritico = cnReporte.ObtenerProductosStockBajo();
                txtStockCritico.Text = $"{stockCritico.Count} productos";

                // 5. Gráficos
                CargarGraficoVentasPorTipo(inicioMes, finMes);
                CargarGraficoTopClientes(inicioMes, finMes);
                CargarGraficoProductosVendidos(inicioMes, finMes);
                CargarGraficoStockCritico();

                // 6. Cargar datos en las tablas
                CargarDatosTablas(inicioMes, finMes);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar dashboard: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CargarGraficoVentasPorTipo(DateTime inicio, DateTime fin)
        {
            try
            {
                var ventasPorTipo = cnReporte.ObtenerVentasPorTipo(inicio, fin);

                SeriesVentasPorTipo.Clear();
                var valores = new ChartValues<decimal>();
                var labels = new List<string>();

                foreach (var venta in ventasPorTipo.Take(6))
                {
                    valores.Add(venta.Total);
                    labels.Add(ObtenerNombreCorto(venta.Tipo, 15));
                }

                if (valores.Any())
                {
                    SeriesVentasPorTipo.Add(new ColumnSeries
                    {
                        Title = "Ventas",
                        Values = valores,
                        Fill = System.Windows.Media.Brushes.SteelBlue
                    });
                }

                LabelsVentasPorTipo = labels.ToArray();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar ventas por tipo: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void CargarGraficoTopClientes(DateTime inicio, DateTime fin)
        {
            try
            {
                var topClientes = cnReporte.ObtenerTopClientes(inicio, fin, 5);

                SeriesTopClientes.Clear();
                var valores = new ChartValues<decimal>();
                var labels = new List<string>();

                foreach (var cliente in topClientes)
                {
                    valores.Add(cliente.TotalGastado);
                    labels.Add(ObtenerNombreCorto(cliente.Nombre, 15));
                }

                if (valores.Any())
                {
                    SeriesTopClientes.Add(new ColumnSeries
                    {
                        Title = "Total Gastado",
                        Values = valores,
                        Fill = System.Windows.Media.Brushes.SeaGreen
                    });
                }

                LabelsTopClientes = labels.ToArray();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar top clientes: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void CargarGraficoProductosVendidos(DateTime inicio, DateTime fin)
        {
            try
            {
                var productosVendidos = cnReporte.ObtenerProductosMasVendidos(inicio, fin, 5);

                SeriesProductosVendidos.Clear();
                var valores = new ChartValues<int>();
                var labels = new List<string>();

                foreach (var producto in productosVendidos)
                {
                    valores.Add(producto.CantidadVendida);
                    labels.Add(ObtenerNombreCorto(producto.NombreProducto, 15));
                }

                if (valores.Any())
                {
                    SeriesProductosVendidos.Add(new ColumnSeries
                    {
                        Title = "Cantidad",
                        Values = valores,
                        Fill = System.Windows.Media.Brushes.Purple
                    });
                }

                LabelsProductosVendidos = labels.ToArray();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar productos vendidos: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void CargarGraficoStockCritico()
        {
            try
            {
                var stockCritico = cnReporte.ObtenerProductosStockBajo();

                SeriesStockCritico.Clear();
                var valores = new ChartValues<int>();
                var labels = new List<string>();

                foreach (var producto in stockCritico.Take(6))
                {
                    valores.Add(producto.StockActual);
                    labels.Add(ObtenerNombreCorto(producto.Nombre, 15));
                }

                if (valores.Any())
                {
                    SeriesStockCritico.Add(new ColumnSeries
                    {
                        Title = "Stock",
                        Values = valores,
                        Fill = System.Windows.Media.Brushes.OrangeRed
                    });
                }

                LabelsStockCritico = labels.ToArray();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar stock crítico: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void CargarDatosTablas(DateTime inicio, DateTime fin)
        {
            try
            {
                // Cargar datos reales en las tablas
                var ventasPorTipo = cnReporte.ObtenerVentasPorTipo(inicio, fin);
                dgvVentasPorTipo.ItemsSource = ventasPorTipo;

                var topClientes = cnReporte.ObtenerTopClientes(inicio, fin, 5);
                dgvTopClientes.ItemsSource = topClientes;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar datos de tablas: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private string ObtenerNombreCorto(string nombre, int maxLength)
        {
            if (string.IsNullOrEmpty(nombre)) return "N/A";
            if (nombre.Length <= maxLength) return nombre;
            return nombre.Substring(0, maxLength - 3) + "...";
        }

        #region Event Handlers
        private void btnReportesGenerales_Click(object sender, RoutedEventArgs e)
        {
            FormReportes formReportes = new FormReportes();
            formReportes.Show();
        }

        private void BtnCerrarSesion_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Application.Current.Windows.OfType<MenuPrincipal>().FirstOrDefault();
            if (mainWindow != null)
            {
                mainWindow.Show();
            }
            this.Close();
        }

        private void dgvTopClientes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Puedes implementar funcionalidad adicional aquí si es necesario
        }
        #endregion

        private void chartVentasPorTipo_Loaded(object sender, RoutedEventArgs e)
        {
            // Este método puede quedar vacío o eliminarse si no es necesario
        }
    }
}