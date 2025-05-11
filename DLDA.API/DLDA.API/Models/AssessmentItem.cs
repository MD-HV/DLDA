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

        // Patientens svar (0 = inget problem, 4 = mycket stort problem)
        public int? PatientAnswer { get; set; }

        // Patientens kommentar till frågan
        public string? PatientComment { get; set; }

        // Personalens svar
        public int? StaffAnswer { get; set; }

        // Personalens kommentar till frågan
        public string? StaffComment { get; set; }

        // När svaret lämnades (senaste svarstid)
        public DateTime AnsweredAt { get; set; } = DateTime.UtcNow;

        // Flagga från personalen för vidare diskussion
        public bool Flag { get; set; }

        // Navigationsegenskaper
        public Assessment? Assessment { get; set; }
        public Question? Question { get; set; }
    }
}
