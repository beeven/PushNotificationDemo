using System.Text;
using WebPush;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Crypto.Parameters;
namespace testpushnotification.Services;

public class WebPushVAPIDService: IVAPIDService
{
    private readonly IHttpClientFactory httpClientFactory;

    private readonly VapidDetails vapidDetails;
    private readonly WebPushClient webPushClient;
    private readonly string serverPrivateKey;

    public string ServerUncompressedPublicKey {get;}
    public string UrlBase64ServerUncompressedPublicKey => ServerUncompressedPublicKey.TrimEnd('=').Replace("+","-").Replace("/","_");

    public WebPushVAPIDService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        this.httpClientFactory = httpClientFactory;
        webPushClient = new WebPushClient(httpClientFactory.CreateClient());

        var reader = new PemReader(new StringReader(Encoding.UTF8.GetString(Convert.FromBase64String(configuration["PushServiceKeys:PrivateKeyPemBase64"]))));
        var privateKeyParams = (ECPrivateKeyParameters)reader.ReadObject();
        var q = privateKeyParams.Parameters.G.Multiply(privateKeyParams.D);
        var publicKeyParams = new ECPublicKeyParameters(q, privateKeyParams.Parameters);

        ServerUncompressedPublicKey = Convert.ToBase64String(publicKeyParams.Q.GetEncoded(false));
        serverPrivateKey = Convert.ToBase64String(privateKeyParams.D.ToByteArrayUnsigned());

        vapidDetails = new VapidDetails("mailto:beeven@hotmail.com", 
            Convert.ToBase64String(publicKeyParams.Q.GetEncoded(false)), 
            Convert.ToBase64String(privateKeyParams.D.ToByteArrayUnsigned()));
        
    
    }

    public string GenerateAuthorizationHeader(string endpoint, DateTime? expiration = null, string? subject = null)
    {
        var uri = new Uri(endpoint);
        var audience = uri.Scheme + @"://" + uri.Host;
        var headers = WebPush.VapidHelper.GetVapidHeaders(audience, subject, ServerUncompressedPublicKey, serverPrivateKey);
        var jwtToken = headers["Authorization"].Split(" ")[1];
        return jwtToken;
    }

    public async Task SendPush(string endpoint, string receiverKey, string receiverSecret, byte[]? content)
    {
        
        var subscription = new PushSubscription(endpoint,receiverKey, receiverSecret);
        try
        {
            await webPushClient.SendNotificationAsync(subscription, Encoding.UTF8.GetString(content ?? "subscribed"u8.ToArray()),vapidDetails);
        }
        catch(WebPushException ex)
        {
            System.Console.WriteLine(ex.Message);
        }
    }


}