using System;

namespace CapaEntidad
{
    public class Produccion
    {
        public int IdProduccion { get; set; }
        public int IdProducto { get; set; }
        public string NombreProducto { get; set; }
        public int CantidadProducida { get; set; }
        public DateTime FechaProduccion { get; set; }
        public bool Estado { get; set; }


       
    }
}