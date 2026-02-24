using System.Collections.Generic;

namespace SmartWinners.Models.BusinessAPI.Sales;

	public class SalesOrdersPaymentsGetResponse : ApiResponse
	{
		public List<SalesOrderPayment>? OrderPayments { get; set; }
	}
