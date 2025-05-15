namespace DLDA.API.DTOs
{
    public class StaffResultDto
    {
        public int AssessmentId { get; set; }
        public int UserId { get; set; }
        public string? Username { get; set; }
        public string? ScaleType { get; set; }
        public DateTime CreatedAt { get; set; }

        public List<StaffResultRowDto> Questions { get; set; } = new();
    }

    public class StaffResultRowDto
    {
        public int ItemID { get; set; }
        public int Order { get; set; } // För nummer i listan
        public string QuestionText { get; set; } = string.Empty;

        public int? PatientAnswer { get; set; }
        public int? StaffAnswer { get; set; }
        public string? PatientComment { get; set; }
        public string? StaffComment { get; set; }
        public bool Flag { get; set; }
        public bool SkippedByPatient { get; set; }

        public int Difference =>
            (PatientAnswer.HasValue && StaffAnswer.HasValue)
                ? Math.Abs(PatientAnswer.Value - StaffAnswer.Value)
                : -1;
    }

    public class StaffResultOverviewDto
    {
        public int AssessmentId { get; set; }
        public int UserId { get; set; }
        public string? Username { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsStaffComplete { get; set; } 
        public List<StaffResultRowDto> Questions { get; set; } = new();

    }

}
