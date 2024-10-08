using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MutualFundStats.Migrations
{
    /// <inheritdoc />
    public partial class AddMutualFundsTableToDb_v10 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Scheme_Id",
                table: "MutualFundDatas");

            migrationBuilder.AlterColumn<string>(
                name: "SchemeName",
                table: "Schemes",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "SchemeName",
                table: "Schemes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Scheme_Id",
                table: "MutualFundDatas",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
