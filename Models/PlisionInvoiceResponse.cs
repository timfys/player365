using Newtonsoft.Json;

namespace SmartWinners.Models;

public class PlisioInvoiceResponse
{
    public string Status { get; set; }
    public InvoiceData Data { get; set; }

    public class InvoiceData
    {
        [JsonProperty("txn_id")]
        public string TxnId { get; set; }
        [JsonProperty("invoice_url")]
        public string InvoiceUrl { get; set; }
        [JsonProperty("message")]
        public string Message { get; set; }
    }
}