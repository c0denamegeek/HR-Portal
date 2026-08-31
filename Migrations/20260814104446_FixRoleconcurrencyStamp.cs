using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HR_Portal.Migrations
{
    /// <inheritdoc />
    public partial class FixRoleconcurrencyStamp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "role-admin-001",
                column: "ConcurrencyStamp",
                value: "c3e2a1b0-0002-0000-0000-000000000002");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "role-user-001",
                column: "ConcurrencyStamp",
                value: "c3e2a1b0-0001-0000-0000-000000000001");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "role-admin-001",
                column: "ConcurrencyStamp",
                value: "32f558a7-311c-47e5-bf96-4cd3593cbc7f");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "role-user-001",
                column: "ConcurrencyStamp",
                value: "6598b58a-076f-4883-9ccb-b89269d394ae");
        }
    }
}
