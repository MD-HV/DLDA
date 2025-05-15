using Microsoft.EntityFrameworkCore;

namespace DLDA.API.Models
{
    [PrimaryKey(nameof(UserID))]
    public class User
    {
        //Primarykey för users
        public int UserID { get; set; }
        //Användarnamn för users
        public string Username { get; set; } = string.Empty;
        //Epostadress för användaren
        public string? Email { get; set; } = string.Empty;
        //Hash för lösenord
        public string PasswordHash { get; set; } = string.Empty;
        //Vilken roll användaren har, om det är en patient eller skötare
        public string Role { get; set; } = string.Empty;
        //När användaren skapades
        public DateTime CreatedAt { get; set; }

        public ICollection<Assessment> Assessments { get; set; } = new List<Assessment>();
    }
}
