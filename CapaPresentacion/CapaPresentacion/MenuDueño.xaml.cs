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

                // 6. Cargar datos en las tablas (opcional)
                CargarDatosTablas(inicioMes, finMes);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar dashboard: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                CargarDatosEjemplo();
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

                foreach (var venta in ventasPorTipo.Take(6)) // Máximo 6 tipos
                {
                    valores.Add(venta.Total);
                    labels.Add(venta.Tipo.Length > 15 ? venta.Tipo.Substring(0, 12) + "..." : venta.Tipo);
                }

                SeriesVentasPorTipo.Add(new ColumnSeries
                {
                    Title = "Ventas",
                    Values = valores,
                    Fill = System.Windows.Media.Brushes.SteelBlue
                });

                LabelsVentasPorTipo = labels.ToArray();
            }
            catch (Exception ex)
            {
                // Datos de ejemplo
                CargarDatosEjemploVentasPorTipo();
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

                SeriesTopClientes.Add(new ColumnSeries
                {
                    Title = "Total Gastado",
                    Values = valores,
                    Fill = System.Windows.Media.Brushes.SeaGreen
                });

                LabelsTopClientes = labels.ToArray();
            }
            catch (Exception ex)
            {
                // Datos de ejemplo
                CargarDatosEjemploTopClientes();
            }
        }

        private void CargarGraficoProductosVendidos(DateTime inicio, DateTime fin)
        {
            try
            {
                var productosVendidos = ObtenerProductosMasVendidosSimulado(inicio, fin);

                SeriesProductosVendidos.Clear();
                var valores = new ChartValues<int>();
                var labels = new List<string>();

                foreach (var producto in productosVendidos.Take(5))
                {
                    valores.Add(producto.CantidadVendida);
                    labels.Add(ObtenerNombreCorto(producto.NombreProducto, 15));
                }

                SeriesProductosVendidos.Add(new ColumnSeries
                {
                    Title = "Cantidad",
                    Values = valores,
                    Fill = System.Windows.Media.Brushes.Purple
                });

                LabelsProductosVendidos = labels.ToArray();
            }
            catch (Exception ex)
            {
                CargarDatosEjemploProductosVendidos();
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

                SeriesStockCritico.Add(new ColumnSeries
                {
                    Title = "Stock",
                    Values = valores,
                    Fill = System.Windows.Media.Brushes.OrangeRed
                });

                LabelsStockCritico = labels.ToArray();
            }
            catch (Exception ex)
            {
                CargarDatosEjemploStockCritico();
            }
        }

        private void CargarDatosTablas(DateTime inicio, DateTime fin)
        {
            try
            {
                // Cargar datos en las tablas (opcional)
                var ventasPorTipo = cnReporte.ObtenerVentasPorTipo(inicio, fin);
                dgvVentasPorTipo.ItemsSource = ventasPorTipo;

                var topClientes = cnReporte.ObtenerTopClientes(inicio, fin, 5);
                dgvTopClientes.ItemsSource = topClientes;
            }
            catch (Exception ex)
            {
                // Datos de ejemplo para tablas
                var ventasEjemplo = new List<dynamic>
                {
                    new { Tipo = "Ñoquis", Cantidad = 150, Total = 75000m },
                    new { Tipo = "Ravioles", Cantidad = 200, Total = 45000m }
                };
                dgvVentasPorTipo.ItemsSource = ventasEjemplo;

                var clientesEjemplo = new List<dynamic>
                {
                    new { Nombre = "María González", CantidadCompras = 8, TotalGastado = 18500m }
                };
                dgvTopClientes.ItemsSource = clientesEjemplo;
            }
        }

        private string ObtenerNombreCorto(string nombre, int maxLength)
        {
            if (nombre.Length <= maxLength) return nombre;
            return nombre.Substring(0, maxLength - 3) + "...";
        }

        private List<dynamic> ObtenerProductosMasVendidosSimulado(DateTime inicio, DateTime fin)
        {
            return new List<dynamic>
            {
                new { NombreProducto = "Ñoquis de Papa", CantidadVendida = 150 },
                new { NombreProducto = "Ravioles de Ricota", CantidadVendida = 120 },
                new { NombreProducto = "Tallarines Frescos", CantidadVendida = 80 },
                new { NombreProducto = "Canelones de Carne", CantidadVendida = 60 },
                new { NombreProducto = "Lasagna Tradicional", CantidadVendida = 45 }
            };
        }

        private void CargarDatosEjemplo()
        {
            txtVentasMes.Text = "$75,000.00";
            txtTotalClientes.Text = "150";
            txtProductosVendidos.Text = "455";
            txtStockCritico.Text = "8 productos";

            CargarDatosEjemploVentasPorTipo();
            CargarDatosEjemploTopClientes();
            CargarDatosEjemploProductosVendidos();
            CargarDatosEjemploStockCritico();
        }

        private void CargarDatosEjemploVentasPorTipo()
        {
            SeriesVentasPorTipo.Clear();
            SeriesVentasPorTipo.Add(new ColumnSeries
            {
                Title = "Ventas",
                Values = new ChartValues<decimal> { 75000, 45000, 12000, 8000, 5000 },
                Fill = System.Windows.Media.Brushes.SteelBlue
            });
            LabelsVentasPorTipo = new[] { "Ñoquis", "Ravioles", "Tallarines", "Canelones", "Lasagna" };
        }

        private void CargarDatosEjemploTopClientes()
        {
            SeriesTopClientes.Clear();
            SeriesTopClientes.Add(new ColumnSeries
            {
                Title = "Total Gastado",
                Values = new ChartValues<decimal> { 23500, 18500, 14200, 8600, 7200 },
                Fill = System.Windows.Media.Brushes.SeaGreen
            });
            LabelsTopClientes = new[] { "Rest. Don José", "María González", "Carlos Rodríguez", "Ana Martínez", "Juan Pérez" };
        }

        private void CargarDatosEjemploProductosVendidos()
        {
            SeriesProductosVendidos.Clear();
            SeriesProductosVendidos.Add(new ColumnSeries
            {
                Title = "Cantidad",
                Values = new ChartValues<int> { 150, 120, 80, 60, 45 },
                Fill = System.Windows.Media.Brushes.Purple
            });
            LabelsProductosVendidos = new[] { "Ñoquis Papa", "Ravioles Ricota", "Tallarines", "Canelones", "Lasagna" };
        }

        private void CargarDatosEjemploStockCritico()
        {
            SeriesStockCritico.Clear();
            SeriesStockCritico.Add(new ColumnSeries
            {
                Title = "Stock",
                Values = new ChartValues<int> { 5, 3, 10, 2, 8 },
                Fill = System.Windows.Media.Brushes.OrangeRed
            });
            LabelsStockCritico = new[] { "Harina 000", "Queso Ricota", "Huevos", "Espinaca", "Carne Picada" };
        }

        #region Event Handlers
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
    }
}