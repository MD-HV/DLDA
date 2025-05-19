using Microsoft.AspNetCore.Mvc;

// DTO för autentiseringsrespons som returneras av API:t efter lyckad inloggning
namespace DLDA.GUI.DTOs.Authentication
{
    public class AuthResponseDto
    {
        public int UserID { get; set; } // Användarens ID

        public string Username { get; set; } = ""; // Användarens användarnamn, som är tomt som standard

        public string Role { get; set; } = ""; // Användarens roll, som är tom som standard
    }
}
