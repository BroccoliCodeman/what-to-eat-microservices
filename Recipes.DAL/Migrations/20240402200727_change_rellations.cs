using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Recipes.DAL.Migrations
{
    /// <inheritdoc />
    public partial class change_rellations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ingredients_WeightUnits_WeightUnitId",
                table: "Ingredients");

            migrationBuilder.AlterColumn<int>(
                name: "WeightUnitId",
                table: "Ingredients",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Ingredients_WeightUnits_WeightUnitId",
                table: "Ingredients",
                column: "WeightUnitId",
                principalTable: "WeightUnits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ingredients_WeightUnits_WeightUnitId",
                table: "Ingredients");

            migrationBuilder.AlterColumn<int>(
                name: "WeightUnitId",
                table: "Ingredients",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Ingredients_WeightUnits_WeightUnitId",
                table: "Ingredients",
                column: "WeightUnitId",
                principalTable: "WeightUnits",
                principalColumn: "Id");
        }
    }
}
