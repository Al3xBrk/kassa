namespace Kassa;

public class DishGroup
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty; // Default value to avoid null warnings
    public List<Dish> Dishes { get; set; } = new();
}