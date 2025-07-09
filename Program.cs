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
using LoDCompanion.Services.Combat;

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
//State
builder.Services.AddScoped<CharacterCreationState>();
builder.Services.AddScoped<DungeonState>();
builder.Services.AddScoped<GameState>();
builder.Services.AddScoped<Room>();
//GameData
builder.Services.AddSingleton<GameDataService>();
builder.Services.AddSingleton<EquipmentService>();
builder.Services.AddSingleton<AlchemyService>();
//Combat
builder.Services.AddScoped<CombatManagerService>();
builder.Services.AddScoped<InitiativeService>();
builder.Services.AddSingleton<DefenseService>();
builder.Services.AddSingleton<MonsterCombatService>();
builder.Services.AddSingleton<HeroCombatService>();
builder.Services.AddSingleton<StatusEffectService>();
builder.Services.AddSingleton<MonsterSpecialService>();
//Dungeon
builder.Services.AddScoped<DungeonBuilderService>();
builder.Services.AddScoped<DungeonManagerService>();
builder.Services.AddScoped<WanderingMonsterService>();
builder.Services.AddScoped<GridService>();
builder.Services.AddScoped<QuestService>();
builder.Services.AddScoped<QuestEncounterService>();
builder.Services.AddSingleton<RoomService>();
builder.Services.AddSingleton<EncounterService>();
builder.Services.AddSingleton<LeverService>();
builder.Services.AddSingleton<LockService>();
builder.Services.AddSingleton<RoomFactoryService>();
builder.Services.AddSingleton<SearchService>();
builder.Services.AddSingleton<ThreatService>();
builder.Services.AddSingleton<TrapService>();
builder.Services.AddSingleton<TreasureService>();
//Party
builder.Services.AddScoped<CharacterCreationService>();
builder.Services.AddScoped<PartyManagerService>();
builder.Services.AddScoped<InventoryService>();
builder.Services.AddScoped<PlayerActionService>();
builder.Services.AddSingleton<PartyRestingService>();
builder.Services.AddSingleton<HealingService>();
builder.Services.AddSingleton<IdentificationService>();
//Game
builder.Services.AddScoped<IStatePersistenceService, StatePersistenceService>();
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddScoped<GameStateManagerService>();
builder.Services.AddSingleton<MonsterAIService>();

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
