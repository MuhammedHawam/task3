using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PartnersHub.Synergy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanyNameToOppurtuinty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CompanyName",
                table: "SuccessStories",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompanyName",
                table: "Opportunities",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompanyName",
                table: "SuccessStories");

            migrationBuilder.DropColumn(
                name: "CompanyName",
                table: "Opportunities");
        }
    }
}
