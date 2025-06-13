using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dzeta.TonWatcher.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:uuid-ossp", ",,");

            migrationBuilder.CreateTable(
                name: "transactions",
                columns: table => new
                {
                    hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    lt = table.Column<long>(type: "bigint", nullable: false),
                    account_address = table.Column<string>(type: "character varying(70)", maxLength: 70, nullable: false),
                    success = table.Column<bool>(type: "boolean", nullable: false),
                    utime = table.Column<long>(type: "bigint", nullable: false),
                    webhook_notified = table.Column<bool>(type: "boolean", nullable: false),
                    webhook_notified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    transaction_data = table.Column<string>(type: "jsonb", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transactions", x => x.hash);
                });

            migrationBuilder.CreateIndex(
                name: "IX_transactions_lt",
                table: "transactions",
                column: "lt");

            migrationBuilder.CreateIndex(
                name: "IX_transactions_webhook_notified",
                table: "transactions",
                column: "webhook_notified");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "transactions");
        }
    }
}
