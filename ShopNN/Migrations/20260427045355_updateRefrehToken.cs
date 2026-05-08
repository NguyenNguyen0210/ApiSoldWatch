using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopNN.Migrations
{
    /// <inheritdoc />
    public partial class updateRefrehToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "RefreshTokens",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "ReplacedByToken",
                table: "RefreshTokens",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "3d138719-ee57-44da-9a20-6571ee74c997", "AQAAAAIAAYagAAAAEEgKnuzImpTtxv8cPnmtKUxflbXbg1wHdTJWPpysMg/k6rsu8HgvXgwGXWUGuk+Ldw==", "b4040e0a-7721-4e2c-90de-d2cea8a7b1c3" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "ReplacedByToken",
                table: "RefreshTokens");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "fb3749a4-a9a0-4127-a63d-05c417f22cc3", "AQAAAAIAAYagAAAAEO9MVz6/9jrlO1M4AW+wFP14sUou1yG/2uJlHBaV27qTMLN0xMH89hCxRJMdpNa6Dg==", "e073e69c-6151-4ee9-a375-2e3b933e450b" });
        }
    }
}
