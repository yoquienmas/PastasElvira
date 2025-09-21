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
                try
                {
                    cboProductos.ItemsSource = cnProducto.Listar();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al cargar productos: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            private void CargarMateriasPrimas()
            {
                try
                {
                    cboMateriasPrimas.ItemsSource = cnMateriaPrima.Listar();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al cargar materias primas: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            private void cboProductos_SelectionChanged(object sender, SelectionChangedEventArgs e)
            {
                try
                {
                    if (cboProductos.SelectedItem is Producto producto)
                    {
                        CargarRecetaProducto(producto.IdProducto);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al cargar receta: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            private void cboMateriasPrimas_SelectionChanged(object sender, SelectionChangedEventArgs e)
            {
                try
                {
                    if (cboMateriasPrimas.SelectedItem is MateriaPrima materia)
                    {
                        txtUnidad.Text = materia.Unidad;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al cargar unidad: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            private void CargarRecetaProducto(int idProducto)
            {
                try
                {
                    recetaActual = cnProduccion.ListarRecetaPorProducto(idProducto);
                    dgvReceta.ItemsSource = recetaActual;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al cargar receta: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            private void btnAgregar_Click(object sender, RoutedEventArgs e)
            {
                try
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

                    bool resultado = cnProduccion.AgregarMateriaPrimaAReceta(producto.IdProducto, materia.IdMateria, cantidad);

                    if (resultado)
                    {
                        MessageBox.Show("Materia prima agregada a la receta.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                        CargarRecetaProducto(producto.IdProducto);
                        txtCantidadNecesaria.Text = "";
                        cboMateriasPrimas.SelectedIndex = -1;

                        // Publicar eventos
                        EventAggregator.Publish(new RecetaActualizadaEvent { IdProducto = producto.IdProducto });
                        EventAggregator.Publish(new ProductoActualizadoEvent());
                        EventAggregator.Publish(new AlertasActualizadasEvent());
                    }
                    else
                    {
                        MessageBox.Show("Error al agregar la materia prima.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al agregar a receta: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            private void BtnEliminar_Click(object sender, RoutedEventArgs e)
            {
                try
                {
                    if (dgvReceta.SelectedItem is DetalleReceta item && cboProductos.SelectedItem is Producto producto)
                    {
                        bool resultado = cnProduccion.EliminarMateriaPrimaDeReceta(item.IdProductoMateriaPrima);

                        if (resultado)
                        {
                            MessageBox.Show("Materia prima eliminada de la receta.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                            CargarRecetaProducto(producto.IdProducto);

                            // Publicar eventos
                            EventAggregator.Publish(new RecetaActualizadaEvent { IdProducto = producto.IdProducto });
                            EventAggregator.Publish(new ProductoActualizadoEvent());
                            EventAggregator.Publish(new AlertasActualizadasEvent());
                        }
                        else
                        {
                            MessageBox.Show("Error al eliminar la materia prima.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al eliminar de receta: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
