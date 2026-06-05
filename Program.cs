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

// Database Factory Registration
builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("FreshSqliteDatabase")));

// --- FIXED: REGISTER THE IN-MEMORY ACCOUNT SERVICE ---
builder.Services.AddScoped<IAccountService, InMemoryAccountService>();

// --- FIXED: ADDED AUTHENTICATION SERVICES ENGINE ---
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
    });

builder.Services.AddCascadingAuthenticationState();

var app = builder.Build();

// Async database generation migration to prevent Blazor dependency routing race conditions
await System.Threading.Tasks.Task.Run(async () =>
{
    using var scope = app.Services.CreateScope();
    var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
    using var dbContext = await factory.CreateDbContextAsync();
    await dbContext.Database.EnsureCreatedAsync();
});

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

// Order matters: authentication must run before authorization.
app.UseAuthentication();
app.UseAuthorization();

// Logout endpoint: clears the auth cookie and returns to the login page.
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