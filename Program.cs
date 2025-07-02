using LoDCompanion.Components;
using LoDCompanion.Services.CharacterCreation;
using LoDCompanion.Services.GameData;
using LoDCompanion.Services.Player;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.Configure<JsonSerializerOptions>(options =>
{
    options.PropertyNameCaseInsensitive = true;
    options.WriteIndented = true;
});

//register core services.
builder.Services.AddSingleton<GameDataService>();
builder.Services.AddSingleton<CharacterCreationService>();
builder.Services.AddSingleton<PartyManagerService>();

var app = builder.Build();

app.MapGet("/config", (IConfiguration config) => config.AsEnumerable());

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
