using Microsoft.EntityFrameworkCore;

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
        public DbSet<Kassa.Models.TableReservation> TableReservations { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // optionsBuilder.UseSqlServer("Server=localhost;Database=kassa;User Id=sa;Password=yourStrong(!)Password;TrustServerCertificate=True;");
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
            );

            modelBuilder.Entity<DishGroup>().HasData(
                new DishGroup { Id = 1, Name = "Супы" },
                new DishGroup { Id = 2, Name = "Салаты" },
                new DishGroup { Id = 3, Name = "Напитки" }
            );

            modelBuilder.Entity<Dish>().HasData(
                new Dish { Id = 1, Name = "Борщ", Price = 150, DishGroupId = 1 },
                new Dish { Id = 2, Name = "Солянка", Price = 200, DishGroupId = 1 },
                new Dish { Id = 3, Name = "Цезарь", Price = 250, DishGroupId = 2 },
                new Dish { Id = 4, Name = "Оливье", Price = 180, DishGroupId = 2 },
                new Dish { Id = 5, Name = "Чай", Price = 50, DishGroupId = 3 },
                new Dish { Id = 6, Name = "Кофе", Price = 100, DishGroupId = 3 }
            );

            modelBuilder.Entity<OrderStatus>().HasData(
                new OrderStatus { Id = 1, Name = "Создан" },
                new OrderStatus { Id = 2, Name = "Оплачен" }
            );

            modelBuilder.Entity<Kassa.Models.TableReservation>(entity =>
            {
                // Удалено: HasConversion для DateTimeKind
            });

            modelBuilder.Entity<Kassa.Models.TableReservation>()
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
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Order>()
                .Property(o => o.OrderDate)
                .HasColumnType("timestamp without time zone");
        }
    }
}
