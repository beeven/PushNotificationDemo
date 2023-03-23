using System.Security.Cryptography;
using testpushnotification.Models;

namespace testpushnotification.Services;

public class CrpytoService
{
    private readonly string vapidUncompressedPublicKey;
    public string VAPIDUncompressedPublicKey => vapidUncompressedPublicKey;
    public CrpytoService(IConfiguration configuration)
    {
        var publicKeyPem = Convert.FromBase64String(configuration["PushServiceKeys:PublicKeyPemBase64"]);
        System.Console.WriteLine(System.Text.Encoding.UTF8.GetString(publicKeyPem));
        var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        ecdsa.ImportFromPem(System.Text.Encoding.UTF8.GetString(publicKeyPem));
        ecdsa.ImportFromPem(System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(configuration["PushServiceKeys:PrivateKeyPemBase64"])));

        var ecparam = ecdsa.ExportParameters(true);
        var uncompressed = (new byte[] { 0x04 }).Concat(ecparam.Q.X).Concat(ecparam.Q.Y);
        // uncompressed. ecparam.Q.X+ecparam.Q.Y
        vapidUncompressedPublicKey = Convert.ToBase64String(uncompressed.ToArray());
    }

    public string GenerateAuthorizationHeader(string endpoint)
    {

    }


}