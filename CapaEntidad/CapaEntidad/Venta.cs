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
        public int MetodoPagoId { get; set; } // ✅ PARA GUARDAR EN BD

        // Propiedades para mostrar información (NO se guardan en BD)
        public string Cliente { get; set; }
        public string DocumentoCliente { get; set; }
        public string NombreVendedor { get; set; }

        // Propiedad para compatibilidad con código existente
        public string MetodoPago
        {
            get
            {
                // Convertir el ID a string para compatibilidad
                switch (MetodoPagoId)
                {
                    case 1: return "Efectivo";
                    case 2: return "TarjetaDebito";
                    case 3: return "TarjetaCredito";
                    case 4: return "BilleteraVirtual";
                    default: return "Efectivo";
                }
            }
            set
            {
                // Convertir string a ID
                switch (value)
                {
                    case "Efectivo": MetodoPagoId = 1; break;
                    case "TarjetaDebito": MetodoPagoId = 2; break;
                    case "TarjetaCredito": MetodoPagoId = 3; break;
                    case "BilleteraVirtual": MetodoPagoId = 4; break;
                    default: MetodoPagoId = 1; break;
                }
            }
        }
    }
}