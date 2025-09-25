using ALSS_invoice_back.data;
using ALSS_invoice_back.models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ALSS_invoice_back.controllers;

[ApiController]
[Route("api/[controller]")]
public class InvoicesController : ControllerBase
{
    private readonly DataContext _context;

    public InvoicesController(DataContext context)
    {
        _context = context;
    }

    // GET: api/invoices
   [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetInvoices()
    {
        var userIdClaim = User.FindFirst("Id")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized("Invalid or missing user ID in token.");
        }

        // Загружаем счета, включая связанные данные (Client и Items)
        var invoices = await _context.Invoices
            .Include(i => i.Client) // Включаем клиента
            .Include(i => i.Items) // Включаем элементы счета
            .Where(i => i.UserId == userId)
            .ToListAsync();

        return Ok(invoices);
    }

    // GET: api/invoices/{id}
    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetInvoice(int id)
    {
        var userIdClaim = User.FindFirst("Id")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized("Invalid or missing user ID in token.");
        }

        var invoice = await _context.Invoices
            .Include(i => i.Client)
            .Include(i => i.Items) // Подгружаем позиции счета
            .FirstOrDefaultAsync(i => i.Id == id && i.UserId == userId);

        if (invoice == null)
        {
            return NotFound();
        }

        return Ok(invoice);
    }

    // POST: api/invoices
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateInvoice([FromBody] Invoice invoice)
    {
        var userIdClaim = User.FindFirst("Id")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized("Invalid or missing user ID in token.");
        }

        invoice.UserId = userId;

        // Если указан новый клиент, добавляем его
        if (invoice.ClientId == null && invoice.Client != null)
        {
            var existingClient = await _context.Clients
                .FirstOrDefaultAsync(c => c.Name == invoice.Client.Name && c.Email == invoice.Client.Email);

            if (existingClient != null)
            {
                invoice.ClientId = existingClient.Id;
            }
            else
            {
                _context.Clients.Add(invoice.Client);
                await _context.SaveChangesAsync();
                invoice.ClientId = invoice.Client.Id;
            }
        }

        // Убедимся, что даты в формате UTC
        invoice.InvoiceDate = invoice.InvoiceDate.ToUniversalTime();
        invoice.DueDate = invoice.DueDate.ToUniversalTime();

        // Рассчитываем SubTotal, TaxAmount, и TotalAmount
        invoice.SetSubTotal();

        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetInvoice), new { id = invoice.Id }, invoice);
    }

    // PUT: api/invoices/{id}
    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateInvoice(int id, Invoice updatedInvoice)
        {
            var existingInvoice = await _context.Invoices.FindAsync(id);

            if (existingInvoice == null)
            {
                return NotFound();
            }

            // Обновляем значения
            existingInvoice.InvoiceDate = updatedInvoice.InvoiceDate;
            existingInvoice.DueDate = updatedInvoice.DueDate;
            existingInvoice.TotalAmount = updatedInvoice.TotalAmount;
            existingInvoice.TaxAmount = updatedInvoice.TaxAmount;
            existingInvoice.IsPaid = updatedInvoice.IsPaid;

            // Обновляем суммы напрямую
            existingInvoice.SubTotal = updatedInvoice.SubTotal;
            existingInvoice.TaxAmount = updatedInvoice.TaxAmount;
            existingInvoice.TotalAmount = updatedInvoice.TotalAmount;

            await _context.SaveChangesAsync();

            return NoContent();
        }


    // DELETE: api/invoices/{id}
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteInvoice(int id)
    {
        var userIdClaim = User.FindFirst("Id")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized("Invalid or missing user ID in token.");
        }

        var invoice = await _context.Invoices
            .FirstOrDefaultAsync(i => i.Id == id && i.UserId == userId);

        if (invoice == null)
        {
            return NotFound();
        }

        _context.Invoices.Remove(invoice);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}