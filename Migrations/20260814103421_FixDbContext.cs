using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HR_Portal.Migrations
{
    /// <inheritdoc />
    public partial class FixDbContext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "role-admin-001",
                column: "ConcurrencyStamp",
                value: "52ea519a-f781-4390-8841-f10457667b3b");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "role-user-001",
                column: "ConcurrencyStamp",
                value: "d3cf8027-2467-4751-851e-66e4e34f826e");
        }
    }
}
