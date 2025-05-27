namespace Kassa;

public class Dish
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty; // Default value to avoid null warnings
    public decimal Price { get; set; }
    public int DishGroupId { get; set; }
    public DishGroup DishGroup { get; set; } = null!; // Non-nullable reference type with default initialization
    public bool IsDeleted { get; set; } = false; // Мягкое удаление
}