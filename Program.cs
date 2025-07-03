using Blazored.LocalStorage;
using LoDCompanion.Components;
using LoDCompanion.Models;
using LoDCompanion.Models.Character;
using LoDCompanion.Models.Dungeon;
using LoDCompanion.Services.CharacterCreation;
using LoDCompanion.Services.Dungeon;
using LoDCompanion.Services.GameData;
using LoDCompanion.Services.Player;
using LoDCompanion.Services.Game;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddServerSideBlazor()
    .AddHubOptions(options =>
    {
        options.MaximumReceiveMessageSize = 102400; // 100 KB
    });
builder.Services.Configure<JsonSerializerOptions>(options =>
{
    options.PropertyNameCaseInsensitive = true;
    options.WriteIndented = true;
});

//register core services.
builder.Services.AddScoped<IStatePersistenceService, StatePersistenceService>();
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddScoped<GameDataService>();
builder.Services.AddScoped<EquipmentService>();
builder.Services.AddScoped<CharacterCreationService>();
builder.Services.AddSingleton<CharacterCreationState>();
builder.Services.AddScoped<PartyManagerService>();
builder.Services.AddScoped<DungeonManagerService>();
builder.Services.AddSingleton<DungeonState>();
builder.Services.AddScoped<RoomFactoryService>();
builder.Services.AddScoped<RoomService>();
builder.Services.AddScoped<EncounterService>();
builder.Services.AddScoped<QuestEncounterService>();
builder.Services.AddScoped<WanderingMonsterService>();
builder.Services.AddScoped<GameStateManagerService>();
builder.Services.AddSingleton<GameState>();
builder.Services.AddScoped<DungeonBuilderService>();
builder.Services.AddScoped<ThreatService>();

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
