using Microsoft.EntityFrameworkCore;
using QueryPilot.Web.Components;
using QueryPilot.Web.Services;
using MudBlazor.Services;
using QueryPilot.Web.Data;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var pythonApiBaseUrl = builder.Configuration["PythonApi:BaseUrl"];

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Set ConnectionStrings:DefaultConnection to the PostgreSQL Northwind database connection.");
}

if (!Uri.TryCreate(pythonApiBaseUrl, UriKind.Absolute, out var pythonApiUri))
{
    throw new InvalidOperationException("Set PythonApi:BaseUrl to the absolute URL of the FastAPI service.");
}

if (!pythonApiUri.AbsoluteUri.EndsWith('/'))
{
    pythonApiUri = new Uri($"{pythonApiUri}/");
}

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices();

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddHttpClient<GenerateSqlQueryService>(client =>
{
    client.BaseAddress = pythonApiUri;
    client.Timeout = TimeSpan.FromMinutes(5);
});
builder.Services.AddScoped<PostgresQueryExecutor>();
builder.Services.AddScoped<AnalystQueryService>();
builder.Services.AddScoped<DashboardService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
