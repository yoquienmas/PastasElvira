using CapaDatos;
using CapaEntidad;
using CapaNegocio;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace CapaPresentacion
{
    public partial class FormProduccion : Window
    {
        private CN_Produccion cnProduccion = new CN_Produccion();
        private CN_Producto cnProducto = new CN_Producto();
        private Produccion produccionSeleccionada;
        private bool modoEdicion = false;

        public FormProduccion()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CargarProductos();
            CargarHistorial();
            ActualizarInterfaz();
        }

        private void btnHistorial_Click(object sender, RoutedEventArgs e)
        {
            FormHistorialProduccion formHistorial = new FormHistorialProduccion();
            formHistorial.Show();
        }

        private void CargarProductos()
        {
            try
            {
                var listaProductos = cnProducto.Listar();
                cboProductos.ItemsSource = listaProductos;
                cboProductos.DisplayMemberPath = "NombreProducto";
                cboProductos.SelectedValuePath = "IdProducto";

                if (listaProductos.Count == 0)
                {
                    MessageBox.Show("No hay productos disponibles para producción.", "Información",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar productos: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CargarHistorial()
        {
            try
            {
                dgvProducciones.ItemsSource = cnProduccion.Listar();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar historial: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnRegistrar_Click(object sender, RoutedEventArgs e)
        {
            if (modoEdicion)
            {
                MessageBox.Show("Termine la edición actual antes de registrar una nueva producción.", "Advertencia",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!ValidarCampos()) return;

            try
            {
                int idProducto = (int)cboProductos.SelectedValue;
                var productoSeleccionado = cboProductos.SelectedItem as Producto;
                int cantidad = int.Parse(txtCantidad.Text);

                // ✅ ELIMINADA la validación de materias primas - PRODUCCIÓN DIRECTA

                // Confirmar producción con información del stock
                string mensajeConfirmacion = $"¿Está seguro que desea producir {cantidad} unidades de {productoSeleccionado.NombreProducto}?\n\n" +
                                           $"✅ Se agregarán {cantidad} unidades al stock actual del producto.";

                if (MessageBox.Show(mensajeConfirmacion, "Confirmar Producción",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                {
                    return;
                }

                Produccion nuevaProduccion = new Produccion
                {
                    IdProducto = idProducto,
                    CantidadProducida = cantidad
                };

                string mensaje;
                int idProduccion = cnProduccion.Registrar(nuevaProduccion, out mensaje);

                if (idProduccion > 0)
                {
                    MessageBox.Show($"✅ Producción registrada exitosamente!\n\n" +
                                  $"Producto: {productoSeleccionado.NombreProducto}\n" +
                                  $"Cantidad: {cantidad} unidades\n\n" +
                                  $"Stock actualizado automáticamente.",
                                  "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                    LimpiarCampos();
                    CargarHistorial();
                }
                else
                {
                    MessageBox.Show(mensaje, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al registrar producción: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnEditar_Click(object sender, RoutedEventArgs e)
        {
            if (!modoEdicion)
            {
                // Iniciar modo edición
                if (produccionSeleccionada == null)
                {
                    MessageBox.Show("Seleccione una producción para editar.", "Advertencia",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                modoEdicion = true;
                CargarDatosEdicion();
                ActualizarInterfaz();
            }
            else
            {
                // Guardar cambios
                if (!ValidarCampos()) return;

                try
                {
                    int idProducto = (int)cboProductos.SelectedValue;
                    var productoSeleccionado = cboProductos.SelectedItem as Producto;
                    int cantidad = int.Parse(txtCantidad.Text);

                    // ✅ ELIMINADA la validación de materias primas

                    // Confirmar edición
                    string mensajeConfirmacion = $"¿Está seguro que desea actualizar la producción?\n\n" +
                                               $"Producto: {productoSeleccionado.NombreProducto}\n" +
                                               $"Cantidad: {produccionSeleccionada.CantidadProducida} → {cantidad} unidades";

                    if (MessageBox.Show(mensajeConfirmacion, "Confirmar Edición",
                        MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                    {
                        return;
                    }

                    Produccion produccionActualizada = new Produccion
                    {
                        IdProduccion = produccionSeleccionada.IdProduccion,
                        IdProducto = idProducto,
                        CantidadProducida = cantidad
                    };

                    string mensaje;
                    bool resultado = cnProduccion.Actualizar(produccionActualizada, out mensaje);

                    if (resultado)
                    {
                        MessageBox.Show($"✅ Producción actualizada exitosamente!", "Éxito",
                                      MessageBoxButton.OK, MessageBoxImage.Information);

                        modoEdicion = false;
                        produccionSeleccionada = null;
                        LimpiarCampos();
                        CargarHistorial();
                        ActualizarInterfaz();
                    }
                    else
                    {
                        MessageBox.Show(mensaje, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al actualizar producción: {ex.Message}", "Error",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnEliminar_Click(object sender, RoutedEventArgs e)
        {
            if (produccionSeleccionada == null)
            {
                MessageBox.Show("Seleccione una producción para eliminar.", "Advertencia",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (modoEdicion)
            {
                MessageBox.Show("Termine la edición actual antes de eliminar.", "Advertencia",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                string mensajeConfirmacion = $"¿Está seguro que desea eliminar la producción?\n\n" +
                                           $"ID: {produccionSeleccionada.IdProduccion}\n" +
                                           $"Producto: {produccionSeleccionada.NombreProducto}\n" +
                                           $"Cantidad: {produccionSeleccionada.CantidadProducida} unidades\n\n" +
                                           $"✅ El stock del producto se ajustará automáticamente.";

                if (MessageBox.Show(mensajeConfirmacion, "Confirmar Eliminación",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                {
                    return;
                }

                string mensaje;
                bool resultado = cnProduccion.Eliminar(produccionSeleccionada.IdProduccion, out mensaje);

                if (resultado)
                {
                    MessageBox.Show($"✅ Producción eliminada exitosamente!", "Éxito",
                                  MessageBoxButton.OK, MessageBoxImage.Information);

                    produccionSeleccionada = null;
                    LimpiarCampos();
                    CargarHistorial();
                    ActualizarInterfaz();
                }
                else
                {
                    MessageBox.Show(mensaje, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al eliminar producción: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            modoEdicion = false;
            produccionSeleccionada = null;
            LimpiarCampos();
            ActualizarInterfaz();
        }

        private void dgvProducciones_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (dgvProducciones.SelectedItem is Produccion produccion)
            {
                produccionSeleccionada = produccion;
                ActualizarInterfaz();
            }
        }

        private void CargarDatosEdicion()
        {
            if (produccionSeleccionada != null)
            {
                // Seleccionar el producto en el ComboBox
                cboProductos.SelectedValue = produccionSeleccionada.IdProducto;
                txtCantidad.Text = produccionSeleccionada.CantidadProducida.ToString();
            }
        }

        private void ActualizarInterfaz()
        {
            // Habilitar/deshabilitar controles según el modo
            btnRegistrar.IsEnabled = !modoEdicion;
            btnEditar.Content = modoEdicion ? "💾 Guardar" : "✏️ Editar";
            btnEliminar.IsEnabled = !modoEdicion && produccionSeleccionada != null;
            btnCancelar.IsEnabled = modoEdicion;

            // Cambiar colores según el modo
            if (modoEdicion)
            {
                btnEditar.Background = new SolidColorBrush(Color.FromRgb(76, 175, 80)); // Verde
                BorderPrincipal.BorderBrush = new SolidColorBrush(Color.FromRgb(255, 152, 0)); // Naranja
            }
            else
            {
                btnEditar.Background = new SolidColorBrush(Color.FromRgb(255, 152, 0)); // Naranja
                BorderPrincipal.BorderBrush = new SolidColorBrush(Color.FromRgb(226, 164, 45)); // Original
            }
        }

        private bool ValidarCampos()
        {
            if (cboProductos.SelectedItem == null)
            {
                MessageBox.Show("Seleccione un producto.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                cboProductos.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtCantidad.Text))
            {
                MessageBox.Show("Ingrese la cantidad a producir.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtCantidad.Focus();
                return false;
            }

            if (!int.TryParse(txtCantidad.Text, out int cantidad) || cantidad <= 0)
            {
                MessageBox.Show("La cantidad debe ser un número mayor a 0.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtCantidad.SelectAll();
                txtCantidad.Focus();
                return false;
            }

            return true;
        }

        private void LimpiarCampos()
        {
            cboProductos.SelectedIndex = -1;
            txtCantidad.Text = "";
            cboProductos.Focus();
        }

        private void txtCantidad_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Solo permitir números
            if (!char.IsDigit(e.Text, 0))
            {
                e.Handled = true;
            }
        }
    }
}