using System;
using System.Collections.Generic;

namespace CapaEntidad
{
    public class Venta
    {
        public int IdVenta { get; set; }
        public DateTime FechaVenta { get; set; }
        public int IdCliente { get; set; }
        public int IdVendedor { get; set; }
        public decimal Total { get; set; }
        public List<ItemVenta> Items { get; set; }

        // Propiedades para mostrar información (no se guardan en la BD)
        public string Cliente { get; set; }
        public string DocumentoCliente { get; set; }
        public string NombreVendedor { get; set; }
    }
}