using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using MudBlazor.Services;
using System.Security.Claims;
using WebFormular.Components;
using WebFormular.Data;
using WebFormular.Services;

var builder = WebApplication.CreateBuilder(args);

// Add MudBlazor services
builder.Services.AddMudServices();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add authentication services
builder.Services
    .AddAuthentication(
        CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "CoMiMeRa.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy =
            CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;

        options.LoginPath = "/login";
        options.AccessDeniedPath = "/access-denied";

        options.ExpireTimeSpan = TimeSpan.FromHours(2);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

builder.Services.AddCascadingAuthenticationState();

builder.Services.AddAntiforgery();

//var cs = builder.Configuration.GetConnectionString("WebFormular")!;
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection String 'DefaultConnection' fehlt in appsettings.json.");

builder.Services.AddScoped(_ => new UserRepository(connectionString));
builder.Services.AddScoped(_ => new DocumentRepository(connectionString));
builder.Services.AddScoped(_ => new TemplateRepository(connectionString));

builder.Services.AddScoped<DatabaseContext>(sp =>
{
    var connectionString =
        builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("ConnectionString fehlt.");

    return new DatabaseContext(connectionString);
});

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

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

// Add middleware to handle antiforgery token validation for POST requests
app.MapGet("/auth/antiforgery",
    (HttpContext context, IAntiforgery antiforgery) =>
    {
        context.Response.Headers.CacheControl = "no-store";

        var tokens = antiforgery.GetAndStoreTokens(context);

        return Results.Ok(new
        {
            token = tokens.RequestToken
        });
    }).AllowAnonymous();


// Add middleware to handle login requests
// Login endpoint
app.MapPost("/auth/login", async (
    HttpContext context,
    IAntiforgery antiforgery,
    UserLoginRequest request,
    UserRepository userRepository) =>
{
    try
    {
        await antiforgery.ValidateRequestAsync(context);
    }
    catch (AntiforgeryValidationException)
    {
        return Results.BadRequest();
    }

    var user = await userRepository.ValidateCredentialsAsync(
        request.Username,
        request.Password);

    if (user is null)
    {
        return Results.Problem(
            statusCode: StatusCodes.Status401Unauthorized,
            title: "Ungültige Anmeldedaten");
    }

    var claims = new List<Claim>
    {
        new(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new(ClaimTypes.Name, user.Username)
    };

    if (!string.IsNullOrWhiteSpace(user.Email))
    {
        claims.Add(new Claim(ClaimTypes.Email, user.Email));
    }

    foreach (var role in user.Roles)
    {
        claims.Add(new Claim(ClaimTypes.Role, role));
    }

    var identity = new ClaimsIdentity(
        claims,
        CookieAuthenticationDefaults.AuthenticationScheme);

    await context.SignInAsync(
        CookieAuthenticationDefaults.AuthenticationScheme,
        new ClaimsPrincipal(identity),
        new AuthenticationProperties
        {
            IsPersistent = false
        });

    return Results.Ok();
}).AllowAnonymous();

// Logout endpoint
app.MapPost("/auth/logout", async (
    HttpContext context,
    IAntiforgery antiforgery) =>
{
    try
    {
        await antiforgery.ValidateRequestAsync(context);
    }
    catch (AntiforgeryValidationException)
    {
        return Results.BadRequest();
    }

    await context.SignOutAsync(
        CookieAuthenticationDefaults.AuthenticationScheme);

    return Results.Ok();
}).RequireAuthorization();

app.MapStaticAssets().AllowAnonymous();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

// Record to represent the login request payload for user authentication instead of email authentication
public record UserLoginRequest(string Username, string Password);

