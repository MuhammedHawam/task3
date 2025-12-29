using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PartnersHub.InnovationHub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserEmailInChallengeRequestAndCampaignRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserEmail",
                table: "ChallengeRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserEmail",
                table: "CampaignRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserEmail",
                table: "ChallengeRequests");

            migrationBuilder.DropColumn(
                name: "UserEmail",
                table: "CampaignRequests");
        }
    }
}
