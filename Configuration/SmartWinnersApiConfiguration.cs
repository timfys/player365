using GoldCasino.ApiModule.Integrations.SmartWinners;
using System;
using System.ServiceModel;

namespace SmartWinners.Configuration;

public class SmartWinnersApiConfiguration : MyConfiguration
{
    public int ol_EntityId { get; set; }
    public string ol_UserName { get; set; }
    public string ol_Password { get; set; }
    public string EndpointAddress { get; set; }
    public int Timeout { get; set; }
    public int MaxBufferSize { get; set; }
    public string Password { get; set; } = string.Empty;

	public SmartWinnersClient InitClient()
    {
        var timeSpan = TimeSpan.FromSeconds(Timeout);
        var binding = new BasicHttpBinding
        {
            ReceiveTimeout = timeSpan,
            CloseTimeout = timeSpan,
            OpenTimeout = timeSpan,
            SendTimeout = timeSpan,
            MaxBufferSize = MaxBufferSize * 1024,
            MaxReceivedMessageSize = MaxBufferSize * 1024
        };
        var endpoint = new EndpointAddress(EndpointAddress);
        return new SmartWinnersClient(binding, endpoint);
    }

}