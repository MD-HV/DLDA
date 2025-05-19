namespace DLDA.GUI.DTOs.Patient
{
    // DTO för sammanfattning av en enskild patientbedömning
    public class PatientSingleSummaryDto
    {
        public int AssessmentId { get; set; } // ID för bedömningen

        public int TotalQuestions { get; set; } // Totalt antal frågor i bedömningen

        public DateTime CreatedAt { get; set; } // Datum och tid då bedömningen skapades

        public int WithoutProblem { get; set; } // Antal frågor utan problem

        public int MinorIssues { get; set; } // Antal frågor med mindre problem

        public int Skipped { get; set; } // Antal hoppade frågor

        public Dictionary<int, int> AnswerDistribution { get; set; } = new(); // Fördelning av svar per fråga

        public List<string> Top5ProblematicQuestions { get; set; } = new(); // Lista med de 5 mest problematiska frågorna

        public List<PatientAnswerStatsDto> Answers { get; set; } = new(); // Lista med statistik för varje svar
    }
}
