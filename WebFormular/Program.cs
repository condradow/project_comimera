using MudBlazor.Services;
using WebFormular.Components;
using WebFormular.Data;
using WebFormular.Services;

var builder = WebApplication.CreateBuilder(args);

// Add MudBlazor services
builder.Services.AddMudServices();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
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


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
