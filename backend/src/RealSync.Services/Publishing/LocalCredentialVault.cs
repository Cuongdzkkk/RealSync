using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using RealSync.Core.Interfaces.Publishing;
using RealSync.Core.Models.Publishing;

namespace RealSync.Services.Publishing;

public class LocalCredentialVault : ICredentialVault
{
    private readonly VaultOptions _options;
    private readonly byte[] _keyBytes;

    public LocalCredentialVault(IOptions<VaultOptions> options, IConfiguration configuration)
    {
        _options = options.Value;
        var rawKey = _options.MasterKey;
        if (string.IsNullOrWhiteSpace(rawKey))
        {
            // Fallback key derived from JWT secret to guarantee operation
            rawKey = configuration["Jwt:Secret"] ?? "RealSyncFallbackDefaultSecretKey1234567890";
        }
        
        _keyBytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawKey));
    }

    public (string Ciphertext, string KeyVersion) Encrypt(string plaintext)
    {
        if (string.IsNullOrEmpty(plaintext)) return (string.Empty, _options.ActiveVersion);

        byte[] plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
        byte[] nonce = new byte[12];
        RandomNumberGenerator.Fill(nonce);

        byte[] tag = new byte[16];
        byte[] ciphertext = new byte[plaintextBytes.Length];

        using (var aesGcm = new AesGcm(_keyBytes, 16))
        {
            aesGcm.Encrypt(nonce, plaintextBytes, ciphertext, tag);
        }

        byte[] combined = new byte[nonce.Length + tag.Length + ciphertext.Length];
        Buffer.BlockCopy(nonce, 0, combined, 0, nonce.Length);
        Buffer.BlockCopy(tag, 0, combined, nonce.Length, tag.Length);
        Buffer.BlockCopy(ciphertext, 0, combined, nonce.Length + tag.Length, ciphertext.Length);

        return (Convert.ToBase64String(combined), _options.ActiveVersion);
    }

    public string Decrypt(string ciphertext, string keyVersion)
    {
        if (string.IsNullOrEmpty(ciphertext)) return string.Empty;

        byte[] combined = Convert.FromBase64String(ciphertext);
        byte[] nonce = new byte[12];
        byte[] tag = new byte[16];
        byte[] ciphertextBytes = new byte[combined.Length - nonce.Length - tag.Length];

        Buffer.BlockCopy(combined, 0, nonce, 0, nonce.Length);
        Buffer.BlockCopy(combined, nonce.Length, tag, 0, tag.Length);
        Buffer.BlockCopy(combined, nonce.Length + tag.Length, ciphertextBytes, 0, ciphertextBytes.Length);

        byte[] decryptedBytes = new byte[ciphertextBytes.Length];

        using (var aesGcm = new AesGcm(_keyBytes, 16))
        {
            aesGcm.Decrypt(nonce, ciphertextBytes, tag, decryptedBytes);
        }

        return Encoding.UTF8.GetString(decryptedBytes);
    }
}
