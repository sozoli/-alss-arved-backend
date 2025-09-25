
using ALSS_invoice_back.data;
using ALSS_invoice_back.models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace alss_invoice_back.controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RegisterController : ControllerBase
    {
        private readonly DataContext _context;

        public RegisterController(DataContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email))
            {
                return BadRequest("Kasutaja selle e-postiga juba olemas.");
            }

            if (string.IsNullOrWhiteSpace(registerDto.Username) || 
                string.IsNullOrWhiteSpace(registerDto.Email) || 
                string.IsNullOrWhiteSpace(registerDto.Password))
            {
                return BadRequest("Kõik väljad peavad olema täidetud.");
            }

            var user = new User
            {
                Username = registerDto.Username,
                Email = registerDto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Registreerimine õnnestus." });
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.Users
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email
                })
                .ToListAsync();

            return Ok(users);
        }
    }

    public class RegisterDto
    {
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
    }

    public class UserDto
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
    }
}