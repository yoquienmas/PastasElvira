using CapaDatos;
using CapaEntidad;
using System.Collections.Generic;

namespace CapaNegocio
{
    public class CN_Alerta
    {
        private CD_Alerta cd_alerta = new CD_Alerta();

        public List<AlertaStock> ListarAlertas()
        {
            return cd_alerta.ListarAlertas();
        }

        public bool RegistrarAlerta(int idProducto, string mensaje)
        {
            return cd_alerta.RegistrarAlerta(idProducto, mensaje);
        }

        public bool EliminarAlerta(int idAlerta)
        {
            return cd_alerta.EliminarAlerta(idAlerta);
        }

        public void VerificarYGenerarAlertas()
        {
            cd_alerta.VerificarYGenerarAlertas();
        }

        public int ObtenerCantidadAlertasPendientes()
        {
            return cd_alerta.ObtenerCantidadAlertasPendientes();
        }
    }
}