using Microsoft.EntityFrameworkCore;
using TravelExpenses.Dal.Data;
using TravelExpenses.Dal.Repositories;
using TravelExpenses.Domain.Interfaces;
using TravelExpenses.Api.Services;
using TravelExpenses.Api.Services.Interfaces;
using TravelExpenses.Api.Services.ExchangeRates;

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
            policy.WithOrigins("http://localhost:5173") // URL Vite
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

// Repositories
builder.Services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));

// Services
builder.Services.AddScoped(typeof(IBaseService<>), typeof(BaseService<>));
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ITravelService, TravelService>();
builder.Services.AddScoped<ITravelCurrencyRateService, TravelCurrencyRateService>();
builder.Services.AddScoped<IExpanseService, ExpanseService>();
builder.Services.AddHttpClient<IExchangeRateService, ExchangeRateService>(client =>
{
    client.BaseAddress = new Uri("https://api.frankfurter.dev/v1/");
    client.Timeout = TimeSpan.FromSeconds(5);
});


// Controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseCors(MyAllowSpecificOrigins);

// Enable Swagger only in Development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
