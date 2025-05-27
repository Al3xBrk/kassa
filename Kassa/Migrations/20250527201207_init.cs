using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Kassa.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DishGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DishGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Halls",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    TableCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Halls", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrderStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PaymentMethods",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentMethods", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TableReservations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    HallId = table.Column<int>(type: "integer", nullable: false),
                    TableNumber = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    FromTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    ToTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TableReservations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FullName = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    AutoLogoutMinutes = table.Column<int>(type: "integer", nullable: false, defaultValue: 10)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Dishes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    DishGroupId = table.Column<int>(type: "integer", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dishes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Dishes_DishGroups_DishGroupId",
                        column: x => x.DishGroupId,
                        principalTable: "DishGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    HallId = table.Column<int>(type: "integer", nullable: false),
                    TableNumber = table.Column<int>(type: "integer", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    OrderDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    StatusId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: true),
                    PaymentMethodId = table.Column<int>(type: "integer", nullable: true),
                    PaymentTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CashGiven = table.Column<decimal>(type: "numeric", nullable: true),
                    Change = table.Column<decimal>(type: "numeric", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Orders_Halls_HallId",
                        column: x => x.HallId,
                        principalTable: "Halls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Orders_OrderStatuses_StatusId",
                        column: x => x.StatusId,
                        principalTable: "OrderStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Orders_PaymentMethods_PaymentMethodId",
                        column: x => x.PaymentMethodId,
                        principalTable: "PaymentMethods",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Orders_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Shifts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CashierId = table.Column<int>(type: "integer", nullable: false),
                    OpenedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ClosedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shifts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Shifts_Users_CashierId",
                        column: x => x.CashierId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrderId = table.Column<int>(type: "integer", nullable: false),
                    DishId = table.Column<int>(type: "integer", nullable: false),
                    DishGroupId = table.Column<int>(type: "integer", nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    GuestNumber = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderItems_DishGroups_DishGroupId",
                        column: x => x.DishGroupId,
                        principalTable: "DishGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderItems_Dishes_DishId",
                        column: x => x.DishId,
                        principalTable: "Dishes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderItems_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "DishGroups",
                columns: new[] { "Id", "IsDeleted", "Name" },
                values: new object[,]
                {
                    { 1, false, "Супы" },
                    { 2, false, "Салаты" },
                    { 3, false, "Напитки" },
                    { 4, false, "Горячие блюда" },
                    { 5, false, "Закуски" },
                    { 6, false, "Десерты" },
                    { 7, false, "Пицца" },
                    { 8, false, "Паста" },
                    { 9, false, "Гриль" },
                    { 10, false, "Алкогольные напитки" }
                });

            migrationBuilder.InsertData(
                table: "Halls",
                columns: new[] { "Id", "Name", "TableCount" },
                values: new object[,]
                {
                    { 1, "Веранда", 6 },
                    { 2, "Основной зал", 12 },
                    { 3, "Второй этаж", 8 }
                });

            migrationBuilder.InsertData(
                table: "OrderStatuses",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Создан" },
                    { 2, "Оплачен" }
                });

            migrationBuilder.InsertData(
                table: "PaymentMethods",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Наличные" },
                    { 2, "Карта" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "AutoLogoutMinutes", "FullName", "PasswordHash", "Role" },
                values: new object[] { 1, 10, "АдминАА", "8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92", 1 });

            migrationBuilder.InsertData(
                table: "Dishes",
                columns: new[] { "Id", "DishGroupId", "IsDeleted", "Name", "Price" },
                values: new object[,]
                {
                    { 1, 1, false, "Борщ украинский", 150m },
                    { 2, 1, false, "Солянка мясная", 200m },
                    { 3, 1, false, "Крем-суп грибной", 180m },
                    { 4, 1, false, "Суп-лапша куриная", 120m },
                    { 5, 2, false, "Цезарь с курицей", 250m },
                    { 6, 2, false, "Оливье классический", 180m },
                    { 7, 2, false, "Греческий салат", 220m },
                    { 8, 2, false, "Салат из свежих овощей", 160m },
                    { 9, 2, false, "Салат с креветками", 380m },
                    { 10, 3, false, "Чай черный", 50m },
                    { 11, 3, false, "Кофе американо", 100m },
                    { 12, 3, false, "Капучино", 130m },
                    { 13, 3, false, "Латте", 140m },
                    { 14, 3, false, "Сок апельсиновый", 80m },
                    { 15, 3, false, "Морс клюквенный", 70m },
                    { 16, 4, false, "Котлета по-киевски", 320m },
                    { 17, 4, false, "Бефстроганов", 390m },
                    { 18, 4, false, "Курица в сливочном соусе", 280m },
                    { 19, 4, false, "Рыба запеченная", 350m },
                    { 20, 4, false, "Свинина на гриле", 420m },
                    { 21, 5, false, "Брускетта с томатами", 150m },
                    { 22, 5, false, "Сырная тарелка", 450m },
                    { 23, 5, false, "Мясная тарелка", 520m },
                    { 24, 5, false, "Маринованные овощи", 120m },
                    { 25, 6, false, "Тирамису", 180m },
                    { 26, 6, false, "Чизкейк Нью-Йорк", 160m },
                    { 27, 6, false, "Мороженое", 90m },
                    { 28, 6, false, "Фруктовый салат", 140m },
                    { 29, 7, false, "Пицца Маргарита", 350m },
                    { 30, 7, false, "Пицца Пепперони", 420m },
                    { 31, 7, false, "Пицца 4 сыра", 450m },
                    { 32, 7, false, "Пицца с грибами", 380m },
                    { 33, 8, false, "Спагетти Болоньезе", 280m },
                    { 34, 8, false, "Паста Карбонара", 320m },
                    { 35, 8, false, "Феттучини Альфредо", 300m },
                    { 36, 9, false, "Стейк рибай", 890m },
                    { 37, 9, false, "Стейк филе миньон", 1200m },
                    { 38, 9, false, "Куриные крылышки", 220m },
                    { 39, 9, false, "Семга на гриле", 480m },
                    { 40, 10, false, "Пиво светлое", 120m },
                    { 41, 10, false, "Вино красное сухое", 200m },
                    { 42, 10, false, "Водка", 150m },
                    { 43, 10, false, "Коньяк", 300m }
                });

            migrationBuilder.InsertData(
                table: "Orders",
                columns: new[] { "Id", "CashGiven", "Change", "HallId", "OrderDate", "PaymentMethodId", "PaymentTime", "StatusId", "TableNumber", "TotalAmount", "UserId" },
                values: new object[,]
                {
                    { 1, null, null, 1, new DateTime(2025, 5, 25, 12, 30, 0, 0, DateTimeKind.Unspecified), 1, new DateTime(2025, 5, 25, 13, 15, 0, 0, DateTimeKind.Unspecified), 2, 5, 730m, null },
                    { 2, null, null, 2, new DateTime(2025, 5, 25, 14, 20, 0, 0, DateTimeKind.Unspecified), 2, new DateTime(2025, 5, 25, 15, 30, 0, 0, DateTimeKind.Unspecified), 2, 8, 1250m, null },
                    { 3, null, null, 1, new DateTime(2025, 5, 26, 18, 45, 0, 0, DateTimeKind.Unspecified), 1, new DateTime(2025, 5, 26, 19, 30, 0, 0, DateTimeKind.Unspecified), 2, 3, 890m, null },
                    { 4, null, null, 2, new DateTime(2025, 5, 27, 19, 15, 0, 0, DateTimeKind.Unspecified), null, null, 1, 12, 650m, null }
                });

            migrationBuilder.InsertData(
                table: "OrderItems",
                columns: new[] { "Id", "DishGroupId", "DishId", "GuestNumber", "OrderId", "Price" },
                values: new object[,]
                {
                    { 1, 1, 1, 1, 1, 150m },
                    { 2, 2, 5, 1, 1, 250m },
                    { 3, 4, 16, 1, 1, 320m },
                    { 4, 3, 11, 1, 1, 100m },
                    { 5, 9, 36, 1, 2, 890m },
                    { 6, 2, 7, 1, 2, 220m },
                    { 7, 3, 13, 1, 2, 140m },
                    { 8, 7, 29, 1, 3, 350m },
                    { 9, 8, 33, 1, 3, 280m },
                    { 10, 6, 25, 1, 3, 180m },
                    { 11, 3, 14, 1, 3, 80m },
                    { 12, 1, 2, 1, 4, 200m },
                    { 13, 7, 30, 1, 4, 420m },
                    { 14, 3, 10, 1, 4, 50m }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Dishes_DishGroupId",
                table: "Dishes",
                column: "DishGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_DishGroupId",
                table: "OrderItems",
                column: "DishGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_DishId",
                table: "OrderItems",
                column: "DishId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderId",
                table: "OrderItems",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_HallId",
                table: "Orders",
                column: "HallId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_PaymentMethodId",
                table: "Orders",
                column: "PaymentMethodId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_StatusId",
                table: "Orders",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_UserId",
                table: "Orders",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Shifts_CashierId",
                table: "Shifts",
                column: "CashierId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_FullName",
                table: "Users",
                column: "FullName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderItems");

            migrationBuilder.DropTable(
                name: "Shifts");

            migrationBuilder.DropTable(
                name: "TableReservations");

            migrationBuilder.DropTable(
                name: "Dishes");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "DishGroups");

            migrationBuilder.DropTable(
                name: "Halls");

            migrationBuilder.DropTable(
                name: "OrderStatuses");

            migrationBuilder.DropTable(
                name: "PaymentMethods");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
