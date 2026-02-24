using System;

namespace SmartWinners.Models.Payment;

public class PaymentCredentials
{
    public string CardHolder { get; set; }
    
    public string CardNumber { get; set; }
    
    public DateTime ValidDate { get; set; }
    
    public string Cvv { get; set; }
}

public class NewPaymentCredentials
{
    public string CardHolder { get; set; }
    
    public string CardNumber { get; set; }
    
    public string ValidDate { get; set; }
    
    public string Cvv { get; set; }
}