using Microsoft.AspNetCore.Mvc;

// Inloggningsuppgifter som skickas från frontend till API.

namespace DLDA.GUI.DTOs.Authentication
{
    public class LoginDto
    {
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
    }
}
