using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DLDA.API.Migrations
{
    /// <inheritdoc />
    public partial class AddPatientAndStaffAnswersToAssessmentItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PatientAnswer",
                table: "AssessmentItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StaffAnswer",
                table: "AssessmentItems",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PatientAnswer",
                table: "AssessmentItems");

            migrationBuilder.DropColumn(
                name: "StaffAnswer",
                table: "AssessmentItems");
        }
    }
}
