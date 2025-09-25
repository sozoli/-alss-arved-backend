using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using ALSS_invoice_back.models;

namespace ALSS_invoice_back.models
{
    public class Client
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }

    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}
}