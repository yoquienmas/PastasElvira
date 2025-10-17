using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using CapaEntidad;

namespace CapaDatos
{
    public class CD_Cliente
    {
        public List<Cliente> ListarClientes()
        {
            List<Cliente> clientes = new List<Cliente>();

            using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
            {
                try
                {
                    oconexion.Open();
                    // CONSULTA COMPLETA con todas las columnas
                    SqlCommand comando = new SqlCommand(@"SELECT IdCliente, Nombre, Apellido, Documento, 
                                    Telefono, Email, Direccion, Cuil, Activo, FechaCreacion 
                                    FROM Cliente ORDER BY Nombre", oconexion);

                    using (SqlDataReader reader = comando.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            clientes.Add(new Cliente
                            {
                                IdCliente = (int)reader["IdCliente"],
                                Nombre = reader["Nombre"].ToString(),
                                Apellido = reader["Apellido"].ToString(),
                                Documento = reader["Documento"].ToString(),
                                Telefono = reader["Telefono"].ToString(),
                                Email = reader["Email"].ToString(),
                                Direccion = reader["Direccion"].ToString(),
                                Cuil = reader["Cuil"].ToString(),
                                Activo = (bool)reader["Activo"],
                                FechaCreacion = (DateTime)reader["FechaCreacion"]
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Error al listar clientes: " + ex.Message);
                }
            }

            return clientes;
        }

        public bool Registrar(Cliente cliente, out string mensaje)
        {
            mensaje = string.Empty;
            try
            {
                using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
                {
                    oconexion.Open();
                    SqlCommand comando = new SqlCommand(@"INSERT INTO Cliente (Nombre, Apellido, Documento, Telefono, Email, Direccion, Cuil, Activo, FechaCreacion) 
                                             VALUES (@Nombre, @Apellido, @Documento, @Telefono, @Email, @Direccion, @Cuil, @Activo, GETDATE())", oconexion);

                    comando.Parameters.AddWithValue("@Nombre", cliente.Nombre);
                    comando.Parameters.AddWithValue("@Apellido", cliente.Apellido);
                    comando.Parameters.AddWithValue("@Documento", cliente.Documento);
                    comando.Parameters.AddWithValue("@Telefono", (object)cliente.Telefono ?? DBNull.Value);
                    comando.Parameters.AddWithValue("@Email", (object)cliente.Email ?? DBNull.Value);
                    comando.Parameters.AddWithValue("@Direccion", (object)cliente.Direccion ?? DBNull.Value);
                    comando.Parameters.AddWithValue("@Cuil", (object)cliente.Cuil ?? DBNull.Value);
                    comando.Parameters.AddWithValue("@Activo", cliente.Activo);

                    int result = comando.ExecuteNonQuery();
                    mensaje = "Cliente registrado correctamente";
                    return result > 0;
                }
            }
            catch (Exception ex)
            {
                mensaje = ex.Message;
                return false;
            }
        }

        public bool Editar(Cliente cliente, out string mensaje)
        {
            mensaje = string.Empty;
            try
            {
                using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
                {
                    oconexion.Open();
                    SqlCommand comando = new SqlCommand(@"UPDATE Cliente SET Nombre = @Nombre, Apellido = @Apellido, Documento = @Documento, 
                                             Telefono = @Telefono, Email = @Email, Direccion = @Direccion, Cuil = @Cuil, Activo = @Activo 
                                             WHERE IdCliente = @IdCliente", oconexion);

                    comando.Parameters.AddWithValue("@IdCliente", cliente.IdCliente);
                    comando.Parameters.AddWithValue("@Nombre", cliente.Nombre);
                    comando.Parameters.AddWithValue("@Apellido", cliente.Apellido);
                    comando.Parameters.AddWithValue("@Documento", cliente.Documento);
                    comando.Parameters.AddWithValue("@Telefono", (object)cliente.Telefono ?? DBNull.Value);
                    comando.Parameters.AddWithValue("@Email", (object)cliente.Email ?? DBNull.Value);
                    comando.Parameters.AddWithValue("@Direccion", (object)cliente.Direccion ?? DBNull.Value);
                    comando.Parameters.AddWithValue("@Cuil", (object)cliente.Cuil ?? DBNull.Value);
                    comando.Parameters.AddWithValue("@Activo", cliente.Activo);

                    int result = comando.ExecuteNonQuery();
                    mensaje = "Cliente actualizado correctamente";
                    return result > 0;
                }
            }
            catch (Exception ex)
            {
                mensaje = ex.Message;
                return false;
            }
        }

        public bool Eliminar(int idCliente, out string mensaje)
        {
            mensaje = string.Empty;
            try
            {
                using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
                {
                    oconexion.Open();
                    SqlCommand comando = new SqlCommand("UPDATE Cliente SET Activo = 0 WHERE IdCliente = @IdCliente", oconexion);
                    comando.Parameters.AddWithValue("@IdCliente", idCliente);

                    int result = comando.ExecuteNonQuery();
                    mensaje = "Cliente eliminado correctamente";
                    return result > 0;
                }
            }
            catch (Exception ex)
            {
                mensaje = ex.Message;
                return false;
            }
        }

        // NUEVOS MÉTODOS AGREGADOS
        public bool ExisteDocumento(string documento, int idClienteActual = 0)
        {
            using (SqlConnection conexion = new SqlConnection(Conexion.cadena))
            {
                try
                {
                    string query = @"SELECT COUNT(1) FROM Cliente 
                                   WHERE Documento = @Documento 
                                   AND IdCliente != @IdClienteActual";

                    SqlCommand cmd = new SqlCommand(query, conexion);
                    cmd.Parameters.AddWithValue("@Documento", documento);
                    cmd.Parameters.AddWithValue("@IdClienteActual", idClienteActual);

                    conexion.Open();
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count > 0;
                }
                catch (Exception ex)
                {
                    // Manejar la excepción
                    return false;
                }
            }
        }


        public bool ExisteCuil(string cuil, int idClienteActual = 0)
        {
            using (SqlConnection conexion = new SqlConnection(Conexion.cadena))
            {
                try
                {
                    string query = @"SELECT COUNT(1) FROM Cliente 
                                   WHERE Cuil = @Cuil 
                                   AND IdCliente != @IdClienteActual";

                    SqlCommand cmd = new SqlCommand(query, conexion);
                    cmd.Parameters.AddWithValue("@Cuil", cuil);
                    cmd.Parameters.AddWithValue("@IdClienteActual", idClienteActual);

                    conexion.Open();
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count > 0;
                }
                catch (Exception ex)
                {
                    // Manejar la excepción
                    return false;
                }
            }

        }
    }
}