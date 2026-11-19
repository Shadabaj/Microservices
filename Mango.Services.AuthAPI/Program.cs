using Mango.AuthAPI.Data;
using Mango.AuthAPI.Models;
using Mango.AuthAPI.Service;
using Mango.AuthAPI.Service.IService;
using Mango.MessageBus;
using Mango.MessageBus.RabbitMQSender;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(option =>
{
    option.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("ApiSettings:JwtOptions"));
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();
#region Azure Service Bus
var serviceBusConnectionString = builder.Configuration["ServiceBusConnectionString"] ?? throw new InvalidOperationException("ServiceBusConnectionString is not configured");
//builder.Services.AddScoped<IMessageBus>(_ => new MessageBus(serviceBusConnectionString));
#endregion
#region RabbitMQ
//builder.Configuration["RabbitMQ"]
builder.Services.AddScoped<IRabbitMQExchangeSender>(_ => new RabbitMQAuthMessageSender(builder.Configuration.GetSection("RabbitMQ")));
#endregion
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddAutoMapper(o =>
{
});
var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapOpenApi();
app.MapScalarApiReference();

app.MapGet("/", () => Results.Redirect("/scalar/v1"));

// Disabled for IIS HTTP-only binding on port 7002
// app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
ApplyMigration();
app.Run();

void ApplyMigration()
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate();
}
