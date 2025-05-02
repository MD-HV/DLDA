using Microsoft.EntityFrameworkCore;

namespace DLDA.API.Models
{
    [PrimaryKey(nameof(AssessmentId))]
    public class Assessment
    {
        public int AssessmentId { get; set; }
        public string Type { get; set; }
        public string ScaleType { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsComplete { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }


    }
}
