namespace CapaEntidad
{
    public class MateriaPrima
    {
        public int IdMateria { get; set; }
        public string Nombre { get; set; }
        public string Unidad { get; set; }
        public float CantidadDisponible { get; set; }
        public int StockMinimo { get; set; }
        public decimal PrecioUnitario { get; set; }
    }
}