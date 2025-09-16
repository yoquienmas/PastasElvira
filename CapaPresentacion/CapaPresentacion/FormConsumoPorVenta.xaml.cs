using CapaDatos;
using CapaEntidad;
using CapaNegocio;
using System;
using System.Collections.Generic;
using System.Windows;

namespace CapaPresentacion
{
    public partial class FormConsumoPorVenta : Window
    {
        private CN_Reporte cnReporte = new CN_Reporte();

        public FormConsumoPorVenta()
        {
            InitializeComponent();
        }

        private void btnConsultar_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(txtIdVenta.Text, out int idVenta) || idVenta <= 0)
            {
                MessageBox.Show("Debe ingresar un ID de venta válido.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            List<ConsumoMateriaPrima> lista = cnReporte.ObtenerConsumoMateriaPrimaPorVenta(idVenta);
            dgvConsumoVenta.ItemsSource = lista;

            if (lista.Count == 0)
            {
                MessageBox.Show("No se encontró consumo de materias primas para la venta indicada.", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}