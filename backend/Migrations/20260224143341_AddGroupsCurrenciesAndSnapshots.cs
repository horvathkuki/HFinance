using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddGroupsCurrenciesAndSnapshots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "Holdings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "USD");

            migrationBuilder.AddColumn<int>(
                name: "GroupId",
                table: "Holdings",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "HoldingGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoldingGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PortfolioSnapshots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PortfolioId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CapturedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BaseCurrency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TotalMarketValueBase = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalCostBasisBase = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalUnrealizedPnLBase = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsPartial = table.Column<bool>(type: "bit", nullable: false),
                    MissingSymbolsCount = table.Column<int>(type: "int", nullable: false),
                    FxTimestampUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EurUsdRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EurRonRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PortfolioSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PortfolioSnapshots_Portfolios_PortfolioId",
                        column: x => x.PortfolioId,
                        principalTable: "Portfolios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Holdings_GroupId",
                table: "Holdings",
                column: "GroupId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Holdings_Currency",
                table: "Holdings",
                sql: "[Currency] IN ('EUR', 'USD', 'RON')");

            migrationBuilder.CreateIndex(
                name: "IX_HoldingGroups_UserId_Name",
                table: "HoldingGroups",
                columns: new[] { "UserId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PortfolioSnapshots_PortfolioId_CapturedAtUtc",
                table: "PortfolioSnapshots",
                columns: new[] { "PortfolioId", "CapturedAtUtc" });

            migrationBuilder.Sql(
                """
                INSERT INTO HoldingGroups (Name, Description, UserId, CreatedAtUtc, UpdatedAtUtc)
                SELECT 'Uncategorized', 'Default group', p.UserId, SYSUTCDATETIME(), SYSUTCDATETIME()
                FROM (SELECT DISTINCT UserId FROM Portfolios) p
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM HoldingGroups g
                    WHERE g.UserId = p.UserId AND g.Name = 'Uncategorized'
                );
                """);

            migrationBuilder.Sql(
                """
                UPDATE h
                SET h.GroupId = g.Id
                FROM Holdings h
                INNER JOIN Portfolios p ON p.Id = h.PortfolioId
                INNER JOIN HoldingGroups g ON g.UserId = p.UserId AND g.Name = 'Uncategorized'
                WHERE h.GroupId IS NULL;
                """);

            migrationBuilder.AlterColumn<int>(
                name: "GroupId",
                table: "Holdings",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Holdings_HoldingGroups_GroupId",
                table: "Holdings",
                column: "GroupId",
                principalTable: "HoldingGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Holdings_HoldingGroups_GroupId",
                table: "Holdings");

            migrationBuilder.DropTable(
                name: "HoldingGroups");

            migrationBuilder.DropTable(
                name: "PortfolioSnapshots");

            migrationBuilder.DropIndex(
                name: "IX_Holdings_GroupId",
                table: "Holdings");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Holdings_Currency",
                table: "Holdings");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "Holdings");

            migrationBuilder.DropColumn(
                name: "GroupId",
                table: "Holdings");
        }
    }
}
