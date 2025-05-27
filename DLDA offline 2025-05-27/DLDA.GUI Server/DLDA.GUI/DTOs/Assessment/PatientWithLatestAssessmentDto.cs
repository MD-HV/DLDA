namespace DLDA.GUI.DTOs.Assessment
{
    // DTO för att hämta och visa användarnamn samt senaste bedömningens datum till personal
    public class PatientWithLatestAssessmentDto
    {
        public int UserID { get; set; } // Användar-ID för patienten

        public string Username { get; set; } = ""; // Användarnamn för patienten, som är tomt som standard

        public DateTime? LastAssessmentDate { get; set; } // Datum för den senaste bedömningen, kan vara null om ingen bedömning finns
    }
}
