using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasySystems.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserAccountFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FullName",
                table: "UserAccounts",
                newName: "PhoneNumber");

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "UserAccounts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "UserAccounts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "UserAccounts");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "UserAccounts");

            migrationBuilder.RenameColumn(
                name: "PhoneNumber",
                table: "UserAccounts",
                newName: "FullName");
        }
    }
}
