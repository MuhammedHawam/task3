using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PartnersHub.InnovationHub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class initialCreation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AssociatedProviders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssociatedProviders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AssociatedSectors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssociatedSectors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CampaignRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: true),
                    ProblemStatement = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: true),
                    CampaignRequestStatus = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    LaunchDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    SubmissionDeadLine = table.Column<DateTime>(type: "datetime", nullable: true),
                    OwnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ShortId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OwnerName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Comments = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignRequests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Companies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    HeadquarterCountry = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    HeadquarterCity = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: true),
                    Logo = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    RepresentativeName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Position = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Evaluators",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Evaluators", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Sponsors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sponsors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "technologies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TechnologyStage = table.Column<int>(type: "int", nullable: false),
                    Sector = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_technologies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChallengeRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: false),
                    SourceCompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssociatedSectorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubmitterName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PriorityLevelId = table.Column<int>(type: "int", nullable: false),
                    ChallengeStatus = table.Column<int>(type: "int", nullable: false),
                    ShortId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IsDraft = table.Column<bool>(type: "bit", nullable: true),
                    IsArchived = table.Column<bool>(type: "bit", nullable: true),
                    Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChallengeRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChallengeRequests_AssociatedProviders_SourceCompanyId",
                        column: x => x.SourceCompanyId,
                        principalTable: "AssociatedProviders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChallengeRequests_AssociatedSectors_AssociatedSectorId",
                        column: x => x.AssociatedSectorId,
                        principalTable: "AssociatedSectors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CampaignRequestEvaluationCriteria",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CampaignRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CriteriaName = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: false),
                    CriteriaValue = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignRequestEvaluationCriteria", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CampaignRequestEvaluationCriteria_CampaignRequests_CampaignRequestId",
                        column: x => x.CampaignRequestId,
                        principalTable: "CampaignRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CampaignRequestEvaluator",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CampaignRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EvaluatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignRequestEvaluator", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CampaignRequestEvaluator_CampaignRequests_CampaignRequestId",
                        column: x => x.CampaignRequestId,
                        principalTable: "CampaignRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CampaignRequestSponsors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CampaignRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SponsorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignRequestSponsors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CampaignRequestSponsors_CampaignRequests_CampaignRequestId",
                        column: x => x.CampaignRequestId,
                        principalTable: "CampaignRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CampaignRequestTermsAndConditions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CampaignRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FileFormat = table.Column<int>(type: "int", maxLength: 100, nullable: false),
                    FileExtension = table.Column<int>(type: "int", maxLength: 10, nullable: false),
                    FileSizeInBytes = table.Column<long>(type: "bigint", nullable: false),
                    Metadata_Url = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SharePointFileId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SharePointUrl = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    SharePointLibrary = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    UploadedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignRequestTermsAndConditions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CampaignRequestTermsAndConditions_CampaignRequests_CampaignRequestId",
                        column: x => x.CampaignRequestId,
                        principalTable: "CampaignRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CampaignTrackingHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CampaignRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PerformedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PerformedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Comments = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: true),
                    FieldsChanged = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    OldValues = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    NewValues = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignTrackingHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CampaignTrackingHistory_CampaignRequests_CampaignRequestId",
                        column: x => x.CampaignRequestId,
                        principalTable: "CampaignRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CompanySectors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SectorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SectorName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AssignedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanySectors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanySectors_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CampaignRequestLinkedChallenges",
                columns: table => new
                {
                    CampaignRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChallengeRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignRequestLinkedChallenges", x => new { x.CampaignRequestId, x.ChallengeRequestId });
                    table.ForeignKey(
                        name: "FK_CampaignRequestLinkedChallenges_CampaignRequests_CampaignRequestId",
                        column: x => x.CampaignRequestId,
                        principalTable: "CampaignRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CampaignRequestLinkedChallenges_ChallengeRequests_ChallengeRequestId",
                        column: x => x.ChallengeRequestId,
                        principalTable: "ChallengeRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChallengeRequestAttachments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChallengeRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FileFormat = table.Column<int>(type: "int", maxLength: 100, nullable: false),
                    FileExtension = table.Column<int>(type: "int", maxLength: 10, nullable: false),
                    FileSizeInBytes = table.Column<long>(type: "bigint", nullable: false),
                    Metadata_Url = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SharePointFileId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SharePointUrl = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    SharePointLibrary = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    UploadedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChallengeRequestAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChallengeRequestAttachments_ChallengeRequests_ChallengeRequestId",
                        column: x => x.ChallengeRequestId,
                        principalTable: "ChallengeRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "challengeRequestRevisionComments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChallengeRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Author = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CommentedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsCurrent = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_challengeRequestRevisionComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_challengeRequestRevisionComments_ChallengeRequests_ChallengeRequestId",
                        column: x => x.ChallengeRequestId,
                        principalTable: "ChallengeRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "challengeTechnologiesRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChallengeRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TechnologyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JustificationForLinking = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    RequestStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequestedName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_challengeTechnologiesRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_challengeTechnologiesRequests_ChallengeRequests_ChallengeRequestId",
                        column: x => x.ChallengeRequestId,
                        principalTable: "ChallengeRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_challengeTechnologiesRequests_technologies_TechnologyId",
                        column: x => x.TechnologyId,
                        principalTable: "technologies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "challengeTrackingHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChallengeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_challengeTrackingHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_challengeTrackingHistories_ChallengeRequests_ChallengeId",
                        column: x => x.ChallengeId,
                        principalTable: "ChallengeRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CampaignRequestEvaluationCriteria_CampaignRequestId",
                table: "CampaignRequestEvaluationCriteria",
                column: "CampaignRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignRequestEvaluator_CampaignRequestId",
                table: "CampaignRequestEvaluator",
                column: "CampaignRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignRequestLinkedChallenges_ChallengeRequestId",
                table: "CampaignRequestLinkedChallenges",
                column: "ChallengeRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignRequestSponsors_CampaignRequestId",
                table: "CampaignRequestSponsors",
                column: "CampaignRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignRequestCampaignRequestTermsAndConditions_CampaignRequestId",
                table: "CampaignRequestTermsAndConditions",
                column: "CampaignRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignRequestCampaignRequestTermsAndConditions_CampaignRequestId_IsDeleted",
                table: "CampaignRequestTermsAndConditions",
                columns: new[] { "CampaignRequestId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_CampaignRequestCampaignRequestTermsAndConditions_IsDeleted",
                table: "CampaignRequestTermsAndConditions",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignRequestCampaignRequestTermsAndConditions_UploadedAt",
                table: "CampaignRequestTermsAndConditions",
                column: "UploadedAt");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignTrackingHistory_CampaignRequestId",
                table: "CampaignTrackingHistory",
                column: "CampaignRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_ChallengeRequestAttachments_ChallengeRequestId",
                table: "ChallengeRequestAttachments",
                column: "ChallengeRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_ChallengeRequestAttachments_ChallengeRequestId_IsDeleted",
                table: "ChallengeRequestAttachments",
                columns: new[] { "ChallengeRequestId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_ChallengeRequestAttachments_IsDeleted",
                table: "ChallengeRequestAttachments",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_ChallengeRequestAttachments_UploadedAt",
                table: "ChallengeRequestAttachments",
                column: "UploadedAt");

            migrationBuilder.CreateIndex(
                name: "IX_challengeRequestRevisionComments_ChallengeRequestId",
                table: "challengeRequestRevisionComments",
                column: "ChallengeRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_ChallengeRequests_AssociatedSectorId",
                table: "ChallengeRequests",
                column: "AssociatedSectorId");

            migrationBuilder.CreateIndex(
                name: "IX_ChallengeRequests_SourceCompanyId",
                table: "ChallengeRequests",
                column: "SourceCompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_challengeTechnologiesRequests_ChallengeRequestId_TechnologyId",
                table: "challengeTechnologiesRequests",
                columns: new[] { "ChallengeRequestId", "TechnologyId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_challengeTechnologiesRequests_TechnologyId",
                table: "challengeTechnologiesRequests",
                column: "TechnologyId");

            migrationBuilder.CreateIndex(
                name: "IX_challengeTrackingHistories_ChallengeId",
                table: "challengeTrackingHistories",
                column: "ChallengeId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanySectors_CompanyId",
                table: "CompanySectors",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanySectors_CompanyId_SectorId",
                table: "CompanySectors",
                columns: new[] { "CompanyId", "SectorId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompanySectors_SectorId",
                table: "CompanySectors",
                column: "SectorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CampaignRequestEvaluationCriteria");

            migrationBuilder.DropTable(
                name: "CampaignRequestEvaluator");

            migrationBuilder.DropTable(
                name: "CampaignRequestLinkedChallenges");

            migrationBuilder.DropTable(
                name: "CampaignRequestSponsors");

            migrationBuilder.DropTable(
                name: "CampaignRequestTermsAndConditions");

            migrationBuilder.DropTable(
                name: "CampaignTrackingHistory");

            migrationBuilder.DropTable(
                name: "ChallengeRequestAttachments");

            migrationBuilder.DropTable(
                name: "challengeRequestRevisionComments");

            migrationBuilder.DropTable(
                name: "challengeTechnologiesRequests");

            migrationBuilder.DropTable(
                name: "challengeTrackingHistories");

            migrationBuilder.DropTable(
                name: "CompanySectors");

            migrationBuilder.DropTable(
                name: "Evaluators");

            migrationBuilder.DropTable(
                name: "Sponsors");

            migrationBuilder.DropTable(
                name: "CampaignRequests");

            migrationBuilder.DropTable(
                name: "technologies");

            migrationBuilder.DropTable(
                name: "ChallengeRequests");

            migrationBuilder.DropTable(
                name: "Companies");

            migrationBuilder.DropTable(
                name: "AssociatedProviders");

            migrationBuilder.DropTable(
                name: "AssociatedSectors");
        }
    }
}
