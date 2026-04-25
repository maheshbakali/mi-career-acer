using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MiCareerAcer.Api.Data;
using MiCareerAcer.Api.Middleware;
using MiCareerAcer.Api.Options;
using MiCareerAcer.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.Configure<AgentServiceOptions>(builder.Configuration.GetSection(AgentServiceOptions.SectionName));

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAutoMapper(typeof(MiCareerAcer.Api.Mappings.MappingProfile));

var jwt = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SecretKey));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey,
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(2)
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "mi-career-acer API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddCors(o => o.AddPolicy("frontend", p =>
    p.WithOrigins("http://localhost:3000")
        .AllowAnyHeader()
        .AllowAnyMethod()));

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddSingleton<IHtmlToPlainTextService, HtmlToPlainTextService>();
builder.Services.AddSingleton<IFileStorageService, FileStorageService>();
builder.Services.AddScoped<IAgentOrchestrationService, AgentOrchestrationService>();

var agentBase = builder.Configuration.GetSection(AgentServiceOptions.SectionName)["BaseUrl"]?.TrimEnd('/') ?? "http://localhost:8000";
builder.Services.AddHttpClient<IAgentClient, AgentHttpClient>(c =>
{
    c.BaseAddress = new Uri(agentBase + "/");
    c.Timeout = TimeSpan.FromMinutes(15);
});

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("frontend");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

try
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}
catch (Exception ex)
{
    var lf = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
    lf.LogWarning(ex, "Database migration failed — ensure PostgreSQL is running and connection string is valid.");
}

app.Run();
