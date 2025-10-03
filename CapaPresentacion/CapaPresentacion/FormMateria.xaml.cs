using System;
using System.Windows;
using System.Windows.Controls;
using CapaEntidad;
using CapaNegocio;

namespace CapaPresentacion
{
    public partial class FormMateria : Window
    {
        private bool _suscribirEventos = true;
        private CN_MateriaPrima cnMateriaPrima = new CN_MateriaPrima();

        public FormMateria()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ListarMateriasPrimas();

            if (_suscribirEventos)
            {
                EventAggregator.Subscribe<MateriaPrimaActualizadaEvent>(e => ListarMateriasPrimas());
                _suscribirEventos = false;
            }
        }

        private void btnLimpiar_Click(object sender, RoutedEventArgs e)
        {
            LimpiarCampos();
        }

        private void ListarMateriasPrimas()
        {
            dgvMateriaPrima.ItemsSource = cnMateriaPrima.Listar();
        }

        // MÉTODO MEJORADO: Validaciones más robustas
        private bool ValidarCampos()
        {
            // Validar nombre
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("El nombre es obligatorio.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtNombre.Focus();
                return false;
            }

            // Validar unidad
            if (string.IsNullOrWhiteSpace(txtUnidad.Text))
            {
                MessageBox.Show("La unidad es obligatoria.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtUnidad.Focus();
                return false;
            }

            // Validar cantidad disponible
            if (!float.TryParse(txtCantidad.Text, out float cantidad) || cantidad < 0)
            {
                MessageBox.Show("La cantidad disponible debe ser un número válido mayor o igual a 0.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtCantidad.Focus();
                return false;
            }

            // Validar stock mínimo
            if (!int.TryParse(txtStockMinimo.Text, out int stockMin) || stockMin < 0)
            {
                MessageBox.Show("El stock mínimo debe ser un número entero válido mayor o igual a 0.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtStockMinimo.Focus();
                return false;
            }

            // Validar precio unitario
            if (!decimal.TryParse(txtPrecioUnitario.Text, out decimal precioUnitario) || precioUnitario < 0)
            {
                MessageBox.Show("El precio unitario debe ser un número válido mayor o igual a 0.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtPrecioUnitario.Focus();
                return false;
            }

            return true;
        }

        private void btnAgregar_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidarCampos())
                return;

            MateriaPrima mp = new MateriaPrima
            {
                Nombre = txtNombre.Text.Trim(),
                Unidad = txtUnidad.Text.Trim(),
                CantidadDisponible = float.Parse(txtCantidad.Text),
                StockMinimo = int.Parse(txtStockMinimo.Text),
                PrecioUnitario = decimal.Parse(txtPrecioUnitario.Text)
            };

            string mensaje;
            int id = cnMateriaPrima.Registrar(mp, out mensaje);
            MessageBox.Show(mensaje);

            if (id != 0)
            {
                LimpiarCampos();
                ListarMateriasPrimas();
                EventAggregator.Publish(new MateriaPrimaActualizadaEvent());
                EventAggregator.Publish(new AlertasActualizadasEvent());
            }
        }

        private void btnEditar_Click(object sender, RoutedEventArgs e)
        {
            if (dgvMateriaPrima.SelectedItem is MateriaPrima seleccionado)
            {
                if (!ValidarCampos())
                    return;

                seleccionado.Nombre = txtNombre.Text.Trim();
                seleccionado.Unidad = txtUnidad.Text.Trim();
                seleccionado.CantidadDisponible = float.Parse(txtCantidad.Text);
                seleccionado.StockMinimo = int.Parse(txtStockMinimo.Text);
                seleccionado.PrecioUnitario = decimal.Parse(txtPrecioUnitario.Text);

                string mensaje;
                bool ok = cnMateriaPrima.Editar(seleccionado, out mensaje);
                MessageBox.Show(mensaje);

                if (ok)
                {
                    LimpiarCampos();
                    ListarMateriasPrimas();
                    EventAggregator.Publish(new MateriaPrimaActualizadaEvent());
                    EventAggregator.Publish(new AlertasActualizadasEvent());
                }
            }
            else
            {
                MessageBox.Show("Por favor, seleccione una materia prima para editar.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void btnEliminar_Click(object sender, RoutedEventArgs e)
        {
            if (dgvMateriaPrima.SelectedItem is MateriaPrima seleccionado)
            {
                MessageBoxResult result = MessageBox.Show(
                    $"¿Está seguro de que desea eliminar la materia prima '{seleccionado.Nombre}'?",
                    "Confirmar eliminación",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    string mensaje;
                    bool ok = cnMateriaPrima.Eliminar(seleccionado.IdMateria, out mensaje);
                    MessageBox.Show(mensaje);

                    if (ok)
                    {
                        LimpiarCampos();
                        ListarMateriasPrimas();
                        EventAggregator.Publish(new MateriaPrimaActualizadaEvent());
                        EventAggregator.Publish(new AlertasActualizadasEvent());
                    }
                }
            }
            else
            {
                MessageBox.Show("Por favor, seleccione una materia prima para eliminar.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void dgvMateriaPrima_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgvMateriaPrima.SelectedItem is MateriaPrima mp)
            {
                txtNombre.Text = mp.Nombre;
                txtUnidad.Text = mp.Unidad;
                txtCantidad.Text = mp.CantidadDisponible.ToString();
                txtStockMinimo.Text = mp.StockMinimo.ToString();
                txtPrecioUnitario.Text = mp.PrecioUnitario.ToString("0.00");
            }
        }

        private void LimpiarCampos()
        {
            txtNombre.Text = "";
            txtUnidad.Text = "";
            txtCantidad.Text = "";
            txtStockMinimo.Text = "";
            txtPrecioUnitario.Text = "";
            dgvMateriaPrima.SelectedItem = null;
        }
    }
}