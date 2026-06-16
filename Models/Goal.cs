using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BudgetAndExpenseTracker.Models;

/// <summary>
/// Represents a financial goal for a user (e.g., emergency fund, purchase savings).
/// </summary>
public class Goal
{
    /// <summary>
    /// Primary key for the goal record.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Name of the goal.
    /// </summary>
    [Required(ErrorMessage = "The Goal Name field is required.")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Target monetary amount to reach for this goal.
    /// Stored as a decimal column in the database.
    /// </summary>
    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Enter a positive target amount")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal TargetAmount { get; set; } = 100m;

    /// <summary>
    /// The amount the user has saved so far toward the goal.
    /// </summary>
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Enter a positive target amount")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal SavedAmount { get; set; } = 0.01m;

    /// <summary>
    /// The date by which the user expects to reach the target amount.
    /// </summary>
    [Required]
    public DateTime TargetDate { get; set; } = DateTime.Today.AddMonths(1);

    /// <summary>
    /// Optional completion timestamp set when the goal is finished.
    /// </summary>
    public DateTime? CompletedDate { get; set; }

    /// <summary>
    /// Convenience computed property indicating if the goal is completed.
    /// Not mapped to the database.
    /// </summary>
    [NotMapped]
    public bool IsCompleted => TargetAmount > 0 && SavedAmount >= TargetAmount;

    /// <summary>
    /// The owning user's id (string to match seeded/demo values).
    /// </summary>
    public string UserId { get; set; } = string.Empty;
}
