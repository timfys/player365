using System.Collections.Generic;

namespace SmartWinners.Models.BusinessAPI.Sales;

	public class SalesOrderProductsGetResponse : ApiResponse
	{
		public List<SalesOrderProduct>? OrderPayments { get; set; }
	}
