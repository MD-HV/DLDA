namespace DLDA.API.DTOs
{
    public class AssessmentItemDto
    {
        public int ItemID { get; set; }
        public int AssessmentID { get; set; }
        public int QuestionID { get; set; }
        // public int AnswerValue { get; set; } Har bytt ut denna med PatientAnswer och StaffAnswer

        public int? PatientAnswer { get; set; }  // Nytt fält
        public int? StaffAnswer { get; set; }    // Nytt fält
        public bool Flag { get; set; }
    }
}
