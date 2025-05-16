using Microsoft.AspNetCore.Mvc;

namespace DLDA.GUI.DTOs
{
    // Representerar själva bedömningen (typ, användare, skala osv.).  
    public class AssessmentDto
    {
        public int AssessmentID { get; set; }
        public string? ScaleType { get; set; }
        public bool IsComplete { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool HasStarted { get; set; }
        public int AnsweredCount { get; set; }   // Antal frågor som är besvarade
        public int TotalQuestions { get; set; }  // Totalt antal frågor
        public bool IsStaffComplete { get; set; } 
    }
}
