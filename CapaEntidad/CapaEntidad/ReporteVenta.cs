using System;

namespace CapaEntidad
{
    public class ReporteVenta
    {
        public int IdVenta { get; set; }
        public DateTime Fecha { get; set; }
        public string Cliente { get; set; }
        public string DNI { get; set; }
        public string Usuario { get; set; }
        public int IdUsuario { get; set; }
        public decimal Total { get; set; }
        public int CantidadProductos { get; set; }
        public string Productos { get; set; }

        // ✅ AGREGAR ESTA PROPIEDAD
        public int MetodoPago { get; set; }

        // Propiedad para mostrar el texto del método de pago
        public string MetodoPagoTexto
        {
            get
            {
                return MetodoPago switch
                {
                    1 => "EFECTIVO",
                    2 => "TARJETA DÉBITO",
                    3 => "TARJETA CRÉDITO",
                    4 => "BILLETERA VIRTUAL",
                    _ => "DESCONOCIDO"
                };
            }
        }
    }
}