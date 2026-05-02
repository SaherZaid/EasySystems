using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasySystems.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCrmFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AssignedTo",
                table: "StoreRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "InternalNote",
                table: "StoreRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Priority",
                table: "StoreRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AssignedTo",
                table: "StoreRequests");

            migrationBuilder.DropColumn(
                name: "InternalNote",
                table: "StoreRequests");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "StoreRequests");
        }
    }
}
