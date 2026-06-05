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

// Fixed Factory Registration to guarantee matching directory scopes
builder.Services.AddDbContextFactory<AppDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("FreshSqliteDatabase");
    
    // If the connection string is a simple relative path like "Data Source=budget.db", 
    // force it to resolve explicitly to the absolute folder root directory path
    if (connectionString != null && connectionString.Contains("budget.db") && !connectionString.Contains(":\\"))
    {
        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        string dbPath = System.IO.Path.Combine(baseDir, "budget.db");
        connectionString = $"Data Source={dbPath}";
    }
    
    options.UseSqlite(connectionString);
});

// --- AUTHENTICATION ENGINE ---
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
    });

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IAccountService, InMemoryAccountService>();

var app = builder.Build();

// --- FORCED SINGLE-POINT DATA ENGINE GENERATION ---
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

// Logout endpoint
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

// Use the IEndpointRouteBuilder interface on 'app' to scan the final compiled routing graph
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