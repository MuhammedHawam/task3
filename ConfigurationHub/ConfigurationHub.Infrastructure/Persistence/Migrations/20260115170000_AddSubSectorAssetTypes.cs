using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using PartnersHub.ConfigurationHub.Infrastructure.Persistence;

#nullable disable

namespace PartnersHub.ConfigurationHub.Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(ConfigurationHubDbContext))]
    [Migration("20260115170000_AddSubSectorAssetTypes")]
    public partial class AddSubSectorAssetTypes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SubSectorAssetTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubSectorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssetTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubSectorAssetTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubSectorAssetTypes_AssetTypes_AssetTypeId",
                        column: x => x.AssetTypeId,
                        principalTable: "AssetTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubSectorAssetTypes_SubSectors_SubSectorId",
                        column: x => x.SubSectorId,
                        principalTable: "SubSectors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SubSectorAssetTypes_AssetTypeId",
                table: "SubSectorAssetTypes",
                column: "AssetTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_SubSectorAssetTypes_SubSectorId_AssetTypeId",
                table: "SubSectorAssetTypes",
                columns: new[] { "SubSectorId", "AssetTypeId" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SubSectorAssetTypes");
        }
    }
}

