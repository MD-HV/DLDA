using Microsoft.AspNetCore.Mvc;

namespace DLDA.GUI.DTOs.Assessment
{
    // Representerar själva bedömningen (typ, användare, skala osv.). 
    public class AssessmentDto
    {
        public int AssessmentID { get; set; } // Unikt ID för varje bedömning

        public string? ScaleType { get; set; } // Typ av skala för bedömningen (t.ex. Smiley, Likert-skala)

        public bool IsComplete { get; set; } // Anger om bedömningen är klar eller inte

        public int UserId { get; set; } // Användar-ID för den som utför bedömningen

        public DateTime CreatedAt { get; set; } // Datum och tid då bedömningen skapades

        public bool HasStarted { get; set; } // Anger om bedömningen har påbörjats

        public int AnsweredCount { get; set; } // Antal frågor som har besvarats

        public int TotalQuestions { get; set; } // Totalt antal frågor i bedömningen

        public bool IsStaffComplete { get; set; } // Anger om personalen har markerat bedömningen som klar
    }
}
