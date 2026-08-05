using Books.Mcp.Data;
using Books.Mcp.Services;
using Books.Mcp.Tools;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using System.Security.Claims;

namespace Books.Mcp.Extensions;

public static class ConfigurationExtensions
{
    public static void AddConfigurations(this WebApplicationBuilder builder)
    {
        var services = builder.Services;
        var configuration = builder.Configuration;

        var tenantId = configuration["AzureAd:TenantId"]
            ?? throw new InvalidOperationException("AzureAd:TenantId is required.");
        var apiClientId = configuration["AzureAd:ClientId"]
            ?? throw new InvalidOperationException("AzureAd:ClientId is required.");
        var foundryObjectId = configuration["FoundryCaller:ObjectId"]
            ?? throw new InvalidOperationException("FoundryCaller:ObjectId is required.");
        var applicationIdUri = $"api://{apiClientId}";

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApi(configuration.GetSection("AzureAd"));

        services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
        {
            options.TokenValidationParameters.ValidateAudience = true;
            options.TokenValidationParameters.ValidAudiences = [apiClientId, applicationIdUri];
        });

        services.AddAuthorization(options =>
        {
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();

            options.AddPolicy("FoundryProjectOnly", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireAssertion(context => IsFoundryProjectCaller(context.User, tenantId, foundryObjectId));
            });
        });

        services.AddDbContext<BooksDbContext>(options => options.UseInMemoryDatabase("BooksDb"));
        services.AddScoped<BookService>();

        services.AddMcpServer()
            .WithHttpTransport(options => options.Stateless = true)
            .WithTools<BookTools>();
    }

    public static void ConfigureApplication(this WebApplication app)
    {
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapGet("/health", () => Results.Ok(new { status = "Healthy", message = "Books MCP Server is running." }))
            .AllowAnonymous();

        app.MapMcp("/mcp").RequireAuthorization("FoundryProjectOnly");
    }

    private static bool IsFoundryProjectCaller(ClaimsPrincipal caller, string tenantId, string foundryObjectId)
    {
        var tokenTenantId = caller.FindFirstValue("tid")
            ?? caller.FindFirstValue("http://schemas.microsoft.com/identity/claims/tenantid");
        var tokenObjectId = caller.FindFirstValue("oid")
            ?? caller.FindFirstValue("http://schemas.microsoft.com/identity/claims/objectidentifier");
        var tokenSubject = caller.FindFirstValue("sub")
            ?? caller.FindFirstValue(ClaimTypes.NameIdentifier);

        var isApplicationOnlyToken = !string.IsNullOrWhiteSpace(tokenObjectId)
            && !string.IsNullOrWhiteSpace(tokenSubject)
            && string.Equals(tokenObjectId, tokenSubject, StringComparison.OrdinalIgnoreCase);

        var isAllowedTenant = string.Equals(tokenTenantId, tenantId, StringComparison.OrdinalIgnoreCase);
        var isAllowedFoundryIdentity = string.Equals(tokenObjectId, foundryObjectId, StringComparison.OrdinalIgnoreCase);

        return isApplicationOnlyToken && isAllowedTenant && isAllowedFoundryIdentity;
    }
}
