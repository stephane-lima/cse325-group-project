using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BudgetAndExpenseTracker.Models;

/// <summary>
/// Represents a user account in the application.
/// Stored fields include an identifier(Id), email, human-friendly display name,
/// and a salted password hash.
/// </summary>
public class ApplicationUser
{
    /// <summary>
    /// Primary key for the user account (autoincremented integer).
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// User email address. Used as the unique login identifier.
    /// </summary>
    [Required]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable display name shown in the UI.
    /// </summary>
    [Required]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Stored salted password hash created with PBKDF2.
    /// Format: base64(salt) + "." + base64(hash).
    /// </summary>
    [Required]
    public string PasswordHash { get; set; } = string.Empty;
}