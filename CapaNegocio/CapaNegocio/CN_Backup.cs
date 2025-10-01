using CapaDatos;
using System.Windows;

namespace CapaNegocio
{
    public class CN_Backup
    {
        private CD_Backup cdBackup = new CD_Backup();

        public bool RealizarBackup(string rutaCompleta)
        {
            return cdBackup.RealizarBackup(rutaCompleta);
        }

        public string ObtenerRutaBackupPredeterminada()
        {
            return cdBackup.ObtenerRutaBackupPredeterminada();
        }
    }
}