using System.Windows;
using CapaNegocio;

namespace CapaPresentacion
{
    public partial class Login : Window
    {
        private CN_Usuario cn_usuario = new CN_Usuario();

        public Login()
        {
            InitializeComponent();
        }

        private void BtnIngresar_Click(object sender, RoutedEventArgs e)
        {
            string usuario = txtUsuario.Text;
            string clave = txtClave.Password;

            bool valido = cn_usuario.ValidarLogin(usuario, clave);

            if (valido)
            {
                MessageBox.Show("Bienvenido!");
            }
            else
            {
                MessageBox.Show("Usuario o clave incorrectos.");
            }
        }
    }
}
