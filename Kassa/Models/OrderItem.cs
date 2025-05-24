namespace Kassa;

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int DishId { get; set; }
    public Dish Dish { get; set; } = null!;
    public int DishGroupId { get; set; }
    public DishGroup DishGroup { get; set; } = null!;
    public decimal Price { get; set; }
    public int GuestNumber { get; set; } // номер гостя, которому принадлежит блюдо

    public Order Order { get; set; } = null!;
}