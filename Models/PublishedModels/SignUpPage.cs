using SmartWinners.Models.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Web.Common.PublishedModels;

public partial class SignUpPage :ISignUpPage
{
    //public bool? IsModel { get; set; }
    //public int ActivePageIndex { get; set; }
    public string PhoneCountry { get; set; }
    public string PhonePrefix { get; set; }

    [Required]
    public string Phone { get; set; }

    [Required]
    public string FirstName { get; set; }

    [Required]
    public string LastName { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    public string Country { get; set; }

    [Required]
    [MinLength(6)]
    public string Password { get; set; }

    [Required]
    [Compare(nameof(Password))]
    public string Password2 { get; set; }

    //public bool CanAcceptCode { get; set; }

    //public string VerificationCode { get; set; }

    //public int UserId { get; set; }
}
