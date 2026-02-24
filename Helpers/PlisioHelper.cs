using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using SmartWinners.Models;
using SmartWinners.PaymentSystem.StartAJob;

namespace SmartWinners.Helpers;

public class PlisioHelper
{
    public static string SecretKey = "osGsic5RHlmTV1904zdcZ-GO1bt2mB0SfHyhxM-mxvEs8NPiXtCc54jVf73UOiZg";

    public static PlisioInvoiceResponse CreateInvoice(PlisioInvoiceRequest request)
    {
        var context = EnvironmentHelper.HttpContextAccessor.HttpContext;

        var recordId = PaymentHelper.LogPayments(request.Type, "USD", request.UsdAmount.ToString("0.00"), "",
            "Plisio transaction", request.PayerEntityId);
        var trId = $"{recordId}_{request.PayerEntityId}_{Convert.ToInt32(request.Type)}";


        var callBackUrl = $"https://{context.Request.Host}/WebHook/Plisio?json=true";
        var q = HttpUtility.ParseQueryString(string.Empty);

        var trName = request.Type switch
        {
            PaymentWindowType.Deposit => "Deposit",
            PaymentWindowType.Lottery => "Lottery",
            PaymentWindowType.Syndicate => "Syndicate",
        };

        var redirectSuccessUrl = $"{request.RedirectSuccessUrl}/{trId}?tT=3";
        var redirectFailUrl = $"{request.RedirectSuccessUrl}/{trId}?tT=3";

        if (request.Type == PaymentWindowType.Deposit)
        {
            var currency = WebStorageUtility.GetUserCurrencyDetails();
            redirectSuccessUrl +=
                $"&d={CryptoUtility.EncryptString($"{request.DepositDisplayAmount:0.00}:;:{(request.IsUsdDeposit ? "USD" : currency.CurrencyIso)}:;:2:;:{(request.IsUsdDeposit ? "$" : currency.Symbol)}").Replace("+", " ")}";

        }
        
        q.Add("source_amount", $"{request.UsdAmount:0.00}");
        q.Add("source_currency", "USD");
        q.Add("order_number", trId);
        q.Add("order_name", $"{trName} {trId}");
        q.Add("callback_url", callBackUrl);
        q.Add("json", "true");
        q.Add("success_invoice_url", redirectSuccessUrl);
        q.Add("fail_invoice_url", redirectFailUrl);
        q.Add("email", $"{request.PayerEntityId}@smart-winners.com");
        q.Add("api_key", SecretKey);

        var url = new Uri($"https://api.plisio.net/api/v1/invoices/new?{q}");

        var client = new HttpClient();

        var resp = client.GetAsync(url).Result;

        var respString = resp.Content.ReadAsStringAsync().Result;

        return JsonConvert.DeserializeObject<PlisioInvoiceResponse>(respString);
    }

    public static async Task<string> PoolTransaction(string transactionId)
    {
        var client = new HttpClient();

        PlisioTransaction transaction = null;

        var ticks = 0;
        
        while (transaction is null
        || (!transaction.Data.Operations.Any(x => x.Status.Equals("completed", StringComparison.OrdinalIgnoreCase))
        && !transaction.Data.Operations.Any(x => x.Status.Equals("expired", StringComparison.OrdinalIgnoreCase))
        && !transaction.Data.Operations.Any(x => x.Status.Equals("error", StringComparison.OrdinalIgnoreCase))
        && !transaction.Data.Operations.Any(x => x.Status.Equals("mismatch", StringComparison.OrdinalIgnoreCase))
        && !transaction.Data.Operations.Any(x => x.Status.Equals("cancelled", StringComparison.OrdinalIgnoreCase))
        && !transaction.Data.Operations.Any(x => x.Status.Equals("expired", StringComparison.OrdinalIgnoreCase)
        && !transaction.Data.Operations.Any(x => x.Status.Equals("mismatch", StringComparison.OrdinalIgnoreCase))
        && !transaction.Data.Operations.Any(x => x.Status.Contains("pending", StringComparison.OrdinalIgnoreCase)))))
        {
            Thread.Sleep(5000);
            
            var apiResponse =
                await client.GetAsync(
                    $"https://api.plisio.net/api/v1/operations?api_key={SecretKey}&search={transactionId}");

            var response = await apiResponse.Content.ReadAsStringAsync();

            transaction = JsonConvert.DeserializeObject<PlisioTransaction>(response);

            if (ticks == 20)
                return "timeout";

            ticks++;
            
            if (transaction.Data.Operations.Count == 0)
                return "expired";
        }

        return transaction.Data.Operations.Any(x =>
            x.Status.Contains("pending", StringComparison.OrdinalIgnoreCase) ||
            x.Status.Contains("completed", StringComparison.OrdinalIgnoreCase) ||
            x.Status.Contains("mismatch", StringComparison.OrdinalIgnoreCase))
            ? "completed"
            : transaction.Data.Operations.FirstOrDefault()?.Status;
    }
}