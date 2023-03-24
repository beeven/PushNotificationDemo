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
    private readonly VAPIDService vapid;
    private readonly SubscriptionDbContext subsContext;

    public SubscriptionController(ILogger<SubscriptionController> logger, IConfiguration configuration, Services.VAPIDService vapid, testpushnotification.Data.SubscriptionDbContext subsContext)
    {
        _logger = logger;
        this.vapid = vapid;
        this.subsContext = subsContext;
    }

    [HttpPost]
    [Consumes("application/json")]
    public async Task<ActionResult> Register([FromBody]PushSubscriptionInfo info)
    {
        var subscription = await subsContext.Subscriptions.FindAsync(info.ClientId);
        if(subscription is null)
        {
            await subsContext.Subscriptions.AddAsync(new ClientSubscription{
                ClientId = info.ClientId,
                Endpoint = info.Subscription.Endpoint,
                Expires = info.Subscription.ExpirationTime,
                P256DH = info.Subscription.Keys.P256DH,
                Auth = info.Subscription.Keys.Auth,
                DateCreated = DateTimeOffset.Now,
                DateModified = DateTimeOffset.Now
            });
        }
        else
        {
            subscription.Endpoint = info.Subscription.Endpoint;
            subscription.Expires = info.Subscription.ExpirationTime;
            subscription.P256DH = info.Subscription.Keys.P256DH;
            subscription.Auth = info.Subscription.Keys.Auth;
            subscription.DateModified = DateTimeOffset.Now;
        }
        await subsContext.SaveChangesAsync();
        


        System.Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(subscription, new System.Text.Json.JsonSerializerOptions{WriteIndented = true}));
        return Ok();
    }


    [HttpPost]
    [Consumes("application/json")]
    public async Task<ActionResult> Unregister([FromBody] PushSubscriptionInfo info)
    {
        await subsContext.Subscriptions.Where(x => x.ClientId == info.ClientId).ExecuteDeleteAsync();
        System.Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(info, new System.Text.Json.JsonSerializerOptions{WriteIndented = true}));
        return Ok();
    }

    [HttpGet]
    public string VAPIDPublicKey()
    {
        return this.vapid.VAPIDUncompressedPublicKey;

    }

}
