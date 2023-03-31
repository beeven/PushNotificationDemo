using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
namespace testpushnotification.Models;

public class PushSubscriptionInfo
{
    public string ClientId {get;set;} = "";
    public PushSubscription? Subscription {get;set;}
    
}

public class PushSubscription
{
    public string Endpoint {get;set;} = "";
    public DateTime? ExpirationTime {get;set;}
    public PushSubscriptionKeys? Keys {get;set;}
}

public class PushSubscriptionKeys
{
    [JsonPropertyName("p256dh")]
    public string P256DH {get;set;} = "";
    public string Auth {get;set;} = "";
}