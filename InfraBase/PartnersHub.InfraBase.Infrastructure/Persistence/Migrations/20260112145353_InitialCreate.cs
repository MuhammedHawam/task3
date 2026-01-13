using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PartnersHub.InfraBase.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Assets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssetCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AssetName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    LocationCity = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SectorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SubSectorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AssetTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AssetTypeOther = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    QuantityOfAsset = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CapacityPerAsset = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UnitOfMeasurementId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UnitOfMeasurementOther = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: true),
                    ConstructionStartingQuarter = table.Column<int>(type: "int", nullable: true),
                    ConstructionStartingYear = table.Column<int>(type: "int", nullable: true),
                    ConstructionCompletionQuarter = table.Column<int>(type: "int", nullable: true),
                    ConstructionCompletionYear = table.Column<int>(type: "int", nullable: true),
                    TenderingStage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DevelopmentType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CapexEntryMode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    OpexEntryMode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FundingModel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExpectedDebt = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ExpectedEquity = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    IsRevenueGenerating = table.Column<bool>(type: "bit", nullable: true),
                    IRR = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    IsPifGuaranteesRequired = table.Column<bool>(type: "bit", nullable: true),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    SubmittedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RejectionReason = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: true),
                    RejectedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    RejectedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AssetAttachments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Metadata_FileExtension = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileSizeInBytes = table.Column<long>(type: "bigint", nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SharePointUrl = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    UploadedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssetAttachments_Assets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "Assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AssetCapexDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetCapexDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssetCapexDetails_Assets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "Assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AssetHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Action = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PerformedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PerformedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Comments = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: true),
                    FieldsChanged = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    OldValues = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    NewValues = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssetHistories_Assets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "Assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AssetOpexDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetOpexDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssetOpexDetails_Assets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "Assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssetAttachments_AssetId_IsDeleted",
                table: "AssetAttachments",
                columns: new[] { "AssetId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_AssetCapexDetails_AssetId_Year",
                table: "AssetCapexDetails",
                columns: new[] { "AssetId", "Year" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssetHistories_AssetId_PerformedAt",
                table: "AssetHistories",
                columns: new[] { "AssetId", "PerformedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AssetOpexDetails_AssetId_Year",
                table: "AssetOpexDetails",
                columns: new[] { "AssetId", "Year" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssetAttachments");

            migrationBuilder.DropTable(
                name: "AssetCapexDetails");

            migrationBuilder.DropTable(
                name: "AssetHistories");

            migrationBuilder.DropTable(
                name: "AssetOpexDetails");

            migrationBuilder.DropTable(
                name: "Assets");
        }
    }
}
