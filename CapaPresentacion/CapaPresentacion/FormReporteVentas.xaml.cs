using CapaEntidad;
using CapaNegocio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;


namespace CapaPresentacion
{
    public partial class FormReporteVentas : Window
    {
        private CN_Reporte cnReporte = new CN_Reporte();

        public FormReporteVentas()
        {
            InitializeComponent();
        }

        private void btnConsultar_Click(object sender, RoutedEventArgs e)
        {
            if (dtpFechaInicio.SelectedDate == null || dtpFechaFin.SelectedDate == null)
            {
                MessageBox.Show("Debe seleccionar ambas fechas.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DateTime fechaInicio = dtpFechaInicio.SelectedDate.Value;
            DateTime fechaFin = dtpFechaFin.SelectedDate.Value;

            List<ReporteVenta> lista = cnReporte.ObtenerVentasPorFecha(fechaInicio, fechaFin); // CORRECCIÓN: ObtenerVentasPorFecha
            dgvReporteVentas.ItemsSource = lista;

            if (lista.Count == 0)
            {
                MessageBox.Show("No se encontraron ventas en el rango seleccionado.", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void GenerarResumenVentas(List<CapaEntidad.ReporteVenta> ventas)
        {
            // Usar CapaEntidad.ReporteVenta en lugar de solo ReporteVenta
            decimal totalVentas = ventas.Sum(v => v.Total);
        }
    }
}