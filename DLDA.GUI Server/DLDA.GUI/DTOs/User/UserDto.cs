using Microsoft.AspNetCore.Mvc;

namespace DLDA.GUI.DTOs.User
{
    // Grundläggande info om användare.
    public class UserDto
    {
        public int UserID { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string Role { get; set; } = string.Empty;
        public string? Password { get; set; }
    }
}
