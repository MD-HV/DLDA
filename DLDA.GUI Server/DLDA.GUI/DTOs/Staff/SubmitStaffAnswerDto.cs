using Microsoft.AspNetCore.Mvc;

namespace DLDA.GUI.DTOs.Staff
{
    // DTO för att skicka ett svar från personal (inkl. flagga och kommentar).
    public class SubmitStaffAnswerDto
    {
        public int ItemID { get; set; }           // Frågeradens ID
        public int Answer { get; set; }           // Svar 0–4
        public bool? Flag { get; set; }           // Markering för vidare diskussion
        public string? Comment { get; set; }      // Kommentar från personalen
    }
}
