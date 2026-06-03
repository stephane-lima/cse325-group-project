using BudgetAndExpenseTracker.Components;
using BudgetAndExpenseTracker.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// --- Authentication & authorization ----------------------------------------
// Cascading auth state lets components (e.g. AuthorizeView in the nav) read the
// signed-in user, and AddAuthorization enables the [Authorize] attribute.
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthorization();

// Bridges the signed-in user from the cookie (HttpContext.User) into Blazor's
// authentication state so AuthorizeView/[Authorize] work in components.
builder.Services.AddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>();

// Cookie-based authentication. The login page writes this cookie on success and
// unauthorized requests are redirected to /login.
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/login";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

// Account store. Registered as a singleton (in-memory) for now; swap this single
// line for an EF Core Identity-backed implementation once the database is ready.
builder.Services.AddSingleton<IAccountService, InMemoryAccountService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

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

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
