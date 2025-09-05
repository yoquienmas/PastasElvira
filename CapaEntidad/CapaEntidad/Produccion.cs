using System;

namespace CapaEntidad
{
    public class Produccion
    {
        public int IdProduccion { get; set; }
        public DateTime Fecha { get; set; }
        public int CantidadProducida { get; set; }
        public int IdProducto { get; set; }

        // Opcional: para mostrar el nombre del producto en el DataGrid
        public string NombreProducto { get; set; }
    }
}


