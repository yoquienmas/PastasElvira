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
        private CN_Reporte cnReporte = new CN_Reporte();
        private int idUsuarioVendedor;

        public FormHistorialVentas(int idUsuario, string nombreUsuario)
        {
            InitializeComponent();
            idUsuarioVendedor = idUsuario;
            txtNombreVendedor.Text = nombreUsuario;

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

            // Obtener valores de los filtros
            string dniCliente = txtDniCliente.Text.Trim();
            string nombreProducto = txtNombreProducto.Text.Trim();

            List<ReporteVenta> lista = cnReporte.ObtenerVentasPorVendedor(
                idUsuarioVendedor, fechaInicio, fechaFin, dniCliente, nombreProducto);

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
            txtDniCliente.Text = "";
            txtNombreProducto.Text = "";
            CargarVentas();
        }

        private void dgvHistorialVentas_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Tu código existente aquí
        }
    }
}