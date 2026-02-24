using System.Collections.Generic;
using System.Linq;
using BusinessApi;
using Newtonsoft.Json;

namespace SmartWinners.Helpers;

public class Message
{
    
    [JsonProperty("Language")]
    public string Language { get; set; }
    [JsonProperty("Body")]
    public string Body { get; set; }
        
    [JsonProperty("messageId")]
    public int Id { get; set; } 
    
    public static List<Message> GetInviteMessages()
    {
        var config = EnvironmentHelper.BusinessApiConfiguration;
        var client = config.InitClient();

        var resp = client.Outgoing_Campaign_Get(new Outgoing_Campaign_GetRequest
        {
            ol_Username = config.ol_UserName,
            ol_Password = config.ol_Password,
            BusinessId = config.BusinessId,
            ol_EntityID = config.ol_EntityId,
            Fields = new[]
            {
                "Language", "Body"
            },
            FilterFields = new[]
            {
                "MessageName"
            },
            FilterValues = new[]
            {
                "entity_affiliates_invite"
            }
        }).@return;

        return JsonConvert.DeserializeObject<List<Message>>(resp);
    }
    public static Message? GetInviteMessage(string language)
    {
        var config = EnvironmentHelper.BusinessApiConfiguration;
        var client = config.InitClient();

        var resp = client.Outgoing_Campaign_Get(new Outgoing_Campaign_GetRequest
        {
            ol_Username = config.ol_UserName,
            ol_Password = config.ol_Password,
            BusinessId = config.BusinessId,
            ol_EntityID = config.ol_EntityId,
            Fields = new[]
            {
                "Language", "Body"
            },
            FilterFields = new[]
            {
                "MessageName", "Language"
            },
            FilterValues = new[]
            {
                "entity_affiliates_invite", language
            }
        }).@return;

        return JsonConvert.DeserializeObject<List<Message>>(resp)?.FirstOrDefault();
    }
    public static Message Get(int id)
    {
        var config = EnvironmentHelper.BusinessApiConfiguration;
        var client = config.InitClient();

        var resp = client.Outgoing_Campaign_Get(new Outgoing_Campaign_GetRequest
        {
            ol_Username = config.ol_UserName,
            ol_Password = config.ol_Password,
            BusinessId = config.BusinessId,
            ol_EntityID = config.ol_EntityId,
            Fields = new[]
            {
                "Language", "Body"
            },
            FilterFields = new[]
            {
                "MessageId"
            },
            FilterValues = new[]
            {
                $"{id}"
            }
        }).@return;

        return JsonConvert.DeserializeObject<List<Message>>(resp).First();
    }
}