using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DLDA.API.Migrations
{
    /// <inheritdoc />
    public partial class AddSkippedByPatientToAssessmentItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "SkippedByPatient",
                table: "AssessmentItems",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SkippedByPatient",
                table: "AssessmentItems");
        }
    }
}
