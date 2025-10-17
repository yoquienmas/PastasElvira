using System;

namespace CapaEntidad
{
    public class MetodoPago
    {
        public int IdMetodoPago { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public bool Activo { get; set; }

        // Propiedad para el icono
        public string Icono
        {
            get
            {
                switch (Nombre.ToLower())
                {
                    case "efectivo": return "💵";
                    case "tarjetadebito": return "💳";
                    case "tarjetacredito": return "💳";
                    case "billeteravirtual": return "📱";
                    default: return "💰";
                }
            }
        }

        // Propiedad para mostrar en el ComboBox - formateada correctamente
        public string DisplayText => $"{Icono} {Nombre.ToUpper()}";

        // Propiedad para el ItemTemplate
        public string TextoItem => $"{Icono} {Nombre.ToUpper()}";
    }
}