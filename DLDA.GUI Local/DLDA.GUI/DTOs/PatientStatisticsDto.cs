namespace DLDA.GUI.DTOs
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

    public class PatientChangeOverviewDto
    {
        public List<ImprovementDto> Förbättringar { get; set; } = new();

        // 🔄 Automatisk beräkning: hur många färre frågor hoppades över?
        public int FärreHoppadeFrågor =>
            Förbättringar.Count(f => f.SkippedPrevious) - Förbättringar.Count(f => f.SkippedCurrent);

        // 🔤 Unika kategorier med förbättringar
        public List<string> FörbättradeKategorier =>
            Förbättringar
                .Where(f => !string.IsNullOrWhiteSpace(f.Category))
                .Select(f => f.Category)
                .Distinct()
                .OrderBy(k => k)
                .ToList();
    }

    public class ImprovementDto
    {
        public string Question { get; set; } = string.Empty;
        public int Previous { get; set; }
        public int Current { get; set; }
        public int Change => Previous - Current;
        public string Category { get; set; } = string.Empty;

        // 🔍 Hoppade över-data
        public bool SkippedPrevious { get; set; }
        public bool SkippedCurrent { get; set; }
    }

    public class ImprovementApiDto
    {
        public string Question { get; set; } = string.Empty;
        public int Previous { get; set; }
        public int Current { get; set; }
        public int Change => Previous - Current;
        public string Category { get; set; } = string.Empty;
        public bool SkippedPrevious { get; set; }
        public bool SkippedCurrent { get; set; }
    }
}
