using CapaDatos;
using CapaEntidad;
using CapaNegocio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using System.IO;

namespace CapaPresentacion
{
    public partial class FormProductosDisponibles : Window
    {
        private CN_Producto cnProducto = new CN_Producto();
        private List<Producto> todosLosProductos = new List<Producto>();
        private bool componentesInicializados = false;

        public FormProductosDisponibles()
        {
            InitializeComponent();
            componentesInicializados = true;

            // Cargar productos después de que los componentes estén inicializados
            CargarProductos();

            if (dgvProductos != null)
            {
                dgvProductos.SelectionChanged += DgvProductos_SelectionChanged;
            }

        }
        private void CargarProductos()
        {
            try
            {
                if (!componentesInicializados)
                {
                    // Retrasar la carga hasta que los componentes estén listos
                    Dispatcher.BeginInvoke(new Action(() => CargarProductos()),
                        System.Windows.Threading.DispatcherPriority.Loaded);
                    return;
                }

                todosLosProductos = cnProducto.Listar() ?? new List<Producto>();

                // Validar y limpiar la lista
                todosLosProductos = todosLosProductos
                    .Where(p => p != null && !string.IsNullOrEmpty(p.Nombre))
                    .ToList();

                ActualizarVista();
                ActualizarEstadisticas();
                CargarTipos();
                CargarFiltroStock();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar productos: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                todosLosProductos = new List<Producto>();

                if (componentesInicializados)
                {
                    ActualizarVista();
                }
            }
        }

        private void CargarTipos()
        {
            try
            {
                cmbFiltroTipo.Items.Clear();
                cmbFiltroTipo.Items.Add(new ComboBoxItem
                {
                    Content = "Todos los tipos",
                    Tag = "all"
                });

                if (todosLosProductos != null && todosLosProductos.Any())
                {
                    var tipos = todosLosProductos
                        .Where(p => p != null && !string.IsNullOrEmpty(p.Tipo) && p.Tipo != "NULL")
                        .Select(p => p.Tipo)
                        .Distinct()
                        .OrderBy(t => t)
                        .ToList();

                    foreach (var tipo in tipos)
                    {
                        if (!string.IsNullOrEmpty(tipo))
                        {
                            cmbFiltroTipo.Items.Add(new ComboBoxItem
                            {
                                Content = tipo,
                                Tag = "type"
                            });
                        }
                    }
                }

                if (cmbFiltroTipo.Items.Count > 0)
                    cmbFiltroTipo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar tipos: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void CargarFiltroStock()
        {
            try
            {
                cmbFiltroStock.Items.Clear();
                cmbFiltroStock.Items.Add(new ComboBoxItem { Content = "Todos", Tag = "all" });
                cmbFiltroStock.Items.Add(new ComboBoxItem { Content = "Con stock", Tag = "with_stock" });
                cmbFiltroStock.Items.Add(new ComboBoxItem { Content = "Sin stock", Tag = "without_stock" });
                cmbFiltroStock.Items.Add(new ComboBoxItem { Content = "Stock bajo", Tag = "low_stock" });

                if (cmbFiltroStock.Items.Count > 0)
                    cmbFiltroStock.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar filtro de stock: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ActualizarVista()
        {
            try
            {
                // Verificar que los componentes estén inicializados
                if (!componentesInicializados || dgvProductos == null)
                {
                    return; // Salir silenciosamente si no está listo
                }

                var productosFiltrados = FiltrarProductos();
                dgvProductos.ItemsSource = productosFiltrados;
            }
            catch (Exception ex)
            {
                // Mostrar error solo si los componentes están inicializados
                if (componentesInicializados)
                {
                    MessageBox.Show($"Error al actualizar vista: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
                if (dgvProductos != null)
                {
                    dgvProductos.ItemsSource = new List<Producto>();
                }
            }
        }
        private List<Producto> FiltrarProductos()
        {
            try
            {
                // Validación completa de la lista principal
                if (todosLosProductos == null)
                {
                    Console.WriteLine("todosLosProductos es NULL - retornando lista vacía");
                    return new List<Producto>();
                }

                // Filtrar productos no nulos y con validación de propiedades
                var filtrados = todosLosProductos
                    .Where(p => p != null && p.Visible)
                    .Where(p => !string.IsNullOrEmpty(p.Nombre)) // ← AÑADIR ESTA LÍNEA
                    .ToList();

                // Resto del código de filtrado...
                return AplicarFiltrosAdicionales(filtrados);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EXCEPCIÓN en FiltrarProductos: {ex.Message}");
                return new List<Producto>();
            }
        }

        private List<Producto> AplicarFiltrosAdicionales(List<Producto> productos)
        {
            if (productos == null || !productos.Any())
                return new List<Producto>();

            var filtrados = productos;

            // Aplicar filtros existentes con validaciones mejoradas
            if (!string.IsNullOrWhiteSpace(txtFiltroNombre?.Text))
            {
                string filtro = txtFiltroNombre.Text.ToLower();
                filtrados = filtrados.Where(p =>
                    p.Nombre?.ToLower().Contains(filtro) == true).ToList();
            }

            // Filtrar por tipo - con validaciones exhaustivas
            if (cmbFiltroTipo != null && cmbFiltroTipo.SelectedItem != null)
            {
                var comboItem = cmbFiltroTipo.SelectedItem as ComboBoxItem;
                if (comboItem != null)
                {
                    string tipoSeleccionado = comboItem.Content?.ToString();
                    Console.WriteLine($"Tipo seleccionado: {tipoSeleccionado}");

                    if (!string.IsNullOrEmpty(tipoSeleccionado) && tipoSeleccionado != "Todos los tipos")
                    {
                        filtrados = filtrados
                            .Where(p => p?.Tipo != null && p.Tipo == tipoSeleccionado)
                            .ToList();
                        Console.WriteLine($"Después de filtrar por tipo: {filtrados.Count}");
                    }
                }
            }

            // Filtrar por stock - con validaciones exhaustivas
            if (cmbFiltroStock != null && cmbFiltroStock.SelectedItem != null)
            {
                var comboItem = cmbFiltroStock.SelectedItem as ComboBoxItem;
                if (comboItem != null)
                {
                    string stockSeleccionado = comboItem.Content?.ToString();
                    Console.WriteLine($"Stock seleccionado: {stockSeleccionado}");

                    if (!string.IsNullOrEmpty(stockSeleccionado) && stockSeleccionado != "Todos")
                    {
                        switch (stockSeleccionado)
                        {
                            case "Con stock":
                                filtrados = filtrados.Where(p => p?.StockActual > 0).ToList();
                                break;
                            case "Sin stock":
                                filtrados = filtrados.Where(p => p?.StockActual == 0).ToList();
                                break;
                            case "Stock bajo":
                                filtrados = filtrados.Where(p =>
                                    p?.StockActual > 0 &&
                                    p.StockActual <= p.StockMinimo).ToList();
                                break;
                        }
                        Console.WriteLine($"Después de filtrar por stock: {filtrados.Count}");
                    }
                }
            }

            return filtrados;
        }

        private void ActualizarEstadisticas()
        {
            try
            {
                if (todosLosProductos == null)
                {
                    txtTotalProductos.Text = "0";
                    txtConStock.Text = "0";
                    txtSinStock.Text = "0";
                    txtStockBajo.Text = "0";
                    return;
                }

                var productosVisibles = todosLosProductos.Where(p => p != null && p.Visible).ToList();

                txtTotalProductos.Text = productosVisibles.Count.ToString();
                txtConStock.Text = productosVisibles.Count(p => p.StockActual > 0).ToString();
                txtSinStock.Text = productosVisibles.Count(p => p.StockActual == 0).ToString();
                txtStockBajo.Text = productosVisibles.Count(p =>
                    p.StockActual > 0 &&
                    p.StockActual <= p.StockMinimo).ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al actualizar estadísticas: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void DgvProductos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var productoSeleccionado = dgvProductos.SelectedItem as Producto;
                if (productoSeleccionado != null)
                {
                    MostrarDetalleProducto(productoSeleccionado);
                }
                else
                {
                    OcultarDetalleProducto();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al seleccionar producto: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void MostrarDetalleProducto(Producto producto)
        {
            try
            {
                if (producto == null || borderDetalle == null)
                {
                    OcultarDetalleProducto();
                    return;
                }

                borderDetalle.Visibility = Visibility.Visible;

                // Usar valores por defecto seguros
                txtDetalleNombre.Text = $"Producto: {producto.NombreProducto ?? "N/A"}";
                txtDetallePrecio.Text = $"Precio: {producto.PrecioVenta.ToString("C")}";
                txtDetalleStock.Text = $"Stock: {producto.StockActual} (Mín: {producto.StockMinimo})";            
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al mostrar detalle: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void OcultarDetalleProducto()
        {
            try
            {
                if (borderDetalle != null)
                    borderDetalle.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al ocultar detalle: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        #region Event Handlers
        private void btnActualizar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CargarProductos();
                MessageBox.Show("Lista de productos actualizada correctamente.",
                    "Actualización", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al actualizar: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnExportar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "Archivo CSV (*.csv)|*.csv|Archivo de texto (*.txt)|*.txt",
                    FileName = $"Productos_Disponibles_{DateTime.Now:yyyyMMdd_HHmmss}"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    ExportarProductos(saveFileDialog.FileName);
                    MessageBox.Show($"Productos exportados correctamente a: {saveFileDialog.FileName}",
                        "Exportación Exitosa", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al exportar productos: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportarProductos(string filePath)
        {
            try
            {
                var productosExportar = FiltrarProductos();
                using (var writer = new StreamWriter(filePath))
                {
                    writer.WriteLine("ID,Producto,Tipo,PrecioVenta,StockActual,StockMinimo,CostoProduccion,MargenGanancia,Visible,EsProductoBase");
                    foreach (var producto in productosExportar)
                    {
                        if (producto != null)
                        {
                            writer.WriteLine($"{producto.IdProducto},\"{producto.NombreProducto ?? ""}\",\"{producto.Tipo ?? ""}\",{producto.PrecioVenta},{producto.StockActual},{producto.StockMinimo},{producto.CostoProduccion},{producto.MargenGanancia},{producto.Visible},{producto.EsProductoBase}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error en exportación: {ex.Message}");
            }
        }

        private void btnCerrar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnLimpiarFiltros_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtFiltroNombre != null)
                    txtFiltroNombre.Text = "";

                if (cmbFiltroTipo != null && cmbFiltroTipo.Items.Count > 0)
                    cmbFiltroTipo.SelectedIndex = 0;

                if (cmbFiltroStock != null && cmbFiltroStock.Items.Count > 0)
                    cmbFiltroStock.SelectedIndex = 0;

                ActualizarVista();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al limpiar filtros: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void txtFiltro_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                ActualizarVista();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al filtrar: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void cmbFiltro_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                ActualizarVista();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cambiar filtro: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        #endregion
    }
}