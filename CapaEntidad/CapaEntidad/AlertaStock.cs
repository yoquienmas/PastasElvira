using System;

public class AlertaStock
{
    public int IdAlerta { get; set; }
    public int IdProducto { get; set; }
    public DateTime FechaAlerta { get; set; }
    public string Tipo { get; set; } // ✅ AGREGADO
    public string NombreProducto { get; set; }
    public string Mensaje { get; set; }
    public int StockActual { get; set; }
    public int StockMinimo { get; set; }
}