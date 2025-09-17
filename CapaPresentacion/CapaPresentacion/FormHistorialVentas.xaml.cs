using CapaEntidad;
using CapaNegocio;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace CapaPresentacion
{
    public partial class FormHistorialVentas : Window
    {
        private int _idUsuario;
        private string _nombreUsuario;
        private CN_Reporte cnReporte = new CN_Reporte();

        // Constructor que acepta parámetros
        public FormHistorialVentas(int idUsuario, string nombreUsuario)
        {
            InitializeComponent();
            _idUsuario = idUsuario;
            _nombreUsuario = nombreUsuario;

            // Mostrar nombre del vendedor
            txtNombreVendedor.Text = _nombreUsuario;

            // Establecer fechas por defecto (mes actual)
            dtpFechaInicio.SelectedDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            dtpFechaFin.SelectedDate = DateTime.Now;

            CargarVentas();
        }

        private void CargarVentas()
        {
            if (dtpFechaInicio.SelectedDate == null || dtpFechaFin.SelectedDate == null)
            {
                MessageBox.Show("Debe seleccionar ambas fechas.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DateTime fechaInicio = dtpFechaInicio.SelectedDate.Value;
            DateTime fechaFin = dtpFechaFin.SelectedDate.Value;

            // MODIFICADO: Ya no hay controles de filtro en el XAML
            string dniCliente = ""; // Filtro vacío
            string nombreProducto = ""; // Filtro vacío

            List<ReporteVenta> lista = cnReporte.ObtenerVentasPorVendedor(
                _idUsuario, fechaInicio, fechaFin, dniCliente, nombreProducto);

            dgvHistorialVentas.ItemsSource = lista;

            // Calcular y mostrar total
            decimal totalVentas = 0;
            foreach (var venta in lista)
            {
                totalVentas += venta.Total;
            }
            txtTotalVentas.Text = totalVentas.ToString("C");

            if (lista.Count == 0)
            {
                MessageBox.Show("No se encontraron ventas en el rango seleccionado.", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void btnConsultar_Click(object sender, RoutedEventArgs e)
        {
            CargarVentas();
        }

        private void btnLimpiarFiltros_Click(object sender, RoutedEventArgs e)
        {
            // MODIFICADO: Ya no hay controles que limpiar, solo recargar
            CargarVentas();
        }

        private void dgvHistorialVentas_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Tu código existente aquí
        }
    }
}