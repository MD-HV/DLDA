using Microsoft.EntityFrameworkCore;

namespace DLDA.API.Models
{
    [PrimaryKey(nameof(AssessmentID))]
    public class Assessment
    {
        //Primärnyckel
        public int AssessmentID { get; set; }
        //Typen av bedömning, är det egen/själv eller med en skötare
        public string Type { get; set; }
        //Vilken skala använder patienten? Smilys eller 0-4?
        public string ScaleType { get; set; }
        //Tiden som den är skapad
        public DateTime CreatedAt { get; set; }
        //Tid som skattningen senast uppdaterades
        public DateTime UpdatedAt { get; set; }
        //Är skattningen klar
        public bool IsComplete { get; set; }

        //Kopplingar till användaren som svarar på skattningen
        public int UserId { get; set; }
        public User User { get; set; }


    }
}
