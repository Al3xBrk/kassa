using System.ComponentModel.DataAnnotations;

namespace Kassa.Models;

public class PaymentMethod
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = null!;
}
