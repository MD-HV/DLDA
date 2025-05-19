namespace DLDA.API.DTOs
{
    public class StaffQuestionDto
    {
        public int ItemID { get; set; }             // För PUT
        public int UserID { get; set; }
        public int AssessmentID { get; set; }
        public int QuestionID { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int Order { get; set; }
        public int Total { get; set; }
        public string? ScaleType { get; set; }

        // Patientens svar
        public int? PatientAnswer { get; set; }
        public string? PatientComment { get; set; }

        // Personalens svar
        public int? StaffAnswer { get; set; }
        public string? StaffComment { get; set; }

        // Flagga för vidare diskussion
        public bool Flag { get; set; }

        // Namn
        public string PatientName { get; set; } = string.Empty;
    }
}
