using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PrjMe.Migrations
{
    /// <inheritdoc />
    public partial class migEmpCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Temp",
                table: "ZaloTokens",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Temp",
                table: "ZaloTokens");
        }
    }
}
