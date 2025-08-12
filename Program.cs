using Blazored.LocalStorage;
using LoDCompanion.Components;
using System.Text.Json;
using LoDCompanion.BackEnd.Models;
using LoDCompanion.BackEnd.Services.Combat;
using LoDCompanion.BackEnd.Services.Dungeon;
using LoDCompanion.BackEnd.Services.Game;
using LoDCompanion.BackEnd.Services.GameData;
using LoDCompanion.BackEnd.Services.Player;
using LoDCompanion.BackEnd.Services.Utilities;

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
builder.Services.AddScoped<Monster>();
//GameData
builder.Services.AddSingleton<GameDataService>();
builder.Services.AddSingleton<EquipmentService>();
builder.Services.AddSingleton<AlchemyService>();
builder.Services.AddSingleton<WeaponFactory>();
builder.Services.AddSingleton<ArmourFactory>();
//Combat
builder.Services.AddScoped<CombatManagerService>();
builder.Services.AddScoped<InitiativeService>();
builder.Services.AddScoped<SpellCastingService>();
builder.Services.AddScoped<SpellResolutionService>();
builder.Services.AddScoped<MonsterSpecialService>();
builder.Services.AddScoped<AttackService>();
builder.Services.AddSingleton<FacingDirectionService>();
//Dungeon
builder.Services.AddScoped<DungeonBuilderService>();
builder.Services.AddScoped<DungeonManagerService>();
builder.Services.AddScoped<WanderingMonsterService>();
builder.Services.AddScoped<RoomFactoryService>();
builder.Services.AddScoped<RoomService>();
builder.Services.AddScoped<QuestService>();
builder.Services.AddScoped<QuestSetupService>();
builder.Services.AddSingleton<EncounterService>();
builder.Services.AddSingleton<LeverService>();
builder.Services.AddSingleton<LockService>();
builder.Services.AddSingleton<SearchService>();
builder.Services.AddSingleton<TreasureService>();
builder.Services.AddSingleton<ThreatService>();
builder.Services.AddSingleton<TrapService>();
//Player
builder.Services.AddScoped<CharacterCreationService>();
builder.Services.AddScoped<PartyManagerService>();
builder.Services.AddScoped<InventoryService>();
builder.Services.AddScoped<ActionService>();
builder.Services.AddScoped<PartyRestingService>();
builder.Services.AddScoped<MovementHighlightingService>();
builder.Services.AddSingleton<PassiveAbilityService>();
builder.Services.AddSingleton<HealingService>();
builder.Services.AddSingleton<IdentificationService>();
//Game
builder.Services.AddScoped<IStatePersistenceService, StatePersistenceService>();
builder.Services.AddScoped<GameStateManagerService>();
builder.Services.AddScoped<MonsterAIService>();
builder.Services.AddScoped<WorldStateService>();
builder.Services.AddScoped<PlacementService>();
builder.Services.AddSingleton<FloatingTextService>();
builder.Services.AddSingleton<UserRequestService>();
builder.Services.AddSingleton<UIService>();

//Package
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddBlazorContextMenu();

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
