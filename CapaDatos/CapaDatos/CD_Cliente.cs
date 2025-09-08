using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using CapaEntidad;

namespace CapaDatos
{
    public class CD_Cliente
    {
        public List<Cliente> Listar()
        {
            List<Cliente> lista = new List<Cliente>();

            using (SqlConnection conexion = new SqlConnection(Conexion.cadena))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("SELECT * FROM Cliente ORDER BY Nombre", conexion);
                    conexion.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lista.Add(new Cliente()
                            {
                                IdCliente = Convert.ToInt32(dr["IdCliente"]),
                                Nombre = dr["Nombre"].ToString(),
                                Documento = dr["Documento"].ToString(),
                                Telefono = dr["Telefono"] != DBNull.Value ? dr["Telefono"].ToString() : "",
                                Email = dr["Email"] != DBNull.Value ? dr["Email"].ToString() : "",
                                Direccion = dr["Direccion"] != DBNull.Value ? dr["Direccion"].ToString() : ""
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    lista = new List<Cliente>();
                }
            }
            return lista;
        }

        public bool Registrar(Cliente cliente, out string mensaje)
        {
            mensaje = string.Empty;
            bool resultado = false;

            try
            {
                using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
                {
                    SqlCommand cmd = new SqlCommand("INSERT INTO Cliente (Nombre, Documento, Telefono, Email, Direccion) VALUES (@nombre, @documento, @telefono, @email, @direccion)", oconexion);

                    cmd.Parameters.AddWithValue("@nombre", cliente.Nombre);
                    cmd.Parameters.AddWithValue("@documento", cliente.Documento);
                    cmd.Parameters.AddWithValue("@telefono", cliente.Telefono ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@email", cliente.Email ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@direccion", cliente.Direccion ?? (object)DBNull.Value);

                    oconexion.Open();
                    resultado = cmd.ExecuteNonQuery() > 0;
                    mensaje = resultado ? "Cliente registrado correctamente" : "No se pudo registrar el cliente";
                }
            }
            catch (Exception ex)
            {
                resultado = false;
                mensaje = ex.Message;
            }

            return resultado;
        }

        public bool Editar(Cliente cliente, out string mensaje)
        {
            mensaje = string.Empty;
            bool resultado = false;

            try
            {
                using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
                {
                    SqlCommand cmd = new SqlCommand("UPDATE Cliente SET Nombre = @nombre, Documento = @documento, Telefono = @telefono, Email = @email, Direccion = @direccion WHERE IdCliente = @id", oconexion);

                    cmd.Parameters.AddWithValue("@id", cliente.IdCliente);
                    cmd.Parameters.AddWithValue("@nombre", cliente.Nombre);
                    cmd.Parameters.AddWithValue("@documento", cliente.Documento);
                    cmd.Parameters.AddWithValue("@telefono", cliente.Telefono ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@email", cliente.Email ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@direccion", cliente.Direccion ?? (object)DBNull.Value);

                    oconexion.Open();
                    resultado = cmd.ExecuteNonQuery() > 0;
                    mensaje = resultado ? "Cliente actualizado correctamente" : "No se pudo actualizar el cliente";
                }
            }
            catch (Exception ex)
            {
                resultado = false;
                mensaje = ex.Message;
            }

            return resultado;
        }

        public bool Eliminar(int idCliente, out string mensaje)
        {
            mensaje = string.Empty;
            bool resultado = false;

            try
            {
                using (SqlConnection oconexion = new SqlConnection(Conexion.cadena))
                {
                    SqlCommand cmd = new SqlCommand("DELETE FROM Cliente WHERE IdCliente = @id", oconexion);
                    cmd.Parameters.AddWithValue("@id", idCliente);

                    oconexion.Open();
                    resultado = cmd.ExecuteNonQuery() > 0;
                    mensaje = resultado ? "Cliente eliminado correctamente" : "No se pudo eliminar el cliente";
                }
            }
            catch (Exception ex)
            {
                resultado = false;
                mensaje = ex.Message;
            }

            return resultado;
        }
    }
}