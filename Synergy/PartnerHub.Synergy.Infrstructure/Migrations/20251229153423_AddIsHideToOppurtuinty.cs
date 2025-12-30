using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PartnersHub.Synergy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIsHideToOppurtuinty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsHide",
                table: "SuccessStories",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsHide",
                table: "Opportunities",
                type: "bit",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsHide",
                table: "SuccessStories");

            migrationBuilder.DropColumn(
                name: "IsHide",
                table: "Opportunities");
        }
    }
}
