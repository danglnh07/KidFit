using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KidFit.Migrations
{
    /// <inheritdoc />
    public partial class UniqueOnSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Modules_Name",
                table: "Modules");

            migrationBuilder.DropIndex(
                name: "IX_Cards_Name",
                table: "Cards");

            migrationBuilder.DropIndex(
                name: "IX_CardCategories_Name",
                table: "CardCategories");

            migrationBuilder.AddColumn<List<Guid>>(
                name: "CardIds",
                table: "Lessons",
                type: "uuid[]",
                nullable: false);

            migrationBuilder.CreateIndex(
                name: "IX_Modules_Name",
                table: "Modules",
                column: "Name",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Cards_Name",
                table: "Cards",
                column: "Name",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_CardCategories_Name",
                table: "CardCategories",
                column: "Name",
                unique: true,
                filter: "\"IsDeleted\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Modules_Name",
                table: "Modules");

            migrationBuilder.DropIndex(
                name: "IX_Cards_Name",
                table: "Cards");

            migrationBuilder.DropIndex(
                name: "IX_CardCategories_Name",
                table: "CardCategories");

            migrationBuilder.DropColumn(
                name: "CardIds",
                table: "Lessons");

            migrationBuilder.CreateIndex(
                name: "IX_Modules_Name",
                table: "Modules",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cards_Name",
                table: "Cards",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CardCategories_Name",
                table: "CardCategories",
                column: "Name",
                unique: true);
        }
    }
}
