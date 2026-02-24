using BusinessApi;
using GoldCasino.ApiModule.Integrations.SmartWinners;
using GoldCasino.ApiModule.Models;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using SmartWinners.Controllers;
using SmartWinners.Models;
using SmartWinners.Models.Payment;
using SmartWinners.PaymentSystem.StartAJob;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Web;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace SmartWinners.Helpers;

public class PaymentHelper
{
	// public static bool ProcessPayment(PaymentWindowType type, PaymentModel model, decimal priceToPayUsd,
	// 		out PaymentInfo result,
	// 		CountryPaymentInfo countryPaymentInfo, bool? isUsd = null)
	// {
	// 	var priceToPay = priceToPayUsd;
	// 	var user = WebStorageUtility.GetSignedUser();
	// 	var currencyIso = /*isUsd.HasValue && isUsd.Value ? "USD" : */countryPaymentInfo.CurrencyIso;
	// 	try
	// 	{
	// 		var apiConfig = EnvironmentHelper.SmartWinnersApiConfiguration;

	// 		var paymentId = countryPaymentInfo.PaymentId;

	// 		var currency = CurrencyHelper.GetCurrency();
	// 		if ((isUsd is not true && type is not PaymentWindowType.Deposit) ||
	// 				(isUsd.HasValue && isUsd.Value && type is PaymentWindowType.Deposit))
	// 		{
	// 			priceToPay *= currency.ExchangeRate;
	// 		}

	// 		var paymentType = GetPaymentType(PaymentType.CreditCardPhone, countryPaymentInfo.PaymentId);

	// 		if (!paymentType.SupportedCurrencies.Contains(currencyIso))
	// 		{
	// 			currencyIso = "USD";

	// 			priceToPay /= currency.ExchangeRate;
	// 		}

	// 		var request = new Sales_Orders_Payment_UpdateRequest
	// 		{
	// 			ol_Username = apiConfig.ol_UserName,
	// 			ol_Password = apiConfig.ol_Password,
	// 			ol_EntityID = apiConfig.ol_EntityId,
	// 			EntityId = user.EntityId,
	// 			BusinessId = 1,
	// 			NamesArray = new[]
	// 				{
	// 									"PayerName",
	// 									"PayerNumber",
	// 									"PayerNumber3",
	// 									"PayerDate",
	// 									"PaymentId",
	// 									"CurrencyISO",
	// 									"PaymentValue",
	// 									"Employee_entityId",
	// 									"OrderId",
	// 									"Order_CurrencyISO"
	// 							},
	// 			ValuesArray = new[]
	// 				{
	// 									model.Credentials.CardHolder,
	// 									model.Credentials.CardNumber,
	// 									model.Credentials.Cvv,
	// 									model.Credentials.ValidDate.ToString("yyyy-MM-dd"),
	// 									$"{paymentId}",
	// 									currencyIso,
	// 									(Math.Round(priceToPay, 2)).ToString("0.00", new CultureInfo(1033)),
	// 									"4",
	// 									"-2",
	// 									"USD"
	// 							},
	// 			ChargePayment = true,
	// 			order_paymentId = -1
	// 		};


	// 		var client = EnvironmentHelper.BusinessApiConfiguration.InitClient();
	// 		var response = client.Sales_Orders_Payment_Update(request);

	// 		result =
	// 				JsonConvert.DeserializeObject<PaymentInfo>(response.@return);

	// 		if (string.IsNullOrEmpty(user.FirstName) && string.IsNullOrEmpty(user.LastName))
	// 		{
	// 			var name = model.Credentials.CardHolder.Split(" ");

	// 			var resp = UpdateUserName(name[0], name[1], user.EntityId);
	// 			user.FirstName = model.Credentials.CardHolder;
	// 			WebStorageUtility.SignIn(EnvironmentHelper.HttpContextAccessor.HttpContext, user);
	// 		}

	// 		if (result.IsSuccess())
	// 		{
	// 			return true;
	// 		}

