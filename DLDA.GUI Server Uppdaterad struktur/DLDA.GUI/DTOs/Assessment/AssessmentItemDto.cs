using Microsoft.AspNetCore.Mvc;

namespace DLDA.GUI.DTOs.Assessment
{
    // Koppling mellan frågor och specifika bedömningar, med svar och kommentarer. Full info (för t.ex. personal, admin, statistik).
    public class AssessmentItemDto
    {
        public int ItemID { get; set; }
        public int AssessmentID { get; set; }
        public int QuestionID { get; set; }

        public int? PatientAnswer { get; set; }
        public string? PatientComment { get; set; }

        public int? StaffAnswer { get; set; }
        public string? StaffComment { get; set; }
        public int Order { get; set; }
        public bool Flag { get; set; }
        public bool SkippedByPatient { get; set; }
    }
}
