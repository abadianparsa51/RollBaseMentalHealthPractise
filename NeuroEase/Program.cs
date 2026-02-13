using Application.Layer.Interface;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Core.Layer.Data;
using Core.Layer.Helpers;
using Core.Layer.Repository;
using Core.Layer.Services;
using Core.Model.Layer.Entity;
using Core.Model.Layer.Model;
using Core.Layer.Settings;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Net.Http.Headers;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ===================== Configuration =====================
// Python config (null-safe)
var pythonConfig = builder.Configuration.GetSection("PythonService").Get<PythonApiConfig>() ?? new PythonApiConfig();

// لاگ امن: Token را چاپ نکن
if (!string.IsNullOrWhiteSpace(pythonConfig.BaseUrl))
{
    Console.WriteLine($"Python API BaseUrl: {pythonConfig.BaseUrl}");
}
else
{
    Console.WriteLine("Python API BaseUrl is not set (PythonService:BaseUrl).");
}

// ===================== CORS =====================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

// ===================== Controllers + NewtonsoftJson =====================
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

// ===================== HttpClient for Python API =====================
// اگر BaseUrl ست نشده بود، این سرویس را اضافه نکن که موقع run کرش نکنه
if (!string.IsNullOrWhiteSpace(pythonConfig.BaseUrl))
{
    builder.Services.AddHttpClient("PythonAPI", client =>
    {
        client.BaseAddress = new Uri(pythonConfig.BaseUrl);

        if (!string.IsNullOrWhiteSpace(pythonConfig.Token))
        {
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", pythonConfig.Token);
        }
    });
}

// ===================== PythonIntegrationService =====================
builder.Services.AddScoped<DiagnosisIntegrationService>();
builder.Services.AddScoped<MlDataExportService>();

// ===================== Session =====================
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ===================== DbContext =====================
// DbProvider: SqlServer (default) | Postgres
var dbProvider = builder.Configuration["DbProvider"] ?? "SqlServer";

builder.Services.AddDbContext<MentalHealthDbContext>(options =>
{
    if (dbProvider.Equals("Postgres", StringComparison.OrdinalIgnoreCase))
        options.UseNpgsql(builder.Configuration.GetConnectionString("PostgresConnection"));
    else
        options.UseSqlServer(builder.Configuration.GetConnectionString("DevConnection"));
});

// ===================== Identity =====================
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<MentalHealthDbContext>()
    .AddDefaultTokenProviders();

// ===================== JWT =====================
builder.Services.Configure<JwtConfig>(builder.Configuration.GetSection("JwtConfig"));
var jwtConfig = builder.Configuration.GetSection("JwtConfig").Get<JwtConfig>() ?? new JwtConfig();
builder.Services.AddSingleton(jwtConfig);

// اگر Secret خالی بود، بهتره واضح fail کنه (برای جلوگیری از runtime خطاهای مبهم)
if (string.IsNullOrWhiteSpace(jwtConfig.Secret))
{
    throw new InvalidOperationException("JwtConfig:Secret is missing. Please configure it in appsettings or environment variables.");
}

var key = Encoding.ASCII.GetBytes(jwtConfig.Secret);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// ===================== Python config DI =====================
builder.Services.Configure<PythonApiConfig>(builder.Configuration.GetSection("PythonService"));
builder.Services.AddSingleton(pythonConfig);

// ===================== MediatR =====================
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblies(
        typeof(Application.Layer.Authontication.Command.LoginCommand).Assembly,
        typeof(Core.Layer.Handlers.EvaluateRulesCommandHandler).Assembly
    );
});

// ===================== Services =====================
builder.Services.AddScoped<IRuleService, RuleService>();

// ===================== Swagger =====================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "NeuroEase API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token. Example: Bearer {token}"
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

// ===================== Autofac container =====================
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
{
    containerBuilder.RegisterAssemblyTypes(
        typeof(Application.Layer.Authontication.Command.LoginCommand).Assembly,
        typeof(Core.Layer.Handlers.EvaluateRulesCommandHandler).Assembly
    )
    .AsClosedTypesOf(typeof(IRequestHandler<,>))
    .InstancePerLifetimeScope();

    containerBuilder.RegisterType<AuthRepository>().As<IAuthenticationRepository>().InstancePerLifetimeScope();
    containerBuilder.RegisterType<JwtHelper>().As<IJwtHelper>().InstancePerLifetimeScope();
    containerBuilder.RegisterType<RuleService>().As<IRuleService>().InstancePerLifetimeScope();
    containerBuilder.RegisterType<DiagnosisIntegrationService>().InstancePerLifetimeScope();
});

// ===================== Build App =====================
var app = builder.Build();

// ===================== Middleware =====================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Developer exception page فقط برای dev
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// HTTPS redirection فقط Production (برای Docker لوکال اذیت نکنه)
if (app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

app.UseRouting();

app.UseCors("AllowAll");

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Health endpoint (برای Docker/CI/HR Demo)
app.MapGet("/health", () => Results.Ok("OK"));

app.Run();
