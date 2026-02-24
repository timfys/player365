using SmartWinners.Models.Payment;
using System;
using System.Collections.Generic;

namespace SmartWinners.Models.Interfaces;

public interface IPersonalInfoPage
{
    public bool? IsModel { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime? Birthday { get; set; }
    public string PhoneNumber { get; set; }
    public string Email { get; set; }
    public string Street { get; set; }
    public string City { get; set; }
    public string ZipCode { get; set; }
    public string State { get; set; }
    public string Country { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
    public bool MobileVerified { get; set; }
    public bool EmailVerified { get; set; }
    public List<PaymentCredentials> CreditCards { get; set; }
}
