var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{ 
    options.AddPolicy("NextJs", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });

});

var app = builder.Build();

app.UseCors("NextJs");

app.Run();