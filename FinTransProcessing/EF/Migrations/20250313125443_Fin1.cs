using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinTransProcessing.Migrations
{
    /// <inheritdoc />
    public partial class Fin1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "finance");

            migrationBuilder.RenameTable(
                name: "Transactions",
                newName: "Transactions",
                newSchema: "finance");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "Transactions",
                schema: "finance",
                newName: "Transactions");
        }
    }
}
