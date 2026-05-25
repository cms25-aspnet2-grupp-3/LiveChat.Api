using LiveChat.Api.Hubs;
using LiveChat.Api.Stores;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("NextJs", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",
                "https://shiko-webapp-nu.vercel.app")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
});

builder.Services.AddMemoryCache();
builder.Services.AddSingleton<LiveChatCacheStore>();

var app = builder.Build();

app.UseCors("NextJs");

app.MapHub<LiveChatHub>("/hubs/live/chat");

app.Run();