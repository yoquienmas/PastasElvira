using CapaDatos;
using CapaEntidad;
using CapaNegocio;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace CapaPresentacion
{
    public partial class FormReportes : Window
    {
        private CN_Reporte cnReporte = new CN_Reporte();
        private CN_Cliente cnCliente = new CN_Cliente();
        private CN_Producto cnProducto = new CN_Producto();
        private CN_Venta cnVenta = new CN_Venta();

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

                // Stock crítico
                var productos = cnProducto.Listar();
                var stockCritico = productos.Count(p => p.StockActual <= p.StockMinimo);
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

        private string ObtenerTextoMetodoPago(int metodoPago)
        {
            return metodoPago switch
            {
                1 => "EFECTIVO",
                2 => "TARJETA DÉBITO",
                3 => "TARJETA CRÉDITO",
                4 => "BILLETERA VIRTUAL",
                _ => "DESCONOCIDO"
            };
        }

        private string ObtenerIconoMetodoPago(int metodoPago)
        {
            return metodoPago switch
            {
                1 => "💵",
                2 => "💳",
                3 => "💳",
                4 => "📱",
                _ => "💰"
            };
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
            ventasClienteActual = cnReporte.ObtenerVentasPorCliente(cliente.IdCliente);
            clienteActualNombre = cliente.Nombre;

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
            var productosCriticos = productos.Where(p => p.StockActual <= p.StockMinimo).ToList();
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

            // ✅ CORRECCIÓN: Usar DNI en lugar de DniCliente
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
                Header = "ID Venta",
                Binding = new System.Windows.Data.Binding("IdVenta"),
                Width = 80
            });

            dgvReporte.Columns.Add(new DataGridTextColumn
            {
                Header = "Fecha",
                Binding = new System.Windows.Data.Binding("Fecha") { StringFormat = "dd/MM/yyyy HH:mm" },
                Width = 120
            });

            dgvReporte.Columns.Add(new DataGridTextColumn
            {
                Header = "Cliente",
                Binding = new System.Windows.Data.Binding("Cliente"),
                Width = 150
            });

            dgvReporte.Columns.Add(new DataGridTextColumn
            {
                Header = "DNI Cliente",
                Binding = new System.Windows.Data.Binding("DNI"),
                Width = 100
            });

            dgvReporte.Columns.Add(new DataGridTextColumn
            {
                Header = "Vendedor",
                Binding = new System.Windows.Data.Binding("Usuario"),
                Width = 120
            });

            var templateColumn = new DataGridTemplateColumn
            {
                Header = "Detalles",
                Width = 80
            };

            var factory = new FrameworkElementFactory(typeof(Button));
            factory.SetValue(Button.ContentProperty, "📋 Detalles");
            factory.SetValue(Button.BackgroundProperty, (Brush)new BrushConverter().ConvertFromString("#2196F3"));
            factory.SetValue(Button.ForegroundProperty, Brushes.White);
            factory.SetValue(Button.FontSizeProperty, 10.0);
            factory.SetValue(Button.PaddingProperty, new Thickness(5, 2, 5, 2));
            factory.SetValue(Button.CursorProperty, Cursors.Hand);
            factory.SetValue(Button.ToolTipProperty, "Ver detalles de la venta");

            factory.AddHandler(Button.ClickEvent, new RoutedEventHandler(BtnVerDetallesVenta_Click));

            templateColumn.CellTemplate = new DataTemplate()
            {
                VisualTree = factory
            };

            dgvReporte.Columns.Add(templateColumn);
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
                var detalles = cnVenta.ObtenerDetallesVenta(idVenta);

                if (detalles == null || detalles.Count == 0)
                {
                    MessageBox.Show("No se encontraron detalles para esta venta.", "Información",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // ✅ OBTENER INFORMACIÓN COMPLETA DE LA VENTA PARA EL MÉTODO DE PAGO
                var ventasRecientes = cnReporte.ObtenerVentasPorFecha(DateTime.Today.AddYears(-1), DateTime.Today);
                var ventaCompleta = ventasRecientes.FirstOrDefault(v => v.IdVenta == idVenta);

                var ventanaDetalles = new Window
                {
                    Title = $"Detalles de Venta # {idVenta}",
                    Width = 700, // ✅ Aumentar ancho para más información
                    Height = 500,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Owner = this,
                    Background = Brushes.White,
                    Padding = new Thickness(20)
                };

                var dgvDetalles = new DataGrid
                {
                    AutoGenerateColumns = false,
                    IsReadOnly = true,
                    Margin = new Thickness(0, 15, 0, 0),
                    Background = Brushes.WhiteSmoke
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

                decimal total = detalles.Sum(d => d.Subtotal);
                int totalProductos = detalles.Sum(d => d.Cantidad);

                var stackPanel = new StackPanel();

                // ✅ ENCABEZADO MEJORADO CON MÉTODO DE PAGO
                var gridEncabezado = new Grid();
                gridEncabezado.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
                gridEncabezado.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
                gridEncabezado.Margin = new Thickness(0, 0, 0, 15);

                // Columna izquierda
                var stackPanelIzquierdo = new StackPanel();
                stackPanelIzquierdo.Children.Add(new TextBlock
                {
                    Text = $"📋 Detalles de Venta #{idVenta}",
                    FontSize = 16,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.DarkSlateBlue,
                    Margin = new Thickness(0, 0, 0, 5)
                });

                if (ventaCompleta != null)
                {
                    stackPanelIzquierdo.Children.Add(new TextBlock
                    {
                        Text = $"📅 Fecha: {ventaCompleta.Fecha:dd/MM/yyyy HH:mm}",
                        FontSize = 12,
                        Margin = new Thickness(0, 2, 0, 2)
                    });

                    stackPanelIzquierdo.Children.Add(new TextBlock
                    {
                        Text = $"👤 Cliente: {ventaCompleta.Cliente ?? "CONSUMIDOR FINAL"}",
                        FontSize = 12,
                        Margin = new Thickness(0, 2, 0, 2)
                    });
                }

                // Columna derecha
                var stackPanelDerecho = new StackPanel();

                if (ventaCompleta != null)
                {
                    stackPanelDerecho.Children.Add(new TextBlock
                    {
                        Text = $"👨‍💼 Vendedor: {ventaCompleta.Usuario}",
                        FontSize = 12,
                        Margin = new Thickness(0, 2, 0, 2)
                    });

                    // ✅ MÉTODO DE PAGO
                    string iconoMetodoPago = ObtenerIconoMetodoPago(ventaCompleta.MetodoPago);
                    string textoMetodoPago = ObtenerTextoMetodoPago(ventaCompleta.MetodoPago);

                    stackPanelDerecho.Children.Add(new TextBlock
                    {
                        Text = $"{iconoMetodoPago} Método de Pago: {textoMetodoPago}",
                        FontSize = 12,
                        FontWeight = FontWeights.Bold,
                        Margin = new Thickness(0, 2, 0, 2),
                        Foreground = Brushes.DarkGreen
                    });
                }

                Grid.SetColumn(stackPanelIzquierdo, 0);
                Grid.SetColumn(stackPanelDerecho, 1);
                gridEncabezado.Children.Add(stackPanelIzquierdo);
                gridEncabezado.Children.Add(stackPanelDerecho);

                stackPanel.Children.Add(gridEncabezado);
                stackPanel.Children.Add(dgvDetalles);

                // ✅ PIE DE PÁGINA MEJORADO
                var stackPanelPie = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Margin = new Thickness(0, 15, 0, 0)
                };

                stackPanelPie.Children.Add(new TextBlock
                {
                    Text = $"📦 Total Productos: {totalProductos} | ",
                    FontSize = 12,
                    FontWeight = FontWeights.Bold,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 0, 10, 0)
                });

                stackPanelPie.Children.Add(new TextBlock
                {
                    Text = $"💰 Total: {total:C}",
                    FontSize = 14,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.DarkRed,
                    VerticalAlignment = VerticalAlignment.Center
                });

                stackPanel.Children.Add(stackPanelPie);

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
                Foreground = (Brush)new BrushConverter().ConvertFromString(color),
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
            try
            {
                if (dgvReporte.ItemsSource == null)
                {
                    MessageBox.Show("No hay datos para exportar.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Diálogo para elegir dónde guardar
                Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog();
                saveFileDialog.Filter = "Archivo CSV (*.csv)|*.csv";
                saveFileDialog.FileName = $"Reporte_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

                if (saveFileDialog.ShowDialog() == true)
                {
                    using (var writer = new System.IO.StreamWriter(saveFileDialog.FileName, false, System.Text.Encoding.UTF8))
                    {
                        // Obtener el tipo de reporte para determinar qué datos exportar
                        var selectedReport = cboTipoReporte.SelectedItem != null ?
                            ((ComboBoxItem)cboTipoReporte.SelectedItem).Content.ToString() : "Reporte";

                        // Escribir información del reporte
                        writer.WriteLine($"Reporte: {selectedReport}");
                        writer.WriteLine($"Fecha de generación: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");

                        // Información adicional según el tipo de reporte
                        if (selectedReport == "Ventas por Fecha" && dtpFechaInicio.SelectedDate != null && dtpFechaFin.SelectedDate != null)
                        {
                            writer.WriteLine($"Período: {dtpFechaInicio.SelectedDate.Value:dd/MM/yyyy} - {dtpFechaFin.SelectedDate.Value:dd/MM/yyyy}");
                        }
                        else if (selectedReport == "Ventas por Cliente" && cboClientes.SelectedItem != null)
                        {
                            var cliente = (Cliente)cboClientes.SelectedItem;
                            writer.WriteLine($"Cliente: {cliente.Nombre}");
                            writer.WriteLine($"DNI Cliente: {cliente.Documento}");
                        }

                        writer.WriteLine(); // Línea en blanco

                        // ✅ NUEVO: Para reportes de ventas, exportar con detalles expandidos
                        if (selectedReport.Contains("Ventas"))
                        {
                            ExportarVentasConDetalles(writer, selectedReport);
                        }
                        else
                        {
                            // Exportación normal para otros reportes
                            ExportarReporteNormal(writer);
                        }

                        // Agregar resumen si es un reporte de ventas
                        if (selectedReport.Contains("Ventas"))
                        {
                            var ventas = dgvReporte.ItemsSource.Cast<ReporteVenta>().ToList();
                            if (ventas.Any())
                            {
                                writer.WriteLine();
                                writer.WriteLine("RESUMEN:");
                                writer.WriteLine($"Total Ventas: {ventas.Sum(v => v.Total):C}");
                                writer.WriteLine($"Cantidad de Ventas: {ventas.Count}");
                                writer.WriteLine($"Ticket Promedio: {(ventas.Sum(v => v.Total) / ventas.Count):C}");

                                // Resumen por método de pago
                                writer.WriteLine();
                                writer.WriteLine("DISTRIBUCIÓN POR MÉTODO DE PAGO:");
                                var ventasPorMetodo = ventas
                                    .GroupBy(v => v.MetodoPago)
                                    .Select(g => new {
                                        Metodo = ObtenerTextoMetodoPago(g.Key),
                                        Cantidad = g.Count(),
                                        Total = g.Sum(v => v.Total)
                                    })
                                    .OrderByDescending(x => x.Total);

                                foreach (var grupo in ventasPorMetodo)
                                {
                                    writer.WriteLine($"{grupo.Metodo}: {grupo.Cantidad} ventas - {grupo.Total:C}");
                                }
                            }
                        }
                    }

                    MessageBox.Show("📄 Archivo exportado correctamente.\nPodés abrirlo con Excel.",
                                    "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al exportar: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ✅ NUEVO MÉTODO: Exportar ventas con detalles expandidos
        private void ExportarVentasConDetalles(System.IO.StreamWriter writer, string selectedReport)
        {
            var ventas = dgvReporte.ItemsSource.Cast<ReporteVenta>().ToList();

            // Cabeceras para ventas con detalles
            var headers = new List<string>
    {
        "ID Venta", "Fecha", "Cliente", "DNI Cliente", "Vendedor", "Método Pago",
        "Producto", "Cantidad", "Precio Unitario", "Subtotal", "Total Venta"
    };

            writer.WriteLine(string.Join(";", headers));

            // Escribir datos expandidos
            foreach (var venta in ventas)
            {
                try
                {
                    // Obtener detalles de la venta
                    var detalles = cnVenta.ObtenerDetallesVenta(venta.IdVenta);

                    if (detalles != null && detalles.Count > 0)
                    {
                        // Escribir cada producto como una fila separada
                        foreach (var detalle in detalles)
                        {
                            var values = new List<string>
                    {
                        venta.IdVenta.ToString(),
                        venta.Fecha.ToString("dd/MM/yyyy HH:mm"),
                        FormatearValorParaExportacion(venta.Cliente, "Cliente"),
                        FormatearValorParaExportacion(venta.DNI, "DNI"),
                        FormatearValorParaExportacion(venta.Usuario, "Usuario"),
                        ObtenerTextoMetodoPago(venta.MetodoPago),
                        FormatearValorParaExportacion(ObtenerPropiedadDetalle(detalle, "NombreProducto"), "NombreProducto"),
                        FormatearValorParaExportacion(ObtenerPropiedadDetalle(detalle, "Cantidad"), "Cantidad"),
                        FormatearValorParaExportacion(ObtenerPropiedadDetalle(detalle, "PrecioUnitario"), "PrecioUnitario"),
                        FormatearValorParaExportacion(ObtenerPropiedadDetalle(detalle, "Subtotal"), "Subtotal"),
                        FormatearValorParaExportacion(venta.Total, "Total")
                    };

                            writer.WriteLine(string.Join(";", values));
                        }
                    }
                    else
                    {
                        // Si no hay detalles, escribir solo la venta
                        var values = new List<string>
                {
                    venta.IdVenta.ToString(),
                    venta.Fecha.ToString("dd/MM/yyyy HH:mm"),
                    FormatearValorParaExportacion(venta.Cliente, "Cliente"),
                    FormatearValorParaExportacion(venta.DNI, "DNI"),
                    FormatearValorParaExportacion(venta.Usuario, "Usuario"),
                    ObtenerTextoMetodoPago(venta.MetodoPago),
                    "SIN DETALLES",
                    "0",
                    "$0.00",
                    "$0.00",
                    FormatearValorParaExportacion(venta.Total, "Total")
                };

                        writer.WriteLine(string.Join(";", values));
                    }
                }
                catch (Exception ex)
                {
                    // En caso de error, escribir solo la venta básica
                    var values = new List<string>
            {
                venta.IdVenta.ToString(),
                venta.Fecha.ToString("dd/MM/yyyy HH:mm"),
                FormatearValorParaExportacion(venta.Cliente, "Cliente"),
                FormatearValorParaExportacion(venta.DNI, "DNI"),
                FormatearValorParaExportacion(venta.Usuario, "Usuario"),
                ObtenerTextoMetodoPago(venta.MetodoPago),
                "ERROR AL CARGAR DETALLES",
                "0",
                "$0.00",
                "$0.00",
                FormatearValorParaExportacion(venta.Total, "Total")
            };

                    writer.WriteLine(string.Join(";", values));
                }
            }
        }

        // ✅ NUEVO MÉTODO: Exportación normal para otros reportes
        private void ExportarReporteNormal(System.IO.StreamWriter writer)
        {
            // Escribir cabeceras (headers) - excluir columna de detalles
            var headers = dgvReporte.Columns
                .Where(c => c.Header != null && c.Header.ToString() != "Detalles")
                .Select(c => c.Header.ToString())
                .ToArray();

            writer.WriteLine(string.Join(";", headers));

            // Escribir las filas
            foreach (var item in dgvReporte.ItemsSource)
            {
                var values = new List<string>();

                foreach (var column in dgvReporte.Columns)
                {
                    if (column.Header?.ToString() == "Detalles") continue;

                    if (column is DataGridBoundColumn boundColumn)
                    {
                        var binding = boundColumn.Binding as System.Windows.Data.Binding;
                        if (binding != null)
                        {
                            var propertyName = binding.Path.Path;
                            var prop = item.GetType().GetProperty(propertyName);
                            if (prop != null)
                            {
                                var value = prop.GetValue(item);
                                string text = FormatearValorParaExportacion(value, propertyName);
                                values.Add(text);
                            }
                        }
                    }
                }

                writer.WriteLine(string.Join(";", values));
            }
        }

        // ✅ NUEVO MÉTODO: Obtener propiedad de detalle de forma segura
        private object ObtenerPropiedadDetalle(object detalle, string propiedad)
        {
            try
            {
                var prop = detalle.GetType().GetProperty(propiedad);
                return prop?.GetValue(detalle) ?? "";
            }
            catch
            {
                return "";
            }
        }

        private string FormatearValorParaExportacion(object valor, string propiedad)
{
    if (valor == null) return "";

    string texto = ""; // ✅ DECLARAR UNA SOLA VEZ

    if (propiedad == "Fecha" && valor is DateTime)
    {
        texto = ((DateTime)valor).ToString("dd/MM/yyyy HH:mm");
    }
    else if ((propiedad == "Total" || propiedad == "PrecioVenta") && valor is decimal)
    {
        texto = ((decimal)valor).ToString("C");
    }
    else if (propiedad == "MetodoPagoTexto")
    {
        // Para método de pago, usar el texto sin emojis para CSV
        texto = valor.ToString();
        // Remover emojis para CSV
        texto = System.Text.RegularExpressions.Regex.Replace(texto, @"[^\u0000-\u007F]+", "").Trim();
    }
    else
    {
        texto = valor.ToString(); // ✅ USAR LA VARIABLE YA DECLARADA
    }
    
    // Escapar caracteres especiales para CSV
    if (texto.Contains(";") || texto.Contains("\"") || texto.Contains("\n") || texto.Contains("\r"))
    {
        texto = $"\"{texto.Replace("\"", "\"\"")}\"";
    }
    
    return texto;
}

        private void btnImprimir_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgvReporte.ItemsSource == null)
                {
                    MessageBox.Show("No hay datos para imprimir.", "Aviso",
                                    MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Obtener información del reporte
                var selectedReport = cboTipoReporte.SelectedItem != null ?
                    ((ComboBoxItem)cboTipoReporte.SelectedItem).Content.ToString() : "Reporte";

                // Crear documento
                FlowDocument doc = new FlowDocument();
                doc.PagePadding = new Thickness(40);
                doc.FontFamily = new FontFamily("Segoe UI");
                doc.FontSize = 10; // ✅ Reducir tamaño de fuente para más información
                doc.TextAlignment = TextAlignment.Left;

                // Encabezado principal
                Paragraph header = new Paragraph();
                header.Inlines.Add(new Run("PASTAS ELVIRA\n")
                {
                    FontSize = 18,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.DarkOrange
                });
                header.Inlines.Add(new Run("SISTEMA DE REPORTES\n\n")
                {
                    FontSize = 14,
                    FontWeight = FontWeights.Bold
                });

                // Información específica del reporte
                header.Inlines.Add(new Run($"Tipo de Reporte: {selectedReport}\n"));
                header.Inlines.Add(new Run($"Fecha de generación: {DateTime.Now:dd/MM/yyyy HH:mm:ss}\n"));

                if (selectedReport == "Ventas por Fecha" && dtpFechaInicio.SelectedDate != null && dtpFechaFin.SelectedDate != null)
                {
                    header.Inlines.Add(new Run($"Período: {dtpFechaInicio.SelectedDate.Value:dd/MM/yyyy} - {dtpFechaFin.SelectedDate.Value:dd/MM/yyyy}\n"));
                }
                else if (selectedReport == "Ventas por Cliente" && cboClientes.SelectedItem != null)
                {
                    var cliente = (Cliente)cboClientes.SelectedItem;
                    header.Inlines.Add(new Run($"Cliente: {cliente.Nombre}\n"));
                    header.Inlines.Add(new Run($"DNI: {cliente.Documento}\n"));
                }

                header.Inlines.Add(new Run("\n" + new string('=', 100) + "\n\n"));
                doc.Blocks.Add(header);

                // ✅ NUEVO: Para reportes de ventas, imprimir con detalles expandidos
                if (selectedReport.Contains("Ventas"))
                {
                    ImprimirVentasConDetalles(doc, selectedReport);
                }
                else
                {
                    // Impresión normal para otros reportes
                    ImprimirReporteNormal(doc);
                }

                // Mostrar diálogo de impresión
                PrintDialog printDialog = new PrintDialog();
                if (printDialog.ShowDialog() == true)
                {
                    doc.PageHeight = printDialog.PrintableAreaHeight;
                    doc.PageWidth = printDialog.PrintableAreaWidth;
                    printDialog.PrintDocument(((IDocumentPaginatorSource)doc).DocumentPaginator,
                                            $"Reporte Pastas Elvira - {selectedReport}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al imprimir: {ex.Message}", "Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ✅ NUEVO MÉTODO: Imprimir ventas con detalles expandidos
        private void ImprimirVentasConDetalles(FlowDocument doc, string selectedReport)
        {
            var ventas = dgvReporte.ItemsSource.Cast<ReporteVenta>().ToList();

            foreach (var venta in ventas)
            {
                try
                {
                    // Encabezado de cada venta
                    Paragraph ventaHeader = new Paragraph();
                    ventaHeader.Inlines.Add(new Run($"VENTA # {venta.IdVenta}\n")
                    {
                        FontWeight = FontWeights.Bold,
                        FontSize = 12,
                        Foreground = Brushes.DarkBlue
                    });

                    ventaHeader.Inlines.Add(new Run($"Fecha: {venta.Fecha:dd/MM/yyyy HH:mm} | "));
                    ventaHeader.Inlines.Add(new Run($"Cliente: {venta.Cliente} | "));
                    ventaHeader.Inlines.Add(new Run($"Vendedor: {venta.Usuario} | "));
                    ventaHeader.Inlines.Add(new Run($"Método: {ObtenerTextoMetodoPago(venta.MetodoPago)}\n"));

                    doc.Blocks.Add(ventaHeader);

                    // Obtener detalles de la venta
                    var detalles = cnVenta.ObtenerDetallesVenta(venta.IdVenta);

                    if (detalles != null && detalles.Count > 0)
                    {
                        // Crear tabla de detalles
                        Table tablaDetalles = new Table();

                        // Configurar columnas
                        tablaDetalles.Columns.Add(new TableColumn { Width = new GridLength(200) }); // Producto
                        tablaDetalles.Columns.Add(new TableColumn { Width = new GridLength(60) });  // Cantidad
                        tablaDetalles.Columns.Add(new TableColumn { Width = new GridLength(80) });  // Precio Unit.
                        tablaDetalles.Columns.Add(new TableColumn { Width = new GridLength(80) });  // Subtotal

                        TableRowGroup rowGroup = new TableRowGroup();

                        // Encabezado de tabla de detalles
                        TableRow headerRow = new TableRow { Background = Brushes.LightGray };
                        headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Producto")) { FontWeight = FontWeights.Bold }));
                        headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Cant")) { FontWeight = FontWeights.Bold }));
                        headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Precio Unit.")) { FontWeight = FontWeights.Bold }));
                        headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Subtotal")) { FontWeight = FontWeights.Bold }));
                        rowGroup.Rows.Add(headerRow);

                        // Productos
                        foreach (var detalle in detalles)
                        {
                            TableRow row = new TableRow();
                            row.Cells.Add(new TableCell(new Paragraph(new Run(ObtenerPropiedadDetalle(detalle, "NombreProducto")?.ToString() ?? ""))));
                            row.Cells.Add(new TableCell(new Paragraph(new Run(ObtenerPropiedadDetalle(detalle, "Cantidad")?.ToString() ?? ""))));
                            row.Cells.Add(new TableCell(new Paragraph(new Run(FormatearDecimalParaImpresion(ObtenerPropiedadDetalle(detalle, "PrecioUnitario"))))));
                            row.Cells.Add(new TableCell(new Paragraph(new Run(FormatearDecimalParaImpresion(ObtenerPropiedadDetalle(detalle, "Subtotal"))))));
                            rowGroup.Rows.Add(row);
                        }

                        tablaDetalles.RowGroups.Add(rowGroup);
                        doc.Blocks.Add(tablaDetalles);

                        // Total de la venta
                        Paragraph totalVenta = new Paragraph();
                        totalVenta.Inlines.Add(new Run($"Total Venta: {venta.Total:C}")
                        {
                            FontWeight = FontWeights.Bold,
                            Foreground = Brushes.DarkRed
                        });
                        totalVenta.TextAlignment = TextAlignment.Right;
                        doc.Blocks.Add(totalVenta);
                    }
                    else
                    {
                        doc.Blocks.Add(new Paragraph(new Run("SIN DETALLES DISPONIBLES")));
                    }

                    // Separador entre ventas
                    doc.Blocks.Add(new Paragraph(new Run(new string('-', 100))));
                    doc.Blocks.Add(new Paragraph()); // Espacio en blanco
                }
                catch (Exception ex)
                {
                    doc.Blocks.Add(new Paragraph(new Run($"ERROR AL CARGAR DETALLES DE VENTA #{venta.IdVenta}: {ex.Message}")));
                }
            }

            // Agregar resumen general
            AgregarResumenVentas(doc, ventas);
        }

        // ✅ NUEVO MÉTODO: Impresión normal para otros reportes
        private void ImprimirReporteNormal(FlowDocument doc)
        {
            // Crear tabla
            Table tabla = new Table();
            doc.Blocks.Add(tabla);

            // Detectar columnas visibles del DataGrid (excluir columna de detalles)
            var columnas = dgvReporte.Columns
                .Where(c => c.Header != null && c.Header.ToString() != "Detalles")
                .ToList();

            // Crear columnas en el documento
            foreach (var c in columnas)
            {
                tabla.Columns.Add(new TableColumn());
            }

            // Encabezado de tabla
            TableRowGroup encabezado = new TableRowGroup();
            tabla.RowGroups.Add(encabezado);
            TableRow filaEncabezado = new TableRow();
            encabezado.Rows.Add(filaEncabezado);

            foreach (var c in columnas)
            {
                var celda = new TableCell(new Paragraph(new Bold(new Run(c.Header.ToString()))))
                {
                    Padding = new Thickness(6),
                    Background = new SolidColorBrush(Color.FromRgb(42, 157, 143)),
                    Foreground = Brushes.White,
                    BorderBrush = Brushes.White,
                    BorderThickness = new Thickness(1)
                };
                filaEncabezado.Cells.Add(celda);
            }

            // Cuerpo de la tabla
            TableRowGroup cuerpo = new TableRowGroup();
            tabla.RowGroups.Add(cuerpo);

            // Recorrer los elementos del DataGrid
            foreach (var item in dgvReporte.ItemsSource)
            {
                TableRow fila = new TableRow();

                foreach (var col in columnas)
                {
                    string texto = "";

                    if (col is DataGridBoundColumn boundCol)
                    {
                        var binding = boundCol.Binding as System.Windows.Data.Binding;
                        if (binding != null)
                        {
                            var prop = item.GetType().GetProperty(binding.Path.Path);
                            if (prop != null)
                            {
                                var valor = prop.GetValue(item);
                                if (valor != null)
                                {
                                    texto = FormatearValorParaImpresion(valor, prop.Name);
                                }
                            }
                        }
                    }

                    var celda = new TableCell(new Paragraph(new Run(texto)))
                    {
                        Padding = new Thickness(4),
                        BorderBrush = Brushes.LightGray,
                        BorderThickness = new Thickness(0.5)
                    };
                    fila.Cells.Add(celda);
                }

                cuerpo.Rows.Add(fila);
            }
        }

        // ✅ NUEVO MÉTODO: Formatear decimal para impresión
        private string FormatearDecimalParaImpresion(object valor)
        {
            if (valor is decimal decimalValor)
            {
                return decimalValor.ToString("C");
            }
            return valor?.ToString() ?? "";
        }

        // ✅ NUEVO MÉTODO: Formatear valor para impresión
        private string FormatearValorParaImpresion(object valor, string propiedad)
        {
            if (valor == null) return "";

            if (propiedad == "Fecha" && valor is DateTime)
            {
                return ((DateTime)valor).ToString("dd/MM/yyyy HH:mm");
            }
            else if ((propiedad == "Total" || propiedad == "PrecioVenta") && valor is decimal)
            {
                return ((decimal)valor).ToString("C");
            }
            else if (propiedad == "MetodoPagoTexto")
            {
                return valor.ToString();
            }

            return valor.ToString();
        }

        // ✅ NUEVO MÉTODO: Agregar resumen de ventas
        private void AgregarResumenVentas(FlowDocument doc, List<ReporteVenta> ventas)
        {
            if (ventas.Any())
            {
                Paragraph resumen = new Paragraph();
                resumen.Inlines.Add(new Run("\n" + new string('=', 100) + "\n"));
                resumen.Inlines.Add(new Run("RESUMEN GENERAL DEL REPORTE\n")
                {
                    FontWeight = FontWeights.Bold,
                    FontSize = 14
                });
                resumen.Inlines.Add(new Run($"Total Ventas: {ventas.Sum(v => v.Total):C}\n"));
                resumen.Inlines.Add(new Run($"Cantidad de Ventas: {ventas.Count}\n"));
                resumen.Inlines.Add(new Run($"Ticket Promedio: {(ventas.Sum(v => v.Total) / ventas.Count):C}\n"));

                // Resumen por método de pago
                resumen.Inlines.Add(new Run("\nDISTRIBUCIÓN POR MÉTODO DE PAGO:\n")
                {
                    FontWeight = FontWeights.Bold
                });

                var ventasPorMetodo = ventas
                    .GroupBy(v => v.MetodoPago)
                    .Select(g => new {
                        Metodo = ObtenerTextoMetodoPago(g.Key),
                        Icono = ObtenerIconoMetodoPago(g.Key),
                        Cantidad = g.Count(),
                        Total = g.Sum(v => v.Total),
                        Porcentaje = (g.Sum(v => v.Total) / ventas.Sum(v => v.Total)) * 100
                    })
                    .OrderByDescending(x => x.Total);

                foreach (var grupo in ventasPorMetodo)
                {
                    resumen.Inlines.Add(new Run($"{grupo.Icono} {grupo.Metodo}: {grupo.Cantidad} ventas - {grupo.Total:C} ({grupo.Porcentaje:F1}%)\n"));
                }

                resumen.Inlines.Add(new Run(new string('=', 100)));
                doc.Blocks.Add(resumen);
            }

            // Pie de página
            Paragraph footer = new Paragraph();
            footer.Inlines.Add(new Run("\n\n--- Fin del Reporte ---"));
            footer.TextAlignment = TextAlignment.Center;
            footer.FontStyle = FontStyles.Italic;
            doc.Blocks.Add(footer);
        }

        private void dgvReporte_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Puedes dejar este método vacío o agregar funcionalidad si es necesario
        }
    }
}