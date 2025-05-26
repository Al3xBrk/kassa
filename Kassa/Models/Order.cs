namespace Kassa;

public class Order
{
    public int Id { get; set; }
    public int HallId { get; set; }
    public Hall Hall { get; set; } = null!;
    public int TableNumber { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime OrderDate { get; set; }
    public int StatusId { get; set; }
    public OrderStatus Status { get; set; } = null!;
    public List<OrderItem> Items { get; set; } = new List<OrderItem>();
    public int? UserId { get; set; }
    public Kassa.Models.User? User { get; set; }
    public int? PaymentMethodId { get; set; }
    public Kassa.Models.PaymentMethod? PaymentMethod { get; set; }
    public DateTime? PaymentTime { get; set; }
    public decimal? CashGiven { get; set; }
    public decimal? Change { get; set; }
}