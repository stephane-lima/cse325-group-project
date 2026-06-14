using System.Collections.Concurrent;
using System.Security.Claims;
using System.Security.Cryptography;
using BudgetAndExpenseTracker.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using BudgetAndExpenseTracker.Data;

namespace BudgetAndExpenseTracker.Services;

/// <summary>
/// Data record representing an application user account state profile.
/// </summary>
// public sealed record AppUser(string Id, string Email, string DisplayName, string PasswordHash);

/// <summary>
/// Core contract interface governing access management, registration, and claims mapping.
/// </summary>
public interface IAccountService
{
    // AppUser? ValidateCredentials(string email, string password);
    // (bool Success, string? Error) Register(string email, string displayName, string password);
    // ClaimsPrincipal BuildPrincipal(AppUser user);
    Task<ApplicationUser?> ValidateCredentialsAsync(string email, string password);
    Task<(bool Success, string? Error)> RegisterAsync(string email, string displayName, string password);
    ClaimsPrincipal BuildPrincipal(ApplicationUser user);
}

/// <summary>
/// Thread-safe in-memory simulation engine for credential checking and user profile processing.
/// </summary>
public sealed class AccountService : IAccountService
{
    // private readonly ConcurrentDictionary<string, AppUser> _users = new();
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private const int SaltSize = 16;          
    private const int KeySize = 32;           
    private const int Iterations = 100_000;   
    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

    public AccountService(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    // public InMemoryAccountService()
    // {
    //     Register("demo@budgettracker.com", "Demo User", "Password123!");
    // }

    // public AppUser? ValidateCredentials(string email, string password)
    // {
    //     if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
    //         return null;

    //     if (!_users.TryGetValue(email.Trim().ToLowerInvariant(), out var user))
    //         return null;

    //     return VerifyHash(password, user.PasswordHash) ? user : null;
    // }

    public async Task<ApplicationUser?> ValidateCredentialsAsync(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return null;

        using var db = await _dbFactory.CreateDbContextAsync();

        var user = await db.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.Trim().ToLower());

        if (user == null)
        {
            return null;
        }

        return VerifyHash(password, user.PasswordHash) ? user : null;
    }

    // public (bool Success, string? Error) Register(string email, string displayName, string password)
    // {
    //     if (string.IsNullOrWhiteSpace(email))
    //         return (false, "Email is required.");
    //     if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
    //         return (false, "Password must be at least 6 characters.");

    //     var key = email.Trim().ToLowerInvariant();
    //     var user = new AppUser(
    //         Id: Guid.NewGuid().ToString(),
    //         Email: email.Trim(),
    //         DisplayName: string.IsNullOrWhiteSpace(displayName) ? email.Trim() : displayName.Trim(),
    //         PasswordHash: HashPassword(password));

    //     return _users.TryAdd(key, user)
    //         ? (true, null)
    //         : (false, "An account with that email already exists.");
    // }

    public async Task<(bool Success, string? Error)> RegisterAsync(string email, string displayName, string password)
    {
        if (string.IsNullOrWhiteSpace(email))
            return (false, "Email is required.");
        if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
            return (false, "Password must be at least 6 characters.");

        using var db = await _dbFactory.CreateDbContextAsync();

        var existingUser = await db.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.Trim().ToLower());

        if (existingUser != null)
        {
            return (false, "An account with that email already exists.");
        }

        var user = new ApplicationUser
        {
            Email = email.Trim(),
            DisplayName = string.IsNullOrWhiteSpace(displayName) ? email.Trim() : displayName.Trim(),
            PasswordHash = HashPassword(password)
        };

        db.Users.Add(user);

        await db.SaveChangesAsync();

        return (true, null);
    }

    // public ClaimsPrincipal BuildPrincipal(AppUser user)
    // {
    //     var claims = new List<Claim>
    //     {
    //         new(ClaimTypes.NameIdentifier, user.Id),
    //         new(ClaimTypes.Name, user.DisplayName),
    //         new(ClaimTypes.Email, user.Email),
    //     };
        
    //     var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    //     return new ClaimsPrincipal(identity);
    // }

    public ClaimsPrincipal BuildPrincipal(ApplicationUser user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
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
