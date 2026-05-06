using k8s;
using k8s.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json;

// --------------------------------------------------------------------------------------------
//  ServerApi is a web API that allocates and finds Agones GameServers on a Kubernetes cluster
//  Endpoints:
//  /test - test endpoint to check authentication works
//  /allocate - allocates ready GameServer from cluster
//  /listrooms - get all allocated GameServers
//  
//  Note: AI was used for some parts of this code
// --------------------------------------------------------------------------------------------

var builder = WebApplication.CreateBuilder(args);

var config = KubernetesClientConfiguration.BuildDefaultConfig();
builder.Services.AddSingleton<IKubernetes>(new Kubernetes(config));

var unityProjectId = Environment.GetEnvironmentVariable("UNITY_PROJECT_ID");

var jwksUrl = "https://player-auth.services.api.unity.com/.well-known/jwks.json";
var configManager = new ConfigurationManager<OpenIdConnectConfiguration>(
    jwksUrl,
    new UnityJwksRetriever());

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        //options.Authority = "https://player-auth.services.api.unity.com";

        options.TokenValidationParameters = new TokenValidationParameters {
            ValidateIssuer = true,
            ValidIssuer = "https://player-auth.services.api.unity.com",
            
            ValidateAudience = true,
            AudienceValidator = (audiences, securityToken, validationParameters) => {
                string projectId = $"{unityProjectId}";
                return audiences.Any(aud => aud.Contains(projectId));
            },
            ValidateLifetime = true,
            ConfigurationManager = configManager,
            ValidateIssuerSigningKey = true,
            IssuerSigningKeyResolver = (token, securityToken, kid, parameters) => {
                var config = configManager.GetConfigurationAsync().Result;
                return config.SigningKeys;
            }
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();

app.MapPost("/allocate", async (IKubernetes client) => {
    try {
        var body = new {
            apiVersion = "allocation.agones.dev/v1",
            kind = "GameServerAllocation",
            spec = new {
                selectors = new[] { new { matchLabels = new Dictionary<string, string>() } }
            }
        };
        var response = await client.CustomObjects.CreateNamespacedCustomObjectAsync(
            body, "allocation.agones.dev", "v1", "default", "gameserverallocations");
    
        var item = ((JsonElement)response);
        return Results.Ok(GameServerResponseUtils.ParseJson(item));
    } catch (Exception ex) {
        Console.WriteLine($"Error: {ex.Message}");
        return Results.Problem(ex.Message);
    }
}).RequireAuthorization();

app.MapGet("/listrooms", async (IKubernetes client) => {
    try {
        var response = await client.CustomObjects.ListNamespacedCustomObjectAsync(
            "agones.dev", "v1", "default", "gameservers");
        var root = ((JsonElement) response);
        if (root.TryGetProperty("items", out var items) && items.ValueKind == JsonValueKind.Array) {
            var result = items.EnumerateArray().Select(item => {
                return GameServerResponseUtils.ParseJson(item);
            });
            return Results.Ok(result);
        }
    } catch (Exception ex) {
        Console.WriteLine($"Error: {ex.Message}");
        return Results.Problem(ex.Message);
    }
    return Results.Problem();
}).RequireAuthorization();

app.MapGet("/test", async (IKubernetes client) => {
    return Results.Ok("Auth Succeeded");
}).RequireAuthorization();

app.Run();

// unity has no OIDC discovery document, so get the JWKS and add to an openidConnectConfig for auth
public class UnityJwksRetriever : IConfigurationRetriever<OpenIdConnectConfiguration> {
    public async Task<OpenIdConnectConfiguration> GetConfigurationAsync(string address, IDocumentRetriever retriever, CancellationToken cancel) {
        string doc = await retriever.GetDocumentAsync(address, cancel);
        var jwks = new JsonWebKeySet(doc);
        var config = new OpenIdConnectConfiguration();
        foreach (var key in jwks.GetSigningKeys()) {
            config.SigningKeys.Add(key);
        }
        return config;
    }
}
