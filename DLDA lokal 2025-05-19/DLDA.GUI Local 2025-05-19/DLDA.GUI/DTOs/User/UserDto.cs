using Microsoft.AspNetCore.Mvc;

namespace DLDA.GUI.DTOs.User
{
    // DTO för grundläggande information om användare.
    public class UserDto
    {
        public int UserID { get; set; }                 // Användar-ID
        public string Username { get; set; } = string.Empty;  // Användarnamn, som är tomt som standard
        public string? Email { get; set; }             // E-postadress, som kan vara null
        public string Role { get; set; } = string.Empty;  // Rollen för användaren, som är tomt som standard
        public string? Password { get; set; }          // Lösenord, som kan vara null
    }
}