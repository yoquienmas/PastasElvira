namespace CapaEntidad
{
    public class DetalleReceta
    {
        public int IdProductoMateriaPrima { get; set; }
        public int IdProducto { get; set; }
        public int IdMateria { get; set; }
        public string NombreMateria { get; set; }
        public float CantidadNecesaria { get; set; }
        public string Unidad { get; set; }
    }
}