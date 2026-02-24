using GoldCasino.ApiModule.Models;

namespace GoldCasino.ApiModule.Configuration;

public class BusinessApiOptions : SoapOptionsBase
{
    public UserApiAccess Credentials { get; set; }
}
