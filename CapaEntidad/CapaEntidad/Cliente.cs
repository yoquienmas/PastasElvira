namespace CapaEntidad
{
    public class Cliente
    {
        public int IdCliente { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Documento { get; set; }
        public string Telefono { get; set; }
        public string Email { get; set; }
        public string Direccion { get; set; }
        public string Cuil { get; set; } // ← Esto debe coincidir con la BD
        public bool Activo { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}