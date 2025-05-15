namespace DLDA.API.DTOs
{
    public class StaffAnswerDto
    {
        public int? Answer { get; set; }
        public string? Comment { get; set; }
        public bool? Flag { get; set; } // 👈 Den här behövs!
    }
}
