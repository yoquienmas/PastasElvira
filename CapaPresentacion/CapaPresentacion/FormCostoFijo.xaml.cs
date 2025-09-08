using CapaEntidad;
using CapaNegocio;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace CapaPresentacion
{
    public partial class FormCostoFijo : Window
    {
        private CN_CostoFijo cnCostoFijo = new CN_CostoFijo();
        private CN_Producto cnProducto = new CN_Producto();
        private List<CostoFijo> listaCostos;
        private CostoFijo costoSelccionado = null;

        public FormCostoFijo()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CargarCostosFijos();
            LimpiarFormulario();
            ActualizarEstadisticas();
        }

        private void CargarCostosFijos()
        {
            listaCostos = cnCostoFijo.Listar();
            dgvCostosFijos.ItemsSource = listaCostos;
        }

        private void ActualizarEstadisticas()
        {
            decimal totalCostos = 0;
            decimal costosActivos = 0;
            int cantidadActivos = 0;

            foreach (var costo in listaCostos)
            {
                totalCostos += costo.Monto;
                if (costo.Activo)
                {
                    costosActivos += costo.Monto;
                    cantidadActivos++;
                }
            }

            txtTotalCostos.Text = $"Total Costos: {totalCostos:C} ({listaCostos.Count} conceptos)";
            txtCostosActivos.Text = $"Costos Activos: {costosActivos:C} ({cantidadActivos} activos)";

            txtImpactoInfo.Text = $"💡 Los costos activos se distribuyen entre todos los productos afectando el precio final.";
        }

        private void LimpiarFormulario()
        {
            costoSelccionado = null;
            txtConcepto.Text = "";
            txtMonto.Text = "";
            chkActivo.IsChecked = true;
            btnGuardar.Content = "Guardar";
            btnEliminar.IsEnabled = false;
        }

        private void btnNuevo_Click(object sender, RoutedEventArgs e)
        {
            LimpiarFormulario();
        }

        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            // Validaciones
            if (string.IsNullOrWhiteSpace(txtConcepto.Text))
            {
                MessageBox.Show("El concepto es obligatorio", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(txtMonto.Text, out decimal monto) || monto <= 0)
            {
                MessageBox.Show("Ingrese un monto válido mayor a cero", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            CostoFijo costo = new CostoFijo
            {
                Concepto = txtConcepto.Text.Trim(),
                Monto = monto,
                Activo = chkActivo.IsChecked ?? true
            };

            // Si hay costo seleccionado, es una edición
            if (costoSelccionado != null)
            {
                costo.IdCosto = costoSelccionado.IdCosto;
            }

            string mensaje;
            bool resultado;

            if (costoSelccionado == null)
            {
                resultado = cnCostoFijo.Registrar(costo, out mensaje);
            }
            else
            {
                resultado = cnCostoFijo.Editar(costo, out mensaje);
            }

            if (resultado)
            {
                MessageBox.Show(mensaje, "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                CargarCostosFijos();
                ActualizarEstadisticas();
                LimpiarFormulario();

                // Si se activó/desactivó un costo, recalcular productos
                if (costoSelccionado != null && costoSelccionado.Activo != costo.Activo)
                {
                    RecalcularProductos();
                }
            }
            else
            {
                MessageBox.Show(mensaje, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnEliminar_Click(object sender, RoutedEventArgs e)
        {
            if (costoSelccionado == null)
            {
                MessageBox.Show("Seleccione un costo para eliminar", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var confirmacion = MessageBox.Show($"¿Está seguro de eliminar el costo: '{costoSelccionado.Concepto}'?",
                "Confirmar Eliminación", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (confirmacion == MessageBoxResult.Yes)
            {
                string mensaje;
                bool resultado = cnCostoFijo.Eliminar(costoSelccionado.IdCosto, out mensaje);

                MessageBox.Show(mensaje, resultado ? "Éxito" : "Error",
                    MessageBoxButton.OK, resultado ? MessageBoxImage.Information : MessageBoxImage.Error);

                if (resultado)
                {
                    CargarCostosFijos();
                    ActualizarEstadisticas();
                    LimpiarFormulario();
                    RecalcularProductos();
                }
            }
        }

        private void dgvCostosFijos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgvCostosFijos.SelectedItem is CostoFijo costo)
            {
                costoSelccionado = costo;
                txtConcepto.Text = costo.Concepto;
                txtMonto.Text = costo.Monto.ToString("F2");
                chkActivo.IsChecked = costo.Activo;
                btnGuardar.Content = "Actualizar";
                btnEliminar.IsEnabled = true;
            }
        }

        private void BtnToggleEstado_Click(object sender, RoutedEventArgs e)
        {
            if (dgvCostosFijos.SelectedItem is CostoFijo costo)
            {
                costo.Activo = !costo.Activo;

                string mensaje;
                bool resultado = cnCostoFijo.Editar(costo, out mensaje);

                if (resultado)
                {
                    MessageBox.Show($"Costo {(costo.Activo ? "activado" : "desactivado")} correctamente",
                                    "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    CargarCostosFijos();
                    ActualizarEstadisticas();
                    RecalcularProductos();
                }
                else
                {
                    MessageBox.Show(mensaje, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnRecalcularCostos_Click(object sender, RoutedEventArgs e)
        {
            var confirmacion = MessageBox.Show("¿Está seguro de recalcular todos los precios? Esto actualizará el costo y precio de todos los productos basado en los costos actuales.",
                "Confirmar Recalculo", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (confirmacion == MessageBoxResult.Yes)
            {
                RecalcularProductos();
                MessageBox.Show("Precios recalculados correctamente para todos los productos", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void RecalcularProductos()
        {
            try
            {
                var productos = cnProducto.Listar();
                int productosActualizados = 0;

                foreach (var producto in productos)
                {
                    decimal costo, precio;
                    string mensaje;
                    bool ok = cnProducto.CalcularCostoProducto(producto.IdProducto, out costo, out precio, out mensaje);

                    if (ok) productosActualizados++;
                }

                txtImpactoInfo.Text = $"✅ {productosActualizados} productos actualizados con los nuevos costos";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al recalcular productos: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}