public class ReporteProductoVendido
{
    public string NombreProducto { get; set; }
    public int CantidadVendida { get; set; }
    public decimal TotalVendido { get; set; }
    public string Tipo { get; set; } // Agregar esta propiedad para el filtro por tipo
}