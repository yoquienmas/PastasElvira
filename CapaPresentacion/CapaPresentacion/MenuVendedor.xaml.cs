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
    public partial class MenuVendedor : Window
    {
        private int _idVendedor;
        private string _nombreCompletoVendedor;
        private CN_Reporte cnReporte = new CN_Reporte();
        private CN_Producto cnProducto = new CN_Producto();

        public MenuVendedor(int idVendedor, string nombreCompletoVendedor)
        {
            InitializeComponent();
            _idVendedor = idVendedor;
            _nombreCompletoVendedor = nombreCompletoVendedor;

            // Mostrar información del vendedor
            txtInfoVendedor.Text = $"Vendedor: {_nombreCompletoVendedor}";

            CargarDashboardVendedor();
        }

        private void CargarDashboardVendedor()
        {
            try
            {
                // Obtener fecha actual para cálculos
                DateTime inicioMes = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                DateTime finMes = inicioMes.AddMonths(1).AddDays(-1);

                // 1. Cargar mis ventas del mes
                CargarMisVentasMes(inicioMes, finMes);

                // 2. Cargar productos disponibles
                CargarProductosDisponibles();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar dashboard vendedor: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CargarProductosDisponibles()
        {
            try
            {
                var productos = cnProducto.Listar();

                // Convertir productos a formato adecuado para mostrar
                var productosParaMostrar = new List<ProductoDisplay>();

                foreach (var producto in productos)
                {
                    productosParaMostrar.Add(new ProductoDisplay
                    {
                        Nombre = producto.Nombre ?? "Sin nombre",
                        Precio = producto.Precio,
                        Stock = producto.Stock,
                        EstadoTexto = producto.Estado ?? "Desconocido" // ✅ Manejar Estado como string
                    });
                }

                // Filtrar solo productos activos - CORRECCIÓN: Estado es string, no bool
                var productosActivos = new List<ProductoDisplay>();
                foreach (var producto in productos)
                {
                    // ✅ CORREGIDO: Comparar string en lugar de bool
                    if (producto.Estado?.ToLower() == "activo" || producto.Estado?.ToLower() == "true" || producto.Estado == "1")
                    {
                        productosActivos.Add(new ProductoDisplay
                        {
                            Nombre = producto.Nombre ?? "Sin nombre",
                            Precio = producto.Precio,
                            Stock = producto.Stock,
                            EstadoTexto = "Activo"
                        });
                    }
                }

                dgvProductosDisponibles.ItemsSource = productosActivos;

                // Actualizar contador
                txtContadorProductos.Text = $"Total: {productosActivos.Count} productos disponibles";

                // Mostrar resumen de stock
                int sinStock = 0;
                int stockBajo = 0;

                foreach (var producto in productosActivos)
                {
                    if (producto.Stock == 0)
                        sinStock++;
                    else if (producto.Stock > 0 && producto.Stock <= 10)
                        stockBajo++;
                }

                if (sinStock > 0 || stockBajo > 0)
                {
                    string mensajeStock = "";
                    if (sinStock > 0) mensajeStock += $"{sinStock} sin stock";
                    if (stockBajo > 0)
                    {
                        if (mensajeStock != "") mensajeStock += ", ";
                        mensajeStock += $"{stockBajo} con stock bajo";
                    }

                    txtContadorProductos.ToolTip = $"Advertencia: {mensajeStock}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar productos: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                // Cargar datos de ejemplo
                CargarProductosEjemplo();
            }
        }

        // Clase auxiliar para mostrar productos
        public class ProductoDisplay
        {
            public string Nombre { get; set; }
            public decimal Precio { get; set; }
            public int Stock { get; set; }
            public string EstadoTexto { get; set; }
        }

        private void CargarProductosEjemplo()
        {
            var productosEjemplo = new List<ProductoDisplay>
            {
                new ProductoDisplay {
                    Nombre = "Ñoquis de Papa",
                    Precio = 2500m,
                    Stock = 15,
                    EstadoTexto = "Activo"
                },
                new ProductoDisplay {
                    Nombre = "Ravioles de Ricota",
                    Precio = 2800m,
                    Stock = 8,
                    EstadoTexto = "Activo"
                },
                new ProductoDisplay {
                    Nombre = "Tallarines",
                    Precio = 2200m,
                    Stock = 0,
                    EstadoTexto = "Activo"
                }
            };

            dgvProductosDisponibles.ItemsSource = productosEjemplo;
            txtContadorProductos.Text = $"Total: {productosEjemplo.Count} productos (ejemplo)";
        }

        private void CargarMisVentasMes(DateTime inicio, DateTime fin)
        {
            try
            {
                var misVentas = cnReporte.ObtenerVentasPorVendedor(_idVendedor, inicio, fin);

                // Actualizar métricas
                decimal totalVendido = misVentas.Sum(v => v.Total);
                int cantidadVentas = misVentas.Count;
                decimal ticketPromedio = cantidadVentas > 0 ? totalVendido / cantidadVentas : 0;

                txtMisVentasTotal.Text = totalVendido.ToString("C");
                txtMisVentasCantidad.Text = cantidadVentas.ToString();
                txtMisTicketPromedio.Text = ticketPromedio.ToString("C");

                // Mostrar últimas ventas
                dgvMisVentas.ItemsSource = misVentas.Take(10);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar ventas: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region Event Handlers - Vendedor

        private void btnProductosDisponibles_Click(object sender, RoutedEventArgs e)
        {
            FormProductosDisponibles formproductosdisponibles = new FormProductosDisponibles();
            formproductosdisponibles.Show();
            }

        private void btnActualizarProductos_Click(object sender, RoutedEventArgs e)
        {
            // Actualizar la lista de productos
            CargarProductosDisponibles();
            MessageBox.Show("Lista de productos actualizada correctamente.",
                "Actualización Exitosa", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void btnNuevaVenta_Click(object sender, RoutedEventArgs e)
        {
            FormVenta formVenta = new FormVenta(_idVendedor, _nombreCompletoVendedor);
            formVenta.Show();
        }

        private void btnMisVentas_Click(object sender, RoutedEventArgs e)
        {
            FormHistorialVentas formhistorialventas = new FormHistorialVentas(_idVendedor, _nombreCompletoVendedor);
            formhistorialventas.Show();
        }

        private void btnGestionClientes_Click(object sender, RoutedEventArgs e)
        {
            FormCliente formCliente = new FormCliente();
            formCliente.Show();
        }

        private void btnVerDetalleVenta_Click(object sender, RoutedEventArgs e)
        {
            if (dgvMisVentas.SelectedItem != null)
            {
                MessageBox.Show("Mostrando detalle de venta seleccionada", "Detalle de Venta", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Por favor, seleccione una venta para ver su detalle", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
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
        #endregion
    }
}