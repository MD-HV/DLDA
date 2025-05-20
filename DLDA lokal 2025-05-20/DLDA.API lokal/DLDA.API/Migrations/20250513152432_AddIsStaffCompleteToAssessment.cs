using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DLDA.API.Migrations
{
    /// <inheritdoc />
    public partial class AddIsStaffCompleteToAssessment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsStaffComplete",
                table: "Assessments",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsStaffComplete",
                table: "Assessments");
        }
    }
}
