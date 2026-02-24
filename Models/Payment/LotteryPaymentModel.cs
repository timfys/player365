#nullable enable
using System;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using SmartWinners.Helpers;

namespace SmartWinners.Models.Payment;

public class LotteryPaymentModel : PaymentModel
{
    public int LotteryId { get; set; }

    public string LotteryName { get; set; }

    public string? DrawTime { get; set; }

    public LotteryLine[] Lines
    {
        get
        {
            WebStorageUtility.TryGetString($"Lines{LotteryId}", out var linesStr);
            var linesBytes = Convert.FromBase64String(linesStr);
            return JsonConvert.DeserializeObject<LotteryLine[]>(CompressUtility.DecompressString(linesBytes));
        }
    }

    public int DrawsPerWeek =>
        int.TryParse(EnvironmentHelper.HttpContextAccessor.HttpContext.Request.Headers["DrawsPerWeek"], out var weeks)
            ? weeks
            : 0;

    public WithdrawType Withdraw { get; set; }
}

public class LotteryLine
{
    public string[] Common { get; set; }

    public string[] Stars { get; set; }
    
}

public enum WithdrawType
{
    OneDraw = 0,
    OneWeek = 1,
    TwoWeeks = 2,
    FourWeeks = 4,
    MonthlySub = 12
}

public class PaymentInfo : IdentityHelper.GeneralApiResponse
{
    public int Order_PaymentId { get; set; }

    public int OrderId { get; set; }
    public string PayerNumber { get; set; }

    public DateTime? PayerDate { get; set; }

    public string CardImgUrl { get; set; }
    public string CardPaymentSystemName { get; set; }

    public int PayerNumber3 { get; set; }
    
    [JsonProperty("PayerNumber6")]
    public string StripePaymentMethodId { get; set; }

    public string PayerName { get; set; }

    public string Status { get; set; }
    
    public decimal PaymentValue { get; set; }

    public string CardLastDigits
    {
        get
        {
            var cL = PayerNumber.Length;

            return $"{PayerNumber[cL - 4]}{PayerNumber[cL - 3]}{PayerNumber[cL - 2]}{PayerNumber[cL - 1]}";
        }
    }

    [JsonProperty("PayerNumber5")]
    public string StripeUserId { get; set; }


    public void SetCardParams()
    {
        switch (PayerNumber)
        {
            case var tempStr when Regex.IsMatch(tempStr, "^4[0-9]{12}(?:[0-9]{3})?$"):
            {
                CardImgUrl = @"/images/homepage/Visa-dark.svg";
                CardPaymentSystemName = "Visa";
                break;
            }
            case var tempStr when Regex.IsMatch(tempStr, "^5[1-5][0-9]{14}$"):
            {
                CardImgUrl = @"/images/homepage/MasterCard-dark.svg";
                CardPaymentSystemName = "MasterCard";
                break;
            }
            case var tempStr when Regex.IsMatch(tempStr, "^(5018|5020|5038|5893|6304|6759|6761|6762|6763)[0-9]{8,15}$"):
            {
                CardImgUrl = @"/images/contacts/Maestro-dark.svg";
                CardPaymentSystemName = "Maestro";
                break;
            }
            default:
            {
                CardImgUrl = @"/images/image5.svg";
                CardPaymentSystemName = "";
                break;
            }
        }
    }
}