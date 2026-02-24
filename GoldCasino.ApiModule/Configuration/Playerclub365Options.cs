using GoldCasino.ApiModule.Models;

namespace GoldCasino.ApiModule.Configuration;

public class Playerclub365Options : SoapOptionsBase
{
    public UserApiAccess Credentials { get; set; }
}

