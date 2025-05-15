using Microsoft.AspNetCore.Mvc;

namespace DLDA.GUI.DTOs
{
    // Hämta frågor till frontend.
    public class QuestionDto
    {
        public int QuestionID { get; set; }
        public int AssessmentID { get; set; }
        public int ItemID { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int Order { get; set; }
        public int Total { get; set; }
        public string? ScaleType { get; set; }
        public int AssessmentItemID { get; set; }
    }
}
