using System;
using System.Windows;
using System.Windows.Controls;
using CapaEntidad;
using CapaNegocio;

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
        }

        private void ListarProductos()
        {
            dgvProductos.ItemsSource = cnProducto.Listar();
        }

        private void btnAgregar_Click(object sender, RoutedEventArgs e)
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

            Producto p = new Producto
            {
                Nombre = txtNombre.Text,
                Tipo = txtTipo.Text,
                CostoProduccion = costo,
                MargenGanancia = margen,
                PrecioVenta = precio,
                StockActual = stock,
                StockMinimo = stockMin,
                Visible = chkVisible.IsChecked ?? true
            };

            string mensaje;
            int id = cnProducto.Registrar(p, out mensaje);
            MessageBox.Show(mensaje);
            if (id != 0) ListarProductos();
        }

        private void btnEditar_Click(object sender, RoutedEventArgs e)
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
                seleccionado.Tipo = txtTipo.Text;
                seleccionado.CostoProduccion = costo;
                seleccionado.MargenGanancia = margen;
                seleccionado.PrecioVenta = precio;
                seleccionado.StockActual = stock;
                seleccionado.StockMinimo = stockMin;
                seleccionado.Visible = chkVisible.IsChecked ?? true;

                string mensaje;
                bool ok = cnProducto.Editar(seleccionado, out mensaje);
                MessageBox.Show(mensaje);
                if (ok) ListarProductos();
            }
        }

        private void btnEliminar_Click(object sender, RoutedEventArgs e)
        {
            if (dgvProductos.SelectedItem is Producto seleccionado)
            {
                string mensaje;
                bool ok = cnProducto.Eliminar(seleccionado.IdProducto, out mensaje);
                MessageBox.Show(mensaje);
                if (ok) ListarProductos();
            }
        }

        private void dgvProductos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgvProductos.SelectedItem is Producto p)
            {
                txtNombre.Text = p.Nombre;
                txtTipo.Text = p.Tipo;
                txtCostoProduccion.Text = p.CostoProduccion.ToString("0.00");
                txtMargenGanancia.Text = p.MargenGanancia.ToString("0.00");
                txtPrecio.Text = p.PrecioVenta.ToString("0.00");
                txtStock.Text = p.StockActual.ToString();
                txtStockMinimo.Text = p.StockMinimo.ToString();
                chkVisible.IsChecked = p.Visible;
            }
        }
    }
}
