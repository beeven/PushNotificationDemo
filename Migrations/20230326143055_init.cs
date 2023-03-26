using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace testpushnotification.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Subscriptions",
                columns: table => new
                {
                    ClientId = table.Column<string>(type: "TEXT", nullable: false),
                    Endpoint = table.Column<string>(type: "TEXT", nullable: false),
                    Auth = table.Column<string>(type: "TEXT", nullable: false),
                    P256DH = table.Column<string>(type: "TEXT", nullable: false),
                    Expires = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    DateCreated = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    DateModified = table.Column<DateTimeOffset>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscriptions", x => x.ClientId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Subscriptions");
        }
    }
}
