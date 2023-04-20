using Azure.Identity;
using Azure.Security.KeyVault.Certificates;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using TeamsNotificationFunc.Interfaces;

namespace TeamsNotificationFunc.Services;

public class DecryptionService
{
    private readonly ILogger _logger;
    private readonly NotificationOptions _options;
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
    private readonly MemoryCache _certificates = new(new MemoryCacheOptions()
    {
        ExpirationScanFrequency = TimeSpan.FromMinutes(10)
    });

    public DecryptionService(ILoggerFactory loggerFactory, IOptions<NotificationOptions> options)
    {
        _logger = loggerFactory.CreateLogger<DecryptionService>();
        _options = options.Value;
    }

    // Taken from sample code:
    // https://github.com/OfficeDev/Microsoft-Teams-Samples/blob/main/samples/graph-meeting-notification/csharp/MeetingNotification/Helper/DecryptionHelper.cs
    public async Task<string> DecryptAsync(EncryptedContentData encryptedContent)
    {
        var decryptedSymmetricKey = await GetSymmetricKeyAsync(encryptedContent);
        if (decryptedSymmetricKey == null)
        {
            _logger.LogError("Unable to find a matching certificate with thumbprint {Thumbprint}", encryptedContent.EncryptionCertificateThumbprint);
            throw new CryptographicException("Unable to find certificate.");
        }

        // Can now use decryptedSymmetricKey with the AES algorithm.
        var encryptedPayload = Convert.FromBase64String(encryptedContent.Data);
        var expectedSignature = Convert.FromBase64String(encryptedContent.DataSignature);
        byte[] actualSignature;

        using (HMACSHA256 hmac = new HMACSHA256(decryptedSymmetricKey))
        {
            actualSignature = hmac.ComputeHash(encryptedPayload);
        }
        if (actualSignature.SequenceEqual(expectedSignature))
        {
            // Continue with decryption of the encryptedPayload.
        }
        else
        {
            throw new CryptographicException("Notification payload has been tampered. Please investigate.");
            // Log alert
            // Do not attempt to decrypt encryptedPayload. Assume notification payload has been tampered with and investigate.
        }

        using var aesProvider = new AesCryptoServiceProvider
        {
            Key = decryptedSymmetricKey,
            Padding = PaddingMode.PKCS7,
            Mode = CipherMode.CBC
        };

        // Obtain the initialization vector from the symmetric key itself.
        var vectorSize = 16;
        var iv = new byte[vectorSize];
        Array.Copy(decryptedSymmetricKey, iv, vectorSize);
        aesProvider.IV = iv;

        // Decrypt the resource data content.
        using var decryptor = aesProvider.CreateDecryptor();
        using var msDecrypt = new MemoryStream(encryptedPayload);
        using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
        using var srDecrypt = new StreamReader(csDecrypt);
        return srDecrypt.ReadToEnd();
    }

    private async Task<byte[]?> GetSymmetricKeyAsync(EncryptedContentData encryptedContent)
    {
        _semaphore.Wait();

        try
        {
            X509Certificate2? certificate = null;
            if (_certificates.TryGetValue<X509Certificate2>(encryptedContent.EncryptionCertificateThumbprint, out var certificateFromCache))
            {
                _logger.LogTrace("Found matching certificate with thumbprint {Thumbprint} from cache", encryptedContent.EncryptionCertificateThumbprint);
                certificate = certificateFromCache;
            }
            else
            {
                _logger.LogTrace("Looking certificate with thumbprint {Thumbprint} from key vault", encryptedContent.EncryptionCertificateThumbprint);

                var client = new CertificateClient(vaultUri: new Uri(_options.KeyVaultUrl), credential: new DefaultAzureCredential());
                foreach (var certificateProperties in client.GetPropertiesOfCertificates())
                {
                    var thumbprint = BitConverter.ToString(certificateProperties.X509Thumbprint).Replace("-", "");
                    if (encryptedContent.EncryptionCertificateThumbprint == thumbprint)
                    {
                        _logger.LogTrace("Found matching certificate with thumbprint {Thumbprint} from key vault", encryptedContent.EncryptionCertificateThumbprint);

                        certificate = (await client.DownloadCertificateAsync(certificateProperties.Name)).Value;

                        // Cache the certificate for 4 hours
                        var cacheEntryOptions = new MemoryCacheEntryOptions()
                            .SetSlidingExpiration(TimeSpan.FromHours(4));
                        _certificates.Set(encryptedContent.EncryptionCertificateThumbprint, certificate, cacheEntryOptions);
                        break;
                    }
                }
            }

            if (certificate == null)
            {
                _logger.LogError("Could not fine certificate with thumbprint {Thumbprint}", encryptedContent.EncryptionCertificateThumbprint);
                return null;
            }

            using var privateKey = certificate.GetRSAPrivateKey();
            if (privateKey == null)
            {
                _logger.LogError("Could not get private key for certificate with thumbprint {Thumbprint}", encryptedContent.EncryptionCertificateThumbprint);
                return null;
            }
            return privateKey.Decrypt(Convert.FromBase64String(encryptedContent.DataKey), RSAEncryptionPadding.OaepSHA1);
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
