namespace DLDA.GUI.DTOs.Assessment
{
    // DTO för översikt av bedömning med information om bedömningens detaljer och frågor
    public class AssessmentOverviewDto
    {
        public int AssessmentId { get; set; } // Unikt ID för varje bedömning

        public string? ScaleType { get; set; } // Typ av skala för bedömningen (t.ex. Smiley, Likert-skala)

        public bool IsComplete { get; set; } // Anger om bedömningen är klar eller inte

        public DateTime CreatedAt { get; set; } // Datum och tid då bedömningen skapades

        public List<QuestionOverviewDto> Questions { get; set; } = new List<QuestionOverviewDto>(); // Lista med översikter över frågor i bedömningen
    }

    // DTO för översikt av fråga inom en bedömning
    public class QuestionOverviewDto
    {
        public int ItemID { get; set; } // Unikt ID för varje fråga i bedömningen

        public int QuestionId { get; set; } // ID för den specifika frågan

        public string? QuestionText { get; set; } // Texten för den specifika frågan

        public int? PatientAnswer { get; set; } // Patientens svar på frågan (kan vara null vid obesvarad)

        public string? PatientComment { get; set; } // Eventuell kommentar från patienten

        public int Order { get; set; } // Ordningen för frågan i bedömningen (t.ex. 0-37)

        public int Total { get; set; } // Totalt antal frågor i bedömningen (t.ex. 38)
    }
}
