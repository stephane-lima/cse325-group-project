using Microsoft.EntityFrameworkCore;
using BudgetAndExpenseTracker.Models;
using System;

namespace BudgetAndExpenseTracker.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Transaction> Transactions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // SEED DATA: Automatically insert a starting ledger account row
        modelBuilder.Entity<Transaction>().HasData(
            new Transaction
            {
                Id = 1, // Seeded data must have an explicit primary key assigned
                UserId = "1",
                Title = "Opening Account Baseline Balance",
                Type = "income",
                Category = "Opening Balance",
                Amount = "10000.00",
                Date = DateTime.Today.ToString("yyyy-MM-dd")
            }
        );
    }
}