using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace testpushnotification.Migrations
{
    /// <inheritdoc />
    public partial class Addjwtfield : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "JwtToken",
                table: "Subscriptions",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "JwtToken",
                table: "Subscriptions");
        }
    }
}
