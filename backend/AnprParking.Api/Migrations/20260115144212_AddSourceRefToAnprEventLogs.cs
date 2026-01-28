using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AnprParking.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddSourceRefToAnprEventLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "StripeSubscriptionId",
                table: "MemberVehicles",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "StripeCustomerId",
                table: "MemberVehicles",
                newName: "StripeSessionId");

            migrationBuilder.RenameColumn(
                name: "StartUtc",
                table: "MemberVehicles",
                newName: "ValidUntilUtc");

            migrationBuilder.RenameColumn(
                name: "EndUtc",
                table: "MemberVehicles",
                newName: "ValidFromUtc");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "PaymentRecords",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ProviderRef",
                table: "PaymentRecords",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Provider",
                table: "PaymentRecords",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Plate",
                table: "PaymentRecords",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Kind",
                table: "PaymentRecords",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Currency",
                table: "PaymentRecords",
                type: "character varying(8)",
                maxLength: 8,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<bool>(
                name: "IsMember",
                table: "ParkingSessions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "LastAmount",
                table: "ParkingSessions",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "ParkingSessions",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Source",
                table: "AnprEventLogs",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Plate",
                table: "AnprEventLogs",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "EventType",
                table: "AnprEventLogs",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "SourceRef",
                table: "AnprEventLogs",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRecords_Kind_PaidAtUtc",
                table: "PaymentRecords",
                columns: new[] { "Kind", "PaidAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRecords_PaidAtUtc",
                table: "PaymentRecords",
                column: "PaidAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRecords_UserId",
                table: "PaymentRecords",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ParkingSessions_EntryUtc",
                table: "ParkingSessions",
                column: "EntryUtc");

            migrationBuilder.CreateIndex(
                name: "IX_ParkingSessions_UserId",
                table: "ParkingSessions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_MemberVehicles_UserId",
                table: "MemberVehicles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AnprEventLogs_Plate",
                table: "AnprEventLogs",
                column: "Plate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PaymentRecords_Kind_PaidAtUtc",
                table: "PaymentRecords");

            migrationBuilder.DropIndex(
                name: "IX_PaymentRecords_PaidAtUtc",
                table: "PaymentRecords");

            migrationBuilder.DropIndex(
                name: "IX_PaymentRecords_UserId",
                table: "PaymentRecords");

            migrationBuilder.DropIndex(
                name: "IX_ParkingSessions_EntryUtc",
                table: "ParkingSessions");

            migrationBuilder.DropIndex(
                name: "IX_ParkingSessions_UserId",
                table: "ParkingSessions");

            migrationBuilder.DropIndex(
                name: "IX_MemberVehicles_UserId",
                table: "MemberVehicles");

            migrationBuilder.DropIndex(
                name: "IX_AnprEventLogs_Plate",
                table: "AnprEventLogs");

            migrationBuilder.DropColumn(
                name: "IsMember",
                table: "ParkingSessions");

            migrationBuilder.DropColumn(
                name: "LastAmount",
                table: "ParkingSessions");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "ParkingSessions");

            migrationBuilder.DropColumn(
                name: "SourceRef",
                table: "AnprEventLogs");

            migrationBuilder.RenameColumn(
                name: "ValidUntilUtc",
                table: "MemberVehicles",
                newName: "StartUtc");

            migrationBuilder.RenameColumn(
                name: "ValidFromUtc",
                table: "MemberVehicles",
                newName: "EndUtc");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "MemberVehicles",
                newName: "StripeSubscriptionId");

            migrationBuilder.RenameColumn(
                name: "StripeSessionId",
                table: "MemberVehicles",
                newName: "StripeCustomerId");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "PaymentRecords",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "ProviderRef",
                table: "PaymentRecords",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "Provider",
                table: "PaymentRecords",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(32)",
                oldMaxLength: 32);

            migrationBuilder.AlterColumn<string>(
                name: "Plate",
                table: "PaymentRecords",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(32)",
                oldMaxLength: 32);

            migrationBuilder.AlterColumn<string>(
                name: "Kind",
                table: "PaymentRecords",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(32)",
                oldMaxLength: 32);

            migrationBuilder.AlterColumn<string>(
                name: "Currency",
                table: "PaymentRecords",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(8)",
                oldMaxLength: 8);

            migrationBuilder.AlterColumn<string>(
                name: "Source",
                table: "AnprEventLogs",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(32)",
                oldMaxLength: 32);

            migrationBuilder.AlterColumn<string>(
                name: "Plate",
                table: "AnprEventLogs",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(32)",
                oldMaxLength: 32);

            migrationBuilder.AlterColumn<string>(
                name: "EventType",
                table: "AnprEventLogs",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(32)",
                oldMaxLength: 32);
        }
    }
}
