using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models; // Serve per Swagger
using TravelExpenses.Api.Services;
using TravelExpenses.Api.Services.ExchangeRates;
using TravelExpenses.Api.Services.Interfaces;
using TravelExpenses.Dal.Data;
using TravelExpenses.Dal.Repositories;
using TravelExpenses.Domain.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// DbContext
builder.Services.AddDbContext<TravelExpensesDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("TravelExpenses"));
});

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
        policy =>
        {
            policy.WithOrigins("http://localhost:5173") // Frontend Vite
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

// --- AUTHENTICATION ---
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "https://accounts.google.com";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "https://accounts.google.com",
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Authentication:Google:ClientId"],
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true
        };
    });

// Repositories & Services
builder.Services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));
builder.Services.AddScoped(typeof(IBaseService<>), typeof(BaseService<>));
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ITravelService, TravelService>();
builder.Services.AddScoped<ITravelCurrencyRateService, TravelCurrencyRateService>();
builder.Services.AddScoped<IExpanseService, ExpanseService>();
builder.Services.AddSingleton<IExchangeRateService, ExchangeRateService>();

// Controllers con PROTEZIONE GLOBALE
builder.Services.AddControllers(options =>
{
    // Questo applica [Authorize] a TUTTI i controller automaticamente
    var policy = new AuthorizationPolicyBuilder()
                      .RequireAuthenticatedUser()
                      .Build();
    options.Filters.Add(new AuthorizeFilter(policy));
});

builder.Services.AddEndpointsApiExplorer();

// --- SWAGGER CONFIGURATO PER JWT (Così puoi testare le API) ---
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "TravelExpenses API", Version = "v1" });

    // Definisce lo schema di sicurezza (Bearer Token)
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Inserisci il token JWT in questo modo: Bearer {tuo_token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});

var app = builder.Build();

// --- MIDDLEWARE PIPELINE (Ordine Importante) ---

// 1. Swagger (Solo in Dev)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// 2. CORS (Prima della security)
app.UseCors(MyAllowSpecificOrigins);

// 3. AUTHENTICATION & AUTHORIZATION
app.UseAuthentication(); // Chi sei?
app.UseAuthorization();  // Puoi entrare?

// (Nota: Ho rimosso il secondo app.UseAuthorization() che avevi messo qui, era duplicato)

app.MapControllers();

app.Run();