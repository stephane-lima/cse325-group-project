using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using BudgetAndExpenseTracker.Data;
using BudgetAndExpenseTracker.Components;
using BudgetAndExpenseTracker.Services;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. SERVICES REGISTRATION STAGE
// ==========================================
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents(); 

// Register DbContextFactory pointing to the matching "SqliteDatabase" key
builder.Services.AddDbContextFactory<AppDbContext>(options =>
{
    // Match the exact name defined inside your appsettings.json
    var connectionString = builder.Configuration.GetConnectionString("SqliteDatabase");
    
    if (string.IsNullOrEmpty(connectionString))
    {
        throw new InvalidOperationException("Connection string 'SqliteDatabase' was not found in configuration files.");
    }
    
    options.UseSqlite(connectionString);
});

// Authentication Engine
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
    });

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IAccountService, InMemoryAccountService>();

var app = builder.Build();

// Core DB Generation hook using the corrected layout registration
using (var scope = app.Services.CreateScope())
{
    var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
    using var dbContext = factory.CreateDbContext();
    dbContext.Database.EnsureCreated();
}

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

app.MapPost("/Account/Logout", async (HttpContext context) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/login");
});

// ==========================================
// 2. ENDPOINT MAPPING & DIAGNOSTICS STAGE
// ==========================================
app.MapRazorComponents<BudgetAndExpenseTracker.Components.App>()
    .AddInteractiveServerRenderMode();

var endpointSources = ((IEndpointRouteBuilder)app).DataSources;
foreach (var dataSource in endpointSources)
{
    foreach (var endpoint in dataSource.Endpoints)
    {
        if (endpoint is RouteEndpoint routeEndpoint && routeEndpoint.RoutePattern.RawText == "/")
        {
            Console.WriteLine($"[ROUTING DEBUG] Found root endpoint mapped to: {routeEndpoint.DisplayName}");
        }
    }
}

app.Run();