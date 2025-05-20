using DLDA.GUI.DTOs.Assessment;

namespace DLDA.GUI.DTOs.User
{
    public class PatientWithAssessmentStatusDto
    {
        public int UserID { get; set; }
        public string Username { get; set; } = "";

        public AssessmentDto? LastAssessment { get; set; }

        public DateTime? LastAssessmentDate => LastAssessment?.CreatedAt;
    }
}