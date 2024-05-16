using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using System.Security.Cryptography;
using System.Text;
using WebTvApiService.BackgroundServices;
using WebTvApiService.SignalR;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();


/*
builder
 .AllowAnyOrigin()
 .AllowAnyMethod()
 .AllowAnyHeader()
 .AllowCredentials();
*/
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => {
    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Description = "Standard Authorization  header using the Bearer scheme (\"Bearer {token}\")",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });
    options.OperationFilter<SecurityRequirementsOperationFilter>();
});
builder.Services.AddSignalR();
builder.Services.AddHostedService<ServerTimeNotifier>();

builder.Services.AddCors(o => o.AddPolicy("CorsPolicy", builder =>
{
    builder//.WithOrigins("http://example.com")
//            .WithOrigins("http://localhost:5153/")
  //          .WithOrigins("http://127.0.0.1:5500/")
           .WithOrigins("http://localhost:5153", "http://127.0.0.1:5500", "http://localhost:3000", "https://localhost:3000", "https://192.168.0.12:3000", "https://192.168.0.12:3001")
//           .AllowAnyOrigin()
           .AllowAnyMethod()
           .AllowAnyHeader()
           //.AllowAnyHeader()
           .AllowCredentials()
           ;
}));

builder.Services.AddAuthorization();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder?.Configuration?.GetSection("AppSettings:Token").Value!)),
            ValidateIssuer = false,
            ValidateAudience = false
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];

                // если запрос направлен хабу
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/commandub"))
                {
                    Console.WriteLine(accessToken);
                    // получаем токен из строки запроса
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddHttpsRedirection(options =>
{
//    options.RedirectStatusCode = Status307TemporaryRedirect;
    options.HttpsPort = 5001;
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
var webSocketOptions = new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromMinutes(2)
};


app.UseCors("CorsPolicy");
app.UseWebSockets(webSocketOptions);

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHub<TvHub>("/tvmirror");
app.MapHub<CommandHub>("/commandub");
app.Run();
