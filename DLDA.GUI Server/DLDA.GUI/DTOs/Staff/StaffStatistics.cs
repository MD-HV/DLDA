using DLDA.GUI.DTOs.Patient;  // Används för att importera PatientAnswerDto

namespace DLDA.GUI.DTOs.Staff
{
    // DTO för att representera en rad i jämförelse mellan personal och patient.
    public class StaffStatistics
    {
        public int QuestionNumber { get; set; }      // Nummer för frågan
        public string QuestionText { get; set; } = string.Empty;    // Texten för frågan, som är tom som standard
        public string Category { get; set; } = string.Empty;        // Kategorin för frågan, som är tom som standard

        // 🧑 Patient
        public int? PatientAnswer { get; set; }      // Patientens svar, kan vara null
        public string? PatientComment { get; set; }  // Kommentar från patienten, kan vara null

        // 👩‍⚕️ Personal
        public int? StaffAnswer { get; set; }        // Personalens svar, kan vara null
        public string? StaffComment { get; set; }    // Kommentar från personalen, kan vara null

        // 🟡 Klassificering (match, mild-diff, strong-diff, skipped)
        public string Classification { get; set; } = string.Empty;  // Klassificering av jämförelsen

        // ⛔ Fråga hoppad över av patient
        public bool SkippedByPatient { get; set; }   // Anger om frågan har hoppat över av patienten

        // 🚩 Markerad av personal för vidare diskussion
        public bool IsFlagged { get; set; }          // Anger om frågan har markerats för vidare diskussion av personalen

        // Info om datum och patientnamn
        public DateTime CreatedAt { get; set; }      // Datum då jämförelsen skapades
        public string Username { get; set; } = string.Empty;  // Användarnamnet för patienten
    }

    // DTO för att representera personalens förändringsöversikt
    public class StaffChangeOverviewDto
    {
        public string Username { get; set; } = string.Empty;  // Användarnamnet för patienten
        public DateTime PreviousDate { get; set; }            // Föregående datum för jämförelse
        public DateTime CurrentDate { get; set; }             // Nuvarande datum för jämförelse

        public List<ImprovementDto> Förbättringar { get; set; } = new();  // Lista över förbättringar
        public List<ImprovementDto> Försämringar { get; set; } = new();  // Lista över försämringar
        public List<ImprovementDto> Flaggade { get; set; } = new();     // Lista över flaggade frågor
        public List<ImprovementDto> Hoppade { get; set; } = new();      // Lista över hoppade frågor
    }
}
