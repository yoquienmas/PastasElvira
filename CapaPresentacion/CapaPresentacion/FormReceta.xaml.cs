using CapaEntidad;
using CapaNegocio;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace CapaPresentacion
{
    public partial class FormReceta : Window
    {
        private CN_Producto cnProducto = new CN_Producto();
        private CN_MateriaPrima cnMateriaPrima = new CN_MateriaPrima();
        private CN_Produccion cnProduccion = new CN_Produccion();

        private List<DetalleReceta> recetaActual = new List<DetalleReceta>();

        public FormReceta()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CargarProductos();
            CargarMateriasPrimas();
        }

        private void CargarProductos()
        {
            cboProductos.ItemsSource = cnProducto.Listar();
        }

        private void CargarMateriasPrimas()
        {
            cboMateriasPrimas.ItemsSource = cnMateriaPrima.Listar();
        }

        private void cboProductos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboProductos.SelectedItem is Producto producto)
            {
                CargarRecetaProducto(producto.IdProducto);
            }
        }

        private void cboMateriasPrimas_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboMateriasPrimas.SelectedItem is MateriaPrima materia)
            {
                txtUnidad.Text = materia.Unidad;
            }
        }

        private void CargarRecetaProducto(int idProducto)
        {
            recetaActual = cnProduccion.ListarRecetaPorProducto(idProducto);
            dgvReceta.ItemsSource = recetaActual;
        }

        private void btnAgregar_Click(object sender, RoutedEventArgs e)
        {
            if (cboProductos.SelectedItem == null)
            {
                MessageBox.Show("Seleccione un producto primero.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (cboMateriasPrimas.SelectedItem == null)
            {
                MessageBox.Show("Seleccione una materia prima.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!float.TryParse(txtCantidadNecesaria.Text, out float cantidad) || cantidad <= 0)
            {
                MessageBox.Show("Ingrese una cantidad válida.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var producto = (Producto)cboProductos.SelectedItem;
            var materia = (MateriaPrima)cboMateriasPrimas.SelectedItem;

            // Agregar a la receta
            bool resultado = cnProduccion.AgregarMateriaPrimaAReceta(producto.IdProducto, materia.IdMateria, cantidad);

            if (resultado)
            {
                MessageBox.Show("Materia prima agregada a la receta.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                CargarRecetaProducto(producto.IdProducto);
                txtCantidadNecesaria.Text = "";
                cboMateriasPrimas.SelectedIndex = -1;
                decimal costo, precio;
                string mensajeCosto;
                bool ok = cnProducto.CalcularCostoProducto(producto.IdProducto, out costo, out precio, out mensajeCosto);

                if (ok)
                {
                    MessageBox.Show($"Materia prima agregada. Nuevo costo: {costo:C}, Precio sugerido: {precio:C}",
                                    "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Materia prima agregada", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                CargarRecetaProducto(producto.IdProducto);
            }
            else
            {
                MessageBox.Show("Error al agregar la materia prima.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnEliminar_Click(object sender, RoutedEventArgs e)
        {
            if (dgvReceta.SelectedItem is DetalleReceta item && cboProductos.SelectedItem is Producto producto)
            {
                bool resultado = cnProduccion.EliminarMateriaPrimaDeReceta(item.IdProductoMateriaPrima);

                if (resultado)
                {
                    MessageBox.Show("Materia prima eliminada de la receta.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    CargarRecetaProducto(producto.IdProducto);
                }
                else
                {
                    MessageBox.Show("Error al eliminar la materia prima.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}