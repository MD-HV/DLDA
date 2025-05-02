using Microsoft.EntityFrameworkCore;

namespace DLDA.API.Models
{
    public class Question
    {
        // Primärnyckel
        public int QuestionID { get; set; }

        // Själva frågetexten
        public string QuestionText { get; set; } = string.Empty;

        // Grupp/kategori för frågan (t.ex. SelfCare, Mobility)
        public string Category { get; set; } = string.Empty;

        // Om frågan är aktiv
        public bool IsActive { get; set; }

        // När frågan skapades
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigationsegenskap för relation till AssessmentItems
        public ICollection<AssessmentItem> AssessmentItems { get; set; } = new List<AssessmentItem>();
    }
}
