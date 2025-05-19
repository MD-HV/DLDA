using Microsoft.AspNetCore.Mvc;

namespace DLDA.GUI.DTOs.Assessment
{
    // Koppling mellan frågor och specifika bedömningar, med svar och kommentarer. Full info (för t.ex. personal, admin, statistik).
    public class AssessmentItemDto
    {
        public int ItemID { get; set; } // Unikt ID för varje bedömningsobjekt

        public int AssessmentID { get; set; } // ID för den övergripande bedömningen som detta objekt tillhör

        public int QuestionID { get; set; } // ID för den specifika frågan som bedöms

        public int? PatientAnswer { get; set; } // Patientens svar på frågan (om det finns)

        public string? PatientComment { get; set; } // Eventuell kommentar från patienten

        public int? StaffAnswer { get; set; } // Personalens svar på frågan (om det finns)

        public string? StaffComment { get; set; } // Eventuell kommentar från personalen

        public int Order { get; set; } // Ordning för frågan i bedömningen

        public bool Flag { get; set; } // Flagga för att markera en fråga för ytterligare diskussion av personal

        public bool SkippedByPatient { get; set; } // Anger om frågan har hoppats över av patienten
    }
}
