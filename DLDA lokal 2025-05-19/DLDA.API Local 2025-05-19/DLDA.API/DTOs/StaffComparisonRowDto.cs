namespace DLDA.API.DTOs
{
    public class StaffComparisonRowDto
    {
        public int QuestionNumber { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;

        // 🧑 Patient
        public int? PatientAnswer { get; set; }
        public string? PatientComment { get; set; }

        // 👩‍⚕️ Personal
        public int? StaffAnswer { get; set; }
        public string? StaffComment { get; set; }

        // 🟡 Klassificering (match, mild-diff, strong-diff, skipped)
        public string Classification { get; set; } = string.Empty;

        // ⛔ Fråga hoppad över av patient
        public bool SkippedByPatient { get; set; }

        // 🚩 Markerad av personal för vidare diskussion
        public bool IsFlagged { get; set; }

        // Info om datum och patientnamn
        public DateTime CreatedAt { get; set; }
        public string Username { get; set; } = string.Empty;

    }


    public class StaffChangeOverviewDto
    {
        public string Username { get; set; } = string.Empty;
        public DateTime PreviousDate { get; set; }
        public DateTime CurrentDate { get; set; }
        public List<ImprovementDto> Förbättringar { get; set; } = new();
        public List<ImprovementDto> Försämringar { get; set; } = new();
        public List<ImprovementDto> Flaggade { get; set; } = new();
        public List<ImprovementDto> Hoppade { get; set; } = new();
    }
}
