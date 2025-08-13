using System;
using System.Windows;
using System.Windows.Controls;
using CapaEntidad;
using CapaNegocio;

namespace CapaPresentacion
{
    public partial class FormProducto : Window
    {
        private CN_Producto cnProducto = new CN_Producto();

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
            if (!float.TryParse(txtPrecio.Text, out float precio) || !int.TryParse(txtStock.Text, out int stock))
            {
                MessageBox.Show("Precio o stock inválido.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Producto p = new Producto
            {
                Nombre = txtNombre.Text,
                Tipo = txtTipo.Text,
                PrecioVenta = precio,
                StockActual = stock,
                StockMinimo = 5,
                Visible = true
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
                if (!float.TryParse(txtPrecio.Text, out float precio) || !int.TryParse(txtStock.Text, out int stock))
                {
                    MessageBox.Show("Precio o stock inválido.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                seleccionado.Nombre = txtNombre.Text;
                seleccionado.Tipo = txtTipo.Text;
                seleccionado.PrecioVenta = precio;
                seleccionado.StockActual = stock;

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
                txtPrecio.Text = p.PrecioVenta.ToString();
                txtStock.Text = p.StockActual.ToString();
            }
        }
    }
}
