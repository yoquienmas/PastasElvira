using System;
using System.Linq;
using System.Windows;
using CapaEntidad;
using CapaNegocio;
using System.Collections.Generic;

namespace CapaPresentacion
{
    public partial class FormDetalleProduccion : Window
    {
        private bool _suscribirEventos = true;
        private CN_DetalleProduccion cnDetalle = new CN_DetalleProduccion();
        private CN_Produccion cnProduccion = new CN_Produccion();
        private CN_MateriaPrima cnMateria = new CN_MateriaPrima();

        public FormDetalleProduccion()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            cmbProduccion.ItemsSource = cnProduccion.Listar();
            cmbMateria.ItemsSource = cnMateria.Listar();

            // Suscribirse a eventos
            if (_suscribirEventos)
            {
                EventAggregator.Subscribe<ProduccionRegistradaEvent>(e => {
                    cmbProduccion.ItemsSource = cnProduccion.Listar();
                });
                EventAggregator.Subscribe<MateriaPrimaActualizadaEvent>(e => {
                    cmbMateria.ItemsSource = cnMateria.Listar();
                });
                _suscribirEventos = false;
            }
        }

        private void CargarDetalles()
        {
            if (cmbProduccion.SelectedValue != null)
            {
                int idProduccion = (int)cmbProduccion.SelectedValue;
                dgvDetalleProduccion.ItemsSource = cnDetalle.ListarPorProduccion(idProduccion);
            }
        }

        private void btnAgregar_Click(object sender, RoutedEventArgs e)
        {
            if (cmbProduccion.SelectedValue == null || cmbMateria.SelectedValue == null)
            {
                MessageBox.Show("Seleccione producción y materia prima.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(txtCantidadUtilizada.Text, out decimal cantidad) || cantidad <= 0)
            {
                MessageBox.Show("Cantidad inválida.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DetalleProduccion detalle = new DetalleProduccion
            {
                IdProduccion = (int)cmbProduccion.SelectedValue,
                IdMateria = (int)cmbMateria.SelectedValue,
                CantidadUtilizada = cantidad
            };

            string mensaje;
            int id = cnDetalle.Registrar(detalle, out mensaje);
            MessageBox.Show(mensaje);

            if (id != 0)
            {
                CargarDetalles();
                EventAggregator.Publish(new ProduccionRegistradaEvent()); // Nueva línea
                EventAggregator.Publish(new AlertasActualizadasEvent()); // Nueva línea

            }
        }

        private void btnEliminar_Click(object sender, RoutedEventArgs e)
        {
            if (dgvDetalleProduccion.SelectedItem is DetalleProduccion seleccionado)
            {
                string mensaje;
                bool ok = cnDetalle.Eliminar(seleccionado.IdDetalleProduccion, out mensaje);
                MessageBox.Show(mensaje);
                if (ok)
                {
                    CargarDetalles();
                    EventAggregator.Publish(new ProduccionRegistradaEvent()); // Nueva línea
                    EventAggregator.Publish(new AlertasActualizadasEvent()); // Nueva línea

                }
            }
        }
    }
}