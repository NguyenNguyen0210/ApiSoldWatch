using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ShopNN.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentEntityV6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("051d1eaf-21c9-4b12-bf47-edbecbb382c6"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("060397ff-f1d9-467b-a047-7d11ab01216a"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("0af969c6-4a09-46ca-bde4-aefd319b981a"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("0be5d2b3-6dcf-4c9a-bbbf-ba5c6de6128f"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("24dc4b20-fa63-46f2-a370-f5b48fbda5e0"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("2bde0fb1-ab5f-4a48-91dc-58b79b7fc7db"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("5046b553-583b-41d3-b095-e21f299cad04"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("a087aeb3-602a-4d9a-a72c-fe3b83a44cba"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("dbff7c0e-87fd-4dad-9635-433704f7c4a2"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("e524d80b-2237-4c66-a9af-da62eaa5c8e8"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("e759eab4-0597-45f4-822c-b96db7f82f8c"));

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TransactionId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: true)
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

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "57d23f7f-e27f-44da-8c38-8739eb371255", "AQAAAAIAAYagAAAAELuLc1saBPJ0ApMkyZYHsp1t3idrv2NhFTHS8CTZdujhxTgBIvoRvpkF6SDNqAvTdA==", "4a7d91f9-bab2-4b69-9112-a2200cfa17dc" });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "CategoryId", "Description", "ImageUrl", "Name", "Price", "Stock" },
                values: new object[,]
                {
                    { new Guid("2686df78-8723-42df-97f6-1e8444a6b096"), new Guid("c2222222-2222-2222-2222-222222222222"), "Automatic diver's watch 200m", "https://images.unsplash.com/photo-1612817159949-195b6eb9e31a?q=80&w=1000&auto=format&fit=crop", "Seiko Prospex 'Turtle'", 550m, 15 },
                    { new Guid("3754ba52-9ef9-40ad-aeab-9aab66b08e75"), new Guid("c4444444-4444-4444-4444-444444444444"), "Traditional swiss automatic watch", "https://images.unsplash.com/photo-1533139502658-0198f920d8e8?q=80&w=1000&auto=format&fit=crop", "Tissot Le Locle", 650m, 12 },
                    { new Guid("3bc26010-c093-4945-a44c-5b6c676052d3"), new Guid("c2222222-2222-2222-2222-222222222222"), "Solar powered multisport GPS watch", "https://images.unsplash.com/photo-1517502884422-41eaead166d4?q=80&w=1000&auto=format&fit=crop", "Garmin Fenix 7X", 999m, 10 },
                    { new Guid("591d51e0-4ccd-4ed9-a23b-50436fa0a6ee"), new Guid("c4444444-4444-4444-4444-444444444444"), "Open heart dial, stainless steel", "https://images.unsplash.com/photo-1509048191080-d2984bad6ad5?q=80&w=1000&auto=format&fit=crop", "Hamilton Jazzmaster", 950m, 7 },
                    { new Guid("6e87bb51-7691-4b60-8754-92c916d8bde4"), new Guid("c1111111-1111-1111-1111-111111111111"), "18ct yellow gold, President bracelet", "https://images.unsplash.com/photo-1523170335258-f5ed11844a49?q=80&w=1000&auto=format&fit=crop", "Rolex Day-Date 40", 38000m, 3 },
                    { new Guid("7c8e8842-2b22-406a-8264-6ddad1607e2d"), new Guid("c4444444-4444-4444-4444-444444444444"), "Elegant moonphase automatic watch", "https://images.unsplash.com/photo-1524592094714-0f0654e20314?q=80&w=1000&auto=format&fit=crop", "Longines Master Collection", 2500m, 8 },
                    { new Guid("945ac84b-41ce-480a-a325-477dd012ac09"), new Guid("c3333333-3333-3333-3333-333333333333"), "Advanced sleep tracking and wellness", "https://images.unsplash.com/photo-1508685096489-77a46807e624?q=80&w=1000&auto=format&fit=crop", "Samsung Galaxy Watch 6", 350m, 30 },
                    { new Guid("97622414-7218-4774-9e46-fa9036c440ef"), new Guid("c1111111-1111-1111-1111-111111111111"), "Selfwinding 'Jumbo' Extra-thin", "https://images.unsplash.com/photo-1614164185128-e4ec99c436d7?q=80&w=1000&auto=format&fit=crop", "Audemars Piguet Royal Oak", 75000m, 2 },
                    { new Guid("aa5902cb-90a5-4f19-9019-964a443c1bd0"), new Guid("c1111111-1111-1111-1111-111111111111"), "Steel blue dial, luxury sports watch", "https://images.unsplash.com/photo-1547996160-81dfa63595aa?q=80&w=1000&auto=format&fit=crop", "Patek Philippe Nautilus", 120000m, 1 },
                    { new Guid("b9a65dc1-4b9a-41a2-abe9-41c1dfbb3feb"), new Guid("c2222222-2222-2222-2222-222222222222"), "Carbon Core Guard, Triple Sensor", "https://images.unsplash.com/photo-1522312346375-d1a52e2b99b3?q=80&w=1000&auto=format&fit=crop", "Casio G-Shock Mudmaster", 850m, 20 },
                    { new Guid("c609f335-79f0-4e92-93cc-421cfeda81e2"), new Guid("c3333333-3333-3333-3333-333333333333"), "Rugged and capable, with GPS + Cellular", "https://images.unsplash.com/photo-1434493907317-a46b5bc78344?q=80&w=1000&auto=format&fit=crop", "Apple Watch Ultra 2", 799m, 25 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_OrderId",
                table: "Payments",
                column: "OrderId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("2686df78-8723-42df-97f6-1e8444a6b096"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("3754ba52-9ef9-40ad-aeab-9aab66b08e75"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("3bc26010-c093-4945-a44c-5b6c676052d3"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("591d51e0-4ccd-4ed9-a23b-50436fa0a6ee"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("6e87bb51-7691-4b60-8754-92c916d8bde4"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("7c8e8842-2b22-406a-8264-6ddad1607e2d"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("945ac84b-41ce-480a-a325-477dd012ac09"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("97622414-7218-4774-9e46-fa9036c440ef"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("aa5902cb-90a5-4f19-9019-964a443c1bd0"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("b9a65dc1-4b9a-41a2-abe9-41c1dfbb3feb"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("c609f335-79f0-4e92-93cc-421cfeda81e2"));

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "ab98fa72-8520-4339-ae5e-9a0fbdcdc6b4", "AQAAAAIAAYagAAAAEI7ZNgHuSPMP7Kblz8oWjpBXmHKm/wp8cenzm/pCK2PtlbQokYvqpdtf82BmsfbXrw==", "4e2ecf6b-74e7-4e32-bb8b-a28a419358d7" });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "CategoryId", "Description", "ImageUrl", "Name", "Price", "Stock" },
                values: new object[,]
                {
                    { new Guid("051d1eaf-21c9-4b12-bf47-edbecbb382c6"), new Guid("c1111111-1111-1111-1111-111111111111"), "Selfwinding 'Jumbo' Extra-thin", "https://images.unsplash.com/photo-1614164185128-e4ec99c436d7?q=80&w=1000&auto=format&fit=crop", "Audemars Piguet Royal Oak", 75000m, 2 },
                    { new Guid("060397ff-f1d9-467b-a047-7d11ab01216a"), new Guid("c2222222-2222-2222-2222-222222222222"), "Carbon Core Guard, Triple Sensor", "https://images.unsplash.com/photo-1522312346375-d1a52e2b99b3?q=80&w=1000&auto=format&fit=crop", "Casio G-Shock Mudmaster", 850m, 20 },
                    { new Guid("0af969c6-4a09-46ca-bde4-aefd319b981a"), new Guid("c3333333-3333-3333-3333-333333333333"), "Advanced sleep tracking and wellness", "https://images.unsplash.com/photo-1508685096489-77a46807e624?q=80&w=1000&auto=format&fit=crop", "Samsung Galaxy Watch 6", 350m, 30 },
                    { new Guid("0be5d2b3-6dcf-4c9a-bbbf-ba5c6de6128f"), new Guid("c1111111-1111-1111-1111-111111111111"), "18ct yellow gold, President bracelet", "https://images.unsplash.com/photo-1523170335258-f5ed11844a49?q=80&w=1000&auto=format&fit=crop", "Rolex Day-Date 40", 38000m, 3 },
                    { new Guid("24dc4b20-fa63-46f2-a370-f5b48fbda5e0"), new Guid("c4444444-4444-4444-4444-444444444444"), "Elegant moonphase automatic watch", "https://images.unsplash.com/photo-1524592094714-0f0654e20314?q=80&w=1000&auto=format&fit=crop", "Longines Master Collection", 2500m, 8 },
                    { new Guid("2bde0fb1-ab5f-4a48-91dc-58b79b7fc7db"), new Guid("c2222222-2222-2222-2222-222222222222"), "Automatic diver's watch 200m", "https://images.unsplash.com/photo-1612817159949-195b6eb9e31a?q=80&w=1000&auto=format&fit=crop", "Seiko Prospex 'Turtle'", 550m, 15 },
                    { new Guid("5046b553-583b-41d3-b095-e21f299cad04"), new Guid("c1111111-1111-1111-1111-111111111111"), "Steel blue dial, luxury sports watch", "https://images.unsplash.com/photo-1547996160-81dfa63595aa?q=80&w=1000&auto=format&fit=crop", "Patek Philippe Nautilus", 120000m, 1 },
                    { new Guid("a087aeb3-602a-4d9a-a72c-fe3b83a44cba"), new Guid("c3333333-3333-3333-3333-333333333333"), "Rugged and capable, with GPS + Cellular", "https://images.unsplash.com/photo-1434493907317-a46b5bc78344?q=80&w=1000&auto=format&fit=crop", "Apple Watch Ultra 2", 799m, 25 },
                    { new Guid("dbff7c0e-87fd-4dad-9635-433704f7c4a2"), new Guid("c2222222-2222-2222-2222-222222222222"), "Solar powered multisport GPS watch", "https://images.unsplash.com/photo-1517502884422-41eaead166d4?q=80&w=1000&auto=format&fit=crop", "Garmin Fenix 7X", 999m, 10 },
                    { new Guid("e524d80b-2237-4c66-a9af-da62eaa5c8e8"), new Guid("c4444444-4444-4444-4444-444444444444"), "Open heart dial, stainless steel", "https://images.unsplash.com/photo-1509048191080-d2984bad6ad5?q=80&w=1000&auto=format&fit=crop", "Hamilton Jazzmaster", 950m, 7 },
                    { new Guid("e759eab4-0597-45f4-822c-b96db7f82f8c"), new Guid("c4444444-4444-4444-4444-444444444444"), "Traditional swiss automatic watch", "https://images.unsplash.com/photo-1533139502658-0198f920d8e8?q=80&w=1000&auto=format&fit=crop", "Tissot Le Locle", 650m, 12 }
                });
        }
    }
}
