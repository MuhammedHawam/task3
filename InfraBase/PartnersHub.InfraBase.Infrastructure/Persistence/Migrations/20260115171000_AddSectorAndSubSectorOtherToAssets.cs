using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PartnersHub.InfraBase.Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(InfrabaseDbContext))]
    [Migration("20260115171000_AddSectorAndSubSectorOtherToAssets")]
    public partial class AddSectorAndSubSectorOtherToAssets : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SectorOther",
                table: "Assets",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SubSectorOther",
                table: "Assets",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SectorOther",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "SubSectorOther",
                table: "Assets");
        }
    }
}

