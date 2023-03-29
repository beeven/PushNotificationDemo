using System.Security.Cryptography;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using testpushnotification.Models;

namespace testpushnotification.Services;



public class VAPIDService : IVAPIDService
{
    private readonly byte[] serverUncompressedPublicKey;
    private readonly ECDsa ecdsa;
    private readonly IHttpClientFactory httpClientFactory;

    public string ServerUncompressedPublicKey => Convert.ToBase64String(serverUncompressedPublicKey);
    public string UrlBase64ServerUncompressedPublicKey => IVAPIDService.UrlBase64Encode(serverUncompressedPublicKey);
    //public string UrlBase64ServerDerEncodedPublicKey { get; }

    public VAPIDService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
        var publicKeyPem = Convert.FromBase64String(configuration["PushServiceKeys:PublicKeyPemBase64"]);
        //System.Console.WriteLine(System.Text.Encoding.UTF8.GetString(publicKeyPem));
        ecdsa = ECDsa.Create();
        ecdsa.ImportFromPem(System.Text.Encoding.UTF8.GetString(publicKeyPem));
        ecdsa.ImportFromPem(System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(configuration["PushServiceKeys:PrivateKeyPemBase64"])));

        var ecparam = ecdsa.ExportParameters(true);
        serverUncompressedPublicKey = (new byte[] { 0x04 }).Concat(ecparam.Q.X).Concat(ecparam.Q.Y).ToArray();

        //UrlBase64ServerDerEncodedPublicKey = IVAPIDService.UrlBase64Encode(ecdsa.ExportSubjectPublicKeyInfo());
        this.httpClientFactory = httpClientFactory;
    }

    public string GenerateAuthorizationHeader(string endpoint, DateTime? expiration = null, string? subject = null)
    {
        if (expiration is null)
        {
            expiration = DateTime.Now.AddHours(23);
        }
        if (subject is null)
        {
            subject = "mailto:beeven@hotmail.com";
        }

        var uri = new Uri(endpoint);
        var audience = uri.Scheme + @"://" + uri.Host;

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Audience = audience,
            Subject = new System.Security.Claims.ClaimsIdentity(new[] { new System.Security.Claims.Claim("sub", subject) }),
            Expires = expiration,
            SigningCredentials = new SigningCredentials(new ECDsaSecurityKey(ecdsa), SecurityAlgorithms.EcdsaSha256),
        };
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateJwtSecurityToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        return tokenString;

    }



    public IVAPIDService.EncryptionResult Encrypt(string p256dh, string auth, byte[] content)
    {

        var serverEC = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP256);
        //serverEC.GenerateKey(ECCurve.NamedCurves.nistP256);

        var serverPublicParams = serverEC.ExportParameters(false);
        var serverPublicKey = (new byte[] { 0x04 }).Concat(serverPublicParams.Q.X).Concat(serverPublicParams.Q.Y).ToArray();
        //var serverPublicKeyDer = serverEC.ExportSubjectPublicKeyInfo();

        var receiverKey = IVAPIDService.UrlBase64Decode(p256dh);
        //System.Console.WriteLine(Convert.ToHexString(receiverKey));
        var ecparam = new ECParameters();
        ecparam.Curve = ECCurve.NamedCurves.nistP256;
        ecparam.Q.X = receiverKey.Skip(1).Take(32).ToArray();
        ecparam.Q.Y = receiverKey.Skip(33).ToArray();
        var receiverEC = ECDiffieHellman.Create(ecparam);
        
        var secretStep1 = serverEC.DeriveKeyFromHmac(receiverEC.PublicKey, HashAlgorithmName.SHA256, IVAPIDService.UrlBase64Decode(auth));
        var secret = HMACSHA256.HashData(secretStep1, "Content-Encoding: auth\x00\x01"u8.ToArray());
        
        var suffix = new byte[6 + 2 + receiverKey.Length + 2 + serverPublicKey.Length];
        var suffixSpan = suffix.AsSpan();

        Buffer.BlockCopy("P-256\0"u8.ToArray(), 0, suffix, 0, 6);
        System.Buffers.Binary.BinaryPrimitives.WriteUInt16BigEndian(suffixSpan[6..8], (ushort)receiverKey.Length);
        Buffer.BlockCopy(receiverKey, 0, suffix, 8, receiverKey.Length);
        System.Buffers.Binary.BinaryPrimitives.WriteUInt16BigEndian(suffixSpan[(8 + receiverKey.Length)..(10 + receiverKey.Length)], (ushort)serverPublicKey.Length);
        Buffer.BlockCopy(serverPublicKey, 0, suffix, 10 + receiverKey.Length, serverPublicKey.Length);
        //var suffix = "P-256\0"u8.ToArray().Concat(receiverKeyLengthBytes).Concat(receiverKey).Concat(senderKeyLengthBytes).Concat(serverPublicKey).ToArray();
        var salt = System.Security.Cryptography.RandomNumberGenerator.GetBytes(16);

        var encryptionKey = HKDF.DeriveKey(HashAlgorithmName.SHA256, secret, 16, salt, "Content-Encoding: aesgcm\0"u8.ToArray().Concat(suffix).ToArray());
        var nonce = HKDF.DeriveKey(HashAlgorithmName.SHA256, secret, 12, salt, "Content-Encoding: nonce\0"u8.ToArray().Concat(suffix).ToArray());

        var paddingLength = 0;
        var padding = new byte[2 + paddingLength];
        System.Buffers.Binary.BinaryPrimitives.WriteUInt16BigEndian(padding, (ushort)paddingLength);

        var plainText = padding.Concat(content).ToArray();
        using var cipher = new AesGcm(encryptionKey);
        var tag = new byte[AesGcm.TagByteSizes.MaxSize];
        var cipherText = new byte[plainText.Length];
        cipher.Encrypt(nonce, plainText, cipherText, tag);
        
        //System.Console.WriteLine(Convert.ToHexString(cipherText));

        return new IVAPIDService.EncryptionResult
        {
            Salt = salt,
            Payload = cipherText.Concat(tag).ToArray(),
            PublicKey = serverPublicKey
        };
    }


    public async Task SendPush(string endpoint, string receiverKey, string receiverSecret, byte[]? content)
    {

        var client = httpClientFactory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
        if (content?.Length > 0)
        {
            var encryptedPayload = Encrypt(receiverKey, receiverSecret, content);
            request.Content = new ByteArrayContent(encryptedPayload.Payload);
            request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
            request.Content.Headers.ContentLength = encryptedPayload.Payload.Length;
            request.Content.Headers.ContentEncoding.Add("aesgcm");
            request.Headers.Add("Authorization", "WebPush " + GenerateAuthorizationHeader(endpoint));
            request.Headers.Add("Crypto-Key", $"dh={encryptedPayload.Base64EncodedPublicKey};p256ecdsa={UrlBase64ServerUncompressedPublicKey}");
            request.Headers.Add("Encryption", "salt=" + encryptedPayload.Base64EncodedSalt);
        }
        else
        {
            request.Content = new ByteArrayContent(new byte[] { });

            request.Headers.Add("Authorization", "WebPush " + GenerateAuthorizationHeader(endpoint));
            request.Headers.Add("Crypto-Key", $"p256ecdsa={UrlBase64ServerUncompressedPublicKey}");
        }
        request.Headers.Add("TTL", "2419200");


        var resp = await client.SendAsync(request);
        Console.WriteLine("Receive from server: {0}", await resp.Content.ReadAsStringAsync());

    }




}