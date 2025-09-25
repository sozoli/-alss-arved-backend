using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using ALSS_invoice_back.models;
using System.Text.Json.Serialization;


namespace ALSS_invoice_back.models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string? AvatarUrl { get; set; }

        [JsonIgnore]
        public List<Client> Clients { get; set; } = new();

        
    }
}