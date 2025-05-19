namespace DLDA.GUI.DTOs.Patient
{
    // DTO för patientens svar och eventuell kommentar
    public class PatientAnswerDto
    {
        public int? Answer { get; set; } // Patientens svar på frågan (kan vara null om obesvarad)

        public string? Comment { get; set; } // Eventuell kommentar från patienten
    }

    // DTO för att skicka ett patientsvar (inkl. kommentar).
    public class SubmitAnswerDto
    {
        public int ItemID { get; set; }           // Frågeradens ID som ska besvaras

        public int Answer { get; set; }           // Svar från patienten, skalad från 0 till 4

        public string? Comment { get; set; }      // Eventuell kommentar från patienten
    }
}
