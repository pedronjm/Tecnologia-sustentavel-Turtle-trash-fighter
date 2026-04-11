using System.Collections.Concurrent;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

const string issuer = "TurtleTrashFighter";
const string audience = "TurtleTrashFighterClient";

// Troque em produção. Pode mover para appsettings/environment variables.
var jwtKey = builder.Configuration["Jwt:Key"] ?? "troque-essa-chave-super-segura-com-32-caracteres";

builder
    .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.FromSeconds(30),
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet(
    "/",
    () =>
        Results.Ok(
            new
            {
                service = "SaveApi",
                status = "online",
                endpoints = new[] { "/auth/register", "/auth/login", "/save" },
            }
        )
);

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

var users = new ConcurrentDictionary<string, UserRecord>(StringComparer.OrdinalIgnoreCase);
var saves = new ConcurrentDictionary<string, SavePayload>(StringComparer.OrdinalIgnoreCase);

app.MapPost(
    "/auth/register",
    (RegisterRequest req) =>
    {
        if (string.IsNullOrWhiteSpace(req.username) || string.IsNullOrWhiteSpace(req.password))
            return Results.BadRequest(new ApiError("Usuario e senha sao obrigatorios."));

        if (req.password.Length < 6)
            return Results.BadRequest(new ApiError("A senha precisa ter no minimo 6 caracteres."));

        if (users.ContainsKey(req.username))
            return Results.Conflict(new ApiError("Usuario ja existe."));

        var passwordData = PasswordHasher.CreateHash(req.password);
        var user = new UserRecord(req.username, passwordData.hash, passwordData.salt);

        if (!users.TryAdd(req.username, user))
            return Results.Conflict(new ApiError("Nao foi possivel criar o usuario."));

        var token = JwtTokenGenerator.Generate(req.username, jwtKey, issuer, audience);
        return Results.Ok(new AuthResponse(token, req.username));
    }
);

app.MapPost(
    "/auth/login",
    (LoginRequest req) =>
    {
        if (string.IsNullOrWhiteSpace(req.username) || string.IsNullOrWhiteSpace(req.password))
            return Results.BadRequest(new ApiError("Usuario e senha sao obrigatorios."));

        if (!users.TryGetValue(req.username, out var user))
            return Results.Unauthorized();

        var ok = PasswordHasher.Verify(req.password, user.PasswordHash, user.PasswordSalt);
        if (!ok)
            return Results.Unauthorized();

        var token = JwtTokenGenerator.Generate(req.username, jwtKey, issuer, audience);
        return Results.Ok(new AuthResponse(token, req.username));
    }
);

app.MapGet(
    "/save",
    [Authorize]
    (ClaimsPrincipal principal) =>
    {
        var username = principal.Identity?.Name;
        if (string.IsNullOrWhiteSpace(username))
            return Results.Unauthorized();

        if (saves.TryGetValue(username, out var save))
            return Results.Ok(save);

        return Results.NotFound(new ApiError("Nenhum save encontrado para este usuario."));
    }
);

app.MapPut(
    "/save",
    [Authorize]
    (ClaimsPrincipal principal, SavePayload payload) =>
    {
        var username = principal.Identity?.Name;
        if (string.IsNullOrWhiteSpace(username))
            return Results.Unauthorized();

        payload.username = username;
        payload.lastUpdatedUtc = DateTime.UtcNow;

        saves[username] = payload;
        return Results.Ok(payload);
    }
);

app.Run();

record RegisterRequest(string username, string password);

record LoginRequest(string username, string password);

record AuthResponse(string accessToken, string username);

record ApiError(string message);

record UserRecord(string Username, string PasswordHash, string PasswordSalt);

public class SavePayload
{
    public string username { get; set; } = string.Empty;
    public string sceneName { get; set; } = string.Empty;
    public List<string> collectedIds { get; set; } = new();
    public List<string> deadEnemyIds { get; set; } = new();
    public string checkpointId { get; set; } = string.Empty;
    public PositionData checkpointPosition { get; set; } = new();
    public float completionPercent { get; set; }
    public DateTime lastUpdatedUtc { get; set; } = DateTime.UtcNow;
}

public class PositionData
{
    public float x { get; set; }
    public float y { get; set; }
    public float z { get; set; }
}

static class PasswordHasher
{
    public static (string hash, string salt) CreateHash(string password)
    {
        var saltBytes = RandomNumberGenerator.GetBytes(16);
        var hashBytes = Rfc2898DeriveBytes.Pbkdf2(
            password,
            saltBytes,
            100_000,
            HashAlgorithmName.SHA256,
            32
        );
        return (Convert.ToBase64String(hashBytes), Convert.ToBase64String(saltBytes));
    }

    public static bool Verify(string password, string hash, string salt)
    {
        var saltBytes = Convert.FromBase64String(salt);
        var hashBytes = Rfc2898DeriveBytes.Pbkdf2(
            password,
            saltBytes,
            100_000,
            HashAlgorithmName.SHA256,
            32
        );
        var provided = Convert.ToBase64String(hashBytes);
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(provided),
            Encoding.UTF8.GetBytes(hash)
        );
    }
}

static class JwtTokenGenerator
{
    public static string Generate(string username, string key, string issuer, string audience)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var creds = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[] { new Claim(ClaimTypes.Name, username) };

        var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds
        );

        return new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(token);
    }
}
