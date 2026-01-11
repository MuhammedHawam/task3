using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PartnersHub.InfraBase.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class AddCapexOpexEntryMode : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "CapexEntryMode",
            table: "Assets",
            type: "nvarchar(20)",
            maxLength: 20,
            nullable: false,
            defaultValue: "MultiYear");

        migrationBuilder.AddColumn<string>(
            name: "OpexEntryMode",
            table: "Assets",
            type: "nvarchar(20)",
            maxLength: 20,
            nullable: false,
            defaultValue: "MultiYear");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "CapexEntryMode",
            table: "Assets");

        migrationBuilder.DropColumn(
            name: "OpexEntryMode",
            table: "Assets");
    }
}

