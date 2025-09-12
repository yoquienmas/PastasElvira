using CapaEntidad;
using CapaNegocio;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

public partial class Login : Window
{
    private CN_Usuario cd_usuario = new CN_Usuario();

    public Login()
    {
        InitializeComponent();
    }

    private void BtnLogin_Click(object sender, RoutedEventArgs e)
    {
        string nombre = txtUsuario.Text;
        string clave = txtPassword.Password;

        try
        {
            if (cd_usuario.Login(nombre, clave))
            {
                MessageBox.Show("Login correcto");
                // abrir ventana principal
                MainWindow main = new MainWindow();
                main.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Usuario o contraseña incorrectos");
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error: " + ex.Message);
        }
    }
}
