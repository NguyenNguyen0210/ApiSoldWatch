using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ShopNN.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AddColumn<int>(
                name: "PaymentMethod",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PaymentStatus",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "5858d614-4cdb-49cb-acc0-8c369420a320", "AQAAAAIAAYagAAAAEM1H0d+LyPtC5Rk0XZSXy5qrUqqn/RJPSwopiyibfYQOgQG34fghhx/JrqO5J7XgcQ==", "641e0d19-f57c-4c74-8006-e9b31c4e2da7" });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "CategoryId", "Description", "Name", "Price", "Stock" },
                values: new object[,]
                {
                    { new Guid("03f842cc-5927-4829-bdeb-9d9462cba063"), new Guid("c4444444-4444-4444-4444-444444444444"), "Traditional swiss automatic watch", "Tissot Le Locle", 650m, 12 },
                    { new Guid("045a6dd9-f714-4aaf-b2ff-d8e7422168eb"), new Guid("c2222222-2222-2222-2222-222222222222"), "Automatic diver's watch 200m", "Seiko Prospex 'Turtle'", 550m, 15 },
                    { new Guid("32c6fac0-caee-40b4-aea6-97e115cd0af3"), new Guid("c2222222-2222-2222-2222-222222222222"), "Solar powered multisport GPS watch", "Garmin Fenix 7X", 999m, 10 },
                    { new Guid("71ff2bf2-0060-4456-8528-247c5e3b5a86"), new Guid("c1111111-1111-1111-1111-111111111111"), "18ct yellow gold, President bracelet", "Rolex Day-Date 40", 38000m, 3 },
                    { new Guid("7ad7de39-52dc-4f89-b00b-9330b2804520"), new Guid("c4444444-4444-4444-4444-444444444444"), "Elegant moonphase automatic watch", "Longines Master Collection", 2500m, 8 },
                    { new Guid("879a60a5-1f69-44e5-9ea8-589f28e217cf"), new Guid("c1111111-1111-1111-1111-111111111111"), "Selfwinding 'Jumbo' Extra-thin", "Audemars Piguet Royal Oak", 75000m, 2 },
                    { new Guid("c382b7d5-9c45-404e-bed7-b7740a41b17e"), new Guid("c4444444-4444-4444-4444-444444444444"), "Open heart dial, stainless steel", "Hamilton Jazzmaster", 950m, 7 },
                    { new Guid("dd995b34-4c23-416e-8db4-7ad310e2fa2f"), new Guid("c1111111-1111-1111-1111-111111111111"), "Steel blue dial, luxury sports watch", "Patek Philippe Nautilus", 120000m, 1 },
                    { new Guid("ee2842bc-6646-4936-a288-635d75bbb3cb"), new Guid("c3333333-3333-3333-3333-333333333333"), "Rugged and capable, with GPS + Cellular", "Apple Watch Ultra 2", 799m, 25 },
                    { new Guid("efc3bc8d-bc3f-4f8c-8a76-756258acd46e"), new Guid("c2222222-2222-2222-2222-222222222222"), "Carbon Core Guard, Triple Sensor", "Casio G-Shock Mudmaster", 850m, 20 },
                    { new Guid("fb217569-964a-480c-9fb1-ded4df56466e"), new Guid("c3333333-3333-3333-3333-333333333333"), "Advanced sleep tracking and wellness", "Samsung Galaxy Watch 6", 350m, 30 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("03f842cc-5927-4829-bdeb-9d9462cba063"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("045a6dd9-f714-4aaf-b2ff-d8e7422168eb"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("32c6fac0-caee-40b4-aea6-97e115cd0af3"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("71ff2bf2-0060-4456-8528-247c5e3b5a86"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("7ad7de39-52dc-4f89-b00b-9330b2804520"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("879a60a5-1f69-44e5-9ea8-589f28e217cf"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("c382b7d5-9c45-404e-bed7-b7740a41b17e"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("dd995b34-4c23-416e-8db4-7ad310e2fa2f"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("ee2842bc-6646-4936-a288-635d75bbb3cb"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("efc3bc8d-bc3f-4f8c-8a76-756258acd46e"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("fb217569-964a-480c-9fb1-ded4df56466e"));

            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PaymentStatus",
                table: "Orders");

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
    }
}
