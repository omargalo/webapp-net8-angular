namespace AppBackend.Models
{
    public class CatRole
    {
        public int Id { get; set; } // Role ID
        public string? Name { get; set; } // Role Name (Administrador, Colaborador, etc.)

        // Logical deletion flag
        public bool Status { get; set; } = true;
    }

}
