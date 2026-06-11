using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using BudgetAndExpenseTracker.Data;
using BudgetAndExpenseTracker.Models;
using BudgetAndExpenseTracker.Components;
using BudgetAndExpenseTracker.Services;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. SERVICES REGISTRATION STAGE
// ==========================================
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("SqliteDatabase")));

builder.Services.AddScoped<IAccountService, InMemoryAccountService>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
    });

builder.Services.AddCascadingAuthenticationState();

var app = builder.Build();

// ==========================================
// 2. DATABASE INITIALIZATION STAGE
// ==========================================
using (var scope = app.Services.CreateScope())
{
    try
    {
        var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
        using var dbContext = factory.CreateDbContext();
        
        Console.WriteLine("[DB INITIALIZATION] Verifying SQLite structures...");
        dbContext.Database.EnsureCreated();
        Console.WriteLine("[DB INITIALIZATION] Database verification successful!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[DB FATAL ERROR] Initialization failed: {ex.Message}");
    }
}

// ==========================================
// 3. MIDDLEWARE PIPELINE STAGE
// ==========================================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

// Logout endpoint
app.MapPost("/Account/Logout", async (HttpContext context) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/login");
});

// ==========================================
// 4. ROUTING & DIAGNOSTICS STAGE
// ==========================================
app.MapRazorComponents<BudgetAndExpenseTracker.Components.App>()
    .AddInteractiveServerRenderMode();

// Debug route logging
var endpointSources = ((IEndpointRouteBuilder)app).DataSources;
foreach (var dataSource in endpointSources)
{
    foreach (var endpoint in dataSource.Endpoints)
    {
        if (endpoint is RouteEndpoint routeEndpoint && routeEndpoint.RoutePattern.RawText == "/")
        {
            Console.WriteLine($"[ROUTING DEBUG] Root endpoint: {routeEndpoint.DisplayName}");
        }
    }
}

app.Run();