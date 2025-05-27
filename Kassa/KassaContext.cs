using Microsoft.EntityFrameworkCore;
using Kassa.Models;

namespace Kassa
{
    public class KassaContext : DbContext
    {
        public DbSet<DishGroup> DishGroups { get; set; }
        public DbSet<Dish> Dishes { get; set; }
        public DbSet<Hall> Halls { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<OrderStatus> OrderStatuses { get; set; }
        public DbSet<TableReservation> TableReservations { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Shift> Shifts { get; set; }
        public DbSet<PaymentMethod> PaymentMethods { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                //optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=kassa;TrustServerCertificate=True;");
                optionsBuilder.UseNpgsql("Host=localhost;Database=kassa;Username=postgres;Password=manager");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Hall>().HasData(
                new Hall { Id = 1, Name = "Веранда", TableCount = 6 },
                new Hall { Id = 2, Name = "Основной зал", TableCount = 12 },
                new Hall { Id = 3, Name = "Второй этаж", TableCount = 8 }
            ); modelBuilder.Entity<DishGroup>().HasData(
                new DishGroup { Id = 1, Name = "Супы" },
                new DishGroup { Id = 2, Name = "Салаты" },
                new DishGroup { Id = 3, Name = "Напитки" },
                new DishGroup { Id = 4, Name = "Горячие блюда" },
                new DishGroup { Id = 5, Name = "Закуски" },
                new DishGroup { Id = 6, Name = "Десерты" },
                new DishGroup { Id = 7, Name = "Пицца" },
                new DishGroup { Id = 8, Name = "Паста" },
                new DishGroup { Id = 9, Name = "Гриль" }
            );

            modelBuilder.Entity<Dish>().HasData(
                // Супы
                new Dish { Id = 2, Name = "Солянка мясная", Price = 200, DishGroupId = 1 },
                new Dish { Id = 3, Name = "Крем-суп грибной", Price = 180, DishGroupId = 1 },
                new Dish { Id = 4, Name = "Суп-лапша куриная", Price = 120, DishGroupId = 1 },

                // Салаты
                new Dish { Id = 5, Name = "Цезарь с курицей", Price = 250, DishGroupId = 2 },
                new Dish { Id = 6, Name = "Оливье классический", Price = 180, DishGroupId = 2 },
                new Dish { Id = 7, Name = "Греческий салат", Price = 220, DishGroupId = 2 },
                new Dish { Id = 8, Name = "Салат из свежих овощей", Price = 160, DishGroupId = 2 },
                new Dish { Id = 9, Name = "Салат с креветками", Price = 380, DishGroupId = 2 },

                // Напитки
                new Dish { Id = 10, Name = "Чай черный", Price = 50, DishGroupId = 3 },
                new Dish { Id = 11, Name = "Кофе американо", Price = 100, DishGroupId = 3 },
                new Dish { Id = 12, Name = "Капучино", Price = 130, DishGroupId = 3 },
                new Dish { Id = 13, Name = "Латте", Price = 140, DishGroupId = 3 },
                new Dish { Id = 14, Name = "Сок апельсиновый", Price = 80, DishGroupId = 3 },
                new Dish { Id = 15, Name = "Морс клюквенный", Price = 70, DishGroupId = 3 },

                // Горячие блюда
                new Dish { Id = 16, Name = "Котлета по-киевски", Price = 320, DishGroupId = 4 },
                new Dish { Id = 17, Name = "Бефстроганов", Price = 390, DishGroupId = 4 },
                new Dish { Id = 18, Name = "Курица в сливочном соусе", Price = 280, DishGroupId = 4 },
                new Dish { Id = 19, Name = "Рыба запеченная", Price = 350, DishGroupId = 4 },
                new Dish { Id = 20, Name = "Свинина на гриле", Price = 420, DishGroupId = 4 },

                // Закуски
                new Dish { Id = 21, Name = "Брускетта с томатами", Price = 150, DishGroupId = 5 },
                new Dish { Id = 22, Name = "Сырная тарелка", Price = 450, DishGroupId = 5 },
                new Dish { Id = 23, Name = "Мясная тарелка", Price = 520, DishGroupId = 5 },
                new Dish { Id = 24, Name = "Маринованные овощи", Price = 120, DishGroupId = 5 },

                // Десерты
                new Dish { Id = 25, Name = "Тирамису", Price = 180, DishGroupId = 6 },
                new Dish { Id = 26, Name = "Чизкейк Нью-Йорк", Price = 160, DishGroupId = 6 },
                new Dish { Id = 27, Name = "Мороженое", Price = 90, DishGroupId = 6 },
                new Dish { Id = 28, Name = "Фруктовый салат", Price = 140, DishGroupId = 6 },

                // Пицца
                new Dish { Id = 29, Name = "Пицца Маргарита", Price = 350, DishGroupId = 7 },
                new Dish { Id = 30, Name = "Пицца Пепперони", Price = 420, DishGroupId = 7 },
                new Dish { Id = 31, Name = "Пицца 4 сыра", Price = 450, DishGroupId = 7 },
                new Dish { Id = 32, Name = "Пицца с грибами", Price = 380, DishGroupId = 7 },

                // Паста
                new Dish { Id = 33, Name = "Спагетти Болоньезе", Price = 280, DishGroupId = 8 },
                new Dish { Id = 34, Name = "Паста Карбонара", Price = 320, DishGroupId = 8 },
                new Dish { Id = 35, Name = "Феттучини Альфредо", Price = 300, DishGroupId = 8 },

                // Гриль
                new Dish { Id = 36, Name = "Стейк рибай", Price = 890, DishGroupId = 9 },
                new Dish { Id = 37, Name = "Стейк филе миньон", Price = 1200, DishGroupId = 9 },
                new Dish { Id = 38, Name = "Куриные крылышки", Price = 220, DishGroupId = 9 },
                new Dish { Id = 39, Name = "Семга на гриле", Price = 480, DishGroupId = 9 },

            ); modelBuilder.Entity<OrderStatus>().HasData(
                new OrderStatus { Id = 1, Name = "Создан" },
                new OrderStatus { Id = 2, Name = "Оплачен" }
            ); modelBuilder.Entity<PaymentMethod>().HasData(
                new PaymentMethod { Id = 1, Name = "Наличные" },
                new PaymentMethod { Id = 2, Name = "Карта" }
            );            // Создаем несколько тестовых заказов
            modelBuilder.Entity<Order>().HasData(
                new Order
                {
                    Id = 1,
                    TableNumber = 5,
                    OrderDate = new DateTime(2025, 5, 25, 12, 30, 0),
                    StatusId = 2,
                    TotalAmount = 730,
                    PaymentMethodId = 1,
                    PaymentTime = new DateTime(2025, 5, 25, 13, 15, 0),
                    HallId = 1
                },
                new Order
                {
                    Id = 2,
                    TableNumber = 8,
                    OrderDate = new DateTime(2025, 5, 25, 14, 20, 0),
                    StatusId = 2,
                    TotalAmount = 1250,
                    PaymentMethodId = 2,
                    PaymentTime = new DateTime(2025, 5, 25, 15, 30, 0),
                    HallId = 2
                },
                new Order
                {
                    Id = 3,
                    TableNumber = 3,
                    OrderDate = new DateTime(2025, 5, 26, 18, 45, 0),
                    StatusId = 2,
                    TotalAmount = 890,
                    PaymentMethodId = 1,
                    PaymentTime = new DateTime(2025, 5, 26, 19, 30, 0),
                    HallId = 1
                },
                new Order
                {
                    Id = 4,
                    TableNumber = 12,
                    OrderDate = new DateTime(2025, 5, 27, 19, 15, 0),
                    StatusId = 1,
                    TotalAmount = 650,
                    PaymentMethodId = null,
                    PaymentTime = null,
                    HallId = 2
                }
            );// Создаем позиции заказов
            modelBuilder.Entity<OrderItem>().HasData(
                // Заказ 1 (стол 5)
                new OrderItem { Id = 1, OrderId = 1, DishId = 1, DishGroupId = 1, Price = 150, GuestNumber = 1 }, // Борщ украинский
                new OrderItem { Id = 2, OrderId = 1, DishId = 5, DishGroupId = 2, Price = 250, GuestNumber = 1 }, // Цезарь с курицей
                new OrderItem { Id = 3, OrderId = 1, DishId = 16, DishGroupId = 4, Price = 320, GuestNumber = 1 }, // Котлета по-киевски
                new OrderItem { Id = 4, OrderId = 1, DishId = 11, DishGroupId = 3, Price = 100, GuestNumber = 1 }, // Кофе американо

                // Заказ 2 (стол 8)
                new OrderItem { Id = 5, OrderId = 2, DishId = 36, DishGroupId = 9, Price = 890, GuestNumber = 1 }, // Стейк рибай
                new OrderItem { Id = 6, OrderId = 2, DishId = 7, DishGroupId = 2, Price = 220, GuestNumber = 1 }, // Греческий салат
                new OrderItem { Id = 7, OrderId = 2, DishId = 13, DishGroupId = 3, Price = 140, GuestNumber = 1 }, // Латте

                // Заказ 3 (стол 3)
                new OrderItem { Id = 8, OrderId = 3, DishId = 29, DishGroupId = 7, Price = 350, GuestNumber = 1 }, // Пицца Маргарита
                new OrderItem { Id = 9, OrderId = 3, DishId = 33, DishGroupId = 8, Price = 280, GuestNumber = 1 }, // Спагетти Болоньезе
                new OrderItem { Id = 10, OrderId = 3, DishId = 25, DishGroupId = 6, Price = 180, GuestNumber = 1 }, // Тирамису
                new OrderItem { Id = 11, OrderId = 3, DishId = 14, DishGroupId = 3, Price = 80, GuestNumber = 1 }, // Сок апельсиновый

                // Заказ 4 (стол 12) - текущий активный заказ
                new OrderItem { Id = 12, OrderId = 4, DishId = 2, DishGroupId = 1, Price = 200, GuestNumber = 1 }, // Солянка мясная
                new OrderItem { Id = 13, OrderId = 4, DishId = 30, DishGroupId = 7, Price = 420, GuestNumber = 1 }, // Пицца Пепперони
                new OrderItem { Id = 14, OrderId = 4, DishId = 10, DishGroupId = 3, Price = 50, GuestNumber = 1 } // Чай черный
            );

            modelBuilder.Entity<TableReservation>(entity =>
            {
                // Удалено: HasConversion для DateTimeKind
            });

            modelBuilder.Entity<TableReservation>()
                .Property(r => r.Date)
                .HasColumnType("timestamp without time zone");

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.Items)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Dish)
                .WithMany()
                .HasForeignKey(oi => oi.DishId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.DishGroup)
                .WithMany()
                .HasForeignKey(oi => oi.DishGroupId)
                .OnDelete(DeleteBehavior.Restrict); modelBuilder.Entity<Order>()
                .Property(o => o.OrderDate)
                .HasColumnType("timestamp without time zone");

            modelBuilder.Entity<Order>()
                .Property(o => o.PaymentTime)
                .HasColumnType("timestamp without time zone");

            modelBuilder.Entity<Shift>()
                .Property(o => o.OpenedAt)
                .HasColumnType("timestamp without time zone");

            modelBuilder.Entity<Shift>()
                .Property(o => o.ClosedAt)
                .HasColumnType("timestamp without time zone");

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(u => u.FullName).IsUnique();
                entity.Property(u => u.FullName).IsRequired();
                entity.Property(u => u.Role).IsRequired();
                entity.Property(u => u.PasswordHash).IsRequired();
                entity.Property(u => u.AutoLogoutMinutes).IsRequired().HasDefaultValue(10);
            });

            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    FullName = "АдминАА",
                    Role = UserRole.Administrator,
                    PasswordHash = "8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92",
                    AutoLogoutMinutes = 10
                }
            );
        }
    }
}
