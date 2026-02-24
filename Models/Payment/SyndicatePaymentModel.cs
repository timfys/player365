namespace SmartWinners.Models.Payment;

public class SyndicatePaymentModel : PaymentModel
{
    public int SyndicateId { get; set; }
    
    public int MonthsPayCount { get; set; }
    
    public int DrawsToPay { get; set; }
    
    public decimal SyndicatePriceToPayUsd { get; set; }
}