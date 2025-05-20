namespace DLDA.GUI.DTOs.Staff
{
    // DTO för att hantera frågor från personalens perspektiv
    public class StaffQuestionDto
    {
        public int ItemID { get; set; }             // ID för frågeobjektet (används för PUT-operationer)
        public int UserID { get; set; }             // Användar-ID för patienten
        public int AssessmentID { get; set; }       // ID för bedömningen
        public int QuestionID { get; set; }         // ID för frågan
        public string QuestionText { get; set; } = string.Empty;    // Texten för frågan, som är tom som standard
        public string Category { get; set; } = string.Empty;       // Kategorin för frågan, som är tom som standard
        public int Order { get; set; }              // Ordning för frågan i listan
        public int Total { get; set; }              // Totalt antal frågor
        public string? ScaleType { get; set; }      // Typ av skala för frågan, kan vara null

        // Patientens svar och kommentar
        public int? PatientAnswer { get; set; }     // Patientens svar, kan vara null
        public string? PatientComment { get; set; } // Kommentar från patienten, kan vara null

        // Personalens svar och kommentar
        public int? StaffAnswer { get; set; }       // Personalens svar, kan vara null
        public string? StaffComment { get; set; }   // Kommentar från personalen, kan vara null

        // Flagga för vidare diskussion
        public bool Flag { get; set; }              // Indikerar om frågan är flaggad för vidare diskussion

        // Namn på patienten
        public string PatientName { get; set; } = string.Empty;  // Namn på patienten, som är tom som standard
    }

    // DTO för att hantera personalens svar på frågor.
    public class StaffQuiz
    {
        public int? Answer { get; set; }     // Svar från personalen, kan vara null

        public string? Comment { get; set; }  // Kommentar från personalen, kan vara null

        public bool? Flag { get; set; }       // Flagga svar för ytterligare åtgärder, kan vara null
    }

    // DTO för att skicka ett svar från personal (inkl. flagga och kommentar).
    public class SubmitStaffAnswerDto
    {
        public int ItemID { get; set; }           // Frågeradens ID
        public int? Answer { get; set; }           // Svar 0–4
        public bool? Flag { get; set; }           // Markering för vidare diskussion
        public string? Comment { get; set; }      // Kommentar från personalen
    }
}