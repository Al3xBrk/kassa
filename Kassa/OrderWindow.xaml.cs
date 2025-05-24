using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;

namespace Kassa
{
    public partial class OrderWindow : Window
    {
        private readonly KassaContext _context = new KassaContext();
        private readonly int _hallId;
        private readonly int _tableNumber;
        private readonly List<DishViewModel> SelectedDishes = new();
        private readonly bool _viewMode = false;
        private readonly Order? _viewOrder;

        // Модель для гостя
        public class GuestViewModel
        {
            public int Number { get; set; }
            public string DisplayName => $"Гость {Number}";
            public List<DishViewModel> Dishes { get; set; } = new();
        }

        private readonly List<GuestViewModel> Guests = new();
        private GuestViewModel? SelectedGuest;

        // Конструктор для создания нового заказа
        public OrderWindow(int hallId, int tableNumber)
        {
            InitializeComponent();
            _hallId = hallId;
            _tableNumber = tableNumber;
            LoadGroups();
            AddFirstGuest();
        }

        // Конструктор для просмотра существующего заказа
        public OrderWindow(Order order)
        {
            InitializeComponent();
            _viewMode = true;
            _viewOrder = order;
            _hallId = order.HallId;
            _tableNumber = order.TableNumber;
            LoadOrderInfo();
        }

        private void LoadGroups()
        {
            var groups = _context.DishGroups.Include(g => g.Dishes).ToList();
            Console.WriteLine($"Loaded {groups.Count} groups from the database.");

            var groupDictionary = new Dictionary<string, List<DishViewModel>>();

            foreach (var group in groups)
            {
                Console.WriteLine($"Group: {group.Name}, Dishes: {group.Dishes.Count}");
                groupDictionary[group.Name] = group.Dishes.Select(d => new DishViewModel
                {
                    Id = d.Id,
                    DishGroupId = group.Id,
                    Name = d.Name,
                    Price = d.Price
                }).ToList();
            }

            GroupsListBox.ItemsSource = groupDictionary.Keys;
            GroupsListBox.Tag = groupDictionary; // Store the dictionary for later use
        }

        private void GroupsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GroupsListBox.SelectedItem is string groupName && GroupsListBox.Tag is Dictionary<string, List<DishViewModel>> groupDictionary && groupDictionary.TryGetValue(groupName, out var dishes))
            {
                Console.WriteLine($"Selected group: {groupName}, Dishes: {dishes.Count}");
                DishesListBox.ItemsSource = dishes;
            }
            else
            {
                Console.WriteLine("No dishes found for the selected group.");
                DishesListBox.ItemsSource = null;
            }
        }

        private void AddFirstGuest()
        {
            Guests.Clear();
            Guests.Add(new GuestViewModel { Number = 1 });
            SelectedGuest = Guests[0];
            RefreshOrderListBox();
        }

        private void AddGuestButton_Click(object sender, RoutedEventArgs e)
        {
            var guest = new GuestViewModel { Number = Guests.Count + 1 };
            Guests.Add(guest);
            SelectedGuest = guest;
            RefreshOrderListBox();
            OrderListBox.SelectedIndex = Guests.Count - 1;
        }

        private void OrderListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (OrderListBox.SelectedIndex >= 0 && OrderListBox.SelectedIndex < Guests.Count)
            {
                SelectedGuest = Guests[OrderListBox.SelectedIndex];
            }
        }

        private void AddDishToOrder_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is DishViewModel dish)
            {
                if (SelectedGuest == null)
                {
                    MessageBox.Show("Сначала добавьте гостя.");
                    return;
                }
                SelectedGuest.Dishes.Add(dish);
                RefreshOrderListBox();
                UpdateTotalAmount();
            }
        }

        private void RefreshOrderListBox()
        {
            // Для отображения блюда с ценой
            foreach (var guest in Guests)
            {
                foreach (var d in guest.Dishes)
                {
                    d.NameAndPrice = $"{d.Name} ({d.Price:N2} BYN)";
                }
            }
            OrderListBox.ItemsSource = null;
            OrderListBox.ItemsSource = Guests;
        }

        private void UpdateTotalAmount()
        {
            var total = Guests.SelectMany(g => g.Dishes).Sum(d => d.Price);
            TotalAmountTextBlock.Text = $"{total:N2} BYN";
        }

        private void SaveOrder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var total = Guests.SelectMany(g => g.Dishes).Sum(d => d.Price);
                var order = new Order
                {
                    HallId = _hallId,
                    TableNumber = _tableNumber,
                    TotalAmount = total,
                    OrderDate = DateTime.UtcNow,
                    StatusId = 1 // "Создан"
                };
                _context.Orders.Add(order);
                _context.SaveChanges();
                foreach (var guest in Guests)
                {
                    foreach (var dish in guest.Dishes)
                    {
                        _context.OrderItems.Add(new OrderItem
                        {
                            OrderId = order.Id,
                            DishId = dish.Id,
                            DishGroupId = dish.DishGroupId,
                            Price = dish.Price,
                         
                            GuestNumber = guest.Number
                        });
                    }
                }
                _context.SaveChanges();
                MessageBox.Show("Заказ сохранён", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadOrderInfo()
        {
            if (_viewOrder == null) return;
            var items = _context.OrderItems
                .Where(oi => oi.OrderId == _viewOrder.Id)
                .Include(oi => oi.Dish)
                .ToList();
            Guests.Clear();
            // Группируем блюда по номеру гостя
            var grouped = items.GroupBy(i => i.GuestNumber).OrderBy(g => g.Key);
            foreach (var group in grouped)
            {
                var guest = new GuestViewModel { Number = group.Key };
                foreach (var item in group)
                {
                    guest.Dishes.Add(new DishViewModel { Id = item.DishId, DishGroupId = item.DishGroupId, Name = item.Dish.Name, Price = item.Price });
                }
                Guests.Add(guest);
            }
            SelectedGuest = Guests.FirstOrDefault();
            RefreshOrderListBox();
            UpdateTotalAmount();
            GroupsListBox.IsEnabled = false;
            DishesListBox.IsEnabled = false;
            SaveOrderButton.IsEnabled = false;
            PayOrderButton.Visibility = Visibility.Visible;
        }

        private void PayOrder_Click(object sender, RoutedEventArgs e)
        {
            if (_viewOrder != null)
            {
                var order = _context.Orders.FirstOrDefault(o => o.Id == _viewOrder.Id);
                if (order != null)
                {
                    order.StatusId = 2; // "Оплачен"
                    _context.SaveChanges();
                    MessageBox.Show("Заказ оплачен", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    Close();
                }
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            _context.Dispose();
            base.OnClosed(e);
        }
    }

    public class DishViewModel
    {
        public int Id { get; set; }
        public int DishGroupId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string NameAndPrice { get; set; } = string.Empty;
    }
}
