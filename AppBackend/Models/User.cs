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
        [MaxLength(100)]
        public string? Name { get; set; }

        [Required]
        [MaxLength(100)]
        public string? LastName { get; set; }

        [Required]
        [MaxLength(100)]
        public string? MothersMaidenName { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(200)]
        public string? Email { get; set; }

        [Required]
        [Phone]
        public string? CellPhone { get; set; }

        [Required]
        public string? PasswordHash { get; set; }

        // Logical deletion flag
        public bool Status { get; set; } = true;

        // Navigation property for user roles
        public ICollection<UserRole>? UserRoles { get; set; }
    }
}
