using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Extensions.Options;

namespace MusicStreamingService.Infrastructure.Password;

public interface IPasswordService
{
    /// <summary>
    /// Encode password with salt
    /// </summary>
    /// <param name="password">Password to encode</param>
    /// <returns>Encoded password with salt inside</returns>
    public byte[] Encode(string password);
    
    /// <summary>
    /// Match encoded and non encoded passwords
    /// </summary>
    /// <param name="encodedPassword">Encoded password</param>
    /// <param name="passedPassword">Non encoded password</param>
    /// <returns>Flag, that tells if passwords are the same or not</returns>
    public bool Match(byte[] encodedPassword, string passedPassword);
}

public class PasswordService : IPasswordService
{
    private readonly PasswordServiceConfig _config;

    public PasswordService(IOptions<PasswordServiceConfig> config)
    {
        _config = config.Value;
    }
    
    public byte[] Encode(string password)
    {
        var salt = GenerateSalt(_config.SaltSize);
        return salt.Concat(Encode(password, salt)).ToArray();   
    }

    public bool Match(byte[] encodedPassword, string passedPassword)
    {
        var (salt, password) = GetSaltAndPasswordFromBytes(encodedPassword);
        var passedPasswordEncoded = Encode(passedPassword, salt);
        
        return CryptographicOperations.FixedTimeEquals(password, passedPasswordEncoded);
    }
    
    private byte[] Encode(string password, byte[] salt)
    {
        var hashedPass = KeyDerivation.Pbkdf2(
            password: password,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: _config.IterationsCount,
            numBytesRequested: _config.NumBytesRequested);

        return hashedPass;
    }
    
    private static byte[] GenerateSalt(int length) => RandomNumberGenerator.GetBytes(length);

    private (byte[] salt, byte[] password) GetSaltAndPasswordFromBytes(byte[] storedPassword)
    {
        var salt = storedPassword.Take(_config.SaltSize).ToArray();
        var password = storedPassword.Skip(_config.SaltSize).ToArray();

        return (salt, password);
    } 
}