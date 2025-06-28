using LoDCompanion.Components;
using LoDCompanion.Services.CharacterCreation;
using LoDCompanion.Services.GameData;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// --- Configuration Setup ---
builder.Configuration.AddJsonFile("game_data.json", optional: false, reloadOnChange: true);

// --- Dependency Injection Setup ---
builder.Services.AddSingleton(provider => {
    var config = new GameDataConfiguration();
    provider.GetRequiredService<IConfiguration>().Bind(config);
    return config;
});

//register core services.
builder.Services.AddSingleton<GameDataRegistryService>();
builder.Services.AddSingleton<CharacterCreationService>();
builder.Services.AddSingleton<TalentLookupService>();
builder.Services.AddSingleton<SpellLookupService>();
builder.Services.AddSingleton<PrayerLookupService>();

var app = builder.Build();

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
