using SmartWinners.Models.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Web.Common.PublishedModels;

public partial class VerifyEmailPage : IVerifyEmailPage
{
    public int ActivePageIndex { get; set; }
    public string Email { get; set; }
    public bool CanAcceptCode { get; set; }
    [MinLength(4), MaxLength(4)]
    public string VerificationCode { get; set; }
}
    