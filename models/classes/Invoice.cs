using System;
using ALSS_invoice_back.models;

namespace ALSS_invoice_back.models
{
    public class Invoice
    {
        public int Id { get; set; }                  
        public int? ClientId { get; set; }   
        public Client? Client { get; set; }          
        public DateTime InvoiceDate { get; set; }   
        public DateTime DueDate { get; set; } 
        // Полная сумма с налогом
        public decimal TotalAmount { get; set; }     
        // Налог в процентах
        public decimal TaxAmount { get; set; }       
        // Сумма без налога (вычисляется автоматически)
        public decimal SubTotal { get; set; }
        public bool IsPaid { get; set; }      
         public string? ReferenceNumber { get; set; } // Номер ссылки или Viitenumber 
         public decimal Penalty { get; set; } // Viivis 
         public string? IBAN { get; set; }
         public string? RecipientName { get; set; } // Имя получателя платежа
         public ICollection<InvoiceItem> Items { get; set; } = new List<InvoiceItem>(); 

        // Метод для автоматического расчета SubTotal
        public int UserId { get; set; } // Идентификатор пользователя
        public User? User { get; set; }
        public void SetSubTotal()
{
    // Проверка, есть ли данные в Items
    if (Items == null || !Items.Any())
    {
        SubTotal = 0;
        TaxAmount = 0;
        TotalAmount = 0;
        return;
    }

    // Расчет SubTotal, TaxAmount и TotalAmount
    SubTotal = Items.Sum(item => item.Total);
    TaxAmount = Items.Sum(item => item.TaxAmount);
    TotalAmount = SubTotal + TaxAmount;
}       public void ConvertDatesToUtc()
        {
        if (InvoiceDate.Kind == DateTimeKind.Unspecified)
    {
        InvoiceDate = DateTime.SpecifyKind(InvoiceDate, DateTimeKind.Utc);
    }
    else
    {
        InvoiceDate = InvoiceDate.ToUniversalTime();
    }

    if (DueDate.Kind == DateTimeKind.Unspecified)
    {
        DueDate = DateTime.SpecifyKind(DueDate, DateTimeKind.Utc);
    }
    else
    {
        DueDate = DueDate.ToUniversalTime();
    }
}
}
}