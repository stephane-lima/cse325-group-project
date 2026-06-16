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
    /// <summary>
    /// Validate a user's credentials and return the matching <see cref="ApplicationUser"/> on success.
    /// Returns null when credentials are invalid or the user does not exist.
    /// </summary>
    /// <param name="email">Email address submitted by the user.</param>
    /// <param name="password">Plain-text password to validate.</param>
    Task<ApplicationUser?> ValidateCredentialsAsync(string email, string password);

    /// <summary>
    /// Register a new user with the provided email, display name, and password.
    /// Returns a tuple describing success and an optional error message.
    /// </summary>
    Task<(bool Success, string? Error)> RegisterAsync(string email, string displayName, string password);

    /// <summary>
    /// Build a ClaimsPrincipal for the provided user suitable for cookie authentication.
    /// </summary>
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

    /// <summary>
    /// Asynchronously validate credentials against the database and return the matching user.
    /// Returns null when the user cannot be found or the password is incorrect.
    /// </summary>
    /// <param name="email">Submitted email address.</param>
    /// <param name="password">Submitted plain-text password.</param>
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


    /// <summary>
    /// Register a new user record with a salted password hash.
    /// Returns a tuple indicating whether the registration succeeded and an optional error message.
    /// </summary>
    /// <param name="email">Email for the new account.</param>
    /// <param name="displayName">User visible display name.</param>
    /// <param name="password">Plain-text password (will be hashed before storage).</param>
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


    /// <summary>
    /// Create a ClaimsPrincipal for the provided <see cref="ApplicationUser"/>,
    /// including standard name and email claims. Intended for cookie authentication.
    /// </summary>
    /// <param name="user">The user to build claims for.</param>
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

    /// <summary>
    /// Create a salted PBKDF2 hash from the provided password.
    /// Returns a string in the format: base64(salt) + "." + base64(hash).
    /// </summary>
    /// <remarks>Parameters such as iterations and key size are configured by constants in this class.</remarks>
    private static string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, KeySize);
        return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    /// <summary>
    /// Verify a plain-text password against a stored salted hash.
    /// Uses constant-time comparison to reduce timing attack surface.
    /// </summary>
    /// <param name="password">Plain-text password to verify.</param>
    /// <param name="stored">Stored salted hash value to verify against.</param>
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
