using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MicFx.Modules.Auth.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveJobAndDepartment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Department",
                table: "Auth_Users");

            migrationBuilder.DropColumn(
                name: "JobTitle",
                table: "Auth_Users");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Department",
                table: "Auth_Users",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JobTitle",
                table: "Auth_Users",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }
    }
}
