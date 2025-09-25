using ALSS_invoice_back.data;
using ALSS_invoice_back.models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace ALSS_invoice_back.controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientsController : ControllerBase
{
    private readonly DataContext _context;

    public ClientsController(DataContext context)
    {
        _context = context;
    }

    // GET: api/clients
   // GET: api/clients
[HttpGet]
[Authorize]
public async Task<IActionResult> GetClients()
{
    var userIdClaim = User.FindFirst("Id")?.Value;
    if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
    {
        return Unauthorized("Invalid or missing user ID in token.");
    }

    var clients = await _context.Clients
        .Where(c => c.UserId == userId)
        .Select(c => new
        {
            c.Id,
            c.Name,
            c.Email,
            c.PhoneNumber,
            c.Address,
            c.City, // Добавляем поле города
            c.Country
        })
        .ToListAsync();

    return Ok(clients);
}

    // GET: api/clients/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetClient(int id)
    {
        var client = await _context.Clients.FindAsync(id);

        if (client == null)
        {
            return NotFound();
        }

        return Ok(client);
    }

   
[HttpPost]
[Authorize]
public async Task<ActionResult> CreateClient(Client client)
{
    var userIdClaim = User.FindFirst("Id")?.Value;
    if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        return Unauthorized("Invalid or missing user ID in token.");

    var user = await _context.Users.FindAsync(userId);
    if (user == null)
        return Unauthorized("User does not exist.");

    client.UserId = userId;

    _context.Clients.Add(client);
    await _context.SaveChangesAsync();

    return CreatedAtAction(nameof(GetClient), new { id = client.Id }, client);
}

    // PUT: api/clients/{id}
    [HttpPut("{id}")]
public async Task<IActionResult> UpdateClient(int id, Client client)
{
    var existingClient = await _context.Clients.FindAsync(id);

    if (existingClient == null)
        return NotFound();

    // Сохраняем текущий UserId
    client.UserId = existingClient.UserId;

    _context.Entry(existingClient).CurrentValues.SetValues(client);
    await _context.SaveChangesAsync();

    return NoContent();
}

    // DELETE: api/clients/{id}
    [HttpDelete("{id}")]
public async Task<IActionResult> DeleteClient(int id)
{
    var client = await _context.Clients
        .Include(c => c.Invoices)
        .FirstOrDefaultAsync(c => c.Id == id);

    if (client == null)
        return NotFound("Client not found.");

    if (client.Invoices.Any())
        return BadRequest("Cannot delete client with associated invoices.");

    _context.Clients.Remove(client);
    await _context.SaveChangesAsync();

    return NoContent();
}
}