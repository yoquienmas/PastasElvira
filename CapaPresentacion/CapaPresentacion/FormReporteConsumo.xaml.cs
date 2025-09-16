using CapaEntidad;
using CapaNegocio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls; // Asegúrate de tener este using para SelectionChangedEventArgs

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
            // Inicializa las fechas de los DatePicker que existen en ESTA ventana
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

            List<ReporteConsumo> lista = cnReporte.ObtenerConsumoMateriaPrima(fechaInicio, fechaFin);
            dgvConsumo.ItemsSource = lista;

            // Calcular y mostrar total en lblTotal (TextBlock en ESTA ventana)
            decimal totalCosto = lista.Sum(item => item.CostoTotal);
            lblTotal.Text = $"Costo Total: {totalCosto:C}"; // <--- Corregido a .Text y para lblTotal
        }

        private void dgvConsumo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Deja este método si necesitas lógica cuando se selecciona una fila en dgvConsumo
        }
    }
}