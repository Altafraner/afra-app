using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Altafraner.Backbone.WebNotifications.Services;

/// <summary>
///     Sends Web Push notifications using VAPID authentication (RFC 8292) and
///     <c>aes128gcm</c> content encryption (RFC 8188 / RFC 8291).
///     No third-party library is used; only .NET BCL cryptography primitives.
/// </summary>
internal sealed class WebPushSender<TPerson> where TPerson : class, IWebNotificationRecipient

{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<WebPushSender<TPerson>> _logger;

    /// <summary>
    ///     <see langword="true" /> when VAPID keys are configured and push can be sent.
    /// </summary>
    public bool IsEnabled { get; private init; }

    private readonly string _subject = string.Empty;
    private readonly string _publicKeyBase64Url = string.Empty; // original URL-safe base64 (65 bytes)
    private readonly byte[] _publicKeyX = [];                   // 32-byte X coordinate
    private readonly byte[] _publicKeyY = [];                   // 32-byte Y coordinate
    private readonly byte[] _privateKeyD = [];                  // 32-byte private scalar

    /// <summary>
    ///     Constructs a new <see cref="WebPushSender{TPerson}" />.
    ///     Push delivery is silently disabled when the VAPID config section is absent or incomplete.
    /// </summary>
    public WebPushSender(
        IOptions<VapidConfiguration<TPerson>> config,
        IHttpClientFactory httpClientFactory,
        ILogger<WebPushSender<TPerson>> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;

        var cfg = config.Value;
        if (string.IsNullOrEmpty(cfg.PublicKey) || string.IsNullOrEmpty(cfg.PrivateKey))
        {
            IsEnabled = false;
            return;
        }

        byte[] pubKeyBytes;
        byte[] privKeyBytes;
        try
        {
            pubKeyBytes = Base64UrlDecode(cfg.PublicKey);
            privKeyBytes = Base64UrlDecode(cfg.PrivateKey);
        }
        catch (FormatException ex)
        {
            throw new InvalidOperationException(
                "VAPID keys in configuration are not valid URL-safe base64.", ex);
        }

        if (pubKeyBytes.Length != 65 || pubKeyBytes[0] != 0x04)
            throw new InvalidOperationException(
                "VAPID:PublicKey must be a 65-byte uncompressed P-256 point (base64url-encoded).");
        if (privKeyBytes.Length != 32)
            throw new InvalidOperationException(
                "VAPID:PrivateKey must be a 32-byte P-256 private scalar (base64url-encoded).");

        _publicKeyBase64Url = cfg.PublicKey;
        _publicKeyX = pubKeyBytes[1..33];
        _publicKeyY = pubKeyBytes[33..65];
        _privateKeyD = privKeyBytes;
        _subject = cfg.Subject ?? string.Empty;
        IsEnabled = true;
    }

    /// <summary>
    ///     Sends a single Web Push notification.
    /// </summary>
    /// <param name="endpoint">Subscription endpoint URL.</param>
    /// <param name="p256dhBase64Url">Subscription P-256 DH public key (URL-safe base64, 65 bytes).</param>
    /// <param name="authBase64Url">Subscription auth secret (URL-safe base64, 16 bytes).</param>
    /// <param name="jsonPayload">UTF-8 JSON string to deliver.</param>
    /// <exception cref="PushSubscriptionGoneException">
    ///     Thrown when the push service returns 404 or 410 (subscription expired).
    /// </exception>
    public async Task SendAsync(Uri endpoint, string p256dhBase64Url, string authBase64Url, string jsonPayload)
    {
        if (!IsEnabled)
            return;

        var uaPublicKey = Base64UrlDecode(p256dhBase64Url);
        var authSecret = Base64UrlDecode(authBase64Url);

        var body = EncryptPayload(Encoding.UTF8.GetBytes(jsonPayload), uaPublicKey, authSecret);

        var audience = GetAudience(endpoint);
        var jwt = CreateVapidJwt(audience);

        var client = _httpClientFactory.CreateClient("WebPush");
        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);

        request.Headers.TryAddWithoutValidation("Authorization", $"vapid t={jwt},k={_publicKeyBase64Url}");
        request.Headers.TryAddWithoutValidation("TTL", "86400");
        request.Headers.TryAddWithoutValidation("Urgency", "normal");

        request.Content = new ByteArrayContent(body);
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        request.Content.Headers.TryAddWithoutValidation("Content-Encoding", "aes128gcm");

        var response = await client.SendAsync(request);

