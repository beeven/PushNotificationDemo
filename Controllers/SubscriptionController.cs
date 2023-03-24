using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using testpushnotification.Models;
using System.Text.Json.Nodes;
using System.Security.Cryptography;
using testpushnotification.Services;
using testpushnotification.Data;

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
    public ActionResult Register([FromBody]PushSubscriptionInfo subscription)
    {
        
        
        System.Console.WriteLine(json.ToJsonString(new() { WriteIndented = true }));
        return Ok();
    }


    [HttpPost]
    [Consumes("application/json")]
    public ActionResult Unregister([FromBody] JsonNode json)
    {
        System.Console.WriteLine(json.ToJsonString(new() { WriteIndented = true }));
        return Ok();
    }

    [HttpGet]
    public string VAPIDPublicKey()
    {
        return this.vapid.VAPIDUncompressedPublicKey;

    }

}
