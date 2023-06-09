using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using testpushnotification.Models;
using System.Text.Json.Nodes;
using System.Security.Cryptography;
using testpushnotification.Services;
using testpushnotification.Data;
using Microsoft.EntityFrameworkCore;

namespace testpushnotification.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class SubscriptionController : ControllerBase
{
    private readonly ILogger<SubscriptionController> _logger;
    private readonly IVAPIDService vapid;
    private readonly SubscriptionDbContext subsContext;
    private readonly IHttpClientFactory httpClientFactory;

    public SubscriptionController(
        ILogger<SubscriptionController> logger,
        IConfiguration configuration,
        Services.IVAPIDService vapid,
        testpushnotification.Data.SubscriptionDbContext subsContext,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        this.vapid = vapid;
        this.subsContext = subsContext;
        this.httpClientFactory = httpClientFactory;
    }

    [HttpPost]
    [Consumes("application/json")]
    public async Task<ActionResult> Register([FromBody] PushSubscriptionInfo info)
    {
        var subscription = await subsContext.Subscriptions.SingleOrDefaultAsync(x => x.ClientId == info.ClientId && x.Endpoint == info.Subscription.Endpoint);
        if (subscription is null)
        {
            subscription = new ClientSubscription
            {
                ClientId = info.ClientId,
                Endpoint = info.Subscription.Endpoint,
                Expires = info.Subscription.ExpirationTime,
                P256DH = info.Subscription.Keys.P256DH,
                Auth = info.Subscription.Keys.Auth,
                JwtToken = vapid.GenerateAuthorizationHeader(info.Subscription.Endpoint, info.Subscription.ExpirationTime),
                DateCreated = DateTimeOffset.Now,
                DateModified = DateTimeOffset.Now
            };
            await subsContext.Subscriptions.AddAsync(subscription);
        }
        else
        {
            //subscription.Endpoint = info.Subscription.Endpoint;
            //subscription.Expires = info.Subscription.ExpirationTime;
            subscription.P256DH = info.Subscription.Keys.P256DH;
            subscription.Auth = info.Subscription.Keys.Auth;
            if(info.Subscription.ExpirationTime != subscription.Expires && subscription.Expires - DateTimeOffset.Now < TimeSpan.FromHours(1))
            {
                subscription.Expires = info.Subscription.ExpirationTime;
                subscription.JwtToken = vapid.GenerateAuthorizationHeader(subscription.Endpoint, info.Subscription.ExpirationTime);
            }
            subscription.DateModified = DateTimeOffset.Now;
        }
        await subsContext.SaveChangesAsync();
        System.Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(subscription, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));

        await vapid.SendPush(subscription.Endpoint, subscription.P256DH, subscription.Auth, null);
        return Ok();
    }


    [HttpPost]
    [Consumes("application/json")]
    public async Task<ActionResult> Unregister([FromBody] PushSubscriptionInfo info)
    {
        await subsContext.Subscriptions.Where(x => x.ClientId == info.ClientId && x.Endpoint == info.Subscription.Endpoint).ExecuteDeleteAsync();
        System.Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(info, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
        return Ok();
    }

    [HttpGet]
    public string VAPIDPublicKey()
    {
        return this.vapid.UrlBase64ServerUncompressedPublicKey;

    }

    [HttpPost("{clientId}")]
    public async Task<ActionResult> SendPushNotification([FromRoute] string clientId, [FromBody] JsonNode body)
    {
        var info = await subsContext.Subscriptions.SingleOrDefaultAsync(x => x.ClientId == clientId && x.Endpoint == body["endpoint"].ToString());
        if (info is not null)
        {
            await vapid.SendPush(info.Endpoint, info.P256DH, info.Auth, System.Text.Encoding.UTF8.GetBytes(body["content"]?.ToString() ?? ""));
        }
        return Ok();
    }

}
