using System.ComponentModel.DataAnnotations;

namespace Kassa.Models
{
    public enum UserRole
    {
        Cashier,
        Administrator
    }

    public class User
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string FullName { get; set; } // ФИО
        [Required]
        public UserRole Role { get; set; }
        [Required]
        public string PasswordHash { get; set; }
        public int AutoLogoutMinutes { get; set; } = 10; // по умолчанию 10 минут
    }
}
