using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Repository.Migrations;

/// <inheritdoc />
public partial class init2 : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.InsertData(
            table: "AspNetRoles",
            columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
            values: new object[,]
            {
                { "3bd4128c-ced0-414a-9a92-d0e450088764", null, "Manager", "MANAGER" },
                { "55a88faf-e595-430d-9442-52170091d311", null, "Administrator", "ADMINISTRATOR" }
            });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DeleteData(
            table: "AspNetRoles",
            keyColumn: "Id",
            keyValue: "3bd4128c-ced0-414a-9a92-d0e450088764");

        migrationBuilder.DeleteData(
            table: "AspNetRoles",
            keyColumn: "Id",
            keyValue: "55a88faf-e595-430d-9442-52170091d311");
    }
}
