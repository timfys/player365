using Newtonsoft.Json;

namespace SmartWinners.Models;

public class PlisioWebhookRequest
{
    [JsonProperty("txn_id")]
    public string txn_id { get; set; }

    [JsonProperty("ipn_type")]
    public string ipn_type { get; set; }

    [JsonProperty("merchant")]
    public string merchant { get; set; }

    [JsonProperty("merchant_id")]
    public string merchant_id { get; set; }

    [JsonProperty("amount")]
    public decimal amount { get; set; }

    [JsonProperty("currency")]
    public string currency { get; set; }

    [JsonProperty("order_number")]
    public string order_number { get; set; }

    [JsonProperty("order_name")]
    public string order_name { get; set; }

    [JsonProperty("confirmations")]
    public int confirmations { get; set; }

    [JsonProperty("status")]
    public string status { get; set; }

    [JsonProperty("source_currency")]
    public string source_currency { get; set; }

    [JsonProperty("source_amount")]
    public decimal source_amount { get; set; }

    [JsonProperty("source_rate")]
    public decimal source_rate { get; set; }

    [JsonProperty("comment")]
    public string comment { get; set; }

    [JsonProperty("verify_hash")]
    public string verify_hash { get; set; }

    [JsonProperty("invoice_commission")]
    public decimal invoice_commission { get; set; }

    [JsonProperty("invoice_sum")]
    public string invoice_sum { get; set; }

    [JsonProperty("invoice_total_sum")]
    public decimal invoice_total_sum { get; set; }
}