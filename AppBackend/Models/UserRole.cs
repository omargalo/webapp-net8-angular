namespace AppBackend.Models
{
    public class UserRole
    {
        public int UserId { get; set; }
        public User? User { get; set; }

        public int RoleId { get; set; }
        public CatRole? Role { get; set; }

        // Logical deletion flag
        public bool Status { get; set; } = true;
    }

}
