using System;

namespace CapaEntidad
{
    public class Producto
    {
        public int IdProducto { get; set; }
        public string Nombre { get; set; }
        public string Tipo { get; set; }

        // Nuevos campos en la BD
        public decimal CostoProduccion { get; set; }   // costo de elaboración total
        public decimal MargenGanancia { get; set; }   // % de margen que se le suma al costo
        public decimal PrecioVenta { get; set; }      // precio final al cliente

        public int StockActual { get; set; }          // cantidad en stock
        public int StockMinimo { get; set; }          // mínimo permitido antes de alerta
        public bool Visible { get; set; }             // si el producto se muestra o no en la UI
    }
}
