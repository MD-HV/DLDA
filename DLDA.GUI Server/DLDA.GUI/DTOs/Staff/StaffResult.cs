namespace DLDA.GUI.DTOs.Staff
{
    // DTO för att skicka personalens resultat till frontend
    public class StaffResult
    {
        public int AssessmentId { get; set; }          // Bedömnings-ID
        public int UserId { get; set; }                // Användar-ID för patienten
        public string? Username { get; set; }          // Användarnamn
        public string? ScaleType { get; set; }         // Typ av skala för bedömningen
        public DateTime CreatedAt { get; set; }        // Skapat datum för bedömningen

        public List<StaffResultRowDto> Questions { get; set; } = new();  // Lista av resultat för varje fråga
    }

    // DTO för att representera varje frågerad i personalens resultat
    public class StaffResultRowDto
    {
        public int ItemID { get; set; }                // ID för frågeobjektet
        public int Order { get; set; }                 // Ordning för frågan i listan
        public string QuestionText { get; set; } = string.Empty;  // Text för frågan, som är tom som standard

        public int? PatientAnswer { get; set; }         // Patientens svar
        public int? StaffAnswer { get; set; }           // Personalens svar
        public string? PatientComment { get; set; }     // Kommentar från patienten
        public string? StaffComment { get; set; }       // Kommentar från personalen
        public bool Flag { get; set; }                  // Indikerar om frågan är flaggad för vidare diskussion
        public bool SkippedByPatient { get; set; }      // Indikerar om frågan hoppades över av patienten

        public int Difference =>                       // Beräknar skillnaden mellan patientens och personalens svar
            PatientAnswer.HasValue && StaffAnswer.HasValue
                ? Math.Abs(PatientAnswer.Value - StaffAnswer.Value)
                : -1;                                 // Returnerar -1 om antingen svar saknas
    }

    // Översikts-DTO för personalens resultat
    public class StaffResultOverviewDto
    {
        public int AssessmentId { get; set; }          // Bedömnings-ID
        public int UserId { get; set; }                // Användar-ID för patienten
        public string? Username { get; set; }          // Användarnamn
        public DateTime CreatedAt { get; set; }        // Skapat datum för bedömningen
        public bool IsStaffComplete { get; set; }      // Indikerar om personalen har slutfört bedömningen

        public List<StaffResultRowDto> Questions { get; set; } = new();  // Lista av resultat för varje fråga
    }

}
