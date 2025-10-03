using CapaDatos;
using CapaEntidad;
using CapaNegocio;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using Microsoft.Win32;

namespace CapaPresentacion
{
    public partial class MenuAdmin : Window
    {
        private CN_Reporte cnReporte = new CN_Reporte();
        private CN_Backup cnBackup = new CN_Backup();

        public MenuAdmin()
        {
            InitializeComponent();
            CargarDashboardAdmin();
        }

        private void CargarDashboardAdmin()
        {
            try
            {
                // Cargar últimas ventas
                var ventas = cnReporte.ObtenerVentasPorFecha(DateTime.Today.AddDays(-30), DateTime.Today);
                dgvVentasAdmin.ItemsSource = ventas.Take(20);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar dashboard admin: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnBackupDB_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Backup files (*.bak)|*.bak";
                saveFileDialog.FileName = $"Backup_PastasElvira_{DateTime.Now:yyyyMMdd_HHmmss}.bak";
                saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                if (saveFileDialog.ShowDialog() == true)
                {
                    MessageBox.Show("Realizando backup de la base de datos...\nEsto puede tomar unos segundos.",
                                  "Backup en Progreso", MessageBoxButton.OK, MessageBoxImage.Information);

                    bool resultado = cnBackup.RealizarBackup(saveFileDialog.FileName);

                    if (resultado)
                    {
                        MessageBox.Show($"✅ Backup realizado exitosamente!\nUbicación: {saveFileDialog.FileName}",
                                      "Backup Completado", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("❌ Error al realizar el backup.", "Error",
                                      MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al realizar backup: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region Event Handlers - Mantenidos
        private void btnMateriasPrimas_Click(object sender, RoutedEventArgs e)
        {
            FormMateria formMateria = new FormMateria();
            formMateria.Show();
        }

        private void btnGestionUsuarios_Click(object sender, RoutedEventArgs e)
        {
            FormGestionUsuarios formgestionusuarios = new FormGestionUsuarios();
            formgestionusuarios.Show();
        }

        private void btnProductosTerminados_Click(object sender, RoutedEventArgs e)
        {
            FormProducto formProducto = new FormProducto();
            formProducto.Show();
        }

        private void btnAlertasStock_Click(object sender, RoutedEventArgs e)
        {
            FormAlertas formAlerts = new FormAlertas();
            formAlerts.Show();
        }

        private void btnProgramarProduccion_Click(object sender, RoutedEventArgs e)
        {
            // Este botón ahora incluirá la funcionalidad de recetas
            FormProduccion formProduccion = new FormProduccion();
            formProduccion.Show();
        }

        private void btnCostosFijos_Click(object sender, RoutedEventArgs e)
        {
            FormCostoFijo formCostoFijo = new FormCostoFijo();
            formCostoFijo.Show();
        }

        private void btnReportesGenerales_Click(object sender, RoutedEventArgs e)
        {
            FormReportes formReportes = new FormReportes();
            formReportes.Show();
        }

        private void BtnCerrarSesion_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Application.Current.Windows.OfType<MenuPrincipal>().FirstOrDefault();
            if (mainWindow != null)
            {
                mainWindow.Show();
            }
            this.Close();
        }
        #endregion

        // ELIMINADO: btnConsumoPorVenta_Click - Mover a módulo especializado de ventas
        // ELIMINADO: btnVerDetalleVenta_Click - Simplificado el DataGrid
    }
}