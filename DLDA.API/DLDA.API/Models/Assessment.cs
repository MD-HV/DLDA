using Microsoft.EntityFrameworkCore;

namespace DLDA.API.Models
{
    [PrimaryKey(nameof(AssessmentID))]
    public class Assessment
    {
        public int AssessmentID { get; set; } 
        public string? ScaleType { get; set; }
        public DateTime? CreatedAt { get; set; }
        //Tid som skattningen senast uppdaterades
        public DateTime? UpdatedAt { get; set; }
        //Är skattningen klar
        public bool IsComplete { get; set; }

        //Kopplingar till användaren som svarar på skattningen
        public int UserId { get; set; }
        public User? User { get; set; }

        public ICollection<AssessmentItem> AssessmentItems { get; set; } = new List<AssessmentItem>();
    }
}
