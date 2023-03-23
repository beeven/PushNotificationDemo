using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using testpushnotification.Models;
using System.Text.Json.Nodes;
using System.Security.Cryptography;

namespace testpushnotification.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class SubscriptionController : ControllerBase
{
    private readonly ILogger<SubscriptionController> _logger;
    private readonly string vapidPublicKey = "";

    public SubscriptionController(ILogger<SubscriptionController> logger, IConfiguration configuration)
    {
        _logger = logger;

        try
        {
            var publicKeyPem = Convert.FromBase64String(configuration["PushServiceKeys:PublicKeyPemBase64"]);
            System.Console.WriteLine(System.Text.Encoding.UTF8.GetString(publicKeyPem));
            var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
            ecdsa.ImportFromPem(System.Text.Encoding.UTF8.GetString(publicKeyPem));
            ecdsa.ImportFromPem(System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(configuration["PushServiceKeys:PrivateKeyPemBase64"])));
            
            var ecparam = ecdsa.ExportParameters(true);
            var uncompressed =   (new byte[]{0x04}).Concat(ecparam.Q.X).Concat(ecparam.Q.Y);
            // uncompressed. ecparam.Q.X+ecparam.Q.Y
            vapidPublicKey = Convert.ToBase64String(uncompressed.ToArray());
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cannot read public key or private key from configuration");
        }
    }

    [HttpPost]
    [Consumes("application/json")]
    public ActionResult Register([FromBody] JsonNode json)
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
        return vapidPublicKey;

    }

}
