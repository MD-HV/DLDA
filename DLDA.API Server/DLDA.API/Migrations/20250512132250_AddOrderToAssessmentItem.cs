using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DLDA.API.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderToAssessmentItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.AddColumn<int>(
                name: "Order",
                table: "AssessmentItems",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Order",
                table: "AssessmentItems");
        }
    }
}
