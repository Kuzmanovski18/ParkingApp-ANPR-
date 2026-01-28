using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AnprParking.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddUserPayments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "PaymentRecords",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UserCars",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    Plate = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Label = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCars", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserCars_Plate",
                table: "UserCars",
                column: "Plate");

            migrationBuilder.CreateIndex(
                name: "IX_UserCars_UserId_Plate",
                table: "UserCars",
                columns: new[] { "UserId", "Plate" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserCars");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "PaymentRecords");
        }
    }
}
