namespace CapaEntidad
{
    public class ItemVenta
    {
        public int IdProducto { get; set; }
        public string NombreProducto { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }

        // Propiedad calculada
        public decimal Subtotal
        {
            get { return Cantidad * PrecioUnitario; }
        }
    }
}
