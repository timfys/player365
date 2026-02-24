using System;

namespace SmartWinners.Models.BusinessAPI.Sales;

	public class SalesOrdersPaymentUpdate
	{
		public int EntityId { get; set; }
		public int Id { get; set; }
		[UpdateField("PaymentId")] public int PaymentId { get; set; }

		[UpdateField("status")] public int Status { get; set; }

		[UpdateField("OrderId")] public int? OrderId { get; set; }

		[UpdateField("PaymentValue")] public long? AmountTotal { get; set; }

		[UpdateField("currencyIso")] public string? Currency { get; set; }

		[UpdateField("PayerName")] public string? PayerName { get; set; }

		[UpdateField("PayerNumber")] public string? PayerNumber { get; set; }

		[UpdateField("PayerNumber5")] public string? PayerNumber5 { get; set; }

		[UpdateField("PayerNumber6")] public string? PayerNumber6 { get; set; }

		[UpdateField("transactionID")] public string? TransactionId { get; set; }

		[UpdateField("ChargedRemark")] public string? ChargedRemark { get; set; }

		[UpdateField("DeclinedRemark")] public string? DeclinedRemark { get; set; }

		[UpdateField("order_currencyiso")] public string? OrderCurrencyIso { get; set; }

		[UpdateField("PayerDate")] public DateTime PayerDate { get; set; }

		[UpdateField("ChargedDate")] public DateTime ChargedDate { get; set; }
		[UpdateField("Employee_entityId")]
		public int EmployeeEntityId { get; set; }
	}