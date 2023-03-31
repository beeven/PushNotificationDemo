namespace testpushnotification.Services;

public interface IVAPIDService
{
    string ServerUncompressedPublicKey { get; }
    string UrlBase64ServerUncompressedPublicKey { get; }
    //string UrlBase64ServerDerEncodedPublicKey { get; }

    string GenerateAuthorizationHeader(string endpoint, DateTime? expiration = null, string? subject = null);
    Task SendPush(string endpoint, string receiverKey, string receiverSecret, byte[]? content);

    public class EncryptionResult
    {
        public byte[] Salt { get; set; } = new byte[]{};
        public byte[] Payload { get; set; } = new byte[]{};
        public byte[] PublicKey { get; set; } = new byte[]{};

        public string Base64EncodedPublicKey => UrlBase64Encode(PublicKey);
        public string Base64EncodedSalt => UrlBase64Encode(Salt);
    }

    public static string UrlBase64Encode(byte[] content)
    {
        return Convert.ToBase64String(content).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }
    public static byte[] UrlBase64Decode(string content)
    {
        return Convert.FromBase64String(content.Replace("_", "/").Replace("-", "+") + new string('=', (4 - (content.Length % 4)) % 4));
    }
}