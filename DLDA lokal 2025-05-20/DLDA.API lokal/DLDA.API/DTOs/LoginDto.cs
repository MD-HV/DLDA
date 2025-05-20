using Microsoft.AspNetCore.Mvc;

// Inloggningsuppgifter som skickas från frontend till API.

namespace DLDA.API.DTOs
{
    public class LoginDto
    {
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
    }
}
