using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace testpushnotification.Migrations
{
    /// <inheritdoc />
    public partial class Changekeytoid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Subscriptions",
                table: "Subscriptions");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "Subscriptions",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0)
                .Annotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Subscriptions",
                table: "Subscriptions",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_ClientId_Endpoint",
                table: "Subscriptions",
                columns: new[] { "ClientId", "Endpoint" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Subscriptions",
                table: "Subscriptions");

            migrationBuilder.DropIndex(
                name: "IX_Subscriptions_ClientId_Endpoint",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Subscriptions");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Subscriptions",
                table: "Subscriptions",
                column: "ClientId");
        }
    }
}
