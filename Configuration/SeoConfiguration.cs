namespace SmartWinners.Configuration;

public class SeoConfiguration : MyConfiguration
{
    public int CacheControlExpireTime { get; set; }
    public string[] CacheControlPath { get; set; }
    
    public int RamCacheTime { get; set; }
}