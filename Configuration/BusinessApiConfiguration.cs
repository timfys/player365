using System;
using System.ServiceModel;
using BusinessApi;

namespace SmartWinners.Configuration;

public class BusinessApiConfiguration : MyConfiguration
{
    public string EndpointAddress { get; set; }
    public int ol_EntityId { get; set; }
    public string ol_UserName { get; set; }
    public string ol_Password { get; set; }
    public int Timeout { get; set; }
    public int BusinessId { get; set; }
    public int PaymentId { get; set; }
    public int EmployeeEntityId { get; set; }
    public int MaxBufferSize { get; set; }

    public int InventoryID { get; set; }

    public int CategoryID { get; set; }

    public int CampaignID { get; set; }

    public string TzFieldName { get; set; }
    public bool ChargePayment { get; set; }

    public BusinessAPIClient InitClient()
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
        return new BusinessAPIClient(binding, endpoint);
    }
}