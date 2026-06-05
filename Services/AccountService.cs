using System.Collections.Concurrent;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace BudgetAndExpenseTracker.Services;

/// <summary>
/// Represents a single application user.
/// NOTE: This is a lightweight stand-in. When the "User Authentication" / "Database Setup"
/// cards are completed, this can be replaced by an EF Core Identity <c>ApplicationUser</c>
/// without changing the Login/Register pages, because those pages only depend on
/// <see cref="IAccountService"/> below.
/// </summary>
public sealed record AppUser(string Id, string Email, string DisplayName, string PasswordHash);

/// <summary>
/// Abstraction the UI depends on. Keeping the login/register pages bound to this interface
/// (instead of a concrete store) means the data layer can change later with zero UI changes.
/// </summary>
public interface IAccountService
{
    /// <summary>Returns the user when email + password are valid; otherwise <c>null</c>.</summary>
    AppUser? ValidateCredentials(string email, string password);

    /// <summary>Creates a new account. Returns (success, errorMessage).</summary>
    (bool Success, string? Error) Register(string email, string displayName, string password);

    /// <summary>Builds the cookie identity (claims) for a signed-in user.</summary>
    ClaimsPrincipal BuildPrincipal(AppUser user);
}

/// <summary>
/// In-memory implementation of <see cref="IAccountService"/>.
/// Passwords are stored as salted PBKDF2 hashes (never plain text). Registered as a singleton
/// so accounts created during a session persist until the app restarts.
/// </summary>
public sealed class InMemoryAccountService : IAccountService
{
    // Thread-safe store keyed by lower-cased email.
    private readonly ConcurrentDictionary<string, AppUser> _users = new();

    // PBKDF2 tuning parameters.
    private const int SaltSize = 16;          // 128-bit salt
    private const int KeySize = 32;           // 256-bit derived key
    private const int Iterations = 100_000;   // work factor
    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

    public InMemoryAccountService()
    {
        // Seed a demo account so the login page can be demonstrated immediately
        // (e.g., in the group video). Safe to remove once real registration is used.
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

        // TryAdd fails if the email is already registered.
        return _users.TryAdd(key, user)
            ? (true, null)
            : (false, "An account with that email already exists.");
    }

    public ClaimsPrincipal BuildPrincipal(AppUser user)
    {
        // Claims become the contents of the auth cookie and are readable via AuthorizeView.
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.DisplayName),
            new(ClaimTypes.Email, user.Email),
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        return new ClaimsPrincipal(identity);
    }

    // --- PBKDF2 helpers -----------------------------------------------------

    private static string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, KeySize);
        // Store as "salt.hash" (both Base64) so verification needs no extra storage.
        return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    private static bool VerifyHash(string password, string stored)
    {
        var parts = stored.Split('.', 2);
        if (parts.Length != 2) return false;

        var salt = Convert.FromBase64String(parts[0]);
        var expected = Convert.FromBase64String(parts[1]);
        var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, KeySize);

        // Constant-time comparison to avoid timing attacks.
        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }
}
