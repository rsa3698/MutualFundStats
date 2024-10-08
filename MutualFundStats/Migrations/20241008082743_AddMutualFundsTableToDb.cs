using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MutualFundStats.Migrations
{
    /// <inheritdoc />
    public partial class AddMutualFundsTableToDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Schemes",
                columns: table => new
                {
                    SchemeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SchemeName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Schemes", x => x.SchemeId);
                });

            migrationBuilder.CreateTable(
                name: "MutualFundDatas",
                columns: table => new
                {
                    StatId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SchemeId = table.Column<int>(type: "int", nullable: false),
                    NumberOfSchemes = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NumberOfFolios = table.Column<int>(type: "int", nullable: false),
                    FundsMobilized = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RepurchaseRedemption = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NetInflowOutflow = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NetAssetsUnderManagement = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AverageNetAssetsUnderManagement = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Scheme_Id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MutualFundDatas", x => x.StatId);
                    table.ForeignKey(
                        name: "FK_MutualFundDatas_Schemes_SchemeId",
                        column: x => x.SchemeId,
                        principalTable: "Schemes",
                        principalColumn: "SchemeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MutualFundDatas_SchemeId",
                table: "MutualFundDatas",
                column: "SchemeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MutualFundDatas");

            migrationBuilder.DropTable(
                name: "Schemes");
        }
    }
}
