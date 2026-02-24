using BusinessApi;
using Newtonsoft.Json;
using SmartWinners.Helpers;
using SmartWinners.Models.BusinessAPI;
using SmartWinners.Models.BusinessAPI.Entity;
using SmartWinners.Models.BusinessAPI.Sales;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SmartWinners.Services;

public class BusinessApiService
{
	public async Task<SalesOrdersPaymentUpdateResponse> SalesOrdersPaymentUpdate(SalesOrdersPaymentUpdate model)
	{
		if (model.EntityId <= 0)
			throw new ArgumentException("EntityId must be provided", nameof(model.EntityId));

		var updateFields = new List<(string FieldName, string Value)>();
		var properties = typeof(SalesOrdersPaymentUpdate).GetProperties();
		foreach (var prop in properties)
		{
			var updateAttr = prop.GetCustomAttribute<UpdateFieldAttribute>();
			if (updateAttr == null)
				continue;

			if (prop.Name == nameof(Models.BusinessAPI.Sales.SalesOrdersPaymentUpdate.PaymentId) && model.Id > 0)
				continue;

			var rawValue = prop.GetValue(model);
			if (rawValue == null)
				continue;

			string valueStr;
			if (prop.Name == nameof(Models.BusinessAPI.Sales.SalesOrdersPaymentUpdate.AmountTotal) && rawValue is long amount)
			{
				valueStr = FormatsHelper.ConvertCentsToNormal(amount);
			}
			else if (rawValue is DateTime dt)
			{
				if (updateAttr.FieldName == "PayerDate")
					valueStr = dt.ToString("yyyy-MM-dd");
				else if (updateAttr.FieldName == "ChargedDate")
					valueStr = dt.ToString("yyyy-MM-dd HH:mm:ss");
				else
					valueStr = dt.ToString();
			}
			else if (rawValue is string str)
			{
				valueStr = prop.Name == nameof(Models.BusinessAPI.Sales.SalesOrdersPaymentUpdate.Currency)
					? str.ToUpperInvariant()
					: str;
			}
			else
				valueStr = rawValue.ToString()!;

			updateFields.Add((updateAttr.FieldName, valueStr));
		}

		// Always include the Employee_entityId from your options.
		updateFields.Add(("Employee_entityId", "4"));

		var config = EnvironmentHelper.BusinessApiConfiguration;
		var client = config.InitClient();
		// Build the update request.
		var request = new Sales_Orders_Payment_UpdateRequest
		{
			ol_Username = config.ol_UserName,
			ol_Password = config.ol_Password,
			ol_EntityID = config.ol_EntityId,
			BusinessId = config.BusinessId,
			EntityId = model.EntityId,
			order_paymentId = model.Id,
			NamesArray = [.. updateFields.Select(f => f.FieldName)],
			ValuesArray = [.. updateFields.Select(f => f.Value)]
    };

		var response = await client.Sales_Orders_Payment_UpdateAsync(request);
		if (!string.IsNullOrEmpty(response.@return))
		{
			var result = JsonConvert.DeserializeObject<SalesOrdersPaymentUpdateResponse>(response.@return);
			if (result == null)
				throw new Exception("Failed to deserialize response");
			return result;
		}

		return new SalesOrdersPaymentUpdateResponse
		{
			ResultCode = -1,
			ResultMessage = "Can't Update Entity: Empty Response"
		};
	}

	public async Task<SalesOrdersPaymentsGetResponse> SalesOrdersPaymentGet(SalesOrderPaymentGet model)
	{
		var config = EnvironmentHelper.BusinessApiConfiguration;
		var client = config.InitClient();

		var request = new Sales_Orders_Payments_GetRequest
		{
			ol_Username = config.ol_UserName,
			ol_Password = config.ol_Password,
			ol_EntityID = config.ol_EntityId,
			BusinessId = config.BusinessId,
			Fields = model.Fields ?? Array.Empty<string>(),
			FilterFields = model.Filter?.Keys.ToArray() ?? Array.Empty<string>(),
			FilterValues = model.Filter?.Values.ToArray() ?? Array.Empty<string>(),
			LimitFrom = model.LimitFrom,
			LimitCount = model.LimitCount,
		};
		var response = await client.Sales_Orders_Payments_GetAsync(request);
		var result = JsonConvert.DeserializeObject<List<SalesOrderPayment>>(response.@return) ??
			throw new Exception("Failed to deserialize response");
		return new() { OrderPayments = result };
	}



	public async Task<EntityVerifyContactResponse> EntityVerify(EntityVerifyContact model)
	{
		var options = EnvironmentHelper.BusinessApiConfiguration;
		var client = options.InitClient();

		var request = new Entity_VerifyContactInfoRequest
		{
			ol_EntityID = options.ol_EntityId,
			ol_Username = options.ol_UserName,
			ol_Password = options.ol_Password,
			businessId = options.BusinessId,
			entityID = model.EntityId,
			VerifyType = (int)model.VerificationType,
			VerificationCode = model.VerificationCode,
		};

		var response = await client.Entity_VerifyContactInfoAsync(request);
		if (!string.IsNullOrEmpty(response.@return))
			return JsonConvert.DeserializeObject<EntityVerifyContactResponse>(response.@return) ??
			throw new Exception($"Failed to deserialize response: {response.@return}");

		return new EntityVerifyContactResponse() { ResultCode = -1, ResultMessage = "Can`t Verify Entity: Empty Response" };
	}
}