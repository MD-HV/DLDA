namespace DLDA.API.DTOs
{
    // DTO för att skicka ett patientsvar (inkl. kommentar).
    public class SubmitAnswerDto
    {
        public int ItemID { get; set; }           // Frågeradens ID
        public int Answer { get; set; }           // Svar 0–4
        public string? Comment { get; set; }      // Kommentar från patienten
    }
}
