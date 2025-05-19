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
        var user = _context.Users.FirstOrDefault(u => u.Username == dto.Username);

        if (user == null)
        {
            Console.WriteLine("❌ Ingen användare hittades.");
            return Unauthorized("Felaktigt användarnamn eller lösenord.");
        }

        var valid = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
        Console.WriteLine($"🔐 Kontroll: lösenord matchar? {valid}");

        if (!valid)
            return Unauthorized("Felaktigt användarnamn eller lösenord.");

        return Ok(new AuthResponseDto
        {
            UserID = user.UserID,
            Username = user.Username,
            Role = user.Role
        });
    }

    [HttpPost("dev-update-admin")]
    public IActionResult DevUpdateAdmin()
    {
        var existing = _context.Users.FirstOrDefault(u => u.Username == "admin");

        var passwordHash = BCrypt.Net.BCrypt.HashPassword("password");

        if (existing != null)
        {
            existing.PasswordHash = passwordHash;
            existing.Role = "admin";
            existing.Email = "admin@gmail.com";
            existing.CreatedAt = DateTime.Now;
        }
        else
        {
            _context.Users.Add(new User
            {
                Username = "admin",
                Email = "admin@gmail.com",
                PasswordHash = passwordHash,
                Role = "admin",
                CreatedAt = DateTime.Now
            });
        }

        _context.SaveChanges();

        // Valfritt: rensa dubbletter
        var duplicates = _context.Users
            .Where(u => u.Username == "admin")
            .OrderBy(u => u.UserID)
            .Skip(1)
            .ToList();

        if (duplicates.Any())
        {
            _context.Users.RemoveRange(duplicates);
            _context.SaveChanges();
        }

        return Ok("✅ Admin uppdaterad/skapat.");
    }

}
