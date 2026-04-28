using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopNN.Migrations
{
    /// <inheritdoc />
    public partial class addrotationrefreshtoken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReplacedByToken",
                table: "RefreshTokens");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "9cffd9e3-12e3-4675-af92-b6c6654d7c7f", "AQAAAAIAAYagAAAAEKyjlO/4c0nM3ctR4KNWVXH6RMUUSBXd7X1Ck7PK2csb7knJnDkanKU8Z52NoG2Jlw==", "4a6272c2-b394-409b-b9f8-463be5650dcb" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
    }
}
