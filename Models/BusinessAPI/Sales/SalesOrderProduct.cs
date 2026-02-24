namespace SmartWinners.Models.BusinessAPI.Sales;

	public class SalesOrderProduct
	{
		public int ProductId { get; set; }
		public int AffiliateKind { get; set; }
		public int AffiliateValue { get; set; }
		public int AllowPriceChange { get; set; }
		public int BusinessID { get; set; }
		public int CurrencyID { get; set; }
		public int EntityId { get; set; }
		public int ExchangeRate { get; set; }
		public string ExpirationDate { get; set; }
		public int InventoryID { get; set; }
		public int InvoiceID { get; set; }
		public int IsForRecurring { get; set; }
		public int IsRecurrence { get; set; }
		public int OrderID { get; set; }
		public string OrderProductDate { get; set; }
		public int ParentID { get; set; }
		public int PaymentAmount { get; set; }
		public int PaymentAmountTotal { get; set; }
		public int Price { get; set; }
		public int PriceVat { get; set; }
		public int PriceWithVAT { get; set; }
		public string ProductName { get; set; }
		public int ProductType { get; set; }
		public int Purchase_ProductID { get; set; }
		public int Qty { get; set; }
		public int RecurringNumber { get; set; }
		public int RefundID { get; set; }
		public int ReturnLabelID { get; set; }
		public int ShippingLabelID { get; set; }
		public int Total { get; set; }
		public int TotalVat { get; set; }
	}
