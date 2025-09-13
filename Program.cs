
using Blazored.LocalStorage;
using LoDCompanion.Code.Components;
using System.Text.Json;
using LoDCompanion.Code.BackEnd.Models;
using LoDCompanion.Code.BackEnd.Services.Combat;
using LoDCompanion.Code.BackEnd.Services.Dungeon;
using LoDCompanion.Code.BackEnd.Services.Game;
using LoDCompanion.Code.BackEnd.Services.GameData;
using LoDCompanion.Code.BackEnd.Services.Player;
using LoDCompanion.Code.BackEnd.Services.Utilities;

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

// --- Game State Services (Scoped) ---
// These services hold data specific to a single user's gameplay session.
// Each user gets their own instance of these services.
builder.Services.AddScoped<PartyManagerService>();
builder.Services.AddScoped<CombatManagerService>();
builder.Services.AddScoped<DungeonManagerService>();
builder.Services.AddScoped<QuestService>();
builder.Services.AddScoped<GameStateManagerService>();
builder.Services.AddScoped<IStatePersistenceService, StatePersistenceService>();

// --- Player Action & UI Services (Scoped) ---
// These often interact with or modify the user's game state.
builder.Services.AddScoped<SettlementEventService>();
builder.Services.AddScoped<SettlementService>();
builder.Services.AddSingleton<ActionService>();
builder.Services.AddSingleton<CharacterCreationService>();
builder.Services.AddSingleton<HealingService>();
builder.Services.AddSingleton<InventoryService>();
builder.Services.AddSingleton<LevelupService>();
builder.Services.AddSingleton<PartyRestingService>();
builder.Services.AddSingleton<PowerActivationService>();
builder.Services.AddSingleton<SettlementActionService>();
builder.Services.AddSingleton<SpellCastingService>();
builder.Services.AddSingleton<UIService>();

// --- Game Logic & Rule Services (Singleton) ---
// These services are generally stateless and provide calculations, lookups, or
// orchestrate actions without holding user-specific data themselves.
builder.Services.AddSingleton<AttackService>();
builder.Services.AddSingleton<FacingDirectionService>();
builder.Services.AddSingleton<InitiativeService>();
builder.Services.AddSingleton<MonsterAIService>();
builder.Services.AddSingleton<MonsterSpecialService>();
builder.Services.AddSingleton<DungeonBuilderService>();
builder.Services.AddSingleton<EncounterService>();
builder.Services.AddSingleton<LeverService>();
builder.Services.AddSingleton<LockService>();
builder.Services.AddSingleton<RoomService>();
builder.Services.AddSingleton<SearchService>();
builder.Services.AddSingleton<ThreatService>();
builder.Services.AddSingleton<TrapService>();
builder.Services.AddSingleton<TreasureService>();
builder.Services.AddSingleton<WanderingMonsterService>();
builder.Services.AddSingleton<CampaignService>();
builder.Services.AddSingleton<HexGridService>();
builder.Services.AddSingleton<IdentificationService>();
builder.Services.AddSingleton<PassiveAbilityService>();
builder.Services.AddSingleton<PlacementService>();
builder.Services.AddSingleton<PotionActivationService>();
builder.Services.AddSingleton<QuestSetupService>();
builder.Services.AddSingleton<SpellResolutionService>();

// --- Game Data Services (Singleton) ---
// These services are responsible for loading and providing access to static game data (e.g., from JSON files).
// They should be singletons so the data is loaded only once.
builder.Services.AddSingleton<AlchemyService>();
builder.Services.AddSingleton<EquipmentService>();
builder.Services.AddSingleton<GameDataService>();
builder.Services.AddSingleton<PrayerService>();

// UserRequestService might need to be Scoped if it's tied to a user's specific request lifecycle.
// If it's a general-purpose modal/dialog manager for a single user, Scoped is correct.
builder.Services.AddSingleton<UserRequestService>();
builder.Services.AddSingleton<FloatingTextService>();
builder.Services.AddSingleton<MovementHighlightingService>();

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
