using SmartWinners.PaymentSystem.StartAJob;

namespace SmartWinners.Helpers;

public class WebHookParams
{
    public decimal PaySum { get; set; } 
    public string Lid { get; set; } 
    
    public int EntityId { get; set; }

    public PaymentWindowType PaymentWindowType { get; set; }
    
    public int RecordId { get; set; }
}