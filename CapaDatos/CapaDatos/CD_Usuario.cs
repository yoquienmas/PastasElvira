using Microsoft.Data.SqlClient;
using System;
using System.Data;

namespace CapaDatos
    {
        public class CD_Usuario
        {
            private SqlConnection conexion = new SqlConnection(Conexion.cadena);

            // Método para login
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

            // Método para obtener usuario por nombre
            public DataTable ObtenerUsuarioPorNombre(string NombreUsuario)
            {
                try
                {
                    conexion.Open();
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

            // Método para obtener usuario por ID
            public DataTable ObtenerUsuarioPorId(int idUsuario)
            {
                try
                {
                    conexion.Open();
                    SqlCommand comando = new SqlCommand("SELECT * FROM Usuario WHERE IdUsuario = @IdUsuario", conexion);
                    comando.Parameters.AddWithValue("@IdUsuario", idUsuario);

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

            // Mostrar todos los usuarios
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

            // Insertar usuario completo
            public bool InsertarUsuario(string nombreUsuario, string nombre, string apellido, string documento,
                                      string telefono, string email, string cuil, string direccion,
                                      string clave, string rol, bool activo)
            {
                try
                {
                    conexion.Open();
                    SqlCommand comando = new SqlCommand(@"
                    INSERT INTO Usuario (NombreUsuario, Nombre, Apellido, Documento, Telefono, Email, Cuil, Direccion, Clave, Rol, Activo, FechaCreacion, UsuarioCreacion)
                    VALUES (@NombreUsuario, @Nombre, @Apellido, @Documento, @Telefono, @Email, @Cuil, @Direccion, @Clave, @Rol, @Activo, GETDATE(), @UsuarioCreacion)", conexion);

                    comando.Parameters.AddWithValue("@NombreUsuario", nombreUsuario);
                    comando.Parameters.AddWithValue("@Nombre", nombre);
                    comando.Parameters.AddWithValue("@Apellido", apellido);
                    comando.Parameters.AddWithValue("@Documento", documento);
                    comando.Parameters.AddWithValue("@Telefono", telefono);
                    comando.Parameters.AddWithValue("@Email", email);
                    comando.Parameters.AddWithValue("@Cuil", cuil);
                    comando.Parameters.AddWithValue("@Direccion", direccion);
                    comando.Parameters.AddWithValue("@Clave", clave);
                    comando.Parameters.AddWithValue("@Rol", rol);
                    comando.Parameters.AddWithValue("@Activo", activo);
                    comando.Parameters.AddWithValue("@UsuarioCreacion", "Sistema");

                    int result = comando.ExecuteNonQuery();
                    return result > 0;
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

            // Editar usuario completo
            public bool EditarUsuario(int idUsuario, string nombreUsuario, string nombre, string apellido,
                                    string documento, string telefono, string email, string cuil,
                                    string direccion, string clave, string rol, bool activo)
            {
                try
                {
                    conexion.Open();

                    string query = @"UPDATE Usuario SET 
                                NombreUsuario = @NombreUsuario, 
                                Nombre = @Nombre, 
                                Apellido = @Apellido, 
                                Documento = @Documento, 
                                Telefono = @Telefono, 
                                Email = @Email, 
                                Cuil = @Cuil, 
                                Direccion = @Direccion, 
                                Rol = @Rol, 
                                Activo = @Activo";

                    // Solo actualizar clave si se proporciona una nueva
                    if (!string.IsNullOrEmpty(clave))
                    {
                        query += ", Clave = @Clave";
                    }

                    query += " WHERE IdUsuario = @IdUsuario";

                    SqlCommand comando = new SqlCommand(query, conexion);

                    comando.Parameters.AddWithValue("@IdUsuario", idUsuario);
                    comando.Parameters.AddWithValue("@NombreUsuario", nombreUsuario);
                    comando.Parameters.AddWithValue("@Nombre", nombre);
                    comando.Parameters.AddWithValue("@Apellido", apellido);
                    comando.Parameters.AddWithValue("@Documento", documento);
                    comando.Parameters.AddWithValue("@Telefono", telefono);
                    comando.Parameters.AddWithValue("@Email", email);
                    comando.Parameters.AddWithValue("@Cuil", cuil);
                    comando.Parameters.AddWithValue("@Direccion", direccion);
                    comando.Parameters.AddWithValue("@Rol", rol);
                    comando.Parameters.AddWithValue("@Activo", activo);

                    if (!string.IsNullOrEmpty(clave))
                    {
                        comando.Parameters.AddWithValue("@Clave", clave);
                    }

                    int result = comando.ExecuteNonQuery();
                    return result > 0;
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

            // Eliminar usuario
            public bool EliminarUsuario(int idUsuario)
            {
                try
                {
                    conexion.Open();
                    SqlCommand comando = new SqlCommand("DELETE FROM Usuario WHERE IdUsuario = @IdUsuario", conexion);
                    comando.Parameters.AddWithValue("@IdUsuario", idUsuario);

                    int result = comando.ExecuteNonQuery();
                    return result > 0;
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