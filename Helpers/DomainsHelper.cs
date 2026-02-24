namespace SmartWinners.Helpers;

public class DomainsHelper
{
    public static string GetDomainName(DomainType domain)
    {
        return domain switch
        {
            DomainType.PlayerClub => "www.playerclub.app",
            DomainType.PlayerClub365 => "playerclub365.com",
            _ => "beta2.playerclub365.com"
        };
    }

    public static DomainType GetDomainNameType(string? domain = null)
    {
        return (domain?.ToLowerInvariant()) switch
        {
            "www.playerclub.app" => DomainType.PlayerClub,
            "beta2.playerclub365.com" => DomainType.PlayerClub365Test,
            "playerclub365.com" => DomainType.PlayerClub365,
            _ => DomainType.PlayerClub365Test,
        };
    }
}

public enum DomainType
{
    Player1,
    Player1Test,
    PlayerClub,
    PlayerClubTest,
    PlayerClub365Test = 11,
    PlayerClub365 = 12,
}