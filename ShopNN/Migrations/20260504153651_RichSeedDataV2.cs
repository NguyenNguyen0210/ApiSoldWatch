using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ShopNN.Migrations
{
    /// <inheritdoc />
    public partial class RichSeedDataV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-1111-1111-1111-111111111111"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-2222-2222-2222-222222222222"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-3333-3333-3333-333333333333"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("dddddddd-4444-4444-4444-444444444444"));

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "495c7dfc-e814-4d79-9ce8-100977c075de", "AQAAAAIAAYagAAAAEO3T0nZCV1Wr4FPzOUeXNHu80zVNTU51CmD0vDMnHGRGzYtHkiNiJgV9OjhOmZMolg==", "52a1de7a-952a-4508-ba6d-e4a65890923b" });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { new Guid("c1111111-1111-1111-1111-111111111111"), "Luxury Watches" },
                    { new Guid("c2222222-2222-2222-2222-222222222222"), "Sport Watches" },
                    { new Guid("c3333333-3333-3333-3333-333333333333"), "Smart Watches" },
                    { new Guid("c4444444-4444-4444-4444-444444444444"), "Classic Watches" }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "CategoryId", "Description", "Name", "Price", "Stock" },
                values: new object[,]
                {
                    { new Guid("01e75c1a-40e0-46b5-98a0-3e229dba0938"), new Guid("c3333333-3333-3333-3333-333333333333"), "Advanced sleep tracking and wellness", "Samsung Galaxy Watch 6", 350m, 30 },
                    { new Guid("302807c9-f6ad-44c1-99e8-e8414446d76f"), new Guid("c2222222-2222-2222-2222-222222222222"), "Automatic diver's watch 200m", "Seiko Prospex 'Turtle'", 550m, 15 },
                    { new Guid("315877a7-2ee0-4449-88aa-52f8daf2029a"), new Guid("c4444444-4444-4444-4444-444444444444"), "Open heart dial, stainless steel", "Hamilton Jazzmaster", 950m, 7 },
                    { new Guid("36a25b9c-b5bf-43b8-ad58-b6c2795ad1aa"), new Guid("c4444444-4444-4444-4444-444444444444"), "Traditional swiss automatic watch", "Tissot Le Locle", 650m, 12 },
                    { new Guid("86686242-5666-4f3a-b20b-7afa9991f70d"), new Guid("c4444444-4444-4444-4444-444444444444"), "Elegant moonphase automatic watch", "Longines Master Collection", 2500m, 8 },
                    { new Guid("cba671e5-f0af-4f9a-9748-1e9b4f8534f7"), new Guid("c2222222-2222-2222-2222-222222222222"), "Solar powered multisport GPS watch", "Garmin Fenix 7X", 999m, 10 },
                    { new Guid("da8ca88d-edc9-45c9-a4ff-3e8fb6213ac5"), new Guid("c3333333-3333-3333-3333-333333333333"), "Rugged and capable, with GPS + Cellular", "Apple Watch Ultra 2", 799m, 25 },
                    { new Guid("e0966b76-54fd-4e15-a597-c69b0d12fdbb"), new Guid("c1111111-1111-1111-1111-111111111111"), "Steel blue dial, luxury sports watch", "Patek Philippe Nautilus", 120000m, 1 },
                    { new Guid("e103fa5b-043b-4583-9bea-b21dbbe5e34b"), new Guid("c2222222-2222-2222-2222-222222222222"), "Carbon Core Guard, Triple Sensor", "Casio G-Shock Mudmaster", 850m, 20 },
                    { new Guid("ec8164f0-8f21-4308-8c22-c02ae07c1ed7"), new Guid("c1111111-1111-1111-1111-111111111111"), "Selfwinding 'Jumbo' Extra-thin", "Audemars Piguet Royal Oak", 75000m, 2 },
                    { new Guid("f6284c3d-41c1-4b2f-ad69-c031eb664d62"), new Guid("c1111111-1111-1111-1111-111111111111"), "18ct yellow gold, President bracelet", "Rolex Day-Date 40", 38000m, 3 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("01e75c1a-40e0-46b5-98a0-3e229dba0938"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("302807c9-f6ad-44c1-99e8-e8414446d76f"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("315877a7-2ee0-4449-88aa-52f8daf2029a"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("36a25b9c-b5bf-43b8-ad58-b6c2795ad1aa"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("86686242-5666-4f3a-b20b-7afa9991f70d"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("cba671e5-f0af-4f9a-9748-1e9b4f8534f7"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("da8ca88d-edc9-45c9-a4ff-3e8fb6213ac5"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("e0966b76-54fd-4e15-a597-c69b0d12fdbb"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("e103fa5b-043b-4583-9bea-b21dbbe5e34b"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("ec8164f0-8f21-4308-8c22-c02ae07c1ed7"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("f6284c3d-41c1-4b2f-ad69-c031eb664d62"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("c1111111-1111-1111-1111-111111111111"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("c2222222-2222-2222-2222-222222222222"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("c3333333-3333-3333-3333-333333333333"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("c4444444-4444-4444-4444-444444444444"));

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "9964129a-bbb3-49b4-a9e6-25996f5c2399", "AQAAAAIAAYagAAAAEF9S3HPcUm/rxVSZ/RGSZ8PD7+aNv9jWg/9eQkeAZXf+Gs5iKkgvpwPLmm/yai7PiA==", "c5e7658d-2ced-4c86-a08f-aa11e4b2d9ee" });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Name" },
                values: new object[] { new Guid("dddddddd-4444-4444-4444-444444444444"), "Default" });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "CategoryId", "Description", "Name", "Price", "Stock" },
                values: new object[,]
                {
                    { new Guid("aaaaaaaa-1111-1111-1111-111111111111"), new Guid("dddddddd-4444-4444-4444-444444444444"), "Luxury diving watch", "Rolex Submariner", 15000m, 5 },
                    { new Guid("bbbbbbbb-2222-2222-2222-222222222222"), new Guid("dddddddd-4444-4444-4444-444444444444"), "Moonwatch легендарный", "Omega Speedmaster", 8000m, 10 },
                    { new Guid("cccccccc-3333-3333-3333-333333333333"), new Guid("dddddddd-4444-4444-4444-444444444444"), "Durable sport watch", "Casio G-Shock", 150m, 50 }
                });
        }
    }
}
