using Microsoft.AspNetCore.Mvc;
using DLDA.API.Data;
using DLDA.API.DTOs;
using DLDA.API.Models;
using BCrypt.Net;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;

    public AuthController(AppDbContext context)
    {
        _context = context;
    }

    // POST: api/Auth/login
    [HttpPost("login")]
    public ActionResult<AuthResponseDto> Login(LoginDto dto)
    {
        var user = _context.Users.SingleOrDefault(u => u.Username == dto.Username);
        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return Unauthorized("Felaktigt användarnamn eller lösenord.");

        return Ok(new AuthResponseDto
        {
            UserID = user.UserID,
            Username = user.Username,
            Role = user.Role
        });
    }
}
