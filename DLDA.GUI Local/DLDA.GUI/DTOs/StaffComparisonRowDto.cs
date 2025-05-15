namespace DLDA.GUI.DTOs
{
    public class StaffComparisonRowDto
    {
        public int QuestionNumber { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string PatientAnswer { get; set; } = string.Empty;
        public string StaffAnswer { get; set; } = string.Empty;
        public string Classification { get; set; } = string.Empty; // match, mild-diff, strong-diff, skipped
        public string Comment { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
    }
}
