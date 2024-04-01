using SaveIt.Server.UI.Api;
using SaveIt.Server.UI.Components;
using SaveIt.Server.UI.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddServices(builder.Configuration);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.AddApplicationEndpoints(); // I add custom EPs here

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery(); // Antiforgery is enabled here

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
