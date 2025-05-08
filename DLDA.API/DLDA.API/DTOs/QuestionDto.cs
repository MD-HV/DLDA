namespace DLDA.API.DTOs
{
    public class QuestionDto
    {
        public int QuestionID { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    // DTO för patientens svar
    public class SubmitAnswerDto
    {
        public int ItemID { get; set; }
        public int Answer { get; set; }
    }

    // DTO för personalens svar (inkl. flagga)
    public class SubmitStaffAnswerDto
    {
        public int ItemID { get; set; }
        public int Answer { get; set; }
        public bool? Flag { get; set; }
    }

    // DTO för att hoppa över fråga
    public class SkipQuestionDto
    {
        public int ItemID { get; set; }
    }
}
