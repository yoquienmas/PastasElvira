using System;

namespace CapaEntidad
{
    public class Producto
    {
        public int IdProducto { get; set; }
        public string Nombre { get; set; }
        public string Tipo { get; set; }
        public decimal CostoProduccion { get; set; }    // decimal
        public decimal MargenGanancia { get; set; }     // decimal  
        public decimal PrecioVenta { get; set; }         // double ← Este es el problema
        public int StockActual { get; set; }
        public int StockMinimo { get; set; }
        public bool Visible { get; set; }
        public string EstadoStock { get; set; } // Para dashboard
    }
}