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
                MessageBox.Show($"🔍 INICIANDO CONSULTA:\nUsuario: {_idUsuario}\nFecha Inicio: {fechaInicio}\nFecha Fin: {fechaFin}",
                              "Debug Info", MessageBoxButton.OK, MessageBoxImage.Information);

                // 1. PRIMERO: Probar obtener TODAS las ventas sin filtrar
                MessageBox.Show("📊 Obteniendo todas las ventas...", "Paso 1", MessageBoxButton.OK, MessageBoxImage.Information);
                var todasLasVentas = cnReporte.ObtenerVentasPorFecha(fechaInicio, fechaFin);

                MessageBox.Show($"✅ Se obtuvieron {todasLasVentas?.Count ?? 0} ventas totales",
                              "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                // 2. LUEGO: Filtrar por vendedor
                if (todasLasVentas != null)
                {
                    MessageBox.Show($"🔍 Filtrando por usuario ID: {_idUsuario}...", "Paso 2", MessageBoxButton.OK, MessageBoxImage.Information);
                    ventasOriginales = todasLasVentas.Where(v => v.IdUsuario == _idUsuario).ToList();

                    MessageBox.Show($"✅ Ventas filtradas: {ventasOriginales.Count} ventas del vendedor",
                                  "Filtrado Completado", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    ventasOriginales = new List<ReporteVenta>();
                    MessageBox.Show("⚠️ No se obtuvieron ventas para el rango de fechas",
                                  "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

                // 3. FINALMENTE: Aplicar filtros adicionales
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
                    Width = 600,
                    Height = 400,
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