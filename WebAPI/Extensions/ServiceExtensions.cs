
using System.Text;
using CompanyEmployees.API.OutputFortmatter;
using Contracts.OtherContracts;
using Contracts.RepositoryContracts;
using Contracts.ServiceContracts;
using Entities.ConfigurationModels;
using Entities.Models;
using LoggerService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Repository;
using Service;
using AspNetCoreRateLimit;
using Microsoft.OpenApi.Models;

namespace CompanyEmployees.API.Extensions;

public static class ServiceExtensions
{
    // CORS  (Çapraz  Kökenli  Kaynak  Paylaşımı),  kaynak  verme  veya  kısıtlamaya  yönelik  bir  mekanizmadır.
    public static void ConfigureCors(this IServiceCollection services) =>
    services.AddCors(options =>
    {
        options.AddPolicy("CorsPolicy", builder =>
        builder.AllowAnyOrigin() //  Herhangi  bir  kaynaktan  gelen  isteklere  izin  veren  AllowAnyOrigin()  yöntemi  yerine kaynak  olarak  WithOrigins("https://example.com")  komutunu  kullanabiliriz.
        .AllowAnyMethod() // Tüm  HTTP  yöntemlerine  izin  veren  WithMethods("POST",  "GET")  yalnızca  belirli  HTTP  yöntemlerine  izin  verecektir
        .AllowAnyHeader()
        .WithExposedHeaders("X-Pagination"));// istemci  uygulamasının  yeni  X-Pagination'ı  okumasını  sağlamak  için

    });



    //  ASP.NET  Core  uygulamaları  varsayılan  olarak  kendi  kendine  barındırılır  ve  eğer  istersek Uygulamamızı  IIS  üzerinde  barındırıyorsak,  bir  IIS  entegrasyonu  yapılandırmamız  gerekiyor.
    //  Seçeneklerin  içindeki  hiçbir  özelliği  başlatmıyoruz  çünkü  şimdilik  varsayılan  değerlerde  sorun  yok
    public static void ConfigureIISIntegration(this IServiceCollection services) =>
    services.Configure<IISOptions>(options =>
    {
    });


    public static void ConfigureLoggerService(this IServiceCollection services) => services.AddSingleton<ILoggerManager, LoggerManager>();


    public static void ConfigureRepositoryManager(this IServiceCollection services) => services.AddScoped<IRepositoryManager, RepositoryManager>();


    public static void ConfigureServiceManager(this IServiceCollection services) => services.AddScoped<IServiceManager, ServiceManager>();

    // çalışma zamanı için ekledik. derleme zamanı için de contextfactory kullandık. eğer çalışma zamanı için eklediysen derlleme zamanı için eklemesen de olur
    // public static void ConfigureSqlContext(this IServiceCollection services,IConfiguration configuration) 
    // => services.AddDbContext<RepositoryContext>(opts => opts.UseSqlServer(configuration.GetConnectionString("sqlConnection"))); 
    // bu şekilde de eklenebilir  Bu  yöntem  hem  AddDbContext  hem  de  UseSqlServer  yöntemlerinin  yerini  alır ama AddDbContext daha gelişmiş özeliklere sahiptir
    public static void ConfigureSqlContext(this IServiceCollection services, IConfiguration configuration)
    => services.AddSqlServer<RepositoryContext>(configuration.GetConnectionString("sqlConnection"));


    public static IMvcBuilder AddCustomCSVFormatter(this IMvcBuilder builder)
    => builder.AddMvcOptions(config => config.OutputFormatters.Add(new CsvOutputFormatter()));


    public static void ConfigureVersioning(this IServiceCollection services)
    {
        services.AddApiVersioning(opt =>
        {
            opt.ReportApiVersions = true;
            opt.AssumeDefaultVersionWhenUnspecified = true;
            opt.DefaultApiVersion = new ApiVersion(1, 0);
            opt.ApiVersionReader = new HeaderApiVersionReader("api-version"); //hem header versioning 
            opt.ApiVersionReader = new QueryStringApiVersionReader("api-version"); //hem de query string versioning

            //istersek versiyonlama işini controllerlardaki [ApiVersion("2.0")] yerine merkezi bir yerden de atayabiliriz. buradaki gibi, böylece controller sınıflarındaki [ApiVersion("2.0")] gösterimlerini kaldırabiliriz
            // opt.Conventions.Controller<CompaniesController>().HasApiVersion(new ApiVersion(1, 0));
            // opt.Conventions.Controller<CompaniesV2Controller>().HasDeprecatedApiVersion(new ApiVersion(2, 0));
        });
    }


    public static void ConfigureRateLimitingOptions(this IServiceCollection services)
    {
        var rateLimitRules = new List<RateLimitRule> { new() { Endpoint = "*", Limit = 3, Period = "5m" } };

        services.Configure<IpRateLimitOptions>(opt => opt.GeneralRules = rateLimitRules);

        services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
        services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
        services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
        services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();
    }


    public static void ConfigureIdentity(this IServiceCollection services)
    {
        var builder = services.AddIdentity<User, IdentityRole>(o =>
        {
            o.Password.RequireDigit = true;
            o.Password.RequireLowercase = false;
            o.Password.RequireUppercase = false;
            o.Password.RequireNonAlphanumeric = false;
            o.Password.RequiredLength = 10;
            o.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<RepositoryContext>()
        .AddDefaultTokenProviders();
    }


    public static void ConfigureJWT(this IServiceCollection services, IConfiguration configuration)
    {
        // var jwtSettings = configuration.GetSection("JwtSettings");
        var jwtConfiguration = new JwtConfiguration();
        configuration.Bind(jwtConfiguration.Section, jwtConfiguration);
        var secretKey = Environment.GetEnvironmentVariable("SECRET");

        services.AddAuthentication(opt =>
        {
            opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                // ValidIssuer = jwtSettings["validIssuer"],
                // ValidAudience = jwtSettings["validAudience"],
                ValidIssuer = jwtConfiguration.ValidIssuer,
                ValidAudience = jwtConfiguration.ValidAudience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
            };
        });
    }



    public static void AddJwtConfiguration(this IServiceCollection services, IConfiguration configuration)
    => services.Configure<JwtConfiguration>(configuration.GetSection("JwtSettings"));


    public static void ConfigureSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(s =>
        {
            s.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Code Maze API",
                Version = "v1",
                Description = "CompanyEmployees API by CodeMaze",
                TermsOfService = new Uri("https://example.com/terms"),
                Contact = new OpenApiContact
                {
                    Name = "John Doe",
                    Email = "John.Doe@gmail.com",
                    Url = new Uri("https://twitter.com/johndoe"),
                },
                License = new OpenApiLicense
                {
                    Name = "CompanyEmployees API LICX",
                    Url = new Uri("https://example.com/license"),
                }
            });
            s.SwaggerDoc("v2", new OpenApiInfo { Title = "Code Maze API", Version = "v2" });

            s.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Place to add JWT with Bearer",
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });
            s.AddSecurityRequirement(new OpenApiSecurityRequirement()
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        },
                        Name = "Bearer",
                    },
                    new List<string>()
                }
            });
        });
    }
}