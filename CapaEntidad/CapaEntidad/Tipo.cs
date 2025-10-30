namespace CapaEntidad
{
    public class Tipo
    {
        public int IdTipo { get; set; }
        public string Descripcion { get; set; }
        public bool Activo { get; set; }

        public Tipo()
        {
            Descripcion = string.Empty;
            Activo = true;
        }
    }

}