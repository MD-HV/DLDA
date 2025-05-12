
// Returneras av API:t efter lyckad inloggning.

namespace DLDA.API.DTOs
{
    public class AuthResponseDto
    {
        public int UserID { get; set; }
        public string Username { get; set; } = "";
        public string Role { get; set; } = "";
    }
}
