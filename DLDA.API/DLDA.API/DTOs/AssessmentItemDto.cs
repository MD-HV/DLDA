namespace DLDA.API.DTOs
{
    public class AssessmentItemDto
    {
        public int ItemID { get; set; }
        public int AssessmentID { get; set; }
        public int QuestionID { get; set; }
        public int AnswerValue { get; set; }
        public bool Flag { get; set; }
    }
}
