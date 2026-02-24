using System.ServiceModel;
using SmartWinners.IWhatsappservice;
using System;

namespace SmartWinners.Configuration;

public class WhatsAppConfiguration : MyConfiguration
{
    public string EndpointAddress { get; set; }
    public int ol_EntityId { get; set; }
    public string ol_UserName { get; set; }
    public string ol_Password { get; set; }
    public int Timeout { get; set; }

    public WhatsappClient InitClient()
    {

        var timeout = TimeSpan.FromSeconds(Timeout);
        var binding = new BasicHttpBinding
        {
            ReceiveTimeout = timeout,
            CloseTimeout = timeout,
            OpenTimeout = timeout,
            SendTimeout = timeout
        };

        return new WhatsappClient(binding, new EndpointAddress(EndpointAddress));
    }
}