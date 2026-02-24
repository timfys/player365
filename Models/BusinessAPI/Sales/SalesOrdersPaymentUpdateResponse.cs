using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace SmartWinners.Models.BusinessAPI.Sales;

	public class SalesOrdersPaymentUpdateResponse : ApiResponse
	{
		[JsonProperty("Order_PaymentId")]
		[JsonPropertyName("Order_PaymentId")]
		public int OrderPaymentId { get; set; }
		public int OrderId { get; set; }
	}
