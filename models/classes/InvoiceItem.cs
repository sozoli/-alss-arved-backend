namespace ALSS_invoice_back.models;
public class InvoiceItem
{
    public int Id { get; set; }
    public int InvoiceId { get; set; }
    public Invoice? Invoice { get; set; }
    public string? Service { get; set; } // Название товара или услуги
    public string? Description { get; set; }
    public int Quantity { get; set; } //kogus
    public decimal Unit { get; set; } //ühik
    public decimal TaxRate { get; set; } 
    public decimal Total => Quantity * Unit; // Рассчитывается на основе количества и цены за единицу
    public decimal TaxAmount => Total * (TaxRate / 100); // Сумма налога
}
