
using SmartWinners.Models.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Web.Common.PublishedModels;

public partial class VerifyPhonePage : IVerifyPhonePage
{
    public int ActivePageIndex { get; set; }

    public string PhoneCountry { get; set; }
    public string PhonePrefix { get; set; }
    public string Phone { get; set; }

    public bool CanAcceptCode { get; set; }

    [MinLength(4), MaxLength(4)]
    public string VerificationCode { get; set; }

    public int? AffiliateEntityId { get; set; }
}