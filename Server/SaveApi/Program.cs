using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using SaveApi.Data;
using SaveApi.Models;
using SaveApi.Repositories;
using SaveApi.Services;

var builder = WebApplication.CreateBuilder(args);

const string issuer = "TurtleTrashFighter";
const string audience = "TurtleTrashFighterClient";
var jwtKey = builder.Configuration["Jwt:Key"] ?? "troque-essa-chave-super-segura-com-32-caracteres";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.FromSeconds(30),
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddSingleton<MySqlDb>();
builder.Services.AddScoped<IUserRepository, MySqlUserRepository>();
builder.Services.AddScoped<IConfigRepository, MySqlConfigRepository>();
builder.Services.AddScoped<ISaveRepository, MySqlSaveRepository>();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Ok(new
{
    service = "SaveApi",
    status = "online",
    endpoints = new[]
    {
        "/health",
        "/auth/register",
        "/auth/login",
        "/config",
        "/saves",
    }
}));

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

var authGroup = app.MapGroup("/auth");
authGroup.MapPost("/register", RegisterAsync);
authGroup.MapPost("/login", LoginAsync);

var configGroup = app.MapGroup("/config").RequireAuthorization();
configGroup.MapGet("", GetConfigAsync);
configGroup.MapPut("", UpsertConfigAsync);

var saveGroup = app.MapGroup("/saves").RequireAuthorization();
saveGroup.MapGet("", GetAllSavesAsync);
saveGroup.MapGet("/{slotIndex:int}", GetSaveAsync);
saveGroup.MapPut("", UpsertSaveAsync);
saveGroup.MapDelete("/{slotIndex:int}", DeleteSaveAsync);

app.Run();

async Task<IResult> RegisterAsync(
    RegisterRequest request,
    IUserRepository users,
    CancellationToken cancellationToken)
{
    if (string.IsNullOrWhiteSpace(request.Login) || string.IsNullOrWhiteSpace(request.Password))
        return Results.BadRequest(new ApiError("Login e senha sao obrigatorios."));

    if (string.IsNullOrWhiteSpace(request.Nome))
        return Results.BadRequest(new ApiError("Nome e obrigatorio."));

    if (request.Password.Length < 6)
        return Results.BadRequest(new ApiError("A senha precisa ter no minimo 6 caracteres."));

    var existing = await users.GetByLoginAsync(request.Login, cancellationToken);
    if (existing != null)
        return Results.Conflict(new ApiError("Usuario ja existe."));

    var (hash, salt) = PasswordHasher.CreateHash(request.Password);
    var created = await users.CreateAsync(request.Login, hash, salt, request.Nome, cancellationToken);
    var token = JwtTokenGenerator.Generate(created.Login, created.Nome, jwtKey, issuer, audience);

    return Results.Ok(new AuthResponse(token, created.Login, created.Nome));
}

async Task<IResult> LoginAsync(
    LoginRequest request,
    IUserRepository users,
    CancellationToken cancellationToken)
{
    if (string.IsNullOrWhiteSpace(request.Login) || string.IsNullOrWhiteSpace(request.Password))
        return Results.BadRequest(new ApiError("Login e senha sao obrigatorios."));

    var user = await users.GetByLoginAsync(request.Login, cancellationToken);
    if (user == null)
        return Results.Unauthorized();

    if (!PasswordHasher.Verify(request.Password, user.PasswordHash, user.PasswordSalt))
        return Results.Unauthorized();

    var token = JwtTokenGenerator.Generate(user.Login, user.Nome, jwtKey, issuer, audience);
    return Results.Ok(new AuthResponse(token, user.Login, user.Nome));
}

async Task<IResult> GetConfigAsync(
    ClaimsPrincipal principal,
    IUserRepository users,
    IConfigRepository configs,
    CancellationToken cancellationToken)
{
    var user = await GetCurrentUserAsync(principal, users, cancellationToken);
    if (user == null)
        return Results.Unauthorized();

    var config = await configs.GetAsync(user.Id, cancellationToken);
    if (config == null)
        return Results.Ok(new UserConfigResponse
        {
            VolumeMaster = 1f,
            VolumeMusic = 1f,
            VolumeSfx = 1f,
            Keybinds = System.Text.Json.JsonDocument.Parse("{}").RootElement.Clone(),
        });

    return Results.Ok(new UserConfigResponse
    {
        VolumeMaster = config.VolumeMaster,
        VolumeMusic = config.VolumeMusic,
        VolumeSfx = config.VolumeSfx,
        Keybinds = ParseJsonElement(config.KeybindsJson),
    });
}

async Task<IResult> UpsertConfigAsync(
    ClaimsPrincipal principal,
    UserConfigUpsertRequest request,
    IUserRepository users,
    IConfigRepository configs,
    CancellationToken cancellationToken)
{
    var user = await GetCurrentUserAsync(principal, users, cancellationToken);
    if (user == null)
        return Results.Unauthorized();

    var saved = await configs.UpsertAsync(user.Id, request, cancellationToken);
    return Results.Ok(new UserConfigResponse
    {
        VolumeMaster = saved.VolumeMaster,
        VolumeMusic = saved.VolumeMusic,
        VolumeSfx = saved.VolumeSfx,
        Keybinds = ParseJsonElement(saved.KeybindsJson),
    });
}

async Task<IResult> GetAllSavesAsync(
    ClaimsPrincipal principal,
    IUserRepository users,
    ISaveRepository saves,
    CancellationToken cancellationToken)
{
    var user = await GetCurrentUserAsync(principal, users, cancellationToken);
    if (user == null)
        return Results.Unauthorized();

    var items = await saves.GetAllAsync(user.Id, cancellationToken);
    return Results.Ok(items);
}

async Task<IResult> GetSaveAsync(
    ClaimsPrincipal principal,
    int slotIndex,
    IUserRepository users,
    ISaveRepository saves,
    CancellationToken cancellationToken)
{
    var user = await GetCurrentUserAsync(principal, users, cancellationToken);
    if (user == null)
        return Results.Unauthorized();

    var save = await saves.GetAsync(user.Id, slotIndex, cancellationToken);
    return save == null ? Results.NotFound(new ApiError("Save nao encontrado.")) : Results.Ok(save);
}

async Task<IResult> UpsertSaveAsync(
    ClaimsPrincipal principal,
    SaveUpsertRequest request,
    IUserRepository users,
    ISaveRepository saves,
    CancellationToken cancellationToken)
{
    var user = await GetCurrentUserAsync(principal, users, cancellationToken);
    if (user == null)
        return Results.Unauthorized();

    var saved = await saves.UpsertAsync(user.Id, request, cancellationToken);
    return Results.Ok(saved);
}

async Task<IResult> DeleteSaveAsync(
    ClaimsPrincipal principal,
    int slotIndex,
    IUserRepository users,
    ISaveRepository saves,
    CancellationToken cancellationToken)
{
    var user = await GetCurrentUserAsync(principal, users, cancellationToken);
    if (user == null)
        return Results.Unauthorized();

    var deleted = await saves.DeleteAsync(user.Id, slotIndex, cancellationToken);
    return deleted ? Results.NoContent() : Results.NotFound(new ApiError("Save nao encontrado."));
}

static async Task<UserAccount?> GetCurrentUserAsync(
    ClaimsPrincipal principal,
    IUserRepository users,
    CancellationToken cancellationToken)
{
    var login = principal.Identity?.Name;
    if (string.IsNullOrWhiteSpace(login))
        return null;

    return await users.GetByLoginAsync(login, cancellationToken);
}

static System.Text.Json.JsonElement ParseJsonElement(string json)
{
    using var document = System.Text.Json.JsonDocument.Parse(string.IsNullOrWhiteSpace(json) ? "{}" : json);
    return document.RootElement.Clone();
}
