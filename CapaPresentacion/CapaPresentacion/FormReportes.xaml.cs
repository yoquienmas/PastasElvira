using CapaEntidad;
using CapaNegocio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CapaPresentacion
{
    public partial class FormReportes : Window
    {
        private CN_Reporte cnReporte = new CN_Reporte();
        private CN_Cliente cnCliente = new CN_Cliente();
        private List<ReporteVenta> ventasClienteActual = new List<ReporteVenta>();
        private string clienteActualNombre = "";

        public FormReportes()
        {
            InitializeComponent();
            dtpFechaInicio.SelectedDate = DateTime.Today.AddMonths(-1);
            dtpFechaFin.SelectedDate = DateTime.Today;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CargarClientes();
            ActualizarEstadisticasRapidas();
            cboTipoReporte.SelectedIndex = 0;
        }

        private void CargarClientes()
        {
            cboClientes.ItemsSource = cnCliente.Listar();
        }

        private void ActualizarEstadisticasRapidas()
        {
            try
            {
                // Ventas de hoy
                decimal ventasHoy = cnReporte.ObtenerTotalVentasPeriodo(DateTime.Today, DateTime.Today);
                txtVentasHoy.Text = $"Ventas Hoy: {ventasHoy:C}";

                // Stock crítico
                var stockCritico = cnReporte.ObtenerProductosStockBajo();
                txtStockCritico.Text = $"Stock Crítico: {stockCritico.Count} productos";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar estadísticas: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void cboTipoReporte_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboTipoReporte.SelectedItem == null) return;

            string reporteSeleccionado = ((ComboBoxItem)cboTipoReporte.SelectedItem).Content.ToString();

            // Configurar visibilidad de filtros
            dtpFechaInicio.Visibility = Visibility.Visible;
            dtpFechaFin.Visibility = Visibility.Visible;
            cboClientes.Visibility = Visibility.Collapsed;
            panelFiltrosCliente.Visibility = Visibility.Collapsed;

            switch (reporteSeleccionado)
            {
                case "Ventas por Cliente":
                    dtpFechaInicio.Visibility = Visibility.Collapsed;
                    dtpFechaFin.Visibility = Visibility.Collapsed;
                    cboClientes.Visibility = Visibility.Visible;
                    panelFiltrosCliente.Visibility = Visibility.Visible;
                    break;

                case "Stock Crítico":
                case "Inventario Completo":
                    dtpFechaInicio.Visibility = Visibility.Collapsed;
                    dtpFechaFin.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        private void btnGenerarReporte_Click(object sender, RoutedEventArgs e)
        {
            if (cboTipoReporte.SelectedItem == null)
            {
                MessageBox.Show("Seleccione un tipo de reporte", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string reporteSeleccionado = ((ComboBoxItem)cboTipoReporte.SelectedItem).Content.ToString();

            try
            {
                switch (reporteSeleccionado)
                {
                    case "Consumo Materia Prima":
                        GenerarReporteConsumo();
                        break;

                    case "Ventas por Fecha":
                        GenerarReporteVentasPorFecha();
                        break;

                    case "Ventas por Cliente":
                        GenerarReporteVentasPorCliente();
                        break;

                    case "Stock Crítico":
                        GenerarReporteStockCritico();
                        break;

                    case "Inventario Completo":
                        GenerarReporteInventarioCompleto();
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al generar reporte: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GenerarReporteConsumo()
        {
            if (dtpFechaInicio.SelectedDate == null || dtpFechaFin.SelectedDate == null)
            {
                MessageBox.Show("Seleccione ambas fechas", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var consumo = cnReporte.ObtenerConsumoMateriaPrima(
                dtpFechaInicio.SelectedDate.Value,
                dtpFechaFin.SelectedDate.Value);

            dgvReporte.ItemsSource = consumo;

            // Configurar columnas
            dgvReporte.Columns.Clear();
            dgvReporte.Columns.Add(new DataGridTextColumn { Header = "Materia Prima", Binding = new System.Windows.Data.Binding("MateriaPrima"), Width = 200 });
            dgvReporte.Columns.Add(new DataGridTextColumn { Header = "Cantidad", Binding = new System.Windows.Data.Binding("CantidadConsumida"), Width = 100 });
            dgvReporte.Columns.Add(new DataGridTextColumn { Header = "Unidad", Binding = new System.Windows.Data.Binding("Unidad"), Width = 80 });

            GenerarResumenConsumo(consumo);
        }

        private void GenerarReporteVentasPorFecha()
        {
            if (dtpFechaInicio.SelectedDate == null || dtpFechaFin.SelectedDate == null)
            {
                MessageBox.Show("Seleccione ambas fechas", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var ventas = cnReporte.ObtenerVentasPorFecha(
                dtpFechaInicio.SelectedDate.Value,
                dtpFechaFin.SelectedDate.Value);

            dgvReporte.ItemsSource = ventas;

            // Configurar columnas
            dgvReporte.Columns.Clear();
            dgvReporte.Columns.Add(new DataGridTextColumn { Header = "ID", Binding = new System.Windows.Data.Binding("IdVenta"), Width = 60 });
            dgvReporte.Columns.Add(new DataGridTextColumn { Header = "Fecha", Binding = new System.Windows.Data.Binding("Fecha") { StringFormat = "dd/MM/yyyy HH:mm" }, Width = 120 });
            dgvReporte.Columns.Add(new DataGridTextColumn { Header = "Cliente", Binding = new System.Windows.Data.Binding("Cliente"), Width = 150 });
            dgvReporte.Columns.Add(new DataGridTextColumn { Header = "Usuario", Binding = new System.Windows.Data.Binding("Usuario"), Width = 100 });
            dgvReporte.Columns.Add(new DataGridTextColumn { Header = "Total", Binding = new System.Windows.Data.Binding("Total") { StringFormat = "C" }, Width = 100 });

            GenerarResumenVentas(ventas);
        }

        private void GenerarReporteVentasPorCliente()
        {
            if (cboClientes.SelectedItem == null)
            {
                MessageBox.Show("Seleccione un cliente", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var cliente = (Cliente)cboClientes.SelectedItem;
            ventasClienteActual = cnReporte.ObtenerVentasPorCliente(cliente.IdCliente);
            clienteActualNombre = cliente.Nombre;

            // Aplicar filtros si existen
            AplicarFiltrosVentasCliente();

            GenerarResumenVentasCliente(ventasClienteActual, clienteActualNombre);
        }

        private void AplicarFiltrosVentasCliente()
        {
            var ventasFiltradas = ventasClienteActual;

            // Filtrar por DNI si hay texto
            if (!string.IsNullOrWhiteSpace(txtFiltroDNI.Text))
            {
                ventasFiltradas = ventasFiltradas
                    .Where(v => v.DNI != null && v.DNI.Contains(txtFiltroDNI.Text))
                    .ToList();
            }

            // Filtrar por producto si hay texto (asumiendo que tienes propiedad Productos)
            if (!string.IsNullOrWhiteSpace(txtFiltroProducto.Text))
            {
                ventasFiltradas = ventasFiltradas
                    .Where(v => v.Productos != null && v.Productos.Contains(txtFiltroProducto.Text))
                    .ToList();
            }

            dgvReporte.ItemsSource = ventasFiltradas;

            // Configurar columnas
            dgvReporte.Columns.Clear();
            dgvReporte.Columns.Add(new DataGridTextColumn { Header = "ID", Binding = new System.Windows.Data.Binding("IdVenta"), Width = 60 });
            dgvReporte.Columns.Add(new DataGridTextColumn { Header = "Fecha", Binding = new System.Windows.Data.Binding("Fecha") { StringFormat = "dd/MM/yyyy HH:mm" }, Width = 120 });
            dgvReporte.Columns.Add(new DataGridTextColumn { Header = "DNI", Binding = new System.Windows.Data.Binding("DniCliente"), Width = 100 });
            dgvReporte.Columns.Add(new DataGridTextColumn { Header = "Total", Binding = new System.Windows.Data.Binding("Total") { StringFormat = "C" }, Width = 100 });

            // Solo agregar columna de productos si existe la propiedad
            if (ventasFiltradas.Any(v => !string.IsNullOrEmpty(v.Productos)))
            {
                dgvReporte.Columns.Add(new DataGridTextColumn { Header = "Productos", Binding = new System.Windows.Data.Binding("Productos"), Width = 200 });
            }
        }

        // Nuevos métodos para los filtros
        private void TxtFiltroDNI_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && cboTipoReporte.SelectedItem != null &&
                ((ComboBoxItem)cboTipoReporte.SelectedItem).Content.ToString() == "Ventas por Cliente")
            {
                AplicarFiltrosVentasCliente();
            }
        }

        private void TxtFiltroProducto_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && cboTipoReporte.SelectedItem != null &&
                ((ComboBoxItem)cboTipoReporte.SelectedItem).Content.ToString() == "Ventas por Cliente")
            {
                AplicarFiltrosVentasCliente();
            }
        }

        private void BtnLimpiarFiltros_Click(object sender, RoutedEventArgs e)
        {
            txtFiltroDNI.Text = "";
            txtFiltroProducto.Text = "";

            if (cboTipoReporte.SelectedItem != null &&
                ((ComboBoxItem)cboTipoReporte.SelectedItem).Content.ToString() == "Ventas por Cliente")
            {
                AplicarFiltrosVentasCliente();
            }
        }

        private void GenerarReporteStockCritico()
        {
            var stock = cnReporte.ObtenerProductosStockBajo();
            dgvReporte.ItemsSource = stock;

            // Configurar columnas
            dgvReporte.Columns.Clear();
            dgvReporte.Columns.Add(new DataGridTextColumn { Header = "ID", Binding = new System.Windows.Data.Binding("IdProducto"), Width = 60 });
            dgvReporte.Columns.Add(new DataGridTextColumn { Header = "Producto", Binding = new System.Windows.Data.Binding("Nombre"), Width = 200 });
            dgvReporte.Columns.Add(new DataGridTextColumn { Header = "Stock Actual", Binding = new System.Windows.Data.Binding("StockActual"), Width = 80 });
            dgvReporte.Columns.Add(new DataGridTextColumn { Header = "Stock Mínimo", Binding = new System.Windows.Data.Binding("StockMinimo"), Width = 80 });
            dgvReporte.Columns.Add(new DataGridTextColumn { Header = "Estado", Binding = new System.Windows.Data.Binding("Estado"), Width = 80 });

            GenerarResumenStock(stock);
        }

        private void GenerarReporteInventarioCompleto()
        {
            var inventario = cnReporte.ObtenerTodosProductosConStock();
            dgvReporte.ItemsSource = inventario;

            // Configurar columnas
            dgvReporte.Columns.Clear();
            dgvReporte.Columns.Add(new DataGridTextColumn { Header = "ID", Binding = new System.Windows.Data.Binding("IdProducto"), Width = 60 });
            dgvReporte.Columns.Add(new DataGridTextColumn { Header = "Producto", Binding = new System.Windows.Data.Binding("Nombre"), Width = 200 });
            dgvReporte.Columns.Add(new DataGridTextColumn { Header = "Tipo", Binding = new System.Windows.Data.Binding("Tipo"), Width = 100 });
            dgvReporte.Columns.Add(new DataGridTextColumn { Header = "Stock Actual", Binding = new System.Windows.Data.Binding("StockActual"), Width = 80 });
            dgvReporte.Columns.Add(new DataGridTextColumn { Header = "Stock Mínimo", Binding = new System.Windows.Data.Binding("StockMinimo"), Width = 80 });
            dgvReporte.Columns.Add(new DataGridTextColumn { Header = "Precio", Binding = new System.Windows.Data.Binding("PrecioVenta") { StringFormat = "C" }, Width = 80 });
            dgvReporte.Columns.Add(new DataGridTextColumn { Header = "Estado", Binding = new System.Windows.Data.Binding("Estado"), Width = 80 });

            GenerarResumenInventario(inventario);
        }

        private void GenerarResumenConsumo(List<ReporteConsumo> consumo)
        {
            panelResumen.Children.Clear();

            decimal totalConsumo = consumo.Sum(c => c.CantidadConsumida);
            int totalItems = consumo.Count;

            AddResumenItem("📦 Resumen de Consumo", $"Período: {dtpFechaInicio.SelectedDate.Value:dd/MM/yyyy} - {dtpFechaFin.SelectedDate.Value:dd/MM/yyyy}");
            AddResumenItem("Total de Materias Primas Consumidas", totalItems.ToString());
            AddResumenItem("Cantidad Total Consumida", $"{totalConsumo:N2} unidades");

            // Top 5 materias más consumidas
            var top5 = consumo.OrderByDescending(c => c.CantidadConsumida).Take(5);
            AddResumenItem("🏆 Top 5 Materias Más Consumidas", "");
            foreach (var item in top5)
            {
                AddResumenItem($"   • {item.MateriaPrima}", $"{item.CantidadConsumida:N2} {item.Unidad}");
            }
        }

        private void GenerarResumenVentas(List<ReporteVenta> ventas)
        {
            panelResumen.Children.Clear();

            decimal totalVentas = ventas.Sum(v => v.Total);
            int totalVentasCount = ventas.Count;
            decimal promedioVenta = totalVentasCount > 0 ? totalVentas / totalVentasCount : 0;

            AddResumenItem("💰 Resumen de Ventas", $"Período: {dtpFechaInicio.SelectedDate.Value:dd/MM/yyyy} - {dtpFechaFin.SelectedDate.Value:dd/MM/yyyy}");
            AddResumenItem("Total de Ventas", totalVentasCount.ToString());
            AddResumenItem("Ingreso Total", totalVentas.ToString("C"));
            AddResumenItem("Ticket Promedio", promedioVenta.ToString("C"));

            // Ventas por día
            var ventasPorDia = ventas.GroupBy(v => v.Fecha.Date)
                                   .Select(g => new { Fecha = g.Key, Total = g.Sum(v => v.Total) })
                                   .OrderBy(x => x.Fecha);

            AddResumenItem("📅 Ventas por Día", "");
            foreach (var dia in ventasPorDia)
            {
                AddResumenItem($"   • {dia.Fecha:dd/MM}", dia.Total.ToString("C"));
            }
        }

        private void GenerarResumenVentasCliente(List<ReporteVenta> ventas, string nombreCliente)
        {
            panelResumen.Children.Clear();

            decimal totalVentas = ventas.Sum(v => v.Total);
            int totalVentasCount = ventas.Count;
            decimal promedioVenta = totalVentasCount > 0 ? totalVentas / totalVentasCount : 0;

            AddResumenItem("👤 Resumen de Ventas por Cliente", $"Cliente: {nombreCliente}");
            AddResumenItem("Total de Compras", totalVentasCount.ToString());
            AddResumenItem("Gasto Total", totalVentas.ToString("C"));
            AddResumenItem("Compra Promedio", promedioVenta.ToString("C"));

            // Frecuencia de compras
            if (ventas.Count > 1)
            {
                var diasEntreCompras = new List<double>();
                for (int i = 1; i < ventas.Count; i++)
                {
                    var dias = (ventas[i].Fecha - ventas[i - 1].Fecha).TotalDays;
                    diasEntreCompras.Add(dias);
                }
                double frecuenciaPromedio = diasEntreCompras.Average();
                AddResumenItem("Frecuencia de Compra", $"{frecuenciaPromedio:N1} días");
            }
        }

        private void GenerarResumenStock(List<ReporteStock> stock)
        {
            panelResumen.Children.Clear();

            int criticos = stock.Count(s => s.Estado == "CRÍTICO");
            int alerta = stock.Count(s => s.Estado == "ALERTA");

            AddResumenItem("⚠️ Resumen de Stock Crítico", "");
            AddResumenItem("Productos en Estado CRÍTICO", criticos.ToString());
            AddResumenItem("Productos en ALERTA", alerta.ToString());
            AddResumenItem("Total de Productos con Stock Bajo", stock.Count.ToString());

            // Productos más críticos
            var masCriticos = stock.OrderBy(s => s.StockActual).Take(5);
            AddResumenItem("🚨 Productos Más Críticos", "");
            foreach (var producto in masCriticos)
            {
                AddResumenItem($"   • {producto.Nombre}", $"{producto.StockActual} (mín: {producto.StockMinimo})");
            }
        }

        private void GenerarResumenInventario(List<ReporteStock> inventario)
        {
            panelResumen.Children.Clear();

            int totalProductos = inventario.Count;
            int criticos = inventario.Count(s => s.Estado == "CRÍTICO");
            int alerta = inventario.Count(s => s.Estado == "ALERTA");
            int normal = inventario.Count(s => s.Estado == "NORMAL");
            decimal valorTotalInventario = inventario.Sum(p => p.PrecioVenta * p.StockActual);

            AddResumenItem("📦 Resumen de Inventario", "");
            AddResumenItem("Total de Productos", totalProductos.ToString());
            AddResumenItem("Productos en Estado CRÍTICO", criticos.ToString());
            AddResumenItem("Productos en ALERTA", alerta.ToString());
            AddResumenItem("Productos en Estado NORMAL", normal.ToString());
            AddResumenItem("Valor Total del Inventario", valorTotalInventario.ToString("C"));

            // Distribución por tipo
            var porTipo = inventario.GroupBy(p => p.Tipo)
                                  .Select(g => new { Tipo = g.Key, Count = g.Count() })
                                  .OrderByDescending(x => x.Count);

            AddResumenItem("📊 Distribución por Tipo", "");
            foreach (var tipo in porTipo)
            {
                AddResumenItem($"   • {tipo.Tipo}", $"{tipo.Count} productos");
            }
        }

        private void AddResumenItem(string titulo, string valor)
        {
            var stack = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 5, 0, 5) };
            stack.Children.Add(new TextBlock { Text = titulo, FontWeight = FontWeights.Bold, Width = 250 });
            stack.Children.Add(new TextBlock { Text = valor, Margin = new Thickness(10, 0, 0, 0) });
            panelResumen.Children.Add(stack);
        }

        private void btnExportarExcel_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Funcionalidad de exportación a Excel en desarrollo", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void btnImprimir_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Funcionalidad de impresión en desarrollo", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}