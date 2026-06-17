using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BudgetAndExpenseTracker.Models;

/// <summary>
/// Represents a single financial transaction in the ledger.
/// Amounts are stored as formatted strings to match existing seed data
/// Use <see cref="NumericAmount"/> to work with decimals.
/// </summary>
public class Transaction
{
    /// <summary>
    /// Primary key for the transaction.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Short description or title of the transaction.
    /// </summary>
    [Required(ErrorMessage = "Please provide a description or title.")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Transaction type, e.g. "income" or "expense".
    /// </summary>
    [Required]
    public string Type { get; set; } = "expense";

    /// <summary>
    /// Category grouping, e.g. "Food", "Rent".
    /// </summary>
    [Required]
    public string Category { get; set; } = "Food";

    /// <summary>
    /// Formatted amount string (e.g. "12.34"). Prefer <see cref="NumericAmount"/> in code.
    /// </summary>
    [Required(ErrorMessage = "The Amount field is required.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Enter a positive target amount")]
    [Column(TypeName = "decimal(18,2)")]
    public string Amount { get; set; } = "0.00";

    /// <summary>
    /// Date stored as yyyy-MM-dd string for simple binding with the UI.
    /// </summary>
    [Required(ErrorMessage="Please enter a valid date.")]
    public string Date { get; set; } = DateTime.Today.ToString("yyyy-MM-dd");

    /// <summary>
    /// Owning user's id.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Numeric representation of <see cref="Amount"/> for calculations.
    /// Not mapped to the database.
    /// </summary>
    [NotMapped]
    public decimal NumericAmount
    {
        get => decimal.TryParse(Amount, out var val) ? val : 0.00m;
        set => Amount = value.ToString("F2");
    }
}
