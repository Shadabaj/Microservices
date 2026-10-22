using Mango.GatewaySolution.Extensions;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("yarp.json", optional: false, reloadOnChange: true);
builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
builder.AddJwtAuthentication();
builder.Services.AddAuthorization();

var app = builder.Build();
app.MapGet("/", () => "Hello World!");
app.UseAuthentication();
app.UseAuthorization();
app.MapReverseProxy();
app.Run();
