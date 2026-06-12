using System.Collections.Concurrent;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace BudgetAndExpenseTracker.Services;

/// <summary>
/// Data record representing an application user account state profile.
/// </summary>
public sealed record AppUser(string Id, string Email, string DisplayName, string PasswordHash);

/// <summary>
/// Core contract interface governing access management, registration, and claims mapping.
/// </summary>
public interface IAccountService
{
    AppUser? ValidateCredentials(string email, string password);
    (bool Success, string? Error) Register(string email, string displayName, string password);
    ClaimsPrincipal BuildPrincipal(AppUser user);
}

/// <summary>
/// Thread-safe in-memory simulation engine for credential checking and user profile processing.
/// </summary>
public sealed class InMemoryAccountService : IAccountService
{
    private readonly ConcurrentDictionary<string, AppUser> _users = new();
    private const int SaltSize = 16;          
    private const int KeySize = 32;           
    private const int Iterations = 100_000;   
    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

    public InMemoryAccountService()
    {
        Register("demo@budgettracker.com", "Demo User", "Password123!");
    }

    public AppUser? ValidateCredentials(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return null;

        if (!_users.TryGetValue(email.Trim().ToLowerInvariant(), out var user))
            return null;

        return VerifyHash(password, user.PasswordHash) ? user : null;
    }

    public (bool Success, string? Error) Register(string email, string displayName, string password)
    {
        if (string.IsNullOrWhiteSpace(email))
            return (false, "Email is required.");
        if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
            return (false, "Password must be at least 6 characters.");

        var key = email.Trim().ToLowerInvariant();
        var user = new AppUser(
            Id: Guid.NewGuid().ToString(),
            Email: email.Trim(),
            DisplayName: string.IsNullOrWhiteSpace(displayName) ? email.Trim() : displayName.Trim(),
            PasswordHash: HashPassword(password));

        return _users.TryAdd(key, user)
            ? (true, null)
            : (false, "An account with that email already exists.");
    }

    public ClaimsPrincipal BuildPrincipal(AppUser user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.DisplayName),
            new(ClaimTypes.Email, user.Email),
        };
        
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        return new ClaimsPrincipal(identity);
    }

    private static string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, KeySize);
        return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    private static bool VerifyHash(string password, string stored)
    {
        var parts = stored.Split('.', 2);
        if (parts.Length != 2) return false;

        var salt = Convert.FromBase64String(parts[0]);
        var expected = Convert.FromBase64String(parts[1]);
        var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, KeySize);

        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }
}
