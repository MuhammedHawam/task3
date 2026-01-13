using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PartnersHub.InfraBase.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class AddLookupCodesToAssets : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "SectorCode",
            table: "Assets",
            type: "nvarchar(50)",
            maxLength: 50,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "SubSectorCode",
            table: "Assets",
            type: "nvarchar(50)",
            maxLength: 50,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "AssetTypeCode",
            table: "Assets",
            type: "nvarchar(50)",
            maxLength: 50,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "UnitOfMeasurementCode",
            table: "Assets",
            type: "nvarchar(50)",
            maxLength: 50,
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "SectorCode",
            table: "Assets");

        migrationBuilder.DropColumn(
            name: "SubSectorCode",
            table: "Assets");

        migrationBuilder.DropColumn(
            name: "AssetTypeCode",
            table: "Assets");

        migrationBuilder.DropColumn(
            name: "UnitOfMeasurementCode",
            table: "Assets");
    }
}