        if (response.StatusCode is System.Net.HttpStatusCode.Gone
            or System.Net.HttpStatusCode.NotFound)
        {
            throw new PushSubscriptionGoneException(endpoint.ToString());
        }

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException(
                $"Web Push request failed with {(int)response.StatusCode}: {error}",
                inner: null,
                response.StatusCode);
        }
    }

    private string CreateVapidJwt(string audience)
    {
        var header = Base64UrlEncode("""{"typ":"JWT","alg":"ES256"}"""u8.ToArray());
        var exp = DateTimeOffset.UtcNow.AddHours(12).ToUnixTimeSeconds();
        var payload = Base64UrlEncode(
            Encoding.UTF8.GetBytes(
                JsonSerializer.Serialize(new { aud = audience, exp, sub = _subject })));

        var signingInput = Encoding.ASCII.GetBytes($"{header}.{payload}");
        var ecParams = new ECParameters
        {
            Curve = ECCurve.NamedCurves.nistP256,
            Q = new ECPoint { X = _publicKeyX, Y = _publicKeyY },
            D = _privateKeyD,
        };
        using var ecdsa = ECDsa.Create(ecParams);

        var signature = ecdsa.SignData(
            signingInput, HashAlgorithmName.SHA256,
            DSASignatureFormat.IeeeP1363FixedFieldConcatenation);

        return $"{header}.{payload}.{Base64UrlEncode(signature)}";
    }

    private static byte[] EncryptPayload(byte[] plaintext, byte[] uaPublicKey, byte[] authSecret)
    {
        const int rs = 4096;
        if (plaintext.Length > rs - 1)
            throw new ArgumentException(
                $"Push notification payload ({plaintext.Length} bytes) exceeds the maximum of {rs - 1} bytes.",
                nameof(plaintext));

        using var asKey = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP256);
        var asParams = asKey.ExportParameters(includePrivateParameters: false);

        if (asParams.Q.X is null || asParams.Q.Y is null)
            throw new InvalidOperationException("ECDiffieHellman did not produce a valid public key.");

        var asPublicKey = new byte[65];
        asPublicKey[0] = 0x04;
        asParams.Q.X.CopyTo(asPublicKey.AsSpan(1));
        asParams.Q.Y.CopyTo(asPublicKey.AsSpan(33));

        using var uaDh = ECDiffieHellman.Create(new ECParameters
        {
            Curve = ECCurve.NamedCurves.nistP256,
            Q = new ECPoint { X = uaPublicKey[1..33], Y = uaPublicKey[33..65] },
        });

        var sharedSecret = asKey.DeriveRawSecretAgreement(uaDh.PublicKey);

        var prefix = "WebPush: info\0"u8.ToArray();
        var keyInfo = new byte[prefix.Length + 65 + 65];
        prefix.CopyTo(keyInfo.AsSpan());
        uaPublicKey.CopyTo(keyInfo.AsSpan(prefix.Length));
        asPublicKey.CopyTo(keyInfo.AsSpan(prefix.Length + 65));

        var prk1 = HKDF.Extract(HashAlgorithmName.SHA256, ikm: sharedSecret, salt: authSecret);
        var ikm = HKDF.Expand(HashAlgorithmName.SHA256, prk: prk1, outputLength: 32, info: keyInfo);

        var salt = RandomNumberGenerator.GetBytes(16);
        var prk2 = HKDF.Extract(HashAlgorithmName.SHA256, ikm: ikm, salt: salt);
        var aesKey = HKDF.Expand(HashAlgorithmName.SHA256, prk: prk2, outputLength: 16,
            info: "Content-Encoding: aes128gcm\0"u8.ToArray());
        var aesNonce = HKDF.Expand(HashAlgorithmName.SHA256, prk: prk2, outputLength: 12,
            info: "Content-Encoding: nonce\0"u8.ToArray());

        var record = new byte[plaintext.Length + 1];
        plaintext.CopyTo(record.AsSpan());
        record[^1] = 0x02;

        var ciphertext = new byte[record.Length];
        var tag = new byte[16];
        using var aes = new AesGcm(aesKey, tagSizeInBytes: 16);
        aes.Encrypt(aesNonce, record, ciphertext, tag);

        // +-----------+--------+-----------+---------------+
        // | salt (16) | rs (4) | idlen (1) | keyid (idlen) |
        // +-----------+--------+-----------+---------------+
        Span<byte> header = stackalloc byte[16 + 4 + 1 + 65];
        salt.CopyTo(header);
        header[16] = unchecked((byte)(rs >> 24));
        header[17] = unchecked((byte)(rs >> 16));
        header[18] = unchecked((byte)(rs >> 8));
        header[19] = unchecked((byte)rs);
        header[20] = 65;
        asPublicKey.CopyTo(header[21..]);

        var result = new byte[header.Length + ciphertext.Length + tag.Length];
        header.CopyTo(result.AsSpan());
        ciphertext.CopyTo(result.AsSpan(header.Length));
        tag.CopyTo(result.AsSpan(header.Length + ciphertext.Length));

        return result;
    }

    private static string GetAudience(Uri uri)
    {
        return $"{uri.Scheme}://{uri.Authority}";
    }

    internal static byte[] Base64UrlDecode(string value)
    {
        try
        {
            var padding = (4 - value.Length % 4) % 4;
            var base64 = value.Replace('-', '+').Replace('_', '/') + new string('=', padding);
            return Convert.FromBase64String(base64);
        }
        catch (FormatException ex)
        {
            throw new FormatException($"The value '{value}' is not valid URL-safe base64.", ex);
        }
    }

    internal static string Base64UrlEncode(byte[] data)
        => Convert.ToBase64String(data).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}
