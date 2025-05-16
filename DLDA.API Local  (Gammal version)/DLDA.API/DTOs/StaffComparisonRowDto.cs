namespace DLDA.API.DTOs
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

    public class StaffChangeOverviewDto
    {
        public DateTime PreviousDate { get; set; }
        public DateTime CurrentDate { get; set; }
        public List<ImprovementDto> Förbättringar { get; set; } = new();
        public List<ImprovementDto> Försämringar { get; set; } = new();
        public List<ImprovementDto> Flaggade { get; set; } = new();
        public List<ImprovementDto> Hoppade { get; set; } = new();
    }
}
