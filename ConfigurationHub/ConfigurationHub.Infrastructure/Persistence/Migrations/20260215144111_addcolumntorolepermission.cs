using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PartnersHub.ConfigurationHub.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class addcolumntorolepermission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AssignedBy",
                table: "RolePermissions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "AssignedDate",
                table: "RolePermissions",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AssignedBy",
                table: "RolePermissions");

            migrationBuilder.DropColumn(
                name: "AssignedDate",
                table: "RolePermissions");
        }
    }
}
