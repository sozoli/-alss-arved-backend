using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ALSS_invoice_back.models;
public class Tax
{
    public int Id { get; set; }                 
    public decimal TaxRate { get; set; }         
    public decimal CalculateTax(decimal amount)  
    {
        return amount * TaxRate;
    }
}
