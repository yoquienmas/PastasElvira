using System;

    namespace CapaEntidad
    {
        public class ReporteVenta
        {
            public int IdVenta { get; set; }
            public DateTime Fecha { get; set; }
            public string Cliente { get; set; }
            public string DNI { get; set; }
            public decimal Total { get; set; }

            // ✅ AGREGAR ESTAS PROPIEDADES FALTANTES
            public string Usuario { get; set; }
            public int IdUsuario { get; set; }
            public string Productos { get; set; }
            public int CantidadProductos { get; set; } // Para el dashboard
        }
    }