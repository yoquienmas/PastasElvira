using System;

namespace CapaEntidad
{
    public class Producto
    {
        public int IdProducto { get; set; }
        public string Nombre { get; set; }
        public string Tipo { get; set; }
        public string Sabor { get; set; } // Nueva propiedad
        public decimal PrecioVenta { get; set; }
        public bool Visible { get; set; }
        public decimal CostoProduccion { get; set; }
        public decimal MargenGanancia { get; set; }
        public int StockActual { get; set; }
        public int StockMinimo { get; set; }
        public bool EsProductoBase { get; set; }
        public int IdTipo { get; set; } // Nueva propiedad FK
        public int IdSabor { get; set; } // Nueva propiedad FK

        public string NombreProducto
        {
            get
            {
                // Validar que Nombre no sea null o vacío
                if (string.IsNullOrEmpty(Nombre))
                    return "Producto sin nombre";

                // Si Tipo es null o vacío, mostrar solo el Nombre
                if (string.IsNullOrEmpty(Tipo) || Tipo == "NULL")
                    return Nombre;
                else
                    return $"{Tipo} {Nombre}";
            }
        }

        // Propiedad computada para mostrar el nombre completo (Sabor + Tipo)
        public string NombreCompleto
        {
            get
            {
                if (!string.IsNullOrEmpty(Sabor) && !string.IsNullOrEmpty(Tipo))
                    return $"{Sabor} {Tipo}";
                else if (!string.IsNullOrEmpty(Nombre))
                    return Nombre;
                else
                    return "Producto sin nombre";
            }
        }

        // Propiedades adicionales para compatibilidad
        public decimal Precio => PrecioVenta;
        public int Stock => StockActual;
        public string Estado => Visible ? "Activo" : "Inactivo";

        public Producto()
        {
            // Valores por defecto
            Nombre = string.Empty;
            Tipo = string.Empty;
            Sabor = string.Empty; // Inicializar nueva propiedad
            Visible = true;
            CostoProduccion = 0;
            MargenGanancia = 0;
            PrecioVenta = 0;
            StockActual = 0;
            StockMinimo = 0;
            EsProductoBase = true;
            IdTipo = 0; // Inicializar nueva propiedad
            IdSabor = 0; // Inicializar nueva propiedad
        }

        // Método para facilitar la migración desde la estructura anterior
        public void ActualizarDesdeViejaEstructura(string nombre, string tipo)
        {
            // En la estructura anterior, "Nombre" era el sabor y "Tipo" era el tipo
            this.Sabor = nombre ?? string.Empty;
            this.Tipo = tipo ?? string.Empty;
            this.Nombre = $"{Sabor} {Tipo}".Trim();
        }
    }
}