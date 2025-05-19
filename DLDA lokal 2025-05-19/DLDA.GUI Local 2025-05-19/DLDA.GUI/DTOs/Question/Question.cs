using Microsoft.AspNetCore.Mvc;

namespace DLDA.GUI.DTOs.Question
{
    // DTO för att hämta frågor till frontend.
    public class Question
    {
        public int QuestionID { get; set; }          // Frågans ID

        public int AssessmentID { get; set; }        // Bedömningens ID som frågan tillhör

        public int ItemID { get; set; }              // Frågeradens ID

        public string QuestionText { get; set; } = string.Empty;   // Själva frågetexten, som är tom som standard

        public string Category { get; set; } = string.Empty;      // Kategorin för frågan, som är tom som standard

        public bool IsActive { get; set; }           // Anger om frågan är aktiv eller inte

        public int Order { get; set; }               // Ordning för frågan, t.ex. 0–37

        public int Total { get; set; }               // Totalt antal frågor

        public string? ScaleType { get; set; }       // Typ av skalning för frågan, som är tom om inte specificerad

        public int AssessmentItemID { get; set; }    // ID för bedömningsobjektet som frågan tillhör

        public int? PatientAnswer { get; set; }      // Patientens svar, kan vara null om obesvarad

        public string? PatientComment { get; set; }  // Eventuell kommentar från patienten
    }

    // DTO för att markera en fråga som överhoppad.
    public class SkipQuestionDto
    {
        public int ItemID { get; set; }           // Frågeradens ID att markera som "skippad"
    }
}
