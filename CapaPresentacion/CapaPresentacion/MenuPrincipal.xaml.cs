using CapaDatos;
using CapaEntidad;
using CapaNegocio;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using System.Data;
using System.Windows.Input;

namespace CapaPresentacion
{
    public partial class MenuPrincipal : Window
    {
        private CN_Reporte cnReporte = new CN_Reporte();
        private CN_Venta cnVenta = new CN_Venta();
        private CN_Cliente cnCliente = new CN_Cliente();
        private CN_Producto cnProducto = new CN_Producto();
        private CN_Alerta cnAlerta = new CN_Alerta();
        private CN_Usuario cn_usuario = new CN_Usuario();

        // Variables para almacenar información del usuario
        private int _idUsuarioLogueado;
        private string _nombreUsuarioLogueado;
        private string _rolUsuarioLogueado;

        public MenuPrincipal()
        {
            InitializeComponent();
            txtFechaPrincipal.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            txtFechaPrincipal.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
        }

        private void OcultarError()
        {
            errorMessageBorder.Visibility = Visibility.Collapsed;
        }

        private void MostrarError(string mensaje)
        {
            txtMensajeError.Text = mensaje;
            errorMessageBorder.Visibility = Visibility.Visible;
        }

        // MÉTODO QUE FALTA - AGREGAR ESTO
        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            RealizarLogin();
        }

        private void txtLoginClave_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                RealizarLogin();
            }
        }

        private void RealizarLogin()
        {
            try
            {
                OcultarError();

                string nombre = txtLoginUsuario.Text;
                string clave = txtLoginClave.Password;

                if (string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(clave))
                {
                    MostrarError("Por favor, complete todos los campos.");
                    return;
                }

                bool resultado = cn_usuario.Login(nombre, clave);

                if (resultado)
                {
                    DataTable usuarioInfo = cn_usuario.ObtenerUsuarioPorNombre(nombre);

                    if (usuarioInfo.Rows.Count > 0)
                    {
                        DataRow row = usuarioInfo.Rows[0];
                        _idUsuarioLogueado = Convert.ToInt32(row["IdUsuario"]);
                        _nombreUsuarioLogueado = row["NombreUsuario"].ToString();
                        _rolUsuarioLogueado = row["Rol"].ToString();

                        // ✅ CORRECCIÓN: Obtener el NOMBRE COMPLETO
                        string nombreCompleto = $"{row["Nombre"]} {row["Apellido"]}";

                        // CERRAR esta ventana de login y ABRIR la ventana correspondiente
                        this.Hide(); // Ocultar ventana de login

                        // Abrir ventana según el rol
                        switch (_rolUsuarioLogueado.ToUpper())
                        {
                            case "DUEÑO":
                                MenuDueño menuDueño = new MenuDueño();
                                menuDueño.Show();
                                break;
                            case "ADMIN":
                                MenuAdmin menuAdmin = new MenuAdmin();
                                menuAdmin.Show();
                                break;
                            case "VENDEDOR":
                                // ✅ CORRECCIÓN: Pasar el NOMBRE COMPLETO
                                MenuVendedor menuVendedor = new MenuVendedor(_idUsuarioLogueado, nombreCompleto);
                                menuVendedor.Show();
                                break;
                            default:
                                MessageBox.Show("Rol no reconocido: " + _rolUsuarioLogueado);
                                this.Show(); // Mostrar nuevamente el login
                                break;
                        }
                    }
                }
                else
                {
                    MostrarError("Usuario o contraseña incorrectos");
                }
            }
            catch (Exception ex)
            {
                MostrarError("Error en el sistema: " + ex.Message);
            }
        }

        private void BtnCerrarSesion_Click(object sender, RoutedEventArgs e)
        {
            // Cerrar todas las ventanas abiertas y volver a mostrar el login
            Application.Current.Windows.OfType<Window>()
                .Where(w => w != this && w.GetType() != typeof(MenuPrincipal))
                .ToList()
                .ForEach(w => w.Close());

            // Reiniciar campos de login
            txtLoginUsuario.Text = "";
            txtLoginClave.Password = "";
            OcultarError();

            // Mostrar panel de login
            pnlLogin.Visibility = Visibility.Visible;
            pnlPrincipal.Visibility = Visibility.Collapsed;

            this.Show(); // Mostrar ventana principal (login)
        }
       
    }
}