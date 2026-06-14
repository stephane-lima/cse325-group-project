using Microsoft.EntityFrameworkCore;
using BudgetAndExpenseTracker.Models;
using System;

namespace BudgetAndExpenseTracker.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<Goal> Goals { get; set; }
    public DbSet<ApplicationUser> Users { get; set; }

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

        modelBuilder.Entity<Goal>().HasData(
            new Goal { Id = 1, Name = "Emergency Fund", TargetAmount = 3000m, SavedAmount = 650m, TargetDate = DateTime.Today.AddMonths(6), UserId = "1" },
            new Goal { Id = 2, Name = "New Laptop", TargetAmount = 1200m, SavedAmount = 300m, TargetDate = DateTime.Today.AddMonths(3), UserId = "1" }
        );
    }
}