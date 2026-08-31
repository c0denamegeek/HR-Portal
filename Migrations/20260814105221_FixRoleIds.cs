using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace HR_Portal.Migrations
{
    /// <inheritdoc />
    public partial class FixRoleIds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "role-admin-001");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "role-user-001");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "6e71959f-f22f-47c1-a189-d666bd1fe831", "1ccb1664-5b62-4473-807e-a50ee0c44ef5", "User", "USER" },
                    { "754d806c-c5ac-4e00-a422-5bb35005d358", "cc07f136-fa95-4787-bf6b-d2cbe9b58a06", "Admin", "ADMIN" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "6e71959f-f22f-47c1-a189-d666bd1fe831");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "754d806c-c5ac-4e00-a422-5bb35005d358");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "role-admin-001", "c3e2a1b0-0002-0000-0000-000000000002", "Admin", "ADMIN" },
                    { "role-user-001", "c3e2a1b0-0001-0000-0000-000000000001", "User", "USER" }
                });
        }
    }
}
