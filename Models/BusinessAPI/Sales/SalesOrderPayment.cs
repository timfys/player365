namespace SmartWinners.Models.BusinessAPI;

	public class SalesOrderPayment
	{
		public int order_paymentId { get; set; }
		public int BusinessID { get; set; }
		public int PaymentID { get; set; }
		public int PaymentType { get; set; }
		public decimal PaymentValue { get; set; }
		public string Identification { get; set; }
		public string Date { get; set; }
		public string PaymentDate { get; set; }
		public string ChargedDate { get; set; }
		public string PayerName { get; set; }
		public string PayerNumber { get; set; }
		public string PayerNumber2 { get; set; }
		public string PayerNumber4 { get; set; }
		public string PayerNumber5 { get; set; }
		public string PayerNumber6 { get; set; }
		public string PayerDate { get; set; }
		public int CurrencyID { get; set; }
		public int Status { get; set; }
		public int Approved { get; set; }
		public string DeclinedRemark { get; set; }
		public int PaymentGroup { get; set; }
		public int OrderID { get; set; }
		public int EntityId { get; set; }
		public int ReciptID { get; set; }
		public decimal PaymentTotal { get; set; }
		public decimal ExchangeRate { get; set; }
		public int CancelReciptID { get; set; }
		public int PaymentQty { get; set; }
		public int FirstPayment { get; set; }
		public int ConstPayment { get; set; }
		public int RecurringNumber { get; set; }
		public int RetryCharge { get; set; }
		public string NextChargeRetryDate { get; set; }
		public int RetryChargeQuantity { get; set; }
		public string ChargedRemark { get; set; }
		public string SendToCCDate { get; set; }
		public string PaymentNeto { get; set; }
		public string DeclinedDate { get; set; }
		public string ChargedRemarkLog { get; set; }
		public string TransactionID { get; set; }
		public int ComissionPCT { get; set; }
		public int MinimumComission { get; set; }
		public int TransactionComission { get; set; }
	}
