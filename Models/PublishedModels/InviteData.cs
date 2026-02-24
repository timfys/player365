namespace SmartWinners.Models.PublishedModels;

public class InviteData
{
    public string CountryIso { get; set; }
		
    public string Phone { get; set; }
    
    public int IsKnowFriend { get; set; }
    
    public int MessageId { get; set; }
}

public class InviteResult
{
    public string ResultMessage { get; set; }
    public int ResultCode { get; set; }
    
    public string InviteUrl { get; set; }
    
    public string Message { get; set; }
}