using CapaDatos;
using CapaEntidad;
using CapaNegocio;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CapaPresentacion
{
    public partial class FormReportes : Window
    {
        private CN_Reporte cnReporte = new CN_Reporte();
        private CN_Cliente cnCliente = new CN_Cliente();
        private CN_Producto cnProducto = new CN_Producto();
        private CN_Venta cnVenta = new CN_Venta(); // Agregar esta línea


        private List<ReporteVenta> ventasClienteActual;
        private string clienteActualNombre;

        public FormReportes()
        {
            InitializeComponent();
            // Inicializar fechas por defecto (últimos 30 días)
            dtpFechaInicio.SelectedDate = DateTime.Today.AddDays(-30);
            dtpFechaFin.SelectedDate = DateTime.Today;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CargarEstadisticasRapidas();
        }

        private void CargarEstadisticasRapidas()
        {
            try
            {
                // Ventas de hoy
                var ventasHoy = cnReporte.ObtenerVentasPorFecha(DateTime.Today, DateTime.Today);
                decimal totalHoy = ventasHoy.Sum(v => v.Total);
                txtVentasHoy.Text = $"Ventas Hoy: {ventasHoy.Count} ventas - {totalHoy:C}";

                // Stock crítico - verificar propiedades reales de Producto
                var productos = cnProducto.Listar();
                var stockCritico = productos.Count(p => p.StockActual <= p.StockMinimo); // Ajusta según tus propiedades reales
                txtStockCritico.Text = $"Productos con Stock Crítico: {stockCritico}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar estadísticas: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void cboTipoReporte_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboTipoReporte.SelectedItem == null) return;

            var selectedReport = ((ComboBoxItem)cboTipoReporte.SelectedItem).Content.ToString();

            // Mostrar/ocultar paneles según el tipo de reporte
            panelFiltrosFecha.Visibility = selectedReport == "Ventas por Fecha" ? Visibility.Visible : Visibility.Collapsed;
            panelFiltrosCliente.Visibility = selectedReport == "Ventas por Cliente" ? Visibility.Visible : Visibility.Collapsed;

            // Ocultar ambos paneles para otros reportes
            if (selectedReport != "Ventas por Fecha" && selectedReport != "Ventas por Cliente")
            {
                panelFiltrosFecha.Visibility = Visibility.Collapsed;
                panelFiltrosCliente.Visibility = Visibility.Collapsed;
            }

            // Cargar combobox de clientes si es necesario
            if (selectedReport == "Ventas por Cliente" && cboClientes.Items.Count == 0)
            {
                CargarClientes();
            }

            // Limpiar resultados al cambiar tipo de reporte
            dgvReporte.ItemsSource = null;
            panelResumen.Children.Clear();
        }

        private void CargarClientes()
        {
            try
            {
                var clientes = cnCliente.ListarClientes();
                cboClientes.ItemsSource = clientes;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar clientes: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnGenerarReporte_Click(object sender, RoutedEventArgs e)
        {
            if (cboTipoReporte.SelectedItem == null)
            {
                MessageBox.Show("Seleccione un tipo de reporte", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selectedReport = ((ComboBoxItem)cboTipoReporte.SelectedItem).Content.ToString();

            try
            {
                switch (selectedReport)
                {
                    case "Ventas por Cliente":
                        GenerarReporteVentasPorCliente();
                        break;
                    case "Ventas por Fecha":
                        GenerarReporteVentasPorFecha();
                        break;
                    case "Consumo Materia Prima":
                        GenerarReporteConsumoMateriaPrima();
                        break;
                    case "Stock Crítico":
                        GenerarReporteStockCritico();
                        break;
                    case "Inventario Completo":
                        GenerarReporteInventarioCompleto();
                        break;
                    default:
                        MessageBox.Show("Tipo de reporte no implementado", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al generar reporte: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GenerarReporteVentasPorCliente()
        {
            if (cboClientes.SelectedItem == null)
            {
                MessageBox.Show("Seleccione un cliente", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var cliente = (Cliente)cboClientes.SelectedItem;

            // Obtener ventas del cliente (sin fechas si el método no las acepta)
            ventasClienteActual = cnReporte.ObtenerVentasPorCliente(cliente.IdCliente);
            clienteActualNombre = cliente.Nombre; // Ajusta según tu propiedad real

            // Aplicar filtros
            AplicarFiltrosVentasCliente();
            GenerarResumenVentasCliente(ventasClienteActual, clienteActualNombre);
        }

        private void GenerarReporteVentasPorFecha()
        {
            if (dtpFechaInicio.SelectedDate == null || dtpFechaFin.SelectedDate == null)
            {
                MessageBox.Show("Seleccione ambas fechas", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DateTime fechaInicio = dtpFechaInicio.SelectedDate.Value;
            DateTime fechaFin = dtpFechaFin.SelectedDate.Value;

            if (fechaInicio > fechaFin)
            {
                MessageBox.Show("La fecha de inicio no puede ser mayor a la fecha fin", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var ventas = cnReporte.ObtenerVentasPorFecha(fechaInicio, fechaFin);
            dgvReporte.ItemsSource = ventas;
            ConfigurarColumnasVentas();
            GenerarResumenVentasPorFecha(ventas, fechaInicio, fechaFin);
        }

        private void GenerarReporteConsumoMateriaPrima()
        {
            MessageBox.Show("Reporte de Consumo Materia Prima en desarrollo", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void GenerarReporteStockCritico()
        {
            var productos = cnProducto.Listar();
            var productosCriticos = productos.Where(p => p.StockActual <= p.StockMinimo).ToList(); // Ajusta propiedades
            dgvReporte.ItemsSource = productosCriticos;
            ConfigurarColumnasProductos();
            GenerarResumenStockCritico(productosCriticos);
        }

        private void GenerarReporteInventarioCompleto()
        {
            var productos = cnProducto.Listar();
            dgvReporte.ItemsSource = productos;
            ConfigurarColumnasProductos();
            GenerarResumenInventarioCompleto(productos);
        }

        private void AplicarFiltrosVentasCliente()
        {
            if (ventasClienteActual == null) return;

            var ventasFiltradas = ventasClienteActual.ToList();

            // Filtrar por DNI
            if (!string.IsNullOrWhiteSpace(txtFiltroDNI.Text))
            {
                ventasFiltradas = ventasFiltradas
                    .Where(v => v.DNI != null && v.DNI.IndexOf(txtFiltroDNI.Text, StringComparison.OrdinalIgnoreCase) >= 0)
                    .ToList();
            }

            // Filtrar por producto
            if (!string.IsNullOrWhiteSpace(txtFiltroProducto.Text))
            {
                ventasFiltradas = ventasFiltradas
                    .Where(v => v.Productos != null && v.Productos.IndexOf(txtFiltroProducto.Text, StringComparison.OrdinalIgnoreCase) >= 0)
                    .ToList();
            }

            dgvReporte.ItemsSource = ventasFiltradas;
            ConfigurarColumnasVentas();
        }

        private void ConfigurarColumnasVentas()
        {
            dgvReporte.Columns.Clear();

            dgvReporte.Columns.Add(new DataGridTextColumn
            {
                Header = "ID",
                Binding = new System.Windows.Data.Binding("IdVenta"),
                Width = 60
            });

            dgvReporte.Columns.Add(new DataGridTextColumn
            {
                Header = "Fecha",
                Binding = new System.Windows.Data.Binding("Fecha") { StringFormat = "dd/MM/yyyy HH:mm" },
                Width = 120
            });

            dgvReporte.Columns.Add(new DataGridTextColumn
            {
                Header = "DNI",
                Binding = new System.Windows.Data.Binding("DniCliente"),
                Width = 100
            });

            dgvReporte.Columns.Add(new DataGridTextColumn
            {
                Header = "Total",
                Binding = new System.Windows.Data.Binding("Total") { StringFormat = "C" },
                Width = 100
            });

            dgvReporte.Columns.Add(new DataGridTextColumn
            {
                Header = "Productos",
                Binding = new System.Windows.Data.Binding("Productos"),
                Width = 200
            });

            // Agregar columna de botón de detalles
            dgvReporte.Columns.Add(new DataGridTemplateColumn
            {
                Header = "Detalles",
                Width = 80,
                CellTemplate = new DataTemplate(() =>
                {
                    var button = new Button
                    {
                        Content = "📋 Detalles",
                        Background = (Brush)new BrushConverter().ConvertFromString("#2196F3"),
                        Foreground = Brushes.White,
                        FontSize = 10,
                        Padding = new Thickness(5, 2, 5, 2),
                        Cursor = Cursors.Hand,
                        ToolTip = "Ver detalles de la venta"
                    };

                    button.Click += BtnVerDetallesVenta_Click;
                    return button;
                })
            });
        }

        private void ConfigurarColumnasProductos()
        {
            dgvReporte.Columns.Clear();

            dgvReporte.Columns.Add(new DataGridTextColumn
            {
                Header = "ID",
                Binding = new System.Windows.Data.Binding("IdProducto"),
                Width = 60
            });

            dgvReporte.Columns.Add(new DataGridTextColumn
            {
                Header = "Nombre",
                Binding = new System.Windows.Data.Binding("Nombre"),
                Width = 200
            });

            dgvReporte.Columns.Add(new DataGridTextColumn
            {
                Header = "Stock",
                Binding = new System.Windows.Data.Binding("StockActual"),
                Width = 80
            });

            dgvReporte.Columns.Add(new DataGridTextColumn
            {
                Header = "Stock Mínimo",
                Binding = new System.Windows.Data.Binding("StockMinimo"),
                Width = 100
            });

            dgvReporte.Columns.Add(new DataGridTextColumn
            {
                Header = "Precio",
                Binding = new System.Windows.Data.Binding("PrecioVenta") { StringFormat = "C" },
                Width = 100
            });

            // Para productos no agregamos botón de detalles ya que no es necesario
        }

        // Evento para el botón de detalles de venta
        private void BtnVerDetallesVenta_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = sender as Button;
                if (button != null && button.DataContext is ReporteVenta ventaSeleccionada)
                {
                    MostrarDetallesVenta(ventaSeleccionada.IdVenta);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al mostrar detalles: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MostrarDetallesVenta(int idVenta)
        {
            try
            {
                // Obtener los detalles de la venta usando ItemVenta
                var detalles = cnVenta.ObtenerDetallesVenta(idVenta);

                if (detalles == null || detalles.Count == 0)
                {
                    MessageBox.Show("No se encontraron detalles para esta venta.", "Información",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Crear ventana de detalles
                var ventanaDetalles = new Window
                {
                    Title = $"Detalles de Venta # {idVenta}",
                    Width = 600,
                    Height = 400,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Owner = this,
                    Background = Brushes.White,
                    Padding = new Thickness(20)
                };

                // Crear DataGrid para mostrar detalles
                var dgvDetalles = new DataGrid
                {
                    AutoGenerateColumns = false,
                    IsReadOnly = true,
                    Margin = new Thickness(0, 10, 0, 0)
                };

                dgvDetalles.Columns.Add(new DataGridTextColumn
                {
                    Header = "Producto",
                    Binding = new System.Windows.Data.Binding("NombreProducto"),
                    Width = new DataGridLength(2, DataGridLengthUnitType.Star)
                });
                dgvDetalles.Columns.Add(new DataGridTextColumn
                {
                    Header = "Cantidad",
                    Binding = new System.Windows.Data.Binding("Cantidad"),
                    Width = 80
                });
                dgvDetalles.Columns.Add(new DataGridTextColumn
                {
                    Header = "Precio Unit.",
                    Binding = new System.Windows.Data.Binding("PrecioUnitario") { StringFormat = "C" },
                    Width = 100
                });
                dgvDetalles.Columns.Add(new DataGridTextColumn
                {
                    Header = "Subtotal",
                    Binding = new System.Windows.Data.Binding("Subtotal") { StringFormat = "C" },
                    Width = 100
                });

                dgvDetalles.ItemsSource = detalles;

                // Calcular total
                decimal total = detalles.Sum(d => d.Subtotal);

                var stackPanel = new StackPanel();
                stackPanel.Children.Add(new TextBlock
                {
                    Text = $"Detalles de la Venta #{idVenta}",
                    FontSize = 16,
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(0, 0, 0, 10)
                });
                stackPanel.Children.Add(dgvDetalles);
                stackPanel.Children.Add(new TextBlock
                {
                    Text = $"Total: {total:C}",
                    FontSize = 14,
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(0, 10, 0, 0),
                    HorizontalAlignment = HorizontalAlignment.Right
                });

                ventanaDetalles.Content = stackPanel;
                ventanaDetalles.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar detalles: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GenerarResumenVentasCliente(List<ReporteVenta> ventas, string nombreCliente)
        {
            panelResumen.Children.Clear();

            decimal totalVentas = ventas.Sum(v => v.Total);
            int cantidadVentas = ventas.Count;
            decimal ticketPromedio = cantidadVentas > 0 ? totalVentas / cantidadVentas : 0;

            AddResumenItem($"📊 RESUMEN VENTAS - {nombreCliente.ToUpper()}", "#2c3e50", 16, true);
            AddResumenItem($"Total Ventas: {totalVentas:C}", "#27ae60", 14, false);
            AddResumenItem($"Cantidad de Ventas: {cantidadVentas}", "#2980b9", 14, false);
            AddResumenItem($"Ticket Promedio: {ticketPromedio:C}", "#f39c12", 14, false);
        }

        private void GenerarResumenVentasPorFecha(List<ReporteVenta> ventas, DateTime fechaInicio, DateTime fechaFin)
        {
            panelResumen.Children.Clear();

            decimal totalVentas = ventas.Sum(v => v.Total);
            int cantidadVentas = ventas.Count;
            decimal ticketPromedio = cantidadVentas > 0 ? totalVentas / cantidadVentas : 0;

            AddResumenItem($"📊 RESUMEN VENTAS {fechaInicio:dd/MM/yyyy} - {fechaFin:dd/MM/yyyy}", "#2c3e50", 16, true);
            AddResumenItem($"Total Ventas: {totalVentas:C}", "#27ae60", 14, false);
            AddResumenItem($"Cantidad de Ventas: {cantidadVentas}", "#2980b9", 14, false);
            AddResumenItem($"Ticket Promedio: {ticketPromedio:C}", "#f39c12", 14, false);
        }

        private void GenerarResumenStockCritico(List<Producto> productos)
        {
            panelResumen.Children.Clear();

            AddResumenItem("⚠️ RESUMEN STOCK CRÍTICO", "#c0392b", 16, true);
            AddResumenItem($"Productos con stock crítico: {productos.Count}", "#e74c3c", 14, false);

            foreach (var producto in productos.Take(5))
            {
                AddResumenItem($"{producto.Nombre}: {producto.StockActual} unidades (Mín: {producto.StockMinimo})", "#f39c12", 12, false);
            }

            if (productos.Count > 5)
            {
                AddResumenItem($"... y {productos.Count - 5} productos más", "#7f8c8d", 12, false);
            }
        }

        private void GenerarResumenInventarioCompleto(List<Producto> productos)
        {
            panelResumen.Children.Clear();

            decimal valorTotalInventario = productos.Sum(p => p.StockActual * p.PrecioVenta);
            int productosStockBajo = productos.Count(p => p.StockActual <= p.StockMinimo);

            AddResumenItem("📦 RESUMEN INVENTARIO COMPLETO", "#2c3e50", 16, true);
            AddResumenItem($"Total productos: {productos.Count}", "#27ae60", 14, false);
            AddResumenItem($"Valor total inventario: {valorTotalInventario:C}", "#2980b9", 14, false);
            AddResumenItem($"Productos con stock bajo: {productosStockBajo}", "#f39c12", 14, false);
        }

        private void AddResumenItem(string texto, string color, double fontSize, bool isBold)
        {
            var textBlock = new TextBlock
            {
                Text = texto,
                Foreground = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString(color),
                FontSize = fontSize,
                FontWeight = isBold ? FontWeights.Bold : FontWeights.Normal,
                Margin = new Thickness(0, 5, 0, 5)
            };

            panelResumen.Children.Add(textBlock);
        }

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

        private void btnExportarExcel_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Exportar a Excel - Funcionalidad en desarrollo", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void btnImprimir_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Imprimir - Funcionalidad en desarrollo", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private void dgvReporte_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Puedes dejar este método vacío o agregar funcionalidad si es necesario
            // Por ahora lo dejamos vacío solo para que no genere error
        }
    }
}