using Microsoft.EntityFrameworkCore;

namespace DLDA.API.Models
{
    [PrimaryKey(nameof(ItemID))]
    public class AssessmentItem
    {
        // Primärnyckel
        public int ItemID { get; set; }

        // Foreign key för koppling till Assessment
        public int AssessmentID { get; set; }

        // Foreign key för koppling till Question
        public int QuestionID { get; set; }

        // Skattningsvärde (0 = inget problem, 4 = mycket stort problem)
        public int AnswerValue { get; set; }

        // När frågan besvarades
        public DateTime AnsweredAt { get; set; } = DateTime.UtcNow;

        // Flagga för vidare diskussion
        public bool Flag { get; set; }

        // Navigationsegenskaper
        public Assessment Assessment { get; set; }
        public Question Question { get; set; }
    }
}
