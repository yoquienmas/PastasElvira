using Microsoft.Data.SqlClient;
using System;
using System.Data;

namespace CapaDatos
{
    public class CD_Usuario
    {
        private SqlConnection conexion = new SqlConnection(Conexion.cadena);

        // Método para login - CORREGIDO
        public bool Login(string NombreUsuario, string clave)
        {
            try
            {
                conexion.Open();
                SqlCommand comando = new SqlCommand("SELECT COUNT(*) FROM Usuario WHERE NombreUsuario = @NombreUsuario AND clave = @clave AND activo = 1", conexion);
                comando.Parameters.AddWithValue("@NombreUsuario", NombreUsuario);
                comando.Parameters.AddWithValue("@Clave", clave);

                int result = (int)comando.ExecuteScalar();
                return result > 0;
            }
            catch (Exception ex)
            {
                throw new Exception("Error en login: " + ex.Message);
            }
            finally
            {
                if (conexion.State == ConnectionState.Open)
                    conexion.Close();
            }
        }

        // Método para obtener usuario por nombre - CORREGIDO
        public DataTable ObtenerUsuarioPorNombre(string NombreUsuario)
        {
            try
            {
                conexion.Open();
                // CAMBIÉ: "Usuario" por "Usuarios" y "NombreUsuario" por "Usuario"
                SqlCommand comando = new SqlCommand("SELECT * FROM Usuario WHERE NombreUsuario = @NombreUsuario", conexion);
                comando.Parameters.AddWithValue("@NombreUsuario", NombreUsuario);

                SqlDataAdapter adaptador = new SqlDataAdapter(comando);
                DataTable tabla = new DataTable();
                adaptador.Fill(tabla);
                return tabla;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener usuario: " + ex.Message);
            }
            finally
            {
                if (conexion.State == ConnectionState.Open)
                    conexion.Close();
            }
        }

        // Métodos existentes - TODOS CORREGIDOS para usar "Usuario"
        public DataTable MostrarUsuarios()
        {
            try
            {
                conexion.Open();
                SqlCommand comando = new SqlCommand("SELECT * FROM Usuario", conexion);
                SqlDataAdapter adaptador = new SqlDataAdapter(comando);
                DataTable tabla = new DataTable();
                adaptador.Fill(tabla);
                return tabla;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al mostrar usuarios: " + ex.Message);
            }
            finally
            {
                if (conexion.State == ConnectionState.Open)
                    conexion.Close();
            }
        }

        public DataTable MostrarUsuarioPorId(string idUsuario)
        {
            try
            {
                conexion.Open();
                SqlCommand comando = new SqlCommand("SELECT * FROM Usuario WHERE IdUsuario = @IdUsuario", conexion);
                comando.Parameters.AddWithValue("@IdUsuario", Convert.ToInt32(idUsuario));

                SqlDataAdapter adaptador = new SqlDataAdapter(comando);
                DataTable tabla = new DataTable();
                adaptador.Fill(tabla);
                return tabla;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al mostrar usuario por ID: " + ex.Message);
            }
            finally
            {
                if (conexion.State == ConnectionState.Open)
                    conexion.Close();
            }
        }

        public void InsertarUsuario(string NombreUsuario, string clave, string rol, string activo)
        {
            try
            {
                conexion.Open();
                SqlCommand comando = new SqlCommand("INSERT INTO Usuario (NombreUsuario, clave, Rol, activo) VALUES (@Usuario, @Clave, @Rol, @Activo)", conexion);
                comando.Parameters.AddWithValue("@NombreUsuario", NombreUsuario);
                comando.Parameters.AddWithValue("@Clave", clave);
                comando.Parameters.AddWithValue("@Rol", rol);
                comando.Parameters.AddWithValue("@Activo", Convert.ToInt32(activo));
                comando.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al insertar usuario: " + ex.Message);
            }
            finally
            {
                if (conexion.State == ConnectionState.Open)
                    conexion.Close();
            }
        }
        public void EditarUsuario(string idUsuario, string NombreUsuario, string clave, string rol, string activo)
        {
            try
            {
                conexion.Open();
                SqlCommand comando = new SqlCommand("UPDATE Usuario SET NombreUsuario = @NombreUsuario, Clave = @Clave, Rol = @Rol, Activo = @Activo WHERE IdUsuario = @IdUsuario", conexion);
                comando.Parameters.AddWithValue("@IdUsuario", Convert.ToInt32(idUsuario));
                comando.Parameters.AddWithValue("@NombreUsuario", NombreUsuario);
                comando.Parameters.AddWithValue("@Clave", clave);
                comando.Parameters.AddWithValue("@Rol", rol);
                comando.Parameters.AddWithValue("@Activo", Convert.ToInt32(activo));
                comando.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al editar usuario: " + ex.Message);
            }
            finally
            {
                if (conexion.State == ConnectionState.Open)
                    conexion.Close();
            }
        }

        public void EliminarUsuario(string idUsuario)
        {
            try
            {
                conexion.Open();
                SqlCommand comando = new SqlCommand("DELETE FROM Usuario WHERE IdUsuario = @IdUsuario", conexion);
                comando.Parameters.AddWithValue("@IdUsuario", Convert.ToInt32(idUsuario));
                comando.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al eliminar usuario: " + ex.Message);
            }
            finally
            {
                if (conexion.State == ConnectionState.Open)
                    conexion.Close();
            }
        }
    }
}