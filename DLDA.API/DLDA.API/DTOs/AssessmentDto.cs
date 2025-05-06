namespace DLDA.API.DTOs
{
    public class AssessmentDto
    {
        public int AssessmentID { get; set; }
        public string? Type { get; set; }
        public string? ScaleType { get; set; }
        public bool IsComplete { get; set; }
        public int UserId { get; set; }
    }
}
