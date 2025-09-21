using System;

namespace CapaEntidad
{
    public class Producto
    {
        public int IdProducto { get; set; }
        public string Nombre { get; set; }
        public string Tipo { get; set; }
        public decimal PrecioVenta { get; set; }
        public bool Visible { get; set; }
        public decimal CostoProduccion { get; set; }
        public decimal MargenGanancia { get; set; }
        public int StockActual { get; set; }
        public int StockMinimo { get; set; }
        public bool EsProductoBase { get; set; }

        // Propiedades adicionales para compatibilidad
        public decimal Precio => PrecioVenta;
        public int Stock => StockActual;
        public string Estado => Visible ? "Activo" : "Inactivo";

        public Producto()
        {
            // Valores por defecto
            Visible = true;
            CostoProduccion = 0;
            MargenGanancia = 0;
            PrecioVenta = 0;
            StockActual = 0;
            StockMinimo = 0;
            EsProductoBase = true;
        }
    }
}