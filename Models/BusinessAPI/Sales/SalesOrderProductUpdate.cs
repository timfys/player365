using System;
using System.Collections.Generic;

namespace SmartWinners.Models.BusinessAPI.Sales;

	public class SalesOrderProductUpdate
	{
		public int EntityId { get; set; }
		public Dictionary<string, string> NameValues { get; set; }
		public int Id { get; set; }
		public int Status { get; set; }
		public int OrderId { get; set; }
		public int PaymentId { get; set; }
		public long AmountTotal { get; set; }
		public string Currency { get; set; }
		public string? PayerName { get; set; }
		public string? PayerNumber { get; set; }
		public string? PayerNumber5 { get; set; }
		public string? PayerNumber6 { get; set; }
		public string? TransactionId { get; set; }
		public string? ChargedRemark { get; set; }
		public string? OrderCurrencyIso { get; set; }
		public DateTime PayerDate { get; set; }
		public DateTime ChargedDate { get; set; }
	}