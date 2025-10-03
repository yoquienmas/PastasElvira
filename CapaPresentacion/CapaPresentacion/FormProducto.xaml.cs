using CapaEntidad;
using CapaNegocio;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace CapaPresentacion
{
    public partial class FormProducto : Window
    {
        private CN_Producto cnProducto = new CN_Producto();
        private Producto productoSeleccionado = null;
        private List<Producto> listaProductos;

        public FormProducto()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CargarProductos();
            CargarTiposProducto();
            LimpiarFormulario();
            CalcularCostoYActualizarPrecio(); // Calcular costo automáticamente al cargar
        }

        private void CargarProductos()
        {
            listaProductos = cnProducto.Listar();
            dgvProductos.ItemsSource = listaProductos;
        }

        private void CargarTiposProducto()
        {
            try
            {
                var tipos = cnProducto.ListarTiposProducto();
                cboTipo.ItemsSource = tipos;
                if (tipos.Count > 0)
                    cboTipo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar tipos: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LimpiarFormulario()
        {
            productoSeleccionado = null;
            txtNombre.Text = "";
            txtStock.Text = "";
            txtStockMinimo.Text = "";
            txtMargenGanancia.Text = "30"; // Margen por defecto
            chkVisible.IsChecked = true;

            if (cboTipo.Items.Count > 0)
                cboTipo.SelectedIndex = 0;

            btnAgregar.Content = "➕ Agregar";
            btnEditar.IsEnabled = false;
            btnEliminar.IsEnabled = false;

            // Calcular costo automáticamente al limpiar
            CalcularCostoYActualizarPrecio();
        }

        private void txtMargenGanancia_TextChanged(object sender, TextChangedEventArgs e)
        {
            CalcularPrecioAutomatico();
        }

        private void CalcularPrecioAutomatico()
        {
            try
            {
                if (decimal.TryParse(txtCostoProduccion.Text, out decimal costo) &&
                    decimal.TryParse(txtMargenGanancia.Text, out decimal margen))
                {
                    // Calcular precio automáticamente
                    decimal precio = costo * (1 + (margen / 100));
                    txtPrecioVenta.Text = precio.ToString("F2");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en cálculo automático: {ex.Message}");
            }
        }

        private void CalcularCostoYActualizarPrecio()
        {
            try
            {
                // Calcular el costo automáticamente usando el método existente
                decimal costosFijosUnitarios = cnProducto.CalcularCostosFijosUnitarios();

                // Actualizar el campo de costo
                txtCostoProduccion.Text = costosFijosUnitarios.ToString("F2");

                // Calcular precio basado en el margen
                CalcularPrecioAutomatico();
            }
            catch (Exception ex)
            {
                // Si hay error, establecer valores por defecto
                txtCostoProduccion.Text = "0";
                txtPrecioVenta.Text = "0";
                System.Diagnostics.Debug.WriteLine($"Error al calcular costo: {ex.Message}");
            }
        }

        private void btnRecalcularCosto_Click(object sender, RoutedEventArgs e)
        {
            CalcularCostoYActualizarPrecio();
            MessageBox.Show("Costo recalculado correctamente", "Éxito",
                          MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void btnAgregar_Click(object sender, RoutedEventArgs e)
        {
            GuardarProducto(false);
        }

        private void btnEditar_Click(object sender, RoutedEventArgs e)
        {
            GuardarProducto(true);
        }

        private void GuardarProducto(bool esEdicion)
        {
            // Validaciones básicas
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("El nombre es obligatorio", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(cboTipo.Text))
            {
                MessageBox.Show("El tipo es obligatorio", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validar campos numéricos
            if (!decimal.TryParse(txtCostoProduccion.Text, out decimal costo) || costo < 0)
            {
                MessageBox.Show("El costo de producción debe ser un valor válido", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(txtPrecioVenta.Text, out decimal precio) || precio < 0)
            {
                MessageBox.Show("El precio de venta debe ser un valor válido", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(txtMargenGanancia.Text, out decimal margen) || margen < 0)
            {
                MessageBox.Show("El margen de ganancia debe ser un porcentaje válido", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(txtStock.Text, out int stock) || stock < 0)
            {
                MessageBox.Show("El stock actual debe ser un número válido", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(txtStockMinimo.Text, out int stockMinimo) || stockMinimo < 0)
            {
                MessageBox.Show("El stock mínimo debe ser un número válido", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Crear el objeto producto
            Producto producto = new Producto
            {
                Nombre = txtNombre.Text.Trim(),
                Tipo = cboTipo.Text,
                CostoProduccion = costo,
                PrecioVenta = precio,
                MargenGanancia = margen,
                StockActual = stock,
                StockMinimo = stockMinimo,
                Visible = chkVisible.IsChecked ?? true
            };

            // Si es edición, asignar el ID
            if (esEdicion && productoSeleccionado != null)
            {
                producto.IdProducto = productoSeleccionado.IdProducto;
            }

            string mensaje;
            bool resultado;

            if (esEdicion)
            {
                resultado = cnProducto.Editar(producto, out mensaje);
            }
            else
            {
                resultado = cnProducto.Registrar(producto, out mensaje);
            }

            if (resultado)
            {
                MessageBox.Show(mensaje, "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                CargarProductos();
                LimpiarFormulario();
            }
            else
            {
                MessageBox.Show(mensaje, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnEliminar_Click(object sender, RoutedEventArgs e)
        {
            if (productoSeleccionado == null)
            {
                MessageBox.Show("Seleccione un producto para eliminar", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var confirmacion = MessageBox.Show(
                $"¿Está seguro de eliminar el producto: '{productoSeleccionado.Nombre}'?",
                "Confirmar Eliminación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirmacion == MessageBoxResult.Yes)
            {
                string mensaje;
                bool resultado = cnProducto.Eliminar(productoSeleccionado.IdProducto, out mensaje);

                MessageBox.Show(mensaje, resultado ? "Éxito" : "Error",
                    MessageBoxButton.OK,
                    resultado ? MessageBoxImage.Information : MessageBoxImage.Error);

                if (resultado)
                {
                    CargarProductos();
                    LimpiarFormulario();
                }
            }
        }

        private void btnLimpiar_Click(object sender, RoutedEventArgs e)
        {
            LimpiarFormulario();
        }

        private void dgvProductos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgvProductos.SelectedItem is Producto producto)
            {
                productoSeleccionado = producto;
                txtNombre.Text = producto.Nombre;
                cboTipo.Text = producto.Tipo;
                txtCostoProduccion.Text = producto.CostoProduccion.ToString("F2");
                txtPrecioVenta.Text = producto.PrecioVenta.ToString("F2");
                txtMargenGanancia.Text = producto.MargenGanancia.ToString("F2");
                txtStock.Text = producto.StockActual.ToString();
                txtStockMinimo.Text = producto.StockMinimo.ToString();
                chkVisible.IsChecked = producto.Visible;

                btnAgregar.Content = "➕ Agregar";
                btnEditar.IsEnabled = true;
                btnEliminar.IsEnabled = true;
            }
        }

        private void cboTipo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Puedes agregar lógica adicional si es necesario
        }
    }
}
