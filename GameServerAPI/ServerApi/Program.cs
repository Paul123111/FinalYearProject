using k8s;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

var config = KubernetesClientConfiguration.BuildDefaultConfig();
builder.Services.AddSingleton<IKubernetes>(new Kubernetes(config));

var unityProjectId = Environment.GetEnvironmentVariable("UNITY_PROJECT_ID");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        options.Authority = "https://player-auth.services.api.unity.com";

        options.TokenValidationParameters = new TokenValidationParameters {
            ValidateIssuer = true,
            ValidIssuer = "https://player-auth.services.api.unity.com",
            ValidateAudience = true,
            ValidAudiences = new[] { $"upid:{unityProjectId}", $"{unityProjectId}" },
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKeyResolver = (token, securityToken, kid, parameters) => {
                var client = new HttpClient();
                var json = client.GetStringAsync("https://player-auth.services.api.unity.com/.well-known/jwks.json").Result;
                return new JsonWebKeySet(json).GetSigningKeys();
            }
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();

app.MapPost("/allocate", async (IKubernetes client) => {
    var body = new {
        apiVersion = "allocation.agones.dev/v1",
        kind = "GameServerAllocation",
        spec = new {
            selectors = new[] { new { matchLabels = new Dictionary<string, string>() } }
        }
    };

    var result = await client.CustomObjects.CreateNamespacedCustomObjectAsync(
        body, "allocation.agones.dev", "v1", "default", "gameserverallocations");

    return Results.Ok(result);
}).RequireAuthorization();

app.MapGet("/listrooms", async (IKubernetes client) => {
    var resp = await client.CustomObjects.ListNamespacedCustomObjectAsync(
        "agones.dev", "v1", "default", "gameservers");

    var items = ((dynamic)resp).items;
    return Results.Ok(items);
}).RequireAuthorization();

app.Run();
