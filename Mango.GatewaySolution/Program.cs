using Mango.GatewaySolution.Extensions;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("yarp.json", optional: false, reloadOnChange: true);
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
if (!builder.Environment.IsDevelopment())
{
    // Production overrides (VM IIS): Product:7000 Coupon:7001 Auth:7002 Cart:7003 Order:7004
    builder.Configuration.AddJsonFile("yarp.production.json", optional: false, reloadOnChange: true);
    builder.Configuration.AddJsonFile("ocelot.production.json", optional: false, reloadOnChange: true);
}

// Active gateway: YARP (Ocelot config loaded for production file readiness; middleware kept commented)
builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
// builder.Services.AddOcelot(builder.Configuration);

builder.AddJwtAuthentication();
builder.Services.AddAuthorization();

var app = builder.Build();
app.MapGet("/", () => "Hello World!");
app.UseAuthentication();
app.UseAuthorization();
app.MapReverseProxy();
// await app.UseOcelot();
app.Run();
