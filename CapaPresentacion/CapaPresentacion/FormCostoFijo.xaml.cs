using CapaEntidad;
using CapaNegocio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace CapaPresentacion
{
        public partial class FormCostoFijo : Window
        {
            private bool _suscribirEventos = true;
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

                // Suscribirse a eventos
                if (_suscribirEventos)
                {
                    EventAggregator.Subscribe<CostoFijoActualizadoEvent>(e => {
                        CargarCostosFijos();
                        ActualizarEstadisticas();
                    });
                    _suscribirEventos = false;
                }
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

            // Calcular total de materias primas
            decimal totalMateriasPrimas = CalcularTotalMateriasPrimas();
            decimal totalInversion = costosActivos + totalMateriasPrimas;
            decimal costoVentaUnitario = totalInversion / 2500;

            txtTotalCostos.Text = $"Total Costos: {totalCostos:C} ({listaCostos.Count} conceptos)";
            txtCostosActivos.Text = $"Costos Activos: {costosActivos:C} ({cantidadActivos} activos)";

            txtImpactoInfo.Text = $"💡 Inversión Total: {totalInversion:C} / 2500 = {costoVentaUnitario:C} por producto";
        }

        private decimal CalcularTotalMateriasPrimas()
        {
            try
            {
                CN_MateriaPrima cnMateriaPrima = new CN_MateriaPrima();
                var materias = cnMateriaPrima.Listar();
                return materias.Sum(m => m.PrecioUnitario * (decimal)m.CantidadDisponible);
            }
            catch
            {
                return 0;
            }
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
                    EventAggregator.Publish(new CostoFijoActualizadoEvent());
                    EventAggregator.Publish(new AlertasActualizadasEvent());

                    // Recalculamos productos después de guardar
                    RecalcularProductos();
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
                        EventAggregator.Publish(new CostoFijoActualizadoEvent());
                        EventAggregator.Publish(new AlertasActualizadasEvent());
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
                        EventAggregator.Publish(new CostoFijoActualizadoEvent());
                        EventAggregator.Publish(new AlertasActualizadasEvent());
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
                    // CORRECCIÓN: Llamar al método correcto sin parámetros
                    cnProducto.RecalcularTodosLosProductos();
                    txtImpactoInfo.Text = $"✅ Todos los productos actualizados con los nuevos costos";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al recalcular productos: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }