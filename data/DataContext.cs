using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using alss_invoice_back.models;
using ALSS_invoice_back.models;
using Microsoft.EntityFrameworkCore;

namespace ALSS_invoice_back.data;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options) : base(options) { }

    public required DbSet<Client> Clients { get; set; }
    public required DbSet<Invoice> Invoices { get; set; }
    public required DbSet<InvoiceItem> InvoiceItems { get; set; }
    public required DbSet<Tax> Taxes { get; set; }
    public required DbSet<User> Users { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Client>().ToTable("Clients");

        // relations
        modelBuilder.Entity<Client>()
        .HasMany(c => c.Invoices)
        .WithOne(i => i.Client)
        .HasForeignKey(i => i.ClientId)
        .OnDelete(DeleteBehavior.Restrict); // Удаление клиента не удаляет счета

        modelBuilder.Entity<InvoiceItem>()
        .HasOne(ii => ii.Invoice)
        .WithMany(i => i.Items)
        .HasForeignKey(ii => ii.InvoiceId)
        .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<User>()
        .HasMany(u => u.Clients)
        .WithOne(c => c.User)
        .HasForeignKey(c => c.UserId)
        .OnDelete(DeleteBehavior.Cascade);
    }
}
