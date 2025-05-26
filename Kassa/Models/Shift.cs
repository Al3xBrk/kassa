using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kassa.Models
{
    public class Shift
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int CashierId { get; set; }
        [ForeignKey("CashierId")]
        public User Cashier { get; set; } = null!;
        [Required]
        public DateTime OpenedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
    }
}
