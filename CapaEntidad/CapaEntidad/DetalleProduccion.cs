namespace CapaEntidad
{
    public class DetalleProduccion
    {
        public int IdDetalleProduccion { get; set; }
        public int IdProduccion { get; set; }
        public int IdMateria { get; set; }

        public string NombreProducto { get; set; }
        public decimal CantidadUtilizada { get; set; }
        public string Unidad { get; set; }
    }
}
