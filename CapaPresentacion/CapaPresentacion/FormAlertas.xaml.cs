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

        public FormAlertas()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CargarAlertas();
            ActualizarEstadisticas();
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
                // Implementar limpieza de alertas antiguas
                MessageBox.Show("Funcionalidad de limpieza en desarrollo", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
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
                    MessageBox.Show($"Abrir formulario de producción para: {alerta.NombreProducto}", "Resolver Alerta",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    // Abrir formulario de materias primas
                    MessageBox.Show($"Abrir formulario de materias primas para: {alerta.NombreProducto}", "Resolver Alerta",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }

                // Marcar como resuelta (eliminar alerta)
                bool resultado = cnAlerta.EliminarAlerta(alerta.IdAlerta);
                if (resultado)
                {
                    CargarAlertas();
                    ActualizarEstadisticas();
                }
            }
        }
    }
}