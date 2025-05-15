using Microsoft.AspNetCore.Mvc;

namespace DLDA.GUI.DTOs
{
    // DTO för att markera en fråga som överhoppad.
    public class SkipQuestionDto
    {
        public int ItemID { get; set; }           // Frågeradens ID att markera som "skippad"
    }
}