	// 		if (!WebStorageUtility.TryGetString(WebStorageUtility.FailedChargeSentByEmail, out var value))
	// 		{
	// 			var messageId = 110;

	// 			if (result.ResultMessage.Contains("double transaction"))
	// 				messageId = 117;

	// 			SendFailedChargeInfo(user.EntityId, messageId);
	// 			WebStorageUtility.SetString(WebStorageUtility.FailedChargeSentByEmail, "1",
	// 					WebStorageUtility.GetUserDateTime() + TimeSpan.FromDays(1));
	// 		}

	// 		LogPayments(type, $"{Math.Round(priceToPay, 2):0.00}", currencyIso, result.ResultMessage,
	// 				$"Terminal Id: {paymentId} \n Payment data: {JsonConvert.SerializeObject(model.Credentials)}",
	// 				user.EntityId);
	// 		if (paymentId != 12)
	// 		{
	// 			if (!currencyIso.Equals("USD", StringComparison.OrdinalIgnoreCase))
	// 			{
	// 				currency = CurrencyHelper.GetCurrency();

	// 				priceToPay /= currency.ExchangeRate;
	// 			}

	// 			countryPaymentInfo.CurrencyIso = "USD";
	// 			countryPaymentInfo.PaymentId = 12;
	// 			ProcessPayment(type, model, priceToPay, out result, countryPaymentInfo, true);
	// 			if (result.IsSuccess())
	// 			{
	// 				return true;
	// 			}
	// 		}

	// 		return false;
	// 	}
	// 	catch (Exception e)
	// 	{
	// 		LogPayments(type, $"{Math.Round(priceToPay, 2):0.00}", currencyIso, $"{e.Message} {e.StackTrace}",
	// 				$"Terminal Id: {countryPaymentInfo.PaymentId} \n Payment data: {JsonConvert.SerializeObject(model.Credentials)}",
	// 				user.EntityId);

	// 		result = default;
	// 		return false;
	// 	}
	// }

	// public static int GetUserLastPaymentId(int? userId = null, Dictionary<string, string>? filter = null)
	// {
	// 	var entityId = userId.HasValue ? userId : WebStorageUtility.GetSignedUser()?.EntityId;

	// 	if (entityId < 0)
	// 		return 0;

	// 	var client = EnvironmentHelper.BusinessApiConfiguration.InitClient();

	// 	var apiConfig = EnvironmentHelper.BusinessApiConfiguration;

	// 	if (filter is null)
	// 	{
	// 		filter = new Dictionary<string, string>()
	// 					{
	// 							{"o.entityId", $"{entityId}"},
	// 					};
	// 	}
	// 	else
	// 	{
	// 		filter.Add("o.entityId", $"{entityId}");
	// 	}


	// 	var apiRequest = new Sales_Orders_Payments_GetRequest()
	// 	{
	// 		ol_EntityID = apiConfig.ol_EntityId,
	// 		ol_Username = apiConfig.ol_UserName,
	// 		ol_Password = apiConfig.ol_Password,
	// 		BusinessId = apiConfig.BusinessId,
	// 		FilterFields = [.. filter.Keys],
	// 		FilterValues = [.. filter.Values]
	// 	};

	// 	var apiResponse = client.Sales_Orders_Payments_Get(apiRequest);

	// 	var payments = JsonConvert.DeserializeObject<List<PaymentInfo>>(apiResponse.@return);

	// 	return payments?.Count > 0 ? payments.OrderByDescending(x => x.Order_PaymentId).First().Order_PaymentId : 0;
	// }

	public static bool ProcessPlisioWebhookPayment(int entityId, PlisioWebhookRequest model,
			PaymentWindowType type, out PaymentInfo result, int recordId)
	{
		try
		{
			var apiConfig = EnvironmentHelper.SmartWinnersApiConfiguration;


			var request = new Sales_Orders_Payment_UpdateRequest
			{
				ol_Username = apiConfig.ol_UserName,
				ol_Password = apiConfig.ol_Password,
				ol_EntityID = apiConfig.ol_EntityId,
				EntityId = entityId,
				BusinessId = 1,
				order_paymentId = 0,
				NamesArray = new[]
					{
										"PaymentID", "OrderId", "PaymentValue", "Employee_entityId", "currencyIso", "PayerName",
										"PayerNumber", "PayerNumber3", "PayerDate", "transactionID", "ChargedRemark", "status", "ChargedDate"
								},
				ValuesArray = new[]
					{
										"5", "-2", $"{Math.Round(model.source_amount, 2):0.00}", "4", "USD", "", "", "",
										"2055-10-10", model.txn_id, $"Plisio - {model.currency} {model.amount}", "1", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
								},
			};

			var client = EnvironmentHelper.BusinessApiConfiguration.InitClient();
			var response = client.Sales_Orders_Payment_Update(request);

			result =
					JsonConvert.DeserializeObject<PaymentInfo>(response.@return);

			if (result.IsSuccess())
			{
				return true;
			}

			LogPayments(type, $"{Math.Round(model.amount, 2):0.00}", result.ResultMessage,
					$"Terminal Id: 5 \n Payment data: {JsonConvert.SerializeObject(model)}", "USD", entityId, recordId);
			return false;
		}
		catch (Exception e)
		{
			LogPayments(type, $"{Math.Round(model.amount, 2):0.00}",
					$"{e.Message} {e.StackTrace}",
					$"Terminal Id: 5 \n Payment data: {JsonConvert.SerializeObject(model)}", "USD", entityId, recordId);
			result = default;
			return false;
		}
	}


	public static int LogPayments(PaymentWindowType paymentType, string paymentSum, string currency,
			string paymentError, string paymentDetails, int entityId, int? recordId = null)
	{
		var config = EnvironmentHelper.BusinessApiConfiguration;

		var client = EnvironmentHelper.BusinessApiConfiguration.InitClient();

		var paymentTypeStr = paymentType switch
		{
			PaymentWindowType.Deposit => "6",
			PaymentWindowType.Lottery => "4",
			PaymentWindowType.Syndicate or PaymentWindowType.SyndicatePromotion => "5",
			PaymentWindowType.CardVerification => "9"
		};

		var fieldsDict = new Dictionary<string, string>
				{
						{ "CustomField141", paymentError },
						{ "CustomField140", paymentTypeStr },
						{ "CustomField139", paymentDetails },
						{ "CustomField138", paymentSum },
						{ "CustomField137", DateTime.Now.ToString("yyyy-MM-dd h:mm:ss") },
						{ "ParentRecordID", $"{entityId}" }
				};

		if (!string.IsNullOrEmpty(currency))
		{
			fieldsDict.Add("CustomField190", currency);
		}

		var apiRequest = new CustomFields_Tables_UpdateRequest
		{
			ol_EntityID = config.ol_EntityId,
			ol_Username = config.ol_UserName,
			ol_Password = config.ol_Password,
			TableID = 136,
			NamesArray = [.. fieldsDict.Keys],
			ValuesArray = [.. fieldsDict.Values]
		};
		if (recordId is > 0)
		{
			apiRequest.RecordID = recordId.Value;
		}

		var resp = client.CustomFields_Tables_Update(apiRequest);
		return JsonConvert.DeserializeObject<CustomTableEntry>(resp.@return).RecordId;
	}

	public static void UpdateSuccessfulPaymentLog(int recordId)
	{
		var config = EnvironmentHelper.BusinessApiConfiguration;

		var client = EnvironmentHelper.BusinessApiConfiguration.InitClient();

		var apiRequest = new CustomFields_Tables_UpdateRequest
		{
			ol_EntityID = config.ol_EntityId,
			ol_Username = config.ol_UserName,
			ol_Password = config.ol_Password,
			TableID = 136,
			RecordID = recordId,
			NamesArray = new[]
				{
								"CustomField141", "IsDeleted"
						},
			ValuesArray = new[]
				{
								"OK", "1"
						}
		};

		client.CustomFields_Tables_Update(apiRequest);
	}


	public class CustomTableEntry
	{
		public int RecordId { get; set; }
		public string CustomField139 { get; set; }
	}

	public static void CacheUserBalance(HttpContext context, UserBalanceInfo balanceInfo)
	{
 		WebStorageUtility.SetCookie(context, WebStorageUtility.UserBalance, 
			CryptoUtility.EncryptObject(balanceInfo), WebStorageUtility.GetUserDateTime() + TimeSpan.FromMinutes(30));
	}

	public static void CacheUserBonusBalance(HttpContext context, decimal bonusBalance)
	{
		WebStorageUtility.SetCookie(context, WebStorageUtility.UserBonusBalance,
			CryptoUtility.EncryptObject(new UserBonusBalanceInfo { BonusBalance = bonusBalance }), 
			WebStorageUtility.GetUserDateTime() + TimeSpan.FromMinutes(30));
	}

	public static async Task<UserBalanceInfo> GetUserBalance(UserApiAccess user, bool? cache = true)
	{
		using var client = EnvironmentHelper.BusinessApiConfiguration.InitClient();
		if (user is null)
			throw new ArgumentNullException(nameof(user), "User cannot be null");

		var apiRequest = new Entity_FindRequest
		{
			ol_EntityId = user.EntityId,
			ol_UserName = user.Username,
			ol_Password = user.Password,
			BusinessId = 1,
			Fields = ["CustomField54", "CustomField82"],
			FilterFields = ["entityId"],
			FilterValues = [$"{user.EntityId}"]
		};

		var apiResponse = await client.Entity_FindAsync(apiRequest);

		var recalculationResponse = JsonSerializer.Deserialize<List<UserBalanceInfo>>(apiResponse.@return)?.FirstOrDefault();
		//var currency = WebStorageUtility.GetUserCurrencyDetails(httpContext);

		if (cache is true)
		{
			CacheUserBalance(EnvironmentHelper.HttpContextAccessor.HttpContext, recalculationResponse);
		}

		return recalculationResponse;
	}

	public static async Task<PaymentController.BalanceRecalculationResponse> RecalculateUserBalance(User userInfo = null,
			bool? rewriteExistingUser = true, HttpContext httpContext = null)
	{
		return null;
		// using var smartWinnersClient = EnvironmentHelper.SmartWinnersApiConfiguration.InitClient();
		// httpContext ??= EnvironmentHelper.HttpContextAccessor.HttpContext;

		// userInfo = userInfo is null
		// 		? WebStorageUtility.GetSignedUser(httpContext)
		// 		: userInfo;

		// var resp = await smartWinnersClient.Entity_Balance_CalcAsync(new Entity_Balance_CalcRequest
		// {
		// 	ol_EntityID = userInfo.EntityId,
		// 	ol_Username = userInfo.UserName,
		// 	ol_Password = userInfo.Password
		// });

		// var recalculationResponse =
		// 		JsonConvert.DeserializeObject<PaymentController.BalanceRecalculationResponse>(resp.@return);

		// var currency = WebStorageUtility.GetUserCurrencyDetails(httpContext);

		// userInfo.BalanceUSD = recalculationResponse.AccountBalanceUsd;
		// userInfo.BalanceLocal = currency.Symbol.Equals("$")
		// 		? recalculationResponse.AccountBalanceUsd
		// 		: recalculationResponse.AccountBalanceLocal;

		// var field = WebStorageUtility.GetEntityField(userInfo.EntityId, "customfield178");
		// userInfo.SetVirtualBalance(decimal.Parse((string)field));

		// if (rewriteExistingUser is true)
		// {
		// 	WebStorageUtility.SetString(WebStorageUtility.UserValueName, CryptoUtility.EncryptObject(userInfo),
		// 			WebStorageUtility.GetUserDateTime() + TimeSpan.FromMinutes(30));

		// 	httpContext.Items[WebStorageUtility.UserValueName] = userInfo;
		// }

		// return recalculationResponse;
	}

	public static async Task<List<PaymentInfo>?> GetUserPaymentCards(int entityId)
	{
		var config = EnvironmentHelper.BusinessApiConfiguration;
		var request = new Sales_Orders_Payments_GetRequest
		{
			ol_EntityID = config.ol_EntityId,
			BusinessId = config.BusinessId,
			ol_Password = config.ol_Password,
			ol_Username = config.ol_UserName,
			Fields = ["PayerNumber", "PayerDate", "PayerName", "op.status"],
			FilterFields = ["o.entityId", "PayerDate"],
			FilterValues = [entityId.ToString(), "> CURDATE()"]
		};

		var client = EnvironmentHelper.BusinessApiConfiguration.InitClient();

		var response = await client.Sales_Orders_Payments_GetAsync(request);

		var usedCards = JsonConvert.DeserializeObject<List<PaymentInfo>>(response.@return);

		usedCards?.RemoveAll(x =>
				!x.PayerDate.HasValue || string.IsNullOrEmpty(x.PayerNumber) || x.PayerDate < DateTime.UtcNow);

		for (var i = 0; i < usedCards?.Count; i++)
		{
			usedCards[i].SetCardParams();

			for (var j = 0; j < usedCards.Count; j++)
			{
				if (i == j)
					continue;

				if (usedCards[i].PayerNumber.Equals(usedCards[j].PayerNumber))
				{
					usedCards.RemoveAt(j);
					j--;
				}
			}
		}

		return usedCards;
	}

	public static bool CreateWithdrawMethod(WithdrawApiModel withdrawApiModel,
			out IdentityHelper.GeneralApiResponse response)
	{
		var user = WebStorageUtility.GetSignedUser();

		response = new IdentityHelper.GeneralApiResponse
		{
			ResultMessage = "User is not logged in",
			ResultCode = -1
		};

		if (user is null)
			return false;

		var namesList = new List<string>();
		var valuesList = new List<string>();

		var type = typeof(WithdrawApiModel);

		foreach (var property in type.GetProperties())
		{
			var value = property.GetValue(withdrawApiModel);
			var name = property.Name;

			if (value is null
					|| property.PropertyType != typeof(string)
					|| property.Name.Equals("CurrencyIso")
					|| property.Name.Equals("PaymentId"))
				continue;

			namesList.Add(name);
			valuesList.Add((string)value);
		}

		namesList.Add("PaymentID");
		valuesList.Add(withdrawApiModel.PaymentId);
		namesList.Add("CurrencyIso");
		valuesList.Add(withdrawApiModel.CurrencyIso);
		namesList.Add("BusinessId");
		valuesList.Add("1");
		namesList.Add("PayerName");
		valuesList.Add($"{user.LastName} {user.FirstName}");
		namesList.Add("Status");
		valuesList.Add("1");


		var apiRequest = new Purchase_Payment_UpdateRequest
		{
			ol_EntityID = user.EntityId,
			ol_Username = user.UserName,
			ol_Password = user.Password,
			ValuesArray = [.. valuesList],
			NamesArray = [.. namesList],
		};

		var client = EnvironmentHelper.BusinessApiConfiguration.InitClient();

		var apiResponse = client.Purchase_Payment_Update(apiRequest);

		response = JsonConvert.DeserializeObject<IdentityHelper.GeneralApiResponse>(apiResponse.@return);

		return response is not null && response.IsSuccess();
	}

	public static bool Withdraw(WithdrawModel model, out IdentityHelper.GeneralApiResponse response)
	{
		var user = WebStorageUtility.GetSignedUser();

		response = new IdentityHelper.GeneralApiResponse
		{
			ResultMessage = "User is not logged in",
			ResultCode = -1
		};

		if (user is null)
			return false;

		var apiRequest = new Entity_WithdrawRequest
		{
			ol_EntityID = user.EntityId,
			ol_Username = user.UserName,
			ol_Password = user.Password,
			BusinessId = 1,
			Amount = model.Amount,
			purchase_paymentId = model.PurchasePaymentId,
			ChargedRemark = model.ChargedRemark
		};

		var client = EnvironmentHelper.SmartWinnersApiConfiguration.InitClient();

		var apiResponse = client.Entity_WithdrawAsync(apiRequest).Result;

		response = JsonConvert.DeserializeObject<IdentityHelper.GeneralApiResponse>(apiResponse.@return);

		return response is not null && response.IsSuccess();
	}

	public static List<WithdrawApiModel> GetUserWithdraws(bool isTransactions, int? page = null)
	{
		var user = WebStorageUtility.GetSignedUser();

		var config = EnvironmentHelper.BusinessApiConfiguration;

		if (user is null)
			return null;

		var client = EnvironmentHelper.BusinessApiConfiguration.InitClient();

		var apiRequest = new Purchase_Payments_GetRequest
		{
			ol_EntityID = config.ol_EntityId,
			ol_Username = config.ol_UserName,
			ol_Password = config.ol_Password,
			BusinessId = config.BusinessId,
			FilterFields = new[] { "pp.EntityId", "pp.isDeleted", "pp.purchaseId", /*"p.Deleted",*/ "Order By", },
			FilterValues = new[] { $"{user.EntityId}", "0", isTransactions ? ">0" : "0",/* "0",*/ "PaymentDate desc" },
			Fields = new[]
				{
								"pp.PayerNumber",
								"pp.PayerNumber2",
								"pp.PayerNumber37799",
								"pp.PayerNumber4",
								"pp.PayerNumber5",
								"pp.PayerNumber6",
								"pp.PayerNumber7",
								"pp.CurrencyIso",
								"pp.Parm1",
								"pp.status",
								"pp.PaymentDate",
								"pp.PaymentValue",
								"pp.PaymentId"
						}
		};

		if (page.HasValue && isTransactions)
		{
			apiRequest.LimitCount = 11;
			apiRequest.LimitFrom = page.Value * 10 + 1;
		}

		var apiResponse = client.Purchase_Payments_Get(apiRequest);

		try
		{
			return JsonConvert.DeserializeObject<List<WithdrawApiModel>>(apiResponse.@return);
		}
		catch
		{
			return null;
		}
	}

	public static WithdrawApiModel GetUserWithdrawMethod(int id)
	{
		var user = WebStorageUtility.GetSignedUser();

		var config = EnvironmentHelper.BusinessApiConfiguration;

		if (user is null)
			return null;

		var client = EnvironmentHelper.BusinessApiConfiguration.InitClient();

		var apiRequest = new Purchase_Payments_GetRequest
		{
			ol_EntityID = config.ol_EntityId,
			ol_Username = config.ol_UserName,
			ol_Password = config.ol_Password,
			BusinessId = config.BusinessId,
			FilterFields = new[] { "pp.EntityId", "pp.isDeleted", "pp.purchase_paymentId" },
			FilterValues = new[] { $"{user.EntityId}", "0", $"{id}" },
			Fields = new[]
				{
								"pp.PayerNumber",
								"pp.PayerNumber2",
								"pp.PayerNumber37799",
								"pp.PayerNumber4",
								"pp.PayerNumber5",
								"pp.PayerNumber6",
								"pp.PayerNumber7",
								"pp.CurrencyIso",
								"pp.Parm1",
								"pp.status",
								"pp.PaymentId"
						}
		};

		var apiResponse = client.Purchase_Payments_Get(apiRequest);

		try
		{
			return JsonConvert.DeserializeObject<List<WithdrawApiModel>>(apiResponse.@return).FirstOrDefault();
		}
		catch
		{
			return null;
		}
	}

	public class GeneralTransaction : IdentityHelper.GeneralApiResponse
	{
		internal static List<string> GetJsonPropertiesName()
		{
			var propertiesJsonValue = new List<string>();

			var type = typeof(GeneralTransaction);

			foreach (var propertyInfo in type.GetProperties())
			{
				if (propertyInfo.Name is "UTCTransactionCreatedDate" or "TransactionId" or "UserLocalTransactionDate"
						or "TransactionType" or "UserLocalTransactionCreateDate" or "DrawLinesCount" or "LotteryLines"
						or "ResultCode" or "ResultMessage")
					continue;

				var jsonAttribute =
						propertyInfo.CustomAttributes.FirstOrDefault(x => x.AttributeType == typeof(JsonPropertyAttribute));

				if (jsonAttribute is not null)
				{
					var propertyValue = jsonAttribute.ConstructorArguments.First().Value as string;

					switch (propertyValue)
					{
						case "DrawNo" or "ScanID" or "resultsId" or "EntityExtraNumbers" or "EntityLotteryNumbers"
								or "order_productID":
							{
								propertyValue = $"ee.{propertyValue}";
								break;
							}
						case "LotteryName" or "CountryName":
							{
								propertyValue = $"l.{propertyValue}";
								break;
							}
					}

					propertiesJsonValue.Add(propertyValue);
				}
				else
				{
					var propertyValue = propertyInfo.Name;

					switch (propertyValue)
					{
						case "DrawNo" or "ScanId" or "resultsId" or "EntityExtraNumbers" or "EntityLotteryNumbers"
								or "order_productID":
							{
								propertyValue = $"ee.{propertyValue}";
								break;
							}
						case "LotteryName" or "CountryName":
							{
								propertyValue = $"l.{propertyValue}";
								break;
							}
					}

					propertiesJsonValue.Add(propertyValue);
				}
			}

			return propertiesJsonValue;
		}
	}

	public static bool DeleteWithdrawMethod(int withdrawId, out IdentityHelper.GeneralApiResponse response)
	{
		var user = WebStorageUtility.GetSignedUser();

		response = new IdentityHelper.GeneralApiResponse
		{
			ResultMessage = "User is not logged in",
			ResultCode = -1
		};

		if (user is null)
			return false;

		var client = EnvironmentHelper.BusinessApiConfiguration.InitClient();

		var apiRequest = new Purchase_Payment_UpdateRequest()
		{
			ol_EntityID = user.EntityId,
			ol_Username = user.UserName,
			ol_Password = user.Password,
			purchase_paymentId = withdrawId,
			NamesArray = new[] { "isDeleted" },
			ValuesArray = new[] { "1" }
		};

		var apiResponse = client.Purchase_Payment_Update(apiRequest);

		response = JsonConvert.DeserializeObject<IdentityHelper.GeneralApiResponse>(apiResponse.@return);

		return response.IsSuccess();
	}

	public static PaymentTypeInfo GetPaymentType(PaymentType type, int paymentId)
	{
		var config = EnvironmentHelper.BusinessApiConfiguration;

		var client = EnvironmentHelper.BusinessApiConfiguration.InitClient();

		var apiRequest = new Sales_PaymentsMethod_GetRequest
		{
			ol_EntityId = config.ol_EntityId,
			ol_UserName = config.ol_UserName,
			ol_Password = config.ol_Password,
			BusinessId = config.BusinessId,
			Fields = new[] { "currencies_supportedISO", "PaymentType", "currencyISO" },
			FilterFields = new[] { "PaymentType", "paymentID" },
			FilterValues = new[] { $"{(int)type}", $"{paymentId}" }
		};

		var apiResponse = client.Sales_PaymentsMethod_Get(apiRequest);

		return JsonConvert.DeserializeObject<List<PaymentTypeInfo>>(apiResponse.@return).FirstOrDefault();
	}

	public static void UpdateWebhookPaymentOnChargeConfirmed(int paymentId, string chargeCode, int userId, string status,
			DateTimeOffset chargeCreatedAt)
	{
		var config = EnvironmentHelper.BusinessApiConfiguration;

		var client = EnvironmentHelper.BusinessApiConfiguration.InitClient();

		var apiRequest = new Sales_Orders_Payment_UpdateRequest
		{
			ol_EntityID = config.ol_EntityId,
			ol_Username = config.ol_UserName,
			ol_Password = config.ol_Password,
			EntityId = userId,
			BusinessId = config.BusinessId,
			order_paymentId = paymentId,
			NamesArray = new[] { "ChargedRemark", "status", "ChargedDate" },
			ValuesArray = new[] { chargeCode, status, chargeCreatedAt.ToString("yyyy-MM-dd HH:mm:ss") }
		};

		client.Sales_Orders_Payment_Update(apiRequest);
	}

	public static CountryPaymentInfo GetCountryPaymentInfo(string countryIso)
	{
		var config = EnvironmentHelper.BusinessApiConfiguration;

		var client = EnvironmentHelper.BusinessApiConfiguration.InitClient();

		var apiRequest = new General_DataList_GetRequest
		{
			TableName = "countries",
			FilterFields = new[] { "ISO3166" },
			FilterValues = new[] { countryIso }
		};

		var apiResponse = client.General_DataList_Get(apiRequest);

		return JsonConvert.DeserializeObject<List<CountryPaymentInfo>>(apiResponse.@return).First();
	}

	public static bool CheckIfTransactionExists(string transactionId)
	{
		var config = EnvironmentHelper.BusinessApiConfiguration;

		var client = EnvironmentHelper.BusinessApiConfiguration.InitClient();

		var apiRequest = new Sales_Orders_Payments_GetRequest
		{
			ol_EntityID = config.ol_EntityId,
			ol_Username = config.ol_UserName,
			ol_Password = config.ol_Password,
			BusinessId = config.BusinessId,
			FilterFields = new[] { "transactionID" },
			FilterValues = new[] { transactionId }
		};

		var apiResponse = client.Sales_Orders_Payments_Get(apiRequest);

		return !apiResponse.@return.Equals("[]");
	}

	public static void SendFailedChargeInfo(int entityId, int messageId)
	{
		var config = EnvironmentHelper.BusinessApiConfiguration;

		var client = EnvironmentHelper.BusinessApiConfiguration.InitClient();

		var apiRequest = new Outgoing_addRequest()
		{
			ol_EntityID = config.ol_EntityId,
			ol_Username = config.ol_UserName,
			ol_Password = config.ol_Password,
			EntityIds = new[] { entityId },
			MessageType = 6,
			MessageID = messageId
		};

		var apiResponse = client.Outgoing_add(apiRequest);
	}

}


public class CountryPaymentInfo
{
	[JsonProperty("CustomField_Creditcard")]
	public int PaymentId { get; set; }

	[JsonProperty("CurrencyCode")] public string CurrencyIso { get; set; }
}

public class UserBalanceInfo
{
	[JsonPropertyName("CustomField54")] public decimal BalanceUSD { get; set; }
	[JsonPropertyName("CustomField82")] public decimal BalanceLocal { get; set; }
}

public class UserBonusBalanceInfo
{
	public decimal BonusBalance { get; set; }
}

public class PaymentTypeInfo
{

	[JsonProperty("currencies_supportedISO")]
	public string _supportedCurrencies { get; set; }

	public List<string> SupportedCurrencies
	{
		get { return [.. _supportedCurrencies.Split(",")]; }
	}
}

public enum PaymentType
{
	Cash = 0,
	CreditCardPhone = 1,
	Cheque = 2,
	CreditCard = 3,
	BankClearingSystem = 4,
	EWallet = 5,
	WireTransfer = 6
}

public class StripeUsedCards
{

	[JsonProperty("payerDate")]
	public DateTime PayerDate { get; set; }

	[JsonProperty("payerNumber")]
	public string PayerNumber { get; set; }
}