using System;
using System.Collections.Generic;
using System.Windows;
using CapaEntidad;
using CapaNegocio;

namespace CapaPresentacion
{
    public partial class FormReporteConsumo : Window
    {
        private CN_Reporte cnReporte = new CN_Reporte();

        public FormReporteConsumo()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            dpFechaInicio.SelectedDate = DateTime.Now.AddMonths(-1);
            dpFechaFin.SelectedDate = DateTime.Now;
        }

        private void btnGenerar_Click(object sender, RoutedEventArgs e)
        {
            if (dpFechaInicio.SelectedDate == null || dpFechaFin.SelectedDate == null)
            {
                MessageBox.Show("Debe seleccionar ambas fechas.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DateTime fechaInicio = dpFechaInicio.SelectedDate.Value;
            DateTime fechaFin = dpFechaFin.SelectedDate.Value;

            // CORRECCIÓN: Cambiar el tipo a ReporteConsumo o ajustar CN_Reporte
            List<ReporteConsumo> lista = cnReporte.ObtenerConsumoMateriaPrima(fechaInicio, fechaFin);
            dgvConsumo.ItemsSource = lista;
        }
    }
}
