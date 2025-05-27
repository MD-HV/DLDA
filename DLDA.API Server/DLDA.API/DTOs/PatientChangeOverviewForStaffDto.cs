namespace DLDA.API.DTOs
{
    public class PatientChangeOverviewForStaffDto
    {
        public string Username { get; set; } = string.Empty;
        public DateTime PreviousDate { get; set; }
        public DateTime CurrentDate { get; set; }
        public List<ImprovementDto> Förbättringar { get; set; } = new();
        public List<ImprovementDto> Försämringar { get; set; } = new();
        public List<ImprovementDto> Hoppade { get; set; } = new(); // flaggade ej relevant för patient
    }
}
