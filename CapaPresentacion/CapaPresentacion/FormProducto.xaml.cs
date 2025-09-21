using CapaDatos;
using CapaEntidad;
using CapaNegocio;
using System;
using System.Windows;
using System.Windows.Controls;

    namespace CapaPresentacion
    {
        public partial class FormProducto : Window
        {
            private readonly CN_Producto cnProducto = new CN_Producto();

            public FormProducto()
            {
                InitializeComponent();
            }

            private void Window_Loaded(object sender, RoutedEventArgs e)
            {
                ListarProductos();
                CargarTiposProducto();
            }

            private void btnLimpiar_Click(object sender, RoutedEventArgs e)
            {
                LimpiarCampos();
            }

            private void LimpiarCampos()
            {
                txtNombre.Text = "";
                cboTipo.Text = "";
                txtCostoProduccion.Text = "";
                txtMargenGanancia.Text = "";
                txtPrecio.Text = "";
                txtStock.Text = "";
                txtStockMinimo.Text = "";
                chkVisible.IsChecked = true;
                dgvProductos.SelectedItem = null;
            }

            private void ListarProductos()
            {
                try
                {
                    dgvProductos.ItemsSource = cnProducto.Listar();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al cargar productos: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

        private void CargarTiposProducto()
        {
            try
            {
                cboTipo.ItemsSource = cnProducto.ListarTiposProducto();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar tipos de producto: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnAgregar_Click(object sender, RoutedEventArgs e)
            {
                try
                {
                    if (string.IsNullOrEmpty(txtNombre.Text))
                    {
                        MessageBox.Show("El nombre es obligatorio.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    var producto = new Producto
                    {
                        Nombre = txtNombre.Text,
                        Tipo = cboTipo.SelectedValue?.ToString() ?? cboTipo.Text,
                        CostoProduccion = 0,
                        MargenGanancia = 0,
                        PrecioVenta = 0,
                        StockActual = 0,
                        StockMinimo = string.IsNullOrEmpty(txtStockMinimo.Text) ? 0 : int.Parse(txtStockMinimo.Text),
                        Visible = chkVisible.IsChecked ?? true
                    };

                string mensaje;
                bool ok = cnProducto.Registrar(producto, out mensaje);
                MessageBox.Show(mensaje);

                if (ok)
                {
                    ListarProductos();
                    LimpiarCampos();
                    EventAggregator.Publish(new ProductoActualizadoEvent());
                }
            }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al agregar producto: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            private void btnEditar_Click(object sender, RoutedEventArgs e)
            {
                try
                {
                    if (dgvProductos.SelectedItem is Producto seleccionado)
                    {
                        if (!decimal.TryParse(txtCostoProduccion.Text, out decimal costo) ||
                            !decimal.TryParse(txtMargenGanancia.Text, out decimal margen) ||
                            !decimal.TryParse(txtPrecio.Text, out decimal precio) ||
                            !int.TryParse(txtStock.Text, out int stock) ||
                            !int.TryParse(txtStockMinimo.Text, out int stockMin))
                        {
                            MessageBox.Show("Verifique los valores numéricos.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        seleccionado.Nombre = txtNombre.Text;
                        seleccionado.Tipo = cboTipo.Text;
                        seleccionado.CostoProduccion = costo;
                        seleccionado.MargenGanancia = margen;
                        seleccionado.PrecioVenta = precio;
                        seleccionado.StockActual = stock;
                        seleccionado.StockMinimo = stockMin;
                        seleccionado.Visible = chkVisible.IsChecked ?? true;

                        string mensaje;
                        bool ok = cnProducto.Editar(seleccionado, out mensaje);
                        MessageBox.Show(mensaje);

                        if (ok)
                        {
                            ListarProductos();
                            LimpiarCampos();
                            EventAggregator.Publish(new ProductoActualizadoEvent());
                            EventAggregator.Publish(new AlertasActualizadasEvent());
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al editar producto: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            private void btnEliminar_Click(object sender, RoutedEventArgs e)
            {
                try
                {
                    if (dgvProductos.SelectedItem is Producto seleccionado)
                    {
                        var confirmacion = MessageBox.Show($"¿Está seguro de eliminar el producto: {seleccionado.Nombre}?",
                            "Confirmar Eliminación", MessageBoxButton.YesNo, MessageBoxImage.Question);

                        if (confirmacion == MessageBoxResult.Yes)
                        {
                            string mensaje;
                            bool ok = cnProducto.Eliminar(seleccionado.IdProducto, out mensaje);
                            MessageBox.Show(mensaje);

                            if (ok)
                            {
                                ListarProductos();
                                LimpiarCampos();
                                EventAggregator.Publish(new ProductoActualizadoEvent());
                                EventAggregator.Publish(new AlertasActualizadasEvent());
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al eliminar producto: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            private void dgvProductos_SelectionChanged(object sender, SelectionChangedEventArgs e)
            {
                try
                {
                    if (dgvProductos.SelectedItem is Producto producto)
                    {
                        txtNombre.Text = producto.Nombre;
                        cboTipo.Text = producto.Tipo;
                        txtCostoProduccion.Text = producto.CostoProduccion.ToString("0.00");
                        txtMargenGanancia.Text = producto.MargenGanancia.ToString("0.00");
                        txtPrecio.Text = producto.PrecioVenta.ToString("0.00");
                        txtStock.Text = producto.StockActual.ToString();
                        txtStockMinimo.Text = producto.StockMinimo.ToString();
                        chkVisible.IsChecked = producto.Visible;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al cargar datos del producto: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            private void cboTipo_SelectionChanged(object sender, SelectionChangedEventArgs e)
            {
                // Lógica opcional si es necesaria
            }
        }
    }