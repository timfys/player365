using SmartWinners.Helpers;
using Stripe;
using Stripe.Checkout;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Invoice = Stripe.Invoice;
using StripeConfiguration = Stripe.StripeConfiguration;

namespace SmartWinners.Services;

public class StripeService
{
	static StripeService()
	{
		StripeConfiguration.ApiKey = EnvironmentHelper.StripeConfiguration.SecretKey;
	}
	public async Task<Customer> CreateUserAsync(string name, string? email = null, Dictionary<string, string>? metadata = null)
	{
		var options = new CustomerCreateOptions
		{
			Email = email,
			Name = name,
			Metadata = metadata
		};

		var service = new CustomerService();
		var customer = await service.CreateAsync(options);
		return customer;
	}

	public async Task<Customer> RetrieveUserAsync(string customerId)
	{
		var service = new CustomerService();
		var customer = await service.GetAsync(customerId);
		return customer;
	}

	public async Task<Stripe.Product> GetProductAsync(string productId)
	{
		var service = new Stripe.ProductService();
		var product = await service.GetAsync(productId);
		return product;
	}

	public async Task<Stripe.Subscription> GetSubscriptionAsync(string subscriptionId)
	{
		var service = new SubscriptionService();
		return await service.GetAsync(subscriptionId);
	}

	public async Task<Customer> GetCustomerAsync(string customerId)
	{
		try
		{
			var service = new CustomerService();
			var customer = await service.GetAsync(customerId);
			return customer;
		}
		catch (Exception ex)
		{
			// Log the exception or handle it as needed
			Console.WriteLine($"Error retrieving customer: {ex.Message}");
			return null;
		}
	}

	public async Task<PaymentMethod> GetCustomerPaymentMethodAsync(string customerId, string paymentMethodId)
	{
		var service = new CustomerPaymentMethodService();
		return await service.GetAsync(customerId, paymentMethodId);
	}

	public async Task<PaymentMethod> GetCustomerPaymentMethodCardAsync(string customerId, string paymentMethodId)
	{
		var service = new CustomerPaymentMethodService();
		return await service.GetAsync(customerId, paymentMethodId);
	}

	public async Task<List<PaymentMethod>> RetrievePaymentCardsAsync(string customerId)
	{
		var options = new PaymentMethodListOptions
		{
			Customer = customerId,
			Type = "card",
		};

		var service = new PaymentMethodService();
		var paymentMethods = await service.ListAsync(options);
		return paymentMethods.Data;
	}

	public async Task<PaymentMethod> CreateCardPaymentMethodAsync(string cardNumber, DateTime date, string cvc)
	{
		var options = new PaymentMethodCreateOptions
		{
			Type = "card",
			Card = new PaymentMethodCardOptions
			{
				Number = cardNumber,
				ExpMonth = date.Month,
				ExpYear = date.Year,
				Cvc = cvc,
			},
		};

		var service = new PaymentMethodService();
		var paymentMethod = await service.CreateAsync(options);

		return paymentMethod;
	}

	public async Task<PaymentMethod> AttachPaymentMethodAsync(string customer, string paymentMethodId)
	{
		var options = new PaymentMethodCreateOptions
		{
			PaymentMethod = paymentMethodId,
			Customer = customer,
		};

		var service = new PaymentMethodService();
		var paymentMethod = await service.CreateAsync(options);

		return paymentMethod;
	}

	public async Task<PaymentIntent> GetPaymentIntent(string id)
	{
		var service = new PaymentIntentService();

		return await service.GetAsync(id);
	}

	public async Task<PaymentIntent> CreatePaymentIntentAsync(string currency, decimal amount, string customerId, string description, Dictionary<string, string> metadata, string redirectUrl)
	{
		try
		{
			var options = new PaymentIntentCreateOptions
			{
				Amount = Convert.ToInt64(amount * 100),
				Currency = currency,
				Customer = customerId,
				SetupFutureUsage = "off_session",
				PaymentMethodTypes = new List<string> { "card" },
				Description = description,
				PaymentMethodOptions = new()
				{
					Card = new()
					{
						RequestThreeDSecure = "automatic"
					},
				},
				Metadata = metadata
			};
			var service = new PaymentIntentService();
			return await service.CreateAsync(options);
		}
		catch (Exception e)
		{
			//ErrorsHandlerController.HandleError(e, "/Stripe/Checkout");
			return null;
		}
	}

	public async Task<PaymentIntent> CreateAutoConfirmPaymentIntent(string currency, decimal amount, string customerId, string paymentId, string description, Dictionary<string, string> metadata, string returnUrl)
	{
		var options = new PaymentIntentCreateOptions
		{
			Amount = Convert.ToInt64(amount * 100),
			Currency = currency,
			PaymentMethodTypes = new List<string>() { "card" },
			Customer = customerId,
			PaymentMethod = paymentId,
			Description = description,
			ConfirmationMethod = "automatic",
			Confirm = true,
			Metadata = metadata,
			ReturnUrl = returnUrl
		};
		var service = new PaymentIntentService();
		return await service.CreateAsync(options);
	}

	public async Task<Invoice> CreateInvoiceAsync(string currency, string customerId, string paymentId, string description, Dictionary<string, string> metadata)
	{
		var options = new InvoiceCreateOptions
		{
			Customer = customerId,
			DefaultPaymentMethod = paymentId,
			Description = description,
			Metadata = metadata,
			Currency = currency,
			AutoAdvance = true,
			CollectionMethod = "charge_automatically"
		};
		var service = new InvoiceService();
		return await service.CreateAsync(options);
	}

	public async Task<InvoiceItem> CreateInvoiceItemAsync(string currency, decimal amount, string customerId, string description, Dictionary<string, string>? metadata = null)
	{
		var options = new InvoiceItemCreateOptions
		{
			Customer = customerId,
			Description = description,
			Currency = currency,
			Quantity = 1,
			UnitAmount = Convert.ToInt64(amount * 100),
			Metadata = metadata
		};

		var service = new InvoiceItemService();
		return await service.CreateAsync(options);
	}

	public Task<PaymentIntent> UpdatePaymentIntentData(string currency, decimal amount, string paymentIntent)
	{
		var options = new PaymentIntentUpdateOptions
		{
			Amount = Convert.ToInt64(amount * 100),
			Currency = currency.ToUpper(),
		};

		var service = new PaymentIntentService();
		return service.UpdateAsync(paymentIntent, options);
	}

	public async Task<Session> CreateSessionAsync(string currency, decimal amount, int monthCount, string productInfo, string productDescription, int productId, int userId, string email)
	{
		try
		{
			var context = EnvironmentHelper.HttpContextAccessor.HttpContext;

			/*var recordId = PaymentHelper.LogPayments(PaymentWindowType.Profichat, amount.ToString("0.00"), "", "Stripe payment", userId);
		*/

			SessionCreateOptions options = null;
			if (monthCount == 0)
			{
				options = new SessionCreateOptions
				{
					PaymentMethodTypes = new List<string> { "card" /*, "klarna"*/ },
					UiMode = "embedded",
					LineItems = new List<SessionLineItemOptions>
				{
					new()
					{
						PriceData = new SessionLineItemPriceDataOptions
						{
							Currency = currency,
							UnitAmount = Convert.ToInt64(amount * 100),
							ProductData = new SessionLineItemPriceDataProductDataOptions
							{
								Name = productInfo,
								Description = productDescription,
							},
						},
						Quantity = 1,
					},
				},
					CustomerEmail = email,
					Mode = "payment",
					ReturnUrl = $"http://{context.Request.Host.Value}/payment/{productId}{(productId is (8 or 9 or 10) ? "" : $"/{monthCount}")}?session_id={{CHECKOUT_SESSION_ID}}",
					PaymentMethodOptions = new SessionPaymentMethodOptionsOptions
					{
						Card = new SessionPaymentMethodOptionsCardOptions
						{
							RequestThreeDSecure = "automatic"
						},
					},
					Metadata = new Dictionary<string, string>
				{
					{ "EntityId", $"{userId}" },
					{ "ProductId", $"{productId}" }
				}
				};
			}
			else
			{
				options = new SessionCreateOptions
				{
					PaymentMethodTypes = new List<string> { "card" },
					UiMode = "embedded",
					LineItems = new List<SessionLineItemOptions>
				{
					new()
					{
						PriceData = new SessionLineItemPriceDataOptions
						{
							Currency = currency,
							UnitAmount = Convert.ToInt64(amount * 100),
							Recurring = new SessionLineItemPriceDataRecurringOptions
							{
								Interval = monthCount == 12 ? "year" : "month",
								IntervalCount = monthCount == 12 ? monthCount / 12 : 1,
							},
							ProductData = new SessionLineItemPriceDataProductDataOptions
							{
								Name = productInfo,
								Description = productDescription,
							},
						},
						Quantity = 1,
					},
				},
					CustomerEmail = email,
					ReturnUrl =
						$"http://{context.Request.Host.Value}/payment/{productId}{(productId is (8 or 9 or 10) ? "" : $"/{monthCount}")}?session_id={{CHECKOUT_SESSION_ID}}",
					PaymentMethodOptions = new SessionPaymentMethodOptionsOptions
					{
						Card = new SessionPaymentMethodOptionsCardOptions
						{
							RequestThreeDSecure = "automatic"
						},
					},
					Metadata = new Dictionary<string, string>
				{
					{ "EntityId", $"{userId}" },
					{ "ProductId", $"{productId}" }
				},
					Mode = "subscription"
				};
			}



			var service = new SessionService();
			var session = await service.CreateAsync(options);

			return session;
		}
		catch (Exception e)
		{
			//ErrorsHandlerController.HandleError(e, "/Stripe/Checkout");
			return null;
		}
	}

	public static string PoolTransaction(string transactionId, out PaymentIntent session)
	{
		const int countCheck = 20;
		var i = 0;

		var service = new PaymentIntentService();

		while (i < countCheck)
		{
			session = service.Get(transactionId);

			var status = session.Status;

			if (status.Equals("complete") ||
				status.Equals("expired") ||
				status.Equals("open") ||
				status.Equals("succeeded"))
			{
				return status;
			}

			Thread.Sleep(2000);
			i++;
		}

		session = null;
		return "timeout";
	}

}
