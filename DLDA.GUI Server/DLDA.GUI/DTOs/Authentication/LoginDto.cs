using Microsoft.AspNetCore.Mvc;

// DTO för inloggningsuppgifter som skickas från frontend till API
namespace DLDA.GUI.DTOs.Authentication
{
    public class LoginDto
    {
        public string Username { get; set; } = ""; // Användarens användarnamn, som är tomt som standard

        public string Password { get; set; } = ""; // Användarens lösenord, som är tomt som standard
    }
}
