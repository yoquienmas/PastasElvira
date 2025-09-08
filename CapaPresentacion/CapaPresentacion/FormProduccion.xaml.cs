using CapaDatos;
using CapaEntidad;
using CapaNegocio;
using System;
using System.Collections.Generic;
using System.Windows;


namespace CapaPresentacion
{
    public partial class FormProduccion : Window
    {
        private CN_Produccion cnProduccion = new CN_Produccion();
        private CN_Producto cnProducto = new CN_Producto();

        public FormProduccion()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CargarProductos();
            CargarHistorial();
        }

        private void CargarProductos()
        {
            var listaProductos = cnProducto.Listar();
            cboProductos.ItemsSource = listaProductos;
            cboProductos.DisplayMemberPath = "Nombre";
            cboProductos.SelectedValuePath = "IdProducto";
        }

        private void CargarHistorial()
        {
            dgvProducciones.ItemsSource = cnProduccion.Listar();
        }

        private void btnRegistrar_Click(object sender, RoutedEventArgs e)
        {
            if (cboProductos.SelectedItem == null)
            {
                MessageBox.Show("Seleccione un producto.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(txtCantidad.Text, out int cantidad) || cantidad <= 0)
            {
                MessageBox.Show("La cantidad debe ser mayor a 0.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var producto = (Producto)cboProductos.SelectedItem;

            // VALIDAR MATERIAS PRIMAS ANTES DE REGISTRAR
            string errorMaterias = cnProduccion.ValidarDisponibilidadMateriasPrimas(producto.IdProducto, cantidad);
            if (!string.IsNullOrEmpty(errorMaterias))
            {
                MessageBox.Show(errorMaterias, "Error de Materias Primas", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Produccion nuevaProduccion = new Produccion
            {
                IdProducto = producto.IdProducto,
                CantidadProducida = cantidad
            };

            string mensaje;
            bool ok = cnProduccion.Registrar(nuevaProduccion, out mensaje);

            MessageBox.Show(mensaje, ok ? "Éxito" : "Error",
                MessageBoxButton.OK,
                ok ? MessageBoxImage.Information : MessageBoxImage.Error);

            if (ok)
            {
                CargarHistorial();
                txtCantidad.Text = "";
                cboProductos.SelectedIndex = -1;
            }
        }
    }
}
