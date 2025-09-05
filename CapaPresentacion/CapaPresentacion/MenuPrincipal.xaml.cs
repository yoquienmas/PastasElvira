using System.Windows;
using System.Windows.Controls;
using CapaEntidad;
using CapaNegocio;

namespace CapaPresentacion
{
    public partial class MenuPrincipal : Window
    {
        public MenuPrincipal()
        {
            InitializeComponent();
        }

        private void btnProductos_Click(object sender, RoutedEventArgs e)
        {
            FormProducto formProducto = new FormProducto();
            formProducto.Show();
            this.Close();
        }

        private void btnMateriasPrimas_Click(object sender, RoutedEventArgs e)
        {
            FormMateriaPrima formMateriaPrima = new FormMateriaPrima();
            formMateriaPrima.Show();
            this.Close();
        }

        private void btnVentas_Click(object sender, RoutedEventArgs e)
        {
            FormVenta formVenta = new FormVenta();
            formVenta.Show();
            this.Close();
        }
        private void btnProduccion_Click(object sender, RoutedEventArgs e)
        {
            FormProduccion formProduccion = new FormProduccion();
            formProduccion.Show();
            this.Close();
        }

        private void btnDetalleProduccion_Click(object sender, RoutedEventArgs e)
        {
            FormDetalleProduccion formdetalleproduccion = new FormDetalleProduccion();
            formdetalleproduccion.Show();
            this.Close();
        }
        

        private void btnSalir_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}