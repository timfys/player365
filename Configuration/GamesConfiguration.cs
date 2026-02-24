using System;
using System.ServiceModel;
using SmartWinners.IPlayer1service;

namespace SmartWinners.Configuration;

public class GamesConfiguration : MyConfiguration
{
    public int MaxBufferSize { get; set; }
    public int Timeout { get; set; }
    public string EndpointAddress { get; set; }
    
    public Player1Client InitClient()
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
        return new Player1Client(binding, endpoint);
    }
}