using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DatabaseProxy.Migrations
{
    /// <inheritdoc />
    public partial class IncludingCascadingDeletion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EntityProxies_EntityDependencyProxies_EntityDependencyProxy",
                table: "EntityProxies");

            migrationBuilder.AddForeignKey(
                name: "FK_EntityProxies_EntityDependencyProxies_EntityDependencyProxy",
                table: "EntityProxies",
                column: "EntityDependencyProxy",
                principalTable: "EntityDependencyProxies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EntityProxies_EntityDependencyProxies_EntityDependencyProxy",
                table: "EntityProxies");

            migrationBuilder.AddForeignKey(
                name: "FK_EntityProxies_EntityDependencyProxies_EntityDependencyProxy",
                table: "EntityProxies",
                column: "EntityDependencyProxy",
                principalTable: "EntityDependencyProxies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
