using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ShopNN.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("1499d73a-ebd1-4dc0-a387-95cf8cc72fc3"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("2c9edfac-f589-40de-b1e0-9dd33b65cac3"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("30c45c0e-ff1e-4e60-a0fe-884225543fae"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("452f2029-b678-4fd9-8dd6-d641ebdc54e6"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("51b4546f-ad26-4314-9cb8-3800adc9afb1"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("54fcd63b-1648-448e-ac92-6a20e0ce8116"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("5f3f1700-7b8b-4c76-8727-17d18ad3b597"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("6765a1d8-96f7-46f2-9151-34e2f320c6a9"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("c52483e6-6f5a-4985-9f0f-01677c284bfa"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("f5ff057f-cbe9-458b-820e-5d99e05f541f"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("fa13c327-b014-413b-8af6-c95a7daa09c1"));

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "d2d345fa-4601-46d6-a345-6b08b51f9378", "AQAAAAIAAYagAAAAELfHFop1I/AmtfyHMuIhBN/gmpVESXkbAhJKoobhbwU/uninuq0SUBARleFxnzPP1g==", "b73c3221-12aa-4f18-9a26-84fac812b9ae" });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "CategoryId", "Description", "Name", "Price", "Stock" },
                values: new object[,]
                {
                    { new Guid("0ff6042b-8cba-4d30-98f2-87decb5f5a91"), new Guid("c1111111-1111-1111-1111-111111111111"), "Steel blue dial, luxury sports watch", "Patek Philippe Nautilus", 120000m, 1 },
                    { new Guid("3dd515f6-beab-49f3-ad9c-aed570d67d5e"), new Guid("c4444444-4444-4444-4444-444444444444"), "Traditional swiss automatic watch", "Tissot Le Locle", 650m, 12 },
                    { new Guid("533b6d7d-bf24-4c30-bb00-cf44685e680f"), new Guid("c1111111-1111-1111-1111-111111111111"), "18ct yellow gold, President bracelet", "Rolex Day-Date 40", 38000m, 3 },
                    { new Guid("65333974-e4a5-476b-9b8e-d104436f147a"), new Guid("c1111111-1111-1111-1111-111111111111"), "Selfwinding 'Jumbo' Extra-thin", "Audemars Piguet Royal Oak", 75000m, 2 },
                    { new Guid("793d5a70-5b26-4f82-8c8a-ec8de9c3f1cc"), new Guid("c4444444-4444-4444-4444-444444444444"), "Open heart dial, stainless steel", "Hamilton Jazzmaster", 950m, 7 },
                    { new Guid("8a364d12-2ca6-4509-8d26-1c7790fc8421"), new Guid("c2222222-2222-2222-2222-222222222222"), "Carbon Core Guard, Triple Sensor", "Casio G-Shock Mudmaster", 850m, 20 },
                    { new Guid("8abe8ded-a984-40b7-b2d3-347a5ca5391b"), new Guid("c3333333-3333-3333-3333-333333333333"), "Advanced sleep tracking and wellness", "Samsung Galaxy Watch 6", 350m, 30 },
                    { new Guid("9dbb2d0a-3ae5-4f77-958d-0c60201817bf"), new Guid("c4444444-4444-4444-4444-444444444444"), "Elegant moonphase automatic watch", "Longines Master Collection", 2500m, 8 },
                    { new Guid("bcf30ffb-9dcb-4d0e-b47f-36652b717e30"), new Guid("c3333333-3333-3333-3333-333333333333"), "Rugged and capable, with GPS + Cellular", "Apple Watch Ultra 2", 799m, 25 },
                    { new Guid("e5eaf4aa-864f-4652-a963-c41458ba21e0"), new Guid("c2222222-2222-2222-2222-222222222222"), "Automatic diver's watch 200m", "Seiko Prospex 'Turtle'", 550m, 15 },
                    { new Guid("f9398229-5aa4-4f5b-abe2-51b59ede6463"), new Guid("c2222222-2222-2222-2222-222222222222"), "Solar powered multisport GPS watch", "Garmin Fenix 7X", 999m, 10 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("0ff6042b-8cba-4d30-98f2-87decb5f5a91"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("3dd515f6-beab-49f3-ad9c-aed570d67d5e"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("533b6d7d-bf24-4c30-bb00-cf44685e680f"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("65333974-e4a5-476b-9b8e-d104436f147a"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("793d5a70-5b26-4f82-8c8a-ec8de9c3f1cc"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("8a364d12-2ca6-4509-8d26-1c7790fc8421"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("8abe8ded-a984-40b7-b2d3-347a5ca5391b"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("9dbb2d0a-3ae5-4f77-958d-0c60201817bf"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("bcf30ffb-9dcb-4d0e-b47f-36652b717e30"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("e5eaf4aa-864f-4652-a963-c41458ba21e0"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("f9398229-5aa4-4f5b-abe2-51b59ede6463"));

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Orders");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "20325e23-e60f-471f-93b1-58e85336832d", "AQAAAAIAAYagAAAAECpFAFi2k5PTy6dskXkjazRCEp+uy2eyw/LpCXW6HTqPs3WfxYO3lRKHcNpKAKgSvA==", "a9a203a1-a287-4935-904f-39803cdb36f8" });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "CategoryId", "Description", "Name", "Price", "Stock" },
                values: new object[,]
                {
                    { new Guid("1499d73a-ebd1-4dc0-a387-95cf8cc72fc3"), new Guid("c1111111-1111-1111-1111-111111111111"), "Selfwinding 'Jumbo' Extra-thin", "Audemars Piguet Royal Oak", 75000m, 2 },
                    { new Guid("2c9edfac-f589-40de-b1e0-9dd33b65cac3"), new Guid("c3333333-3333-3333-3333-333333333333"), "Advanced sleep tracking and wellness", "Samsung Galaxy Watch 6", 350m, 30 },
                    { new Guid("30c45c0e-ff1e-4e60-a0fe-884225543fae"), new Guid("c4444444-4444-4444-4444-444444444444"), "Traditional swiss automatic watch", "Tissot Le Locle", 650m, 12 },
                    { new Guid("452f2029-b678-4fd9-8dd6-d641ebdc54e6"), new Guid("c1111111-1111-1111-1111-111111111111"), "Steel blue dial, luxury sports watch", "Patek Philippe Nautilus", 120000m, 1 },
                    { new Guid("51b4546f-ad26-4314-9cb8-3800adc9afb1"), new Guid("c1111111-1111-1111-1111-111111111111"), "18ct yellow gold, President bracelet", "Rolex Day-Date 40", 38000m, 3 },
                    { new Guid("54fcd63b-1648-448e-ac92-6a20e0ce8116"), new Guid("c3333333-3333-3333-3333-333333333333"), "Rugged and capable, with GPS + Cellular", "Apple Watch Ultra 2", 799m, 25 },
                    { new Guid("5f3f1700-7b8b-4c76-8727-17d18ad3b597"), new Guid("c4444444-4444-4444-4444-444444444444"), "Open heart dial, stainless steel", "Hamilton Jazzmaster", 950m, 7 },
                    { new Guid("6765a1d8-96f7-46f2-9151-34e2f320c6a9"), new Guid("c2222222-2222-2222-2222-222222222222"), "Solar powered multisport GPS watch", "Garmin Fenix 7X", 999m, 10 },
                    { new Guid("c52483e6-6f5a-4985-9f0f-01677c284bfa"), new Guid("c4444444-4444-4444-4444-444444444444"), "Elegant moonphase automatic watch", "Longines Master Collection", 2500m, 8 },
                    { new Guid("f5ff057f-cbe9-458b-820e-5d99e05f541f"), new Guid("c2222222-2222-2222-2222-222222222222"), "Carbon Core Guard, Triple Sensor", "Casio G-Shock Mudmaster", 850m, 20 },
                    { new Guid("fa13c327-b014-413b-8af6-c95a7daa09c1"), new Guid("c2222222-2222-2222-2222-222222222222"), "Automatic diver's watch 200m", "Seiko Prospex 'Turtle'", 550m, 15 }
                });
        }
    }
}
