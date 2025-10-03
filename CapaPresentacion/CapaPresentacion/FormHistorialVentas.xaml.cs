using CapaDatos;
using CapaEntidad;
using CapaNegocio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Documents;

namespace CapaPresentacion
{
    public partial class FormHistorialVentas : Window
    {
        private int _idUsuario;
        private string _nombreUsuario;
        private CN_Reporte cnReporte = new CN_Reporte();
        private CN_Venta cnVenta = new CN_Venta();
        private List<ReporteVenta> ventasOriginales;

        public FormHistorialVentas(int idUsuario, string nombreUsuario)
        {
            InitializeComponent();

            _idUsuario = idUsuario;
            _nombreUsuario = nombreUsuario;
            txtNombreVendedor.Text = _nombreUsuario;

            dtpFechaInicio.SelectedDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            dtpFechaFin.SelectedDate = DateTime.Now;

            CargarVentas();
        }

        private void CargarVentas()
        {
            if (dtpFechaInicio.SelectedDate == null || dtpFechaFin.SelectedDate == null)
            {
                MessageBox.Show("Debe seleccionar ambas fechas.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DateTime fechaInicio = dtpFechaInicio.SelectedDate.Value;
            DateTime fechaFin = dtpFechaFin.SelectedDate.Value;

            if (fechaInicio > fechaFin)
            {
                MessageBox.Show("La fecha de inicio no puede ser mayor a la fecha fin.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var todasLasVentas = cnReporte.ObtenerVentasPorFecha(fechaInicio, fechaFin);

                if (todasLasVentas != null)
                {
                    ventasOriginales = todasLasVentas.Where(v => v.IdUsuario == _idUsuario).ToList();
                }
                else
                {
                    ventasOriginales = new List<ReporteVenta>();
                }

                AplicarFiltros();

            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ ERROR DETALLADO:\n{ex.Message}\n\nTipo: {ex.GetType().Name}\n\nStack Trace:\n{ex.StackTrace}",
                              "Error Detallado", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void VerificarDatosVentas()
        {
            try
            {
                DateTime fechaInicio = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                DateTime fechaFin = DateTime.Now;

                var ventas = cnReporte.ObtenerVentasPorFecha(fechaInicio, fechaFin);

                if (ventas == null)
                {
                    MessageBox.Show("❌ El método retornó NULL", "Resultado", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (ventas.Count == 0)
                {
                    MessageBox.Show("⚠️ El método retornó 0 ventas (lista vacía)", "Resultado", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Mostrar información de las primeras ventas
                string infoVentas = $"Total ventas: {ventas.Count}\n\nPrimeras 3 ventas:\n";
                foreach (var venta in ventas.Take(3))
                {
                    infoVentas += $"ID: {venta.IdVenta}, Fecha: {venta.Fecha}, Usuario: {venta.IdUsuario}, Total: {venta.Total:C}\n";
                }

                MessageBox.Show(infoVentas, "Datos Obtenidos", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Error al verificar datos: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AplicarFiltros()
        {
            if (ventasOriginales == null) return;

            var ventasFiltradas = ventasOriginales.ToList();

            if (!string.IsNullOrWhiteSpace(txtFiltroDNI.Text))
            {
                ventasFiltradas = ventasFiltradas
                    .Where(v => v.DNI != null &&
                           v.DNI.IndexOf(txtFiltroDNI.Text, StringComparison.OrdinalIgnoreCase) >= 0)
                    .ToList();
            }

            if (!string.IsNullOrWhiteSpace(txtFiltroProducto.Text))
            {
                ventasFiltradas = ventasFiltradas
                    .Where(v => v.Productos != null &&
                           v.Productos.IndexOf(txtFiltroProducto.Text, StringComparison.OrdinalIgnoreCase) >= 0)
                    .ToList();
            }

            dgvHistorialVentas.ItemsSource = ventasFiltradas;
            decimal totalVentas = ventasFiltradas.Sum(v => v.Total);
            txtTotalVentas.Text = totalVentas.ToString("C");
        }

        private void CargarVentasAlternativo()
        {
            try
            {
                DateTime fechaInicio = dtpFechaInicio.SelectedDate.Value;
                DateTime fechaFin = dtpFechaFin.SelectedDate.Value;

                // Intentar obtener ventas directamente por vendedor si existe el método
                if (cnReporte.GetType().GetMethod("ObtenerVentasPorVendedor") != null)
                {
                    ventasOriginales = cnReporte.ObtenerVentasPorVendedor(_idUsuario, fechaInicio, fechaFin);
                }
                else
                {
                    // Fallback: obtener todas y filtrar
                    var todasVentas = cnReporte.ObtenerVentasPorFecha(fechaInicio, fechaFin);
                    ventasOriginales = todasVentas?.Where(v => v.IdUsuario == _idUsuario).ToList() ?? new List<ReporteVenta>();
                }

                if (ventasOriginales == null || ventasOriginales.Count == 0)
                {
                    MessageBox.Show($"No se encontraron ventas para el vendedor {_nombreUsuario} en el rango de fechas seleccionado.",
                                  "Sin resultados", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                AplicarFiltros();
                MessageBox.Show($"✅ Se cargaron {ventasOriginales.Count} ventas correctamente.",
                              "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ProbarConFiltroFecha()
        {
            try
            {
                DateTime fechaInicio = new DateTime(2024, 1, 1);
                DateTime fechaFin = DateTime.Now;

                var ventasFiltradas = cnReporte.ObtenerVentasPorFecha(fechaInicio, fechaFin);

                MessageBox.Show($"🔍 MÉTODO CON FECHA:\nEntrada: {fechaInicio} a {fechaFin}\nResultado: {ventasFiltradas?.Count ?? 0} ventas",
                              "Prueba Fecha", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ ERROR EN MÉTODO FECHA: {ex.Message}",
                              "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnConsultar_Click(object sender, RoutedEventArgs e)
        {
            CargarVentas();
        }

        private void btnLimpiarFiltros_Click(object sender, RoutedEventArgs e)
        {
            txtFiltroDNI.Text = "";
            txtFiltroProducto.Text = "";

            if (ventasOriginales != null)
            {
                AplicarFiltros();
            }
            else
            {
                CargarVentas();
            }
        }

        private void BtnVerDetalles_Click(object sender, RoutedEventArgs e)
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

                var ventanaDetalles = new Window
                {
                    Title = $"Detalles de Venta # {idVenta}",
                    Width = 650,
                    Height = 450,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Owner = this,
                    Background = Brushes.White,
                    Padding = new Thickness(20)
                };

                var dgvDetalles = new DataGrid
                {
                    AutoGenerateColumns = false,
                    IsReadOnly = true,
                    Margin = new Thickness(0, 10, 0, 0)
                };

                dgvDetalles.Columns.Add(new DataGridTextColumn
                {
                    Header = "Producto",
                    Binding = new Binding("NombreProducto"),
                    Width = new DataGridLength(2, DataGridLengthUnitType.Star)
                });
                dgvDetalles.Columns.Add(new DataGridTextColumn
                {
                    Header = "Cantidad",
                    Binding = new Binding("Cantidad"),
                    Width = 80
                });
                dgvDetalles.Columns.Add(new DataGridTextColumn
                {
                    Header = "Precio Unit.",
                    Binding = new Binding("PrecioUnitario") { StringFormat = "C" },
                    Width = 100
                });
                dgvDetalles.Columns.Add(new DataGridTextColumn
                {
                    Header = "Subtotal",
                    Binding = new Binding("Subtotal") { StringFormat = "C" },
                    Width = 100
                });

                dgvDetalles.ItemsSource = detalles;
                decimal total = detalles.Sum(d => d.Subtotal);

                // Botón Imprimir
                var btnImprimirDetalles = new Button
                {
                    Content = "🖨️ Imprimir",
                    Width = 120,
                    Height = 35,
                    Background = Brushes.SteelBlue,
                    Foreground = Brushes.White,
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(0, 10, 0, 0),
                    HorizontalAlignment = HorizontalAlignment.Right
                };
                btnImprimirDetalles.Click += (s, e) => ImprimirDetallesVenta(idVenta, detalles, total);

                var stackPanel = new StackPanel();
                stackPanel.Children.Add(new TextBlock
                {
                    Text = $"Detalles de la Venta #{idVenta}",
                    FontSize = 16,
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(0, 0, 0, 10)
                });
                stackPanel.Children.Add(dgvDetalles);

                // Panel para el total y botón imprimir
                var panelInferior = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Margin = new Thickness(0, 10, 0, 0)
                };

                panelInferior.Children.Add(new TextBlock
                {
                    Text = $"Total: {total:C}",
                    FontSize = 14,
                    FontWeight = FontWeights.Bold,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 0, 20, 0)
                });

                panelInferior.Children.Add(btnImprimirDetalles);

                stackPanel.Children.Add(panelInferior);

                ventanaDetalles.Content = stackPanel;
                ventanaDetalles.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar detalles: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ImprimirDetallesVenta(int idVenta, object detalles, decimal total)
        {
            try
            {
                // Crear lista de items para impresión
                var itemsParaImprimir = new List<dynamic>();

                if (detalles is System.Collections.IEnumerable enumerable)
                {
                    foreach (var item in enumerable)
                    {
                        // Usar reflexión para acceder a las propiedades dinámicamente
                        var tipo = item.GetType();
                        itemsParaImprimir.Add(new
                        {
                            NombreProducto = tipo.GetProperty("NombreProducto")?.GetValue(item)?.ToString() ?? "Producto",
                            Cantidad = Convert.ToInt32(tipo.GetProperty("Cantidad")?.GetValue(item) ?? 0),
                            PrecioUnitario = Convert.ToDecimal(tipo.GetProperty("PrecioUnitario")?.GetValue(item) ?? 0m),
                            Subtotal = Convert.ToDecimal(tipo.GetProperty("Subtotal")?.GetValue(item) ?? 0m)
                        });
                    }
                }

                // Crear venta temporal para impresión
                var ventaTemporal = new
                {
                    IdVenta = idVenta,
                    FechaVenta = DateTime.Now,
                    IdVendedor = _idUsuario,
                    NombreVendedor = _nombreUsuario,
                    Cliente = "CONSUMIDOR FINAL",
                    DocumentoCliente = "",
                    Total = total,
                    Items = itemsParaImprimir
                };

                GenerarFactura(ventaTemporal);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al preparar impresión: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnImprimir_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Obtener la venta seleccionada
                if (dgvHistorialVentas.SelectedItem is ReporteVenta ventaSeleccionada)
                {
                    var detalles = cnVenta.ObtenerDetallesVenta(ventaSeleccionada.IdVenta);

                    if (detalles == null || detalles.Count == 0)
                    {
                        MessageBox.Show("No hay detalles para imprimir.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // Crear lista de items para impresión
                    var itemsParaImprimir = new List<dynamic>();

                    foreach (var detalle in detalles)
                    {
                        var tipo = detalle.GetType();
                        itemsParaImprimir.Add(new
                        {
                            NombreProducto = tipo.GetProperty("NombreProducto")?.GetValue(detalle)?.ToString() ?? "Producto",
                            Cantidad = Convert.ToInt32(tipo.GetProperty("Cantidad")?.GetValue(detalle) ?? 0),
                            PrecioUnitario = Convert.ToDecimal(tipo.GetProperty("PrecioUnitario")?.GetValue(detalle) ?? 0m),
                            Subtotal = Convert.ToDecimal(tipo.GetProperty("Subtotal")?.GetValue(detalle) ?? 0m)
                        });
                    }

                    // Crear venta temporal para impresión
                    var ventaTemporal = new
                    {
                        IdVenta = ventaSeleccionada.IdVenta,
                        FechaVenta = ventaSeleccionada.Fecha,
                        IdVendedor = _idUsuario,
                        NombreVendedor = _nombreUsuario,
                        Cliente = ventaSeleccionada.Cliente ?? "CONSUMIDOR FINAL",
                        DocumentoCliente = ventaSeleccionada.DNI ?? "",
                        Total = ventaSeleccionada.Total,
                        Items = itemsParaImprimir
                    };

                    GenerarFactura(ventaTemporal);
                }
                else
                {
                    MessageBox.Show("Por favor, seleccione una venta para imprimir.", "Información",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al imprimir: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GenerarFactura(dynamic venta)
        {
            try
            {
                PrintDialog printDialog = new PrintDialog();
                if (printDialog.ShowDialog() == true)
                {
                    // Crear documento para impresión
                    FlowDocument document = new FlowDocument();
                    document.PagePadding = new Thickness(50);
                    document.FontFamily = new System.Windows.Media.FontFamily("Courier New");
                    document.FontSize = 12;

                    // Encabezado
                    Paragraph header = new Paragraph();
                    header.Inlines.Add(new Run("PASTAS ELVIRA\n") { FontSize = 16, FontWeight = FontWeights.Bold });
                    header.Inlines.Add(new Run("Factura de Venta\n\n"));
                    header.Inlines.Add(new Run($"Fecha: {venta.FechaVenta:dd/MM/yyyy HH:mm:ss}\n"));
                    header.Inlines.Add(new Run($"N° Venta: {venta.IdVenta}\n"));
                    header.Inlines.Add(new Run($"Cliente: {venta.Cliente}\n"));
                    if (!string.IsNullOrEmpty(venta.DocumentoCliente))
                        header.Inlines.Add(new Run($"Documento: {venta.DocumentoCliente}\n"));
                    header.Inlines.Add(new Run($"Vendedor: {venta.NombreVendedor}\n"));
                    header.Inlines.Add(new Run("".PadRight(50, '=') + "\n\n"));
                    document.Blocks.Add(header);

                    // Detalle de productos
                    if (venta.Items != null && venta.Items.Count > 0)
                    {
                        Table table = new Table();
                        table.Columns.Add(new TableColumn { Width = new GridLength(300) }); // Producto
                        table.Columns.Add(new TableColumn { Width = new GridLength(80) });  // Cantidad
                        table.Columns.Add(new TableColumn { Width = new GridLength(100) }); // Precio
                        table.Columns.Add(new TableColumn { Width = new GridLength(100) }); // Subtotal

                        TableRowGroup rowGroup = new TableRowGroup();

                        // Encabezado de tabla
                        TableRow headerRow = new TableRow { Background = System.Windows.Media.Brushes.LightGray };
                        headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Producto")) { FontWeight = FontWeights.Bold }));
                        headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Cant")) { FontWeight = FontWeights.Bold }));
                        headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Precio")) { FontWeight = FontWeights.Bold }));
                        headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Subtotal")) { FontWeight = FontWeights.Bold }));
                        rowGroup.Rows.Add(headerRow);

                        // Productos
                        foreach (var item in venta.Items)
                        {
                            TableRow row = new TableRow();
                            row.Cells.Add(new TableCell(new Paragraph(new Run(item.NombreProducto))));
                            row.Cells.Add(new TableCell(new Paragraph(new Run(item.Cantidad.ToString()))));
                            row.Cells.Add(new TableCell(new Paragraph(new Run(item.PrecioUnitario.ToString("C")))));
                            row.Cells.Add(new TableCell(new Paragraph(new Run(item.Subtotal.ToString("C")))));
                            rowGroup.Rows.Add(row);
                        }

                        table.RowGroups.Add(rowGroup);
                        document.Blocks.Add(table);
                    }

                    // Total
                    Paragraph footer = new Paragraph();
                    footer.Inlines.Add(new Run("\n" + "".PadRight(50, '=') + "\n"));
                    footer.Inlines.Add(new Run($"TOTAL: {venta.Total.ToString("C")}") { FontSize = 14, FontWeight = FontWeights.Bold });
                    footer.Inlines.Add(new Run("\n\n¡Gracias por su compra!"));
                    document.Blocks.Add(footer);

                    // Imprimir
                    printDialog.PrintDocument(((IDocumentPaginatorSource)document).DocumentPaginator, $"Factura #{venta.IdVenta} - Pastas Elvira");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al imprimir: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TxtFiltroDNI_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && ventasOriginales != null)
            {
                AplicarFiltros();
            }
        }

        private void TxtFiltroProducto_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && ventasOriginales != null)
            {
                AplicarFiltros();
            }
        }

        private void dgvHistorialVentas_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Tu código existente aquí
        }
    }
}