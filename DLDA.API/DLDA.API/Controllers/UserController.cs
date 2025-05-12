using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using DLDA.API.Data;
using DLDA.API.DTOs;
using DLDA.API.Models;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly AppDbContext _context;

    public UserController(AppDbContext context)
    {
        _context = context;
    }

    // --------------------------
    // [ADMIN] – Hantera userdefinitioner
    // --------------------------

    // GET: api/User
    [HttpGet]
    public ActionResult<IEnumerable<UserDto>> GetUsers()
    {
        return _context.Users
            .Select(u => new UserDto
            {
                UserID = u.UserID,
                Username = u.Username,
                Email = u.Email,
                Role = u.Role
            }).ToList();
    }

    // GET: api/User/patients?search=anna
    // Hämtar alla patienter, och filtrerar på namn om söksträng anges
    [HttpGet("patients")]
    public ActionResult<IEnumerable<UserDto>> GetPatients([FromQuery] string? search)
    {
        var query = _context.Users
            .Where(u => u.Role.ToLower() == "patient");

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(u => u.Username.ToLower().Contains(search.ToLower()));
        }

        return query
            .Select(u => new UserDto
            {
                UserID = u.UserID,
                Username = u.Username,
                Email = u.Email,
                Role = u.Role
            }).ToList();
    }

    // GET: api/User/5
    // Hämtar en specifik användare
    [HttpGet("{id}")]
    public ActionResult<UserDto> GetUser(int id)
    {
        var user = _context.Users.Find(id);
        if (user == null) return NotFound();

        return new UserDto
        {
            UserID = user.UserID,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role
        };
    }

    // POST: api/User
    // Skapar en ny användare med standardlösenord
    [HttpPost]
    public IActionResult CreateUser(UserDto dto)
    {
        var user = new User
        {
            Username = dto.Username,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password ?? "password"), // Om inget lösenord skickas: "password" används som default.
            Role = dto.Role,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        _context.SaveChanges();

        return CreatedAtAction(nameof(GetUser), new { id = user.UserID }, new UserDto
        {
            UserID = user.UserID,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role
        });
    }

    // PUT: api/User/5
    // Uppdaterar användarinformation
    [HttpPut("{id}")]
    public IActionResult UpdateUser(int id, UserDto dto)
    {
        if (id != dto.UserID) return BadRequest();

        var user = _context.Users.Find(id);
        if (user == null) return NotFound();

        user.Username = dto.Username;
        user.Email = dto.Email;
        user.Role = dto.Role;

        // Om nytt lösenord anges, uppdatera det
        if (!string.IsNullOrWhiteSpace(dto.Password))
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
        }

        _context.SaveChanges();
        return NoContent();
    }

    // DELETE: api/User/5
    // Tar bort en användare
    [HttpDelete("{id}")]
    public IActionResult DeleteUser(int id)
    {
        var user = _context.Users.Find(id);
        if (user == null) return NotFound();

        _context.Users.Remove(user);
        _context.SaveChanges();
        return NoContent();
    }

    // --------------------------
    // [PERSONAL] –  hämta listor på patienter
    // --------------------------

    // GET api/user/5
    // visa användarnamn och senaste bedömningens datum
    [HttpGet("with-latest-assessment")]
    public ActionResult<IEnumerable<PatientWithLatestAssessmentDto>> GetUsersWithLatestAssessment()
    {
        var result = _context.Users
            .Where(u => u.Role.ToLower() == "patient")
            .Select(u => new PatientWithLatestAssessmentDto
            {
                UserID = u.UserID,
                Username = u.Username,
                LastAssessmentDate = _context.Assessments
                    .Where(a => a.UserId == u.UserID)
                    .OrderByDescending(a => a.CreatedAt)
                    .Select(a => (DateTime?)a.CreatedAt) // 👈 detta gör att null returneras om inget finns
                    .FirstOrDefault()
            })
            .ToList();

        return Ok(result);
    }

}
