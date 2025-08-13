using System;
using System.Windows;
using System.Windows.Controls;
using CapaEntidad;
using CapaNegocio;
using System.Collections.Generic;

namespace CapaPresentacion
{
    public partial class FormMateriaPrima : Window
    {
        private CN_MateriaPrima cnMateriaPrima = new CN_MateriaPrima();

        public FormMateriaPrima()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ListarMateriasPrimas();
        }

        private void ListarMateriasPrimas()
        {
            dgvMateriaPrima.ItemsSource = cnMateriaPrima.Listar();
        }

        private void btnAgregar_Click(object sender, RoutedEventArgs e)
        {
            if (!float.TryParse(txtCantidad.Text, out float cantidad))
            {
                MessageBox.Show("Cantidad inválida.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MateriaPrima mp = new MateriaPrima
            {
                Nombre = txtNombre.Text,
                Unidad = txtUnidad.Text,
                CantidadDisponible = cantidad
            };

            string mensaje;
            int id = cnMateriaPrima.Registrar(mp, out mensaje);
            MessageBox.Show(mensaje);

            if (id != 0)
            {
                LimpiarCampos();
                ListarMateriasPrimas();
            }
        }

        private void btnEditar_Click(object sender, RoutedEventArgs e)
        {
            if (dgvMateriaPrima.SelectedItem is MateriaPrima seleccionado)
            {
                if (!float.TryParse(txtCantidad.Text, out float cantidad))
                {
                    MessageBox.Show("Cantidad inválida.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                seleccionado.Nombre = txtNombre.Text;
                seleccionado.Unidad = txtUnidad.Text;
                seleccionado.CantidadDisponible = cantidad;

                string mensaje;
                bool ok = cnMateriaPrima.Editar(seleccionado, out mensaje);
                MessageBox.Show(mensaje);

                if (ok)
                {
                    LimpiarCampos();
                    ListarMateriasPrimas();
                }
            }
        }

        private void btnEliminar_Click(object sender, RoutedEventArgs e)
        {
            if (dgvMateriaPrima.SelectedItem is MateriaPrima seleccionado)
            {
                string mensaje;
                bool ok = cnMateriaPrima.Eliminar(seleccionado.IdMateria, out mensaje);
                MessageBox.Show(mensaje);

                if (ok)
                {
                    LimpiarCampos();
                    ListarMateriasPrimas();
                }
            }
        }

        private void dgvMateriaPrima_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgvMateriaPrima.SelectedItem is MateriaPrima mp)
            {
                txtNombre.Text = mp.Nombre;
                txtUnidad.Text = mp.Unidad;
                txtCantidad.Text = mp.CantidadDisponible.ToString();
            }
        }

        private void LimpiarCampos()
        {
            txtNombre.Text = "";
            txtUnidad.Text = "";
            txtCantidad.Text = "";
            dgvMateriaPrima.SelectedItem = null;
        }
    }
}
