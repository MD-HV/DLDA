namespace DLDA.GUI.DTOs.Assessment
{
    // DTO för översikt av bedömningsobjekt med grundläggande information om frågor och svar från patienten
    public class AssessmentItemOverviewDto
    {
        public int ItemID { get; set; } // Unikt ID för varje bedömningsobjekt

        public int QuestionID { get; set; } // ID för den specifika frågan som bedöms

        public string QuestionText { get; set; } = string.Empty; // Texten för den specifika frågan, som är tom som standard

        public int? PatientAnswer { get; set; } // Patientens svar på frågan (null om obesvarad)

        public bool Flag { get; set; } // Flagga för att markera en fråga för ytterligare diskussion

        public string? PatientComment { get; set; } // Eventuell kommentar från patienten

        public bool SkippedByPatient { get; set; } // Anger om frågan har hoppats över av patienten
    }
}
