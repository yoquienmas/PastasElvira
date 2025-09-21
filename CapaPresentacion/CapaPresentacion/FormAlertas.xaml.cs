using CapaEntidad;
using CapaNegocio;
using System;
using System.Windows;
using System.Windows.Controls;

namespace CapaPresentacion
{
    public partial class FormAlertas : Window
    {
        private CN_Alerta cnAlerta = new CN_Alerta();
        private CN_Producto cnProducto = new CN_Producto();
        private bool _suscribirEventos = true; // Nueva variable para controlar suscripciones

        public FormAlertas()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CargarAlertas();
            ActualizarEstadisticas();

            // Suscribirse a eventos del sistema
            if (_suscribirEventos)
            {
                EventAggregator.Subscribe<ProductoActualizadoEvent>(e => {
                    Dispatcher.Invoke(() => {
                        CargarAlertas();
                        ActualizarEstadisticas();
                    });
                });

                EventAggregator.Subscribe<MateriaPrimaActualizadaEvent>(e => {
                    Dispatcher.Invoke(() => {
                        CargarAlertas();
                        ActualizarEstadisticas();
                    });
                });

                EventAggregator.Subscribe<ProduccionRegistradaEvent>(e => {
                    Dispatcher.Invoke(() => {
                        CargarAlertas();
                        ActualizarEstadisticas();
                    });
                });

                EventAggregator.Subscribe<RecetaActualizadaEvent>(e => {
                    Dispatcher.Invoke(() => {
                        CargarAlertas();
                        ActualizarEstadisticas();
                    });
                });

                _suscribirEventos = false;
            }
        }

        private void CargarAlertas()
        {
            dgvAlertas.ItemsSource = cnAlerta.ListarAlertas();
        }

        private void ActualizarEstadisticas()
        {
            int alertasHoy = cnAlerta.ObtenerCantidadAlertasPendientes();
            txtAlertasHoy.Text = alertasHoy.ToString();

            // Contar productos con stock bajo
            var productos = cnProducto.Listar();
            int productosBajoStock = 0;
            foreach (var producto in productos)
            {
                if (producto.StockActual <= producto.StockMinimo && producto.Visible)
                {
                    productosBajoStock++;
                }
            }
            txtProductosBajoStock.Text = productosBajoStock.ToString();

            txtContadorAlertas.Text = $"🔔 {alertasHoy} alertas pendientes";
        }

        private void btnVerificar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                cnAlerta.VerificarYGenerarAlertas();
                CargarAlertas();
                ActualizarEstadisticas();
                MessageBox.Show("Verificación de alertas completada", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                // Publicar evento para que otros formularios sepan que se generaron alertas
                EventAggregator.Publish(new AlertasActualizadasEvent());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al verificar alertas: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnLimpiar_Click(object sender, RoutedEventArgs e)
        {
            var confirmacion = MessageBox.Show("¿Está seguro de eliminar las alertas antiguas? (Se conservarán las de hoy)",
                "Confirmar Limpieza", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (confirmacion == MessageBoxResult.Yes)
            {
                bool resultado = cnAlerta.LimpiarAlertasAntiguas();
                if (resultado)
                {
                    CargarAlertas();
                    ActualizarEstadisticas();
                    MessageBox.Show("Alertas antiguas eliminadas correctamente", "Éxito",
                                   MessageBoxButton.OK, MessageBoxImage.Information);

                    // Publicar evento
                    EventAggregator.Publish(new AlertasActualizadasEvent());
                }
                else
                {
                    MessageBox.Show("Error al limpiar alertas", "Error",
                                   MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnActualizar_Click(object sender, RoutedEventArgs e)
        {
            CargarAlertas();
            ActualizarEstadisticas();
        }

        private void BtnResolver_Click(object sender, RoutedEventArgs e)
        {
            if (dgvAlertas.SelectedItem is AlertaStock alerta)
            {
                if (alerta.Tipo == "Producto")
                {
                    // Abrir formulario de producción para reponer stock
                    FormProduccion formProduccion = new FormProduccion();
                    formProduccion.Show();
                }
                else
                {
                    // Abrir formulario de materias primas
                    FormMateria formMateria = new FormMateria();
                    formMateria.Show();
                }

                // Marcar como resuelta (eliminar alerta)
                bool resultado = cnAlerta.EliminarAlerta(alerta.IdAlerta);
                if (resultado)
                {
                    CargarAlertas();
                    ActualizarEstadisticas();

                    // Publicar evento
                    EventAggregator.Publish(new AlertasActualizadasEvent());
                }
            }
        }

        // Nuevo método para forzar verificación de alertas desde otros formularios
        public void VerificarAlertasDesdeExterno()
        {
            btnVerificar_Click(null, null);
        }
    }
}