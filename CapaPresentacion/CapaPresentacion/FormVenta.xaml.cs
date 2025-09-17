using CapaDatos;
using CapaEntidad;
using CapaNegocio;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Linq;

namespace CapaPresentacion
{

    public partial class FormVenta : Window
    {
        private CN_Producto cnProducto = new CN_Producto();

        private CN_Venta cnVenta = new CN_Venta();

        private List<Producto> listaProductos;

        private ObservableCollection<ItemVenta> listaItemsVenta = new ObservableCollection<ItemVenta>();

        public FormVenta()
        {
            InitializeComponent();
            dgvItemsVenta.ItemsSource = listaItemsVenta;
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CargarProductos();
        }
        // En la clase FormVenta (FormVenta.xaml.cs)


        private void CargarProductos()
        {
            listaProductos = cnProducto.Listar();
            cboProductos.ItemsSource = listaProductos;
            cboProductos.DisplayMemberPath = "Nombre";
            cboProductos.SelectedValuePath = "IdProducto";
        }

        private void cboProductos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboProductos.SelectedItem is Producto productoSeleccionado)
            {
                txtPrecioUnitario.Text = productoSeleccionado.PrecioVenta.ToString("F2");
            }
        }


        private void btnAgregar_Click(object sender, RoutedEventArgs e)
        {
            if (cboProductos.SelectedItem == null)
            {
                MessageBox.Show("Debe seleccionar un producto.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(txtCantidad.Text, out int cantidad) || cantidad <= 0)
            {
                MessageBox.Show("La cantidad debe ser un número entero mayor que cero.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Producto productoSeleccionado = (Producto)cboProductos.SelectedItem;

            ItemVenta nuevoItem = new ItemVenta
            {
                IdProducto = productoSeleccionado.IdProducto,
                NombreProducto = productoSeleccionado.Nombre,
                Cantidad = cantidad,
                PrecioUnitario = productoSeleccionado.PrecioVenta
            };

            listaItemsVenta.Add(nuevoItem);
            ActualizarTotalVenta();

        }


        private void btnEliminar_Click(object sender, RoutedEventArgs e)
        {
            if (dgvItemsVenta.SelectedItem is ItemVenta itemSeleccionado)
            {
                listaItemsVenta.Remove(itemSeleccionado);
                ActualizarTotalVenta();
            }
            else
            {
                MessageBox.Show("Debe seleccionar un ítem para eliminar.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }


        private void btnRegistrarVenta_Click(object sender, RoutedEventArgs e)
        {
            if (listaItemsVenta.Count == 0)
            {
                MessageBox.Show("No se puede registrar una venta sin productos.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Venta nuevaVenta = new Venta
            {
                Items = listaItemsVenta.ToList(),
                Fecha = DateTime.Now
            };

            string mensaje;
            bool ok = cnVenta.Registrar(nuevaVenta, out mensaje);

            MessageBox.Show(mensaje);

            if (ok)
            {
                LimpiarFormulario();
            }
        }

        private void ActualizarTotalVenta()
        {
            decimal total = listaItemsVenta.Sum(item => item.Cantidad * item.PrecioUnitario);
            txtTotalVenta.Text = total.ToString("F2");
        }


        private void LimpiarFormulario()
        {
            listaItemsVenta.Clear();
            txtCantidad.Text = "1";
            cboProductos.SelectedIndex = -1;
            txtPrecioUnitario.Text = "0.00";
            ActualizarTotalVenta();
        }
    }
}