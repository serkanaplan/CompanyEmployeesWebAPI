
using System.Text;
using CompanyEmployees.API.OutputFortmatter;
using Contracts;
using Entities.Models;
using LoggerService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Repository;
using Service;
using Service.Contracts;

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
        var jwtSettings = configuration.GetSection("JwtSettings");
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

                ValidIssuer = jwtSettings["validIssuer"],
                ValidAudience = jwtSettings["validAudience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
            };
        });
    }
}