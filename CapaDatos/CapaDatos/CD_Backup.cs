using CapaDatos;
using CapaEntidad;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.IO;


namespace CapaDatos
{
    public class CD_Backup
    {
        public bool RealizarBackup(string rutaCompleta)
        {
            try
            {
                using (SqlConnection conexion = new SqlConnection(Conexion.cadena))
                {
                    conexion.Open();

                    // Verificar si el directorio existe, si no, crearlo
                    string directorio = Path.GetDirectoryName(rutaCompleta);
                    if (!Directory.Exists(directorio))
                    {
                        Directory.CreateDirectory(directorio);
                    }

                    // Comando SQL para realizar el backup
                    string query = $"BACKUP DATABASE [PastasElviraDB] TO DISK = '{rutaCompleta}' WITH FORMAT, MEDIANAME = 'SQLServerBackups', NAME = 'Backup Completo de PastasElviraDB';";

                    using (SqlCommand comando = new SqlCommand(query, conexion))
                    {
                        comando.ExecuteNonQuery();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al realizar backup: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public string ObtenerRutaBackupPredeterminada()
        {
            string documentos = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string carpetaBackups = Path.Combine(documentos, "BackupsPastasElvira");
            string nombreArchivo = $"Backup_PastasElvira_{DateTime.Now:yyyyMMdd_HHmmss}.bak";
            return Path.Combine(carpetaBackups, nombreArchivo);
        }
    }
}