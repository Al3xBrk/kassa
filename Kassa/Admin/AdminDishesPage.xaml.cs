using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Kassa
{
    public partial class AdminDishesPage : Page
    {
        private readonly KassaContext _context = new KassaContext();
        public AdminDishesPage()
        {
            InitializeComponent();
            LoadGroups();
        }
        private void LoadGroups()
        {
            GroupsListBox.Items.Clear();
            var groups = _context.DishGroups.Where(g => !g.IsDeleted).ToList();
            foreach (var group in groups)
            {
                GroupsListBox.Items.Add(group.Name);
            }
        }

        private void RefreshDishesForSelectedGroup()
        {
            if (GroupsListBox.SelectedItem is string groupName)
            {
                var group = _context.DishGroups.Include(g => g.Dishes).FirstOrDefault(g => g.Name == groupName);
                if (group != null)
                {
                    DishesListBox.Items.Clear();
                    foreach (var dish in group.Dishes.Where(d => !d.IsDeleted))
                    {
                        DishesListBox.Items.Add($"{dish.Name} ({dish.Price:C2})");
                    }
                }
            }
        }

        private void AddGroup_Click(object sender, RoutedEventArgs e)
        {
            var addGroupWindow = new AddGroupWindow();
            if (addGroupWindow.ShowDialog() == true)
            {
                var groupName = addGroupWindow.GroupName;
                if (!string.IsNullOrEmpty(groupName) && !_context.DishGroups.Any(g => g.Name == groupName))
                {
                    var newGroup = new DishGroup { Name = groupName };
                    _context.DishGroups.Add(newGroup);
                    _context.SaveChanges();
                    LoadGroups();
                }
                else
                {
                    ModernMessageBox.Show("Группа с таким названием уже существует.");
                }
            }
        }
        private void GroupsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GroupsListBox.SelectedItem is string groupName)
            {
                var group = _context.DishGroups.Include(g => g.Dishes).FirstOrDefault(g => g.Name == groupName);
                if (group != null)
                {
                    DishesListBox.Items.Clear();
                    foreach (var dish in group.Dishes.Where(d => !d.IsDeleted))
                    {
                        DishesListBox.Items.Add($"{dish.Name} ({dish.Price:C2})");
                    }
                }
            }
        }

        private void AddDish_Click(object sender, RoutedEventArgs e)
        {
            if (GroupsListBox.SelectedItem is string groupName)
            {
                var group = _context.DishGroups.FirstOrDefault(g => g.Name == groupName);
                if (group != null)
                {
                    var addDishWindow = new AddDishWindow { Owner = Window.GetWindow(this) };
                    if (addDishWindow.ShowDialog() == true)
                    {
                        var name = addDishWindow.DishName;
                        var price = addDishWindow.DishPrice ?? 0;
                        var newDish = new Dish { Name = name, Price = price, DishGroupId = group.Id };
                        _context.Dishes.Add(newDish);
                        _context.SaveChanges();
                        DishesListBox.Items.Add($"{name} ({price:C2})");
                    }
                }
            }
            else
            {
                ModernMessageBox.Show("Сначала выберите группу.");
            }
        }

        private void EditGroup_Click(object sender, RoutedEventArgs e)
        {
            string? groupName = (sender as Button)?.DataContext as string;
            if (string.IsNullOrEmpty(groupName))
                groupName = GroupsListBox.SelectedItem as string;
            if (!string.IsNullOrEmpty(groupName))
            {
                var group = _context.DishGroups.FirstOrDefault(g => g.Name == groupName);
                if (group != null)
                {
                    var editGroupWindow = new AddGroupWindow(group.Name);
                    if (editGroupWindow.ShowDialog() == true)
                    {
                        var newName = editGroupWindow.GroupName;
                        if (!string.IsNullOrWhiteSpace(newName) && newName != group.Name)
                        {
                            if (_context.DishGroups.Any(g => g.Name == newName))
                            {
                                ModernMessageBox.Show("Группа с таким названием уже существует.");
                                return;
                            }
                            group.Name = newName;
                            _context.SaveChanges();
                            LoadGroups();
                        }
                    }
                }
            }
        }
        private void DeleteGroup_Click(object sender, RoutedEventArgs e)
        {
            string? groupName = (sender as Button)?.DataContext as string;
            if (string.IsNullOrEmpty(groupName))
                groupName = GroupsListBox.SelectedItem as string;
            if (!string.IsNullOrEmpty(groupName))
            {
                var group = _context.DishGroups.Include(g => g.Dishes).FirstOrDefault(g => g.Name == groupName);
                if (group != null)
                {
                    if (ModernMessageBox.ShowQuestion($"Удалить группу '{group.Name}'?", "Подтверждение") == ModernMessageBoxResult.Yes)
                    {
                        // Проверяем, есть ли заказы с блюдами из этой группы
                        var hasOrdersWithDishes = _context.OrderItems.Any(oi => oi.DishGroupId == group.Id);

                        if (hasOrdersWithDishes)
                        {
                            // Мягкое удаление - помечаем группу и все её блюда как удаленные
                            group.IsDeleted = true;
                            foreach (var dish in group.Dishes)
                            {
                                dish.IsDeleted = true;
                            }

                            ModernMessageBox.ShowInformation(
                                $"Группа '{group.Name}' и все её блюда помечены как удаленные, так как используются в существующих заказах.\n" +
                                "Они больше не будут отображаться в меню, но останутся в истории заказов.",
                                "Группа скрыта"
                            );
                        }
                        else
                        {
                            // Жесткое удаление, если блюда группы не используются в заказах
                            _context.Dishes.RemoveRange(group.Dishes);
                            _context.DishGroups.Remove(group);
                            ModernMessageBox.ShowSuccess($"Группа '{group.Name}' и все её блюда полностью удалены.");
                        }

                        _context.SaveChanges();
                        LoadGroups();
                        DishesListBox.Items.Clear();
                    }
                }
            }
        }

        private void EditDish_Click(object sender, RoutedEventArgs e)
        {
            string? dishDisplay = (sender as Button)?.DataContext as string;
            if (string.IsNullOrEmpty(dishDisplay))
                dishDisplay = DishesListBox.SelectedItem as string;
            string? groupName = GroupsListBox.SelectedItem as string;
            if (!string.IsNullOrEmpty(groupName) && !string.IsNullOrEmpty(dishDisplay))
            {
                var group = _context.DishGroups.Include(g => g.Dishes).FirstOrDefault(g => g.Name == groupName);
                if (group != null)
                {
                    var dishName = dishDisplay.Split('(')[0].Trim();
                    var dish = group.Dishes.FirstOrDefault(d => d.Name == dishName);
                    if (dish != null)
                    {
                        var addDishWindow = new AddDishWindow { Owner = Window.GetWindow(this) };
                        addDishWindow.DishNameTextBox.Text = dish.Name;
                        addDishWindow.DishPriceTextBox.Text = dish.Price.ToString(); if (addDishWindow.ShowDialog() == true)
                        {
                            dish.Name = addDishWindow.DishName;
                            dish.Price = addDishWindow.DishPrice ?? dish.Price;
                            _context.SaveChanges();
                            RefreshDishesForSelectedGroup();
                        }
                    }
                }
            }
        }

        private void DeleteDish_Click(object sender, RoutedEventArgs e)
        {
            string? dishDisplay = (sender as Button)?.DataContext as string;
            if (string.IsNullOrEmpty(dishDisplay))
                dishDisplay = DishesListBox.SelectedItem as string;
            string? groupName = GroupsListBox.SelectedItem as string;
            if (!string.IsNullOrEmpty(groupName) && !string.IsNullOrEmpty(dishDisplay))
            {
                var group = _context.DishGroups.Include(g => g.Dishes).FirstOrDefault(g => g.Name == groupName);
                if (group != null)
                {
                    var dishName = dishDisplay.Split('(')[0].Trim();
                    var dish = group.Dishes.FirstOrDefault(d => d.Name == dishName);
                    if (dish != null)
                    {
                        if (ModernMessageBox.ShowQuestion($"Удалить блюдо '{dish.Name}'?", "Подтверждение") == ModernMessageBoxResult.Yes)
                        {
                            // Проверяем, есть ли заказы с этим блюдом
                            var hasOrdersWithDish = _context.OrderItems.Any(oi => oi.DishId == dish.Id);

                            if (hasOrdersWithDish)
                            {
                                // Мягкое удаление - помечаем блюдо как удаленное
                                dish.IsDeleted = true;

                                ModernMessageBox.ShowInformation(
                                    $"Блюдо '{dish.Name}' помечено как удаленное, так как используется в существующих заказах.\n" +
                                    "Оно больше не будет отображаться в меню, но останется в истории заказов.",
                                    "Блюдо скрыто"
                                );
                            }
                            else
                            {
                                // Жесткое удаление, если блюдо не используется в заказах
                                _context.Dishes.Remove(dish);
                                ModernMessageBox.ShowSuccess($"Блюдо '{dish.Name}' полностью удалено.");
                            }

                            _context.SaveChanges();

                            // Обновляем отображение списка блюд
                            if (GroupsListBox.SelectedItem is string selectedGroupName)
                            {
                                var selectedGroup = _context.DishGroups.Include(g => g.Dishes).FirstOrDefault(g => g.Name == selectedGroupName);
                                if (selectedGroup != null)
                                {
                                    DishesListBox.Items.Clear();
                                    foreach (var d in selectedGroup.Dishes.Where(d => !d.IsDeleted))
                                    {
                                        DishesListBox.Items.Add($"{d.Name} ({d.Price:C2})");
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
