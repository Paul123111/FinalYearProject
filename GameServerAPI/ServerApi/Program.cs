using k8s;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json;
using System.Threading.RateLimiting;

// --------------------------------------------------------------------------------------------
//  ServerApi is a web API that allocates and finds Agones GameServers on a Kubernetes cluster
//
//  Endpoints:
//  /test - test endpoint to check authentication works
//  /allocate - allocates ready GameServer from cluster
//  /listrooms - get all allocated GameServers
//  /get/{server} - find if server exists and is allocated from ip and port in the form ip:port
//  
//  Note: AI was used for some parts of this code
// --------------------------------------------------------------------------------------------

var builder = WebApplication.CreateBuilder(args);

// rate limit by ip for anonymous players
builder.Services.AddRateLimiter(options => {
    options.AddPolicy("anonymous-ip", httpContext => {
        var remoteIp = httpContext.Connection.RemoteIpAddress?.ToString();

        Console.WriteLine($"[RateLimit] Request from: {remoteIp}");

        return RateLimitPartition.GetFixedWindowLimiter(remoteIp, _ => new FixedWindowRateLimiterOptions {
            PermitLimit = 10,
            Window = TimeSpan.FromSeconds(10),
            QueueLimit = 0
        });
    });

    options.OnRejected = async (context, token) => {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsync("Too many requests. Wait a few seconds before retrying.", token);
    };
});

var config = KubernetesClientConfiguration.BuildDefaultConfig();
builder.Services.AddSingleton<IKubernetes>(new Kubernetes(config));

var unityProjectId = Environment.GetEnvironmentVariable("UNITY_PROJECT_ID");

// get unity jwks keys and add to configManager for auth
var jwksUrl = "https://player-auth.services.api.unity.com/.well-known/jwks.json";
var configManager = new ConfigurationManager<OpenIdConnectConfiguration>(
    jwksUrl,
    new UnityJwksRetriever());

// authorise token from unity sign-in
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

// if an ip doesn't exist, stop request
app.Use(async (context, next) => {
    if (context.Connection.RemoteIpAddress == null) {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        await context.Response.WriteAsync("Forbidden: Unknown IP Address.");
        return;
    }
    await next();
});

app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();


// -----------
//  Endpoints
// -----------
app.MapPost("/allocate", async (IKubernetes client) => {
    try {
        // body of GameServerAllocation
        var body = new {
            apiVersion = "allocation.agones.dev/v1",
            kind = "GameServerAllocation",
            spec = new {
                selectors = new[] { new { matchLabels = new Dictionary<string, string>() } }
            }
        };

        // allocate GameServer using body
        var response = await client.CustomObjects.CreateNamespacedCustomObjectAsync(
            body, "allocation.agones.dev", "v1", "default", "gameserverallocations");
    
        // return allocated GameServer
        var item = ((JsonElement)response);
        return Results.Ok(GameServerResponseUtils.ParseAllocationJson(item));
    } catch (Exception ex) {
        Console.WriteLine($"Error: {ex.Message}");
        return Results.Problem(ex.Message);
    }
}).RequireAuthorization().RequireRateLimiting("anonymous-ip");

app.MapGet("/listrooms", async (IKubernetes client) => {
    try {
        // get all GameServers
        var response = await client.CustomObjects.ListNamespacedCustomObjectAsync(
            "agones.dev", "v1", "default", "gameservers");
        var root = ((JsonElement) response);

        // get all GameServers
        if (root.TryGetProperty("items", out var items) && items.ValueKind == JsonValueKind.Array) {
            var result = items.EnumerateArray().Select(item => {
                return GameServerResponseUtils.ParseGameServerJson(item);
            });
            // only return allocated - allocated servers are only servers that should have players to join
            result = result.Where(server => (server.State == "Allocated"));
            return Results.Ok(result);
        }
    } catch (Exception ex) {
        Console.WriteLine($"Error: {ex.Message}");
        return Results.Problem(ex.Message);
    }
    return Results.Problem();
}).RequireAuthorization().RequireRateLimiting("anonymous-ip"); ;

app.MapGet("/get/{server}", async (IKubernetes client, string server) => {
    try {
        // get desired ip and port
        string[] tmp = server.Split(':');
        string ip = tmp[0];
        int port = Int32.Parse(tmp[1]);

        // get all GameServers
        var response = await client.CustomObjects.ListNamespacedCustomObjectAsync(
            "agones.dev", "v1", "default", "gameservers");
        var root = ((JsonElement)response);
        
        // filter for correct GameServer
        if (root.TryGetProperty("items", out var items) && items.ValueKind == JsonValueKind.Array) {
            var result = items.EnumerateArray().Select(item => {
                return GameServerResponseUtils.ParseGameServerJson(item);
            });
            // only return allocated - don't want to join servers that aren't allocated!
            result = result.Where(server => (server.State == "Allocated" && server.Ip == ip && server.Port == port));
            // return true if server was found
            return Results.Ok(result.Count() == 1);
        }
        return Results.Ok(false);

    } catch (Exception ex) {
        Console.WriteLine($"Error: {ex.Message}");
        return Results.Problem(ex.Message);
    }
}).RequireAuthorization().RequireRateLimiting("anonymous-ip");

app.MapGet("/test", async (IKubernetes client) => {
    return Results.Ok("Auth Succeeded");
}).RequireAuthorization().RequireRateLimiting("anonymous-ip"); ;

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
