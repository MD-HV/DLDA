namespace DLDA.API.DTOs
{
    public class AssessmentOverviewDto
    {
        public int AssessmentId { get; set; }
        public string? ScaleType { get; set; }
        public bool IsComplete { get; set; }
        public DateTime CreatedAt { get; set; }

        public List<QuestionOverviewDto> Questions { get; set; } = new List<QuestionOverviewDto>();
    }

    public class QuestionOverviewDto
    {
        public int ItemID { get; set; }
        public int QuestionId { get; set; }
        public string? QuestionText { get; set; }
        public int? PatientAnswer { get; set; }  // Kan vara null vid obesvarad
        public string? PatientComment { get; set; }
        public int Order { get; set; } // t.ex. 0–37
        public int Total { get; set; } // t.ex. 38
    }
}
