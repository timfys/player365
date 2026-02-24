using System.Collections.Generic;

namespace SmartWinners.Models.BusinessAPI.Sales;

	public class SalesOrderPaymentGet
	{
		public string[]? Fields { get; set; }
		public Dictionary<string, string>? Filter { get; set; }
		public int LimitFrom { get; set; } = 0;
		public int LimitCount { get; set; } = 0;
	}
