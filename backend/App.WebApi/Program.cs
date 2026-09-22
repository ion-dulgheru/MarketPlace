using App.Application;
using App.Infrastructure;
using App.Persistence;
using System.Text;
using Azure.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Security.Cryptography.X509Certificates;

var builder = WebApplication.CreateBuilder(args);

// Load Azure Key Vault if configured (via Managed Identity or local Azure CLI credential)
var keyVaultUri = builder.Configuration["KeyVault:Uri"] ?? builder.Configuration["KeyVault__Uri"];
if (!string.IsNullOrWhiteSpace(keyVaultUri))
{
    var clientId = builder.Configuration["AZURE_CLIENT_ID"] ?? builder.Configuration["Azure:ClientId"];
    var credential = !string.IsNullOrWhiteSpace(clientId)
        ? new DefaultAzureCredential(new DefaultAzureCredentialOptions { ManagedIdentityClientId = clientId })
        : new DefaultAzureCredential();
    builder.Configuration.AddAzureKeyVault(new Uri(keyVaultUri), credential);
}

builder.Services.AddApplication();
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);

var certificatePath = builder.Configuration["Jwt:CertificatePath"]
    ?? throw new InvalidOperationException("Jwt:CertificatePath is not configured.");
var certificatePassword = builder.Configuration["Jwt:CertificatePassword"]
    ?? throw new InvalidOperationException("Jwt:CertificatePassword is not configured.");
var signingCertificate = X509CertificateLoader.LoadPkcs12FromFile(certificatePath, certificatePassword);
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new X509SecurityKey(signingCertificate),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter a valid JWT token."
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
    });
});

builder.Services.AddCors(options =>
{
    var frontendBaseUrl = builder.Configuration["Frontend:BaseUrl"];
    options.AddPolicy("Frontend", policy =>
        policy.SetIsOriginAllowed(origin =>
        {
            if (string.IsNullOrWhiteSpace(origin)) return false;
            if (Uri.TryCreate(origin, UriKind.Absolute, out var uri))
            {
                if (uri.Host == "localhost" || uri.Host == "127.0.0.1") return true;
                if (uri.Host.EndsWith(".azurestaticapps.net", StringComparison.OrdinalIgnoreCase)) return true;
                if (!string.IsNullOrWhiteSpace(frontendBaseUrl) &&
                    origin.TrimEnd('/').Equals(frontendBaseUrl.TrimEnd('/'), StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false;
        })
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("Frontend");
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));
app.MapGet("/health/ready", () => Results.Ok(new { status = "ready" }));
app.MapGet("/health/live", () => Results.Ok(new { status = "live" }));

app.Run();

