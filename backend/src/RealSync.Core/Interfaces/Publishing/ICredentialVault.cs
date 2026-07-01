namespace RealSync.Core.Interfaces.Publishing;

public interface ICredentialVault
{
    (string Ciphertext, string KeyVersion) Encrypt(string plaintext);
    string Decrypt(string ciphertext, string keyVersion);
}
