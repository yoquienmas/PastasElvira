
using System.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using CapaEntidad;


namespace CapaDatos
{
    public class CD_Usuario
    {
        // private string conexion = "Data Source=.;Initial Catalog=MiBD;Integrated Security=True";

        public bool ValidarLogin(string nombre, string clave)
        {
            using (SqlConnection conexion = new SqlConnection(Conexion.cadena))
            {
                string query = "SELECT COUNT(*) FROM Usuarios WHERE NombreUsuario=@nombre AND Clave=@clave";
                SqlCommand cmd = new SqlCommand(query, conexion);
                cmd.Parameters.AddWithValue("@nombre", nombre);
                cmd.Parameters.AddWithValue("@clave", clave); // luego se debería usar hash

                conexion.Open();
                int count = (int)cmd.ExecuteScalar();
                return count > 0;
            }
        }
    }


}

