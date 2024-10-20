using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace AppBackend.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string? Username { get; set; }

        [Required]
        public string? PasswordHash { get; set; }

        // Logical deletion flag
        public bool Status { get; set; } = true;

        // Navigation property for user roles
        public ICollection<UserRole>? UserRoles { get; set; }

    }
}
