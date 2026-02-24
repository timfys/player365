using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace SmartWinners.Models.BusinessAPI.Sales;

	public class SalesOrderProductsUpdateResponse : ApiResponse
	{
		[JsonProperty("Order_ProductId")]
		[JsonPropertyName("Order_ProductId")]
		public int ProductId { get; set; }
		public int OrderId { get; set; }
		public int IsNegative { get; set; }
	}
