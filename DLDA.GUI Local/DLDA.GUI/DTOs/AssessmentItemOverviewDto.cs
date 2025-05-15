namespace DLDA.GUI.DTOs
{
    public class AssessmentItemOverviewDto
    {
        public int ItemID { get; set; }
        public int QuestionID { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public int? PatientAnswer { get; set; } // null om obesvarad
        public bool Flag { get; set; }
        public string? PatientComment { get; set; }
        public bool SkippedByPatient { get; set; }
    }
}
