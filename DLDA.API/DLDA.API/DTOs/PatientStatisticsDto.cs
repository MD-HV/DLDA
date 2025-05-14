namespace DLDA.API.DTOs
{
    public class PatientStatisticsDto
    {
        public int AssessmentId { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<PatientAnswerStatsDto> Answers { get; set; } = new();
    }

    public class PatientAnswerStatsDto
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public int? Answer { get; set; }
    }
}
