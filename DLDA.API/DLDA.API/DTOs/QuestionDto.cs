namespace DLDA.API.DTOs
{
    // Hämta frågor till frontend.
    public class QuestionDto
    {
        public int QuestionID { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
