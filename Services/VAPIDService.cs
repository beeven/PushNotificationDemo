using System.Security.Cryptography;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using testpushnotification.Models;

namespace testpushnotification.Services;

public class VAPIDService
{
    private readonly string vapidUncompressedPublicKey;
    private readonly string vapidDerEncodedPublicKey;
    private readonly ECDsa ecdsa;

    public string VAPIDUncompressedPublicKey => vapidUncompressedPublicKey;
    public string VAPIDDerEncodedPublicKey => vapidDerEncodedPublicKey;
    public VAPIDService(IConfiguration configuration)
    {
        var publicKeyPem = Convert.FromBase64String(configuration["PushServiceKeys:PublicKeyPemBase64"]);
        //System.Console.WriteLine(System.Text.Encoding.UTF8.GetString(publicKeyPem));
        var ecdsa = ECDsa.Create();
        ecdsa.ImportFromPem(System.Text.Encoding.UTF8.GetString(publicKeyPem));
        ecdsa.ImportFromPem(System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(configuration["PushServiceKeys:PrivateKeyPemBase64"])));

        var ecparam = ecdsa.ExportParameters(true);
        var uncompressed = (new byte[] { 0x04 }).Concat(ecparam.Q.X).Concat(ecparam.Q.Y);
        // uncompressed. ecparam.Q.X+ecparam.Q.Y
        vapidUncompressedPublicKey = Convert.ToBase64String(uncompressed.ToArray());
        vapidDerEncodedPublicKey = Convert.ToBase64String(ecdsa.ExportSubjectPublicKeyInfo());
    }

    public string GenerateAuthorizationHeader(string endpoint, DateTime? expiration = null, string? subject = null)
    {
        if(expiration is null)
        {
            expiration = DateTime.Now.AddHours(23);
        }
        if(subject is null)
        {
            subject = "mailto:beeven@hotmail.com";
        }

        
        var tokenDescriptor = new SecurityTokenDescriptor{
            Audience = endpoint,
            Subject = new System.Security.Claims.ClaimsIdentity(new[]{new System.Security.Claims.Claim("sub",subject)}),
            Expires = expiration,
            SigningCredentials = new SigningCredentials(new ECDsaSecurityKey(ecdsa), SecurityAlgorithms.EcdsaSha256),
        };
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateJwtSecurityToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);
        var urlSafePublicKey = VAPIDDerEncodedPublicKey.TrimEnd('=').Replace('+','-').Replace('/','_');
        return $"t={tokenString},k={urlSafePublicKey}";

    }


}