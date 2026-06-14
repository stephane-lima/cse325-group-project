using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BudgetAndExpenseTracker.Models;

public class Goal
{
    public int Id { get; set; }

    [Required(ErrorMessage = "The Goal Name field is required.")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Enter a positive target amount")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal TargetAmount { get; set; } = 100m;

    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Enter a positive target amount")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal SavedAmount { get; set; } = 0m;

    [Required]
    public DateTime TargetDate { get; set; } = DateTime.Today.AddMonths(1);

    public DateTime? CompletedDate { get; set; }

    // [NotMapped]
    // public string Status => SavedAmount >= TargetAmount ? "Completed" : "On Track";

    [NotMapped]
    public bool IsCompleted => TargetAmount > 0 && SavedAmount >= TargetAmount;

    public string UserId { get; set; } = string.Empty;
}
