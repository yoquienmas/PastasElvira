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

        // Variables para filtros
        private DateTime fechaInicioFiltro;
        private DateTime fechaFinFiltro;
        private string tipoProductoFiltro = "Todos los tipos";

        // Propiedades para los gráficos
        public SeriesCollection SeriesVentasPorTipo { get; set; }
        public SeriesCollection SeriesTopClientes { get; set; }
        public SeriesCollection SeriesProductosVendidos { get; set; }
        public SeriesCollection SeriesStockCritico { get; set; }

        // ✅ NUEVAS PROPIEDADES para el gráfico de productos por ingresos
        public SeriesCollection SeriesProductosPorIngresos { get; set; }
        public string[] LabelsProductosPorIngresos { get; set; }

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

            // Inicializar las colecciones EXISTENTES
            SeriesVentasPorTipo = new SeriesCollection();
            SeriesTopClientes = new SeriesCollection();
            SeriesProductosVendidos = new SeriesCollection();
            SeriesStockCritico = new SeriesCollection();

            // ✅ INICIALIZAR NUEVA colección
            SeriesProductosPorIngresos = new SeriesCollection();

            // Establecer el DataContext
            DataContext = this;

            // Configurar fechas por defecto (mes actual)
            ConfigurarFechasPorDefecto();
            CargarTiposProducto();
            CargarDashboardConFiltros();
            CargarInformacionUsuario();
        }

        private void ConfigurarFechasPorDefecto()
        {
            fechaInicioFiltro = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            fechaFinFiltro = fechaInicioFiltro.AddMonths(1).AddDays(-1);

            // Verificar que los controles existen antes de asignar valores
            if (dpFechaInicio != null)
                dpFechaInicio.SelectedDate = fechaInicioFiltro;

            if (dpFechaFin != null)
                dpFechaFin.SelectedDate = fechaFinFiltro;
        }

        private void CargarTiposProducto()
        {
            try
            {
                // Verificar que el control existe
                if (cbTipoProducto == null) return;

                // Limpiar combobox
                cbTipoProducto.Items.Clear();
                cbTipoProducto.Items.Add(new ComboBoxItem { Content = "Todos los tipos", IsSelected = true });

                // Obtener tipos de productos únicos desde la base de datos
                var tipos = cnReporte.ObtenerTiposProducto();
                foreach (var tipo in tipos)
                {
                    cbTipoProducto.Items.Add(new ComboBoxItem { Content = tipo });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar tipos de producto: {ex.Message}");
            }
        }

        private void CargarInformacionUsuario()
        {
            // Aquí puedes cargar información del usuario actual
            txtFechaPrincipal.Text = DateTime.Now.ToString("dd/MM/yyyy");
            txtUsuarioPrincipal.Text = "Dueño";
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CargarDashboardConFiltros();
        }

        private void CargarDashboardConFiltros()
        {
            try
            {
                Console.WriteLine($"Cargando dashboard con filtros: {fechaInicioFiltro:dd/MM/yyyy} - {fechaFinFiltro:dd/MM/yyyy}, Tipo: {tipoProductoFiltro}");

                // 1. Ventas del período (ACTUALIZADO con filtros)
                decimal totalVentasPeriodo = cnReporte.ObtenerTotalVentasPeriodo(fechaInicioFiltro, fechaFinFiltro);
                txtVentasMes.Text = totalVentasPeriodo.ToString("C", new CultureInfo("es-AR"));
                Console.WriteLine($"Ventas del período: {totalVentasPeriodo}");

                // 2. Total clientes (ACTUALIZADO - clientes que compraron en el período)
                var clientesPeriodo = cnReporte.ObtenerClientesDelPeriodo(fechaInicioFiltro, fechaFinFiltro);
                txtTotalClientes.Text = clientesPeriodo.ToString();
                Console.WriteLine($"Clientes del período: {clientesPeriodo}");

                // 3. Productos vendidos (ACTUALIZADO con filtros)
                var ventasPeriodo = cnReporte.ObtenerVentasPorFecha(fechaInicioFiltro, fechaFinFiltro);
                int totalProductos = ventasPeriodo?.Sum(v => v.CantidadProductos) ?? 0;
                txtProductosVendidos.Text = totalProductos.ToString();
                Console.WriteLine($"Productos vendidos: {totalProductos}");

                // 4. Stock crítico (siempre actual, no filtrado por fecha)
                var stockCritico = cnReporte.ObtenerProductosStockBajo();
                txtStockCritico.Text = $"{(stockCritico?.Count ?? 0)} productos";
                Console.WriteLine($"Stock crítico: {stockCritico?.Count}");

                // 5. Gráficos (ACTUALIZADOS con filtros)
                Console.WriteLine("Cargando gráficos...");
                CargarGraficoVentasPorTipo(fechaInicioFiltro, fechaFinFiltro);
                CargarGraficoTopClientes(fechaInicioFiltro, fechaFinFiltro);
                CargarGraficoProductosVendidos(fechaInicioFiltro, fechaFinFiltro); // Por cantidad
                CargarGraficoProductosPorIngresos(fechaInicioFiltro, fechaFinFiltro); // ✅ NUEVO MÉTODO
                CargarGraficoStockCritico();

                // 6. Cargar datos en las tablas (ACTUALIZADOS con filtros)
                Console.WriteLine("Cargando tablas...");
                CargarDatosTablas(fechaInicioFiltro, fechaFinFiltro);

                Console.WriteLine("Dashboard cargado exitosamente");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR en CargarDashboardConFiltros: {ex.Message}");
                MessageBox.Show($"Error al cargar dashboard: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);

                // Establecer valores por defecto en caso de error
                txtVentasMes.Text = "$0.00";
                txtTotalClientes.Text = "0";
                txtProductosVendidos.Text = "0";
                txtStockCritico.Text = "0 productos";
            }
        }

        private void CargarGraficoVentasPorTipo(DateTime inicio, DateTime fin)
        {
            try
            {
                var ventasPorTipo = cnReporte.ObtenerVentasPorTipo(inicio, fin);

                // Aplicar filtro por tipo si no es "Todos los tipos"
                if (tipoProductoFiltro != "Todos los tipos")
                {
                    ventasPorTipo = ventasPorTipo.Where(v => v.Tipo == tipoProductoFiltro).ToList();
                }

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

                // Forzar actualización del gráfico
                chartVentasPorTipo.Update(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en gráfico ventas por tipo: {ex.Message}");
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

                // Forzar actualización del gráfico
                chartTopClientes.Update(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en gráfico top clientes: {ex.Message}");
            }
        }

        // ✅ MÉTODO EXISTENTE para productos por CANTIDAD (modificado)
        private void CargarGraficoProductosVendidos(DateTime inicio, DateTime fin)
        {
            try
            {
                var productosVendidos = cnReporte.ObtenerProductosMasVendidos(inicio, fin, 5,
                    tipoProductoFiltro != "Todos los tipos" ? tipoProductoFiltro : null);

                SeriesProductosVendidos.Clear();
                var valores = new ChartValues<int>();
                var labels = new List<string>();

                foreach (var producto in productosVendidos)
                {
                    valores.Add(producto.CantidadVendida);
                    // ✅ MEJORADO: Mostrar nombre y cantidad en la etiqueta
                    labels.Add($"{ObtenerNombreCorto(producto.NombreProducto, 12)}\n({producto.CantidadVendida})");
                }

                if (valores.Any())
                {
                    SeriesProductosVendidos.Add(new ColumnSeries
                    {
                        Title = "Cantidad Vendida",
                        Values = valores,
                        Fill = System.Windows.Media.Brushes.Purple,
                        DataLabels = true // ✅ Mostrar valores en las barras
                    });
                }
                else
                {
                    // ✅ Mostrar mensaje cuando no hay datos
                    SeriesProductosVendidos.Add(new ColumnSeries
                    {
                        Title = "Sin datos",
                        Values = new ChartValues<int> { 0 },
                        Fill = System.Windows.Media.Brushes.LightGray
                    });
                    labels = new List<string> { "Sin datos" };
                }

                LabelsProductosVendidos = labels.ToArray();

                // Forzar actualización del gráfico
                chartProductosVendidos.Update(true);

                Console.WriteLine($"Gráfico productos vendidos: {productosVendidos.Count} productos cargados");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en gráfico productos vendidos: {ex.Message}");

                // ✅ Manejo de error más robusto
                SeriesProductosVendidos.Clear();
                SeriesProductosVendidos.Add(new ColumnSeries
                {
                    Title = "Error",
                    Values = new ChartValues<int> { 0 },
                    Fill = System.Windows.Media.Brushes.Red
                });
                LabelsProductosVendidos = new[] { "Error" };
                chartProductosVendidos.Update(true);
            }
        }

        // ✅ NUEVO MÉTODO para productos por INGRESOS
        private void CargarGraficoProductosPorIngresos(DateTime inicio, DateTime fin)
        {
            try
            {
                var productosVendidos = cnReporte.ObtenerProductosMasVendidos(inicio, fin, 5,
                    tipoProductoFiltro != "Todos los tipos" ? tipoProductoFiltro : null);

                SeriesProductosPorIngresos.Clear();
                var valores = new ChartValues<decimal>();
                var labels = new List<string>();

                foreach (var producto in productosVendidos)
                {
                    valores.Add(producto.TotalVendido);
                    labels.Add($"{ObtenerNombreCorto(producto.NombreProducto, 12)}");
                }

                if (valores.Any())
                {
                    SeriesProductosPorIngresos.Add(new ColumnSeries
                    {
                        Title = "Ingresos",
                        Values = valores,
                        Fill = System.Windows.Media.Brushes.Green,
                        DataLabels = true
                    });
                }
                else
                {
                    // ✅ Mostrar mensaje cuando no hay datos
                    SeriesProductosPorIngresos.Add(new ColumnSeries
                    {
                        Title = "Sin datos",
                        Values = new ChartValues<decimal> { 0 },
                        Fill = System.Windows.Media.Brushes.LightGray
                    });
                    labels = new List<string> { "Sin datos" };
                }

                LabelsProductosPorIngresos = labels.ToArray();

                // Forzar actualización del gráfico
                if (chartProductosPorIngresos != null)
                    chartProductosPorIngresos.Update(true);

                Console.WriteLine($"Gráfico productos por ingresos: {productosVendidos.Count} productos cargados");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en gráfico productos por ingresos: {ex.Message}");

                // ✅ Manejo de error más robusto
                SeriesProductosPorIngresos.Clear();
                SeriesProductosPorIngresos.Add(new ColumnSeries
                {
                    Title = "Error",
                    Values = new ChartValues<decimal> { 0 },
                    Fill = System.Windows.Media.Brushes.Red
                });
                LabelsProductosPorIngresos = new[] { "Error" };
                if (chartProductosPorIngresos != null)
                    chartProductosPorIngresos.Update(true);
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

                // Forzar actualización del gráfico
                chartStockCritico.Update(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en gráfico stock crítico: {ex.Message}");
            }
        }

        private void CargarDatosTablas(DateTime inicio, DateTime fin)
        {
            try
            {
                // Cargar datos reales en las tablas
                var ventasPorTipo = cnReporte.ObtenerVentasPorTipo(inicio, fin);

                // Aplicar filtro por tipo si no es "Todos los tipos"
                if (tipoProductoFiltro != "Todos los tipos")
                {
                    ventasPorTipo = ventasPorTipo.Where(v => v.Tipo == tipoProductoFiltro).ToList();
                }

                dgvVentasPorTipo.ItemsSource = ventasPorTipo;

                var topClientes = cnReporte.ObtenerTopClientes(inicio, fin, 5);
                dgvTopClientes.ItemsSource = topClientes;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar datos de tablas: {ex.Message}");
            }
        }

        private string ObtenerNombreCorto(string nombre, int maxLength)
        {
            if (string.IsNullOrEmpty(nombre)) return "N/A";

            // Limpiar nombre de espacios extra
            nombre = nombre.Trim();

            if (nombre.Length <= maxLength) return nombre;

            // Cortar en el último espacio antes del límite si es posible
            if (nombre.Contains(" "))
            {
                var palabras = nombre.Split(' ');
                var resultado = "";
                foreach (var palabra in palabras)
                {
                    if ((resultado + " " + palabra).Length <= maxLength - 3)
                    {
                        resultado += (resultado == "" ? "" : " ") + palabra;
                    }
                    else
                    {
                        break;
                    }
                }
                if (!string.IsNullOrEmpty(resultado))
                    return resultado + "...";
            }

            return nombre.Substring(0, maxLength - 3) + "...";
        }

        #region Event Handlers de Filtros - ACTUALIZADO para auto-actualización
        private void Filtro_Changed(object sender, RoutedEventArgs e)
        {
            // Actualizar variables de filtro cuando cambien los controles
            if (dpFechaInicio != null && dpFechaInicio.SelectedDate.HasValue)
                fechaInicioFiltro = dpFechaInicio.SelectedDate.Value;

            if (dpFechaFin != null && dpFechaFin.SelectedDate.HasValue)
                fechaFinFiltro = dpFechaFin.SelectedDate.Value;

            if (cbTipoProducto != null && cbTipoProducto.SelectedItem is ComboBoxItem selectedItem)
                tipoProductoFiltro = selectedItem.Content.ToString();

            // ✅ ACTUALIZACIÓN AUTOMÁTICA: Recargar dashboard inmediatamente
            ActualizarDashboardAutomaticamente();
        }

        private void ActualizarDashboardAutomaticamente()
        {
            try
            {
                // Validar fechas antes de actualizar
                if (!dpFechaInicio.SelectedDate.HasValue || !dpFechaFin.SelectedDate.HasValue)
                    return;

                if (fechaInicioFiltro > fechaFinFiltro)
                    return;

                // Actualizar dashboard sin mostrar mensajes
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    CargarDashboardConFiltros();
                }));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en actualización automática: {ex.Message}");
            }
        }

        private void BtnAplicarFiltros_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validar fechas
                if (!dpFechaInicio.SelectedDate.HasValue || !dpFechaFin.SelectedDate.HasValue)
                {
                    MessageBox.Show("Por favor, seleccione ambas fechas.", "Advertencia",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (fechaInicioFiltro > fechaFinFiltro)
                {
                    MessageBox.Show("La fecha de inicio no puede ser mayor que la fecha fin.", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Aplicar filtros
                CargarDashboardConFiltros();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al aplicar filtros: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnLimpiarFiltros_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Restablecer a valores por defecto
                ConfigurarFechasPorDefecto();

                if (cbTipoProducto != null)
                    cbTipoProducto.SelectedIndex = 0;

                tipoProductoFiltro = "Todos los tipos";

                // Recargar dashboard
                CargarDashboardConFiltros();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al limpiar filtros: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region Event Handlers Originales
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