using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BudgetAndExpenseTracker.Models;

public class Transaction
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Please provide a description or title.")]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Type { get; set; } = "expense";

    [Required]
    public string Category { get; set; } = "Food";

    [Required]
    public string Amount { get; set; } = "0.00";

    [Required]
    public string Date { get; set; } = DateTime.Today.ToString("yyyy-MM-dd");

    public string UserId { get; set; } = "1";

    [NotMapped]
    public decimal NumericAmount
    {
        get => decimal.TryParse(Amount, out var val) ? val : 0.00m;
        set => Amount = value.ToString("F2");
    }
}
