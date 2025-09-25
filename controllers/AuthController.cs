using alss_invoice_back.models.classes;
using alss_invoice_back.services;
using ALSS_invoice_back.data;
using ALSS_invoice_back.models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace alss_invoice_back.controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly JwtService _jwtService;
        private readonly DataContext _context;

        public AuthController(JwtService jwtService, DataContext context)
        {
            _jwtService = jwtService;
            _context = context;
        }

 [HttpGet("me")]
[Authorize]
public async Task<IActionResult> GetCurrentUser()
{
    var username = User.Identity.Name;

    if (string.IsNullOrEmpty(username))
    {
        return Unauthorized("Token is invalid or does not contain a username.");
    }

    var user = await _context.Users
        .Where(u => u.Username == username)
        .Select(u => new { u.Id, u.Username, u.Email, u.AvatarUrl })
        .FirstOrDefaultAsync();

    if (user == null)
    {
        return NotFound("User not found.");
    }

    return Ok(user);
}


        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid registration data.");
            }

            // Проверяем существование пользователя с таким же email или username
            if (await _context.Users.AnyAsync(u => u.Email == model.Email || u.Username == model.Username))
            {
                return Conflict("A user with the same email or username already exists.");
            }

            // Создаем пользователя
            var user = new User
            {
                Username = model.Username,
                Email = model.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password) // Хэшируем пароль
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok("User registered successfully.");
        }

        [HttpPost("login")]
public async Task<IActionResult> Login([FromBody] LoginModel model)
{
    if (!ModelState.IsValid)
    {
        return BadRequest("Invalid login data.");
    }

    var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
    if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
    {
        return Unauthorized("Invalid email or password.");
    }

    string token = _jwtService.GenerateToken(user.Username, user.Id);

    return Ok(new { Token = token });

}   [HttpPost("avatar")]
        [Authorize]
        public async Task<IActionResult> UploadAvatar([FromForm] IFormFile avatar)
        {
            if (avatar == null || avatar.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            var username = User.Identity.Name;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Генерация уникального имени файла
            var fileName = $"{Guid.NewGuid()}_{avatar.FileName}";
            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", fileName); // Путь к файлу на сервере

            var directory = Path.GetDirectoryName(uploadPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory); // Создаем директорию, если ее нет
            }

            try
            {
                using (var stream = new FileStream(uploadPath, FileMode.Create))
                {
                    await avatar.CopyToAsync(stream); // Сохраняем файл
                }
                user.AvatarUrl = $"/uploads/{fileName}";
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }

            return Ok(new { AvatarUrl = user.AvatarUrl });
        }

        [HttpGet("users")]
        [Authorize(Roles = "Admin")]  // Убедитесь, что доступ к этому методу ограничен для администраторов, если необходимо
        public async Task<IActionResult> GetAllUsersWithAvatars()
        {
            // Получаем всех пользователей с аватарами из базы данных
            var users = await _context.Users
                .Select(u => new 
                {
                    u.Id,
                    u.Username,
                    u.Email,
                    AvatarUrl = string.IsNullOrEmpty(u.AvatarUrl) ? null : $"{Request.Scheme}://{Request.Host}{u.AvatarUrl}"
                })
                .ToListAsync();

            if (users == null || users.Count == 0)
            {
                return NotFound("No users found.");
            }

            return Ok(users);  // Возвращаем список пользователей с аватарами
        }

        [HttpPut("users/{id}/avatar")]
        [Authorize]
        public async Task<IActionResult> UploadAvatar(int id, IFormFile avatar)
        {
            if (avatar == null || avatar.Length == 0)
            {
                return BadRequest("No file selected.");
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", $"{Guid.NewGuid()}.jpg");

            try
            {
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await avatar.CopyToAsync(fileStream);
                }
            }
            catch (Exception ex)
            {
                // Логирование ошибки
                Console.Error.WriteLine($"Error saving avatar: {ex.Message}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }

            user.AvatarUrl = $"/uploads/{Path.GetFileName(filePath)}";
            await _context.SaveChangesAsync();

            return Ok(new { AvatarUrl = user.AvatarUrl });
        }

    }
    
    
}