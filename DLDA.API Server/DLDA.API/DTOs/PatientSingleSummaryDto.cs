namespace DLDA.API.DTOs
{
    public class PatientSingleSummaryDto
    {
        public int AssessmentId { get; set; }
        public int TotalQuestions { get; set; }
        public DateTime CreatedAt { get; set; }
        public int WithoutProblem { get; set; }
        public int MinorIssues { get; set; }
        public int Skipped { get; set; }
        public Dictionary<int, int> AnswerDistribution { get; set; } = new();
        public List<string> Top5ProblematicQuestions { get; set; } = new();
        public List<PatientAnswerStatsDto> Answers { get; set; } = new();
    }
}
