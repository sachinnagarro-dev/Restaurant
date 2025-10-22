using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TableOrder.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddVegetarianFlag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Restaurants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Address = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Phone = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Restaurants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MenuItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    Price = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                    Category = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    IsAvailable = table.Column<bool>(type: "INTEGER", nullable: false),
                    ImageUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    IsVegetarian = table.Column<bool>(type: "INTEGER", nullable: false),
                    PreparationTimeMinutes = table.Column<int>(type: "INTEGER", nullable: false),
                    RestaurantId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MenuItems_Restaurants_RestaurantId",
                        column: x => x.RestaurantId,
                        principalTable: "Restaurants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tables",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Number = table.Column<int>(type: "INTEGER", nullable: false),
                    Capacity = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    RestaurantId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tables", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tables_Restaurants_RestaurantId",
                        column: x => x.RestaurantId,
                        principalTable: "Restaurants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TableId = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    SubTotal = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                    TaxAmount = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                    SpecialInstructions = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Orders_Tables_TableId",
                        column: x => x.TableId,
                        principalTable: "Tables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrderItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OrderId = table.Column<int>(type: "INTEGER", nullable: false),
                    MenuItemId = table.Column<int>(type: "INTEGER", nullable: false),
                    Quantity = table.Column<int>(type: "INTEGER", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                    SpecialInstructions = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderItems_MenuItems_MenuItemId",
                        column: x => x.MenuItemId,
                        principalTable: "MenuItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderItems_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OrderId = table.Column<int>(type: "INTEGER", nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                    PaymentMethod = table.Column<string>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    TransactionId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    ReferenceNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payments_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Restaurants",
                columns: new[] { "Id", "Address", "CreatedAt", "Email", "Name", "Phone", "UpdatedAt" },
                values: new object[] { 1, "123 Main Street, City, State 12345", new DateTime(2025, 10, 22, 6, 28, 44, 345, DateTimeKind.Utc).AddTicks(7778), "info@tableorder.com", "TableOrder Restaurant", "(555) 123-4567", new DateTime(2025, 10, 22, 6, 28, 44, 345, DateTimeKind.Utc).AddTicks(7779) });

            migrationBuilder.InsertData(
                table: "MenuItems",
                columns: new[] { "Id", "Category", "CreatedAt", "Description", "ImageUrl", "IsAvailable", "IsVegetarian", "Name", "PreparationTimeMinutes", "Price", "RestaurantId", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "Pizza", new DateTime(2025, 10, 22, 6, 28, 44, 345, DateTimeKind.Utc).AddTicks(7987), "Classic tomato sauce, fresh mozzarella, and basil", null, true, true, "Margherita Pizza", 20, 12.99m, 1, new DateTime(2025, 10, 22, 6, 28, 44, 345, DateTimeKind.Utc).AddTicks(7988) },
                    { 2, "Pizza", new DateTime(2025, 10, 22, 6, 28, 44, 345, DateTimeKind.Utc).AddTicks(7996), "Tomato sauce, mozzarella, and spicy pepperoni", null, true, true, "Pepperoni Pizza", 20, 14.99m, 1, new DateTime(2025, 10, 22, 6, 28, 44, 345, DateTimeKind.Utc).AddTicks(7997) },
                    { 3, "Salad", new DateTime(2025, 10, 22, 6, 28, 44, 345, DateTimeKind.Utc).AddTicks(8000), "Crisp romaine lettuce, parmesan cheese, and croutons", null, true, true, "Caesar Salad", 10, 8.99m, 1, new DateTime(2025, 10, 22, 6, 28, 44, 345, DateTimeKind.Utc).AddTicks(8001) },
                    { 4, "Appetizer", new DateTime(2025, 10, 22, 6, 28, 44, 345, DateTimeKind.Utc).AddTicks(8003), "Spicy buffalo wings served with ranch dressing", null, true, true, "Chicken Wings", 15, 9.99m, 1, new DateTime(2025, 10, 22, 6, 28, 44, 345, DateTimeKind.Utc).AddTicks(8003) },
                    { 5, "Pasta", new DateTime(2025, 10, 22, 6, 28, 44, 345, DateTimeKind.Utc).AddTicks(8006), "Creamy pasta with bacon, egg, and parmesan", null, true, true, "Pasta Carbonara", 18, 13.99m, 1, new DateTime(2025, 10, 22, 6, 28, 44, 345, DateTimeKind.Utc).AddTicks(8006) },
                    { 6, "Main Course", new DateTime(2025, 10, 22, 6, 28, 44, 345, DateTimeKind.Utc).AddTicks(8009), "Fresh Atlantic salmon with seasonal vegetables", null, true, true, "Grilled Salmon", 25, 18.99m, 1, new DateTime(2025, 10, 22, 6, 28, 44, 345, DateTimeKind.Utc).AddTicks(8009) },
                    { 7, "Dessert", new DateTime(2025, 10, 22, 6, 28, 44, 345, DateTimeKind.Utc).AddTicks(8011), "Rich chocolate cake with vanilla ice cream", null, true, true, "Chocolate Cake", 5, 6.99m, 1, new DateTime(2025, 10, 22, 6, 28, 44, 345, DateTimeKind.Utc).AddTicks(8012) },
                    { 8, "Beverage", new DateTime(2025, 10, 22, 6, 28, 44, 345, DateTimeKind.Utc).AddTicks(8014), "Refreshing soft drink", null, true, true, "Coca Cola", 2, 2.99m, 1, new DateTime(2025, 10, 22, 6, 28, 44, 345, DateTimeKind.Utc).AddTicks(8014) }
                });

            migrationBuilder.InsertData(
                table: "Tables",
                columns: new[] { "Id", "Capacity", "CreatedAt", "Number", "RestaurantId", "Status", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, 2, new DateTime(2025, 10, 22, 6, 28, 44, 345, DateTimeKind.Utc).AddTicks(7935), 1, 1, "Available", new DateTime(2025, 10, 22, 6, 28, 44, 345, DateTimeKind.Utc).AddTicks(7936) },
                    { 2, 4, new DateTime(2025, 10, 22, 6, 28, 44, 345, DateTimeKind.Utc).AddTicks(7940), 2, 1, "Available", new DateTime(2025, 10, 22, 6, 28, 44, 345, DateTimeKind.Utc).AddTicks(7940) },
                    { 3, 6, new DateTime(2025, 10, 22, 6, 28, 44, 345, DateTimeKind.Utc).AddTicks(7943), 3, 1, "Available", new DateTime(2025, 10, 22, 6, 28, 44, 345, DateTimeKind.Utc).AddTicks(7943) },
                    { 4, 2, new DateTime(2025, 10, 22, 6, 28, 44, 345, DateTimeKind.Utc).AddTicks(7945), 4, 1, "Available", new DateTime(2025, 10, 22, 6, 28, 44, 345, DateTimeKind.Utc).AddTicks(7945) },
                    { 5, 8, new DateTime(2025, 10, 22, 6, 28, 44, 345, DateTimeKind.Utc).AddTicks(7947), 5, 1, "Available", new DateTime(2025, 10, 22, 6, 28, 44, 345, DateTimeKind.Utc).AddTicks(7948) },
                    { 6, 4, new DateTime(2025, 10, 22, 6, 28, 44, 345, DateTimeKind.Utc).AddTicks(7949), 6, 1, "Available", new DateTime(2025, 10, 22, 6, 28, 44, 345, DateTimeKind.Utc).AddTicks(7950) },
                    { 7, 2, new DateTime(2025, 10, 22, 6, 28, 44, 345, DateTimeKind.Utc).AddTicks(7951), 7, 1, "Available", new DateTime(2025, 10, 22, 6, 28, 44, 345, DateTimeKind.Utc).AddTicks(7952) },
                    { 8, 6, new DateTime(2025, 10, 22, 6, 28, 44, 345, DateTimeKind.Utc).AddTicks(7953), 8, 1, "Available", new DateTime(2025, 10, 22, 6, 28, 44, 345, DateTimeKind.Utc).AddTicks(7954) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_MenuItems_RestaurantId",
                table: "MenuItems",
                column: "RestaurantId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_MenuItemId",
                table: "OrderItems",
                column: "MenuItemId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderId",
                table: "OrderItems",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_TableId",
                table: "Orders",
                column: "TableId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_OrderId",
                table: "Payments",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Restaurants_Email",
                table: "Restaurants",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tables_RestaurantId_Number",
                table: "Tables",
                columns: new[] { "RestaurantId", "Number" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderItems");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "MenuItems");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "Tables");

            migrationBuilder.DropTable(
                name: "Restaurants");
        }
    }
}
