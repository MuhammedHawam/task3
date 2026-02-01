using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PartnersHub.Synergy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanyPIFOwnerEmailColumnToSynergyCompanyTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CompanyPIFOwnerEmail",
                table: "SynergyCompanies",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompanyPIFOwnerName",
                table: "SynergyCompanies",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompanyPIFOwnerSupervisorEmail",
                table: "SynergyCompanies",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompanyPIFOwnerSupervisorName",
                table: "SynergyCompanies",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompanyPIFOwnerEmail",
                table: "SynergyCompanies");

            migrationBuilder.DropColumn(
                name: "CompanyPIFOwnerName",
                table: "SynergyCompanies");

            migrationBuilder.DropColumn(
                name: "CompanyPIFOwnerSupervisorEmail",
                table: "SynergyCompanies");

            migrationBuilder.DropColumn(
                name: "CompanyPIFOwnerSupervisorName",
                table: "SynergyCompanies");
        }
    }
}
