using System;
using System.ServiceModel;
using SmartWinners.ITelegramBotservice;

namespace SmartWinners.Configuration;

public class TelegramConfiguration : MyConfiguration
{
    public string TelegramId { get; set; }
    public string[] TelegramErrorsId { get; set; }
    public string TelegramSaleId { get; set; }
    public string EndpointAddress { get; set; }
    public int ol_EntityId { get; set; }
    public string ol_UserName { get; set; }
    public string ol_Password { get; set; }
    public int Timeout { get; set; }

    public TelegramBotClient InitClient()
    {

        var timeout = TimeSpan.FromSeconds(Timeout);
        var binding = new BasicHttpBinding
        {
            ReceiveTimeout = timeout,
            CloseTimeout = timeout,
            OpenTimeout = timeout,
            SendTimeout = timeout
        };

        return new TelegramBotClient(binding, new EndpointAddress(EndpointAddress));
    }
}