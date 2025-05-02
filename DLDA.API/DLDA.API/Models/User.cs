using Microsoft.EntityFrameworkCore;

namespace DLDA.API.Models
{
    [PrimaryKey(nameof(UserID))]
    public class User
    {
        //Primarykey för users
        public int UserID { get; set; }
        //Användarnamn för users
        public string Username { get; set; }
        //Epostadress för användaren
        public string? Email { get; set; }
        //Hash för lösenord
        public string PasswordHash { get; set; }
        //Vilken roll användaren har, om det är en patient eller skötare
        public string Role { get; set; }
        //När användaren skapades
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        public ICollection<Assessment> Assessments { get; set; } = new List<Assessment>();
    }
}
