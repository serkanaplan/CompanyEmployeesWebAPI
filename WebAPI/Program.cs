using AspNetCoreRateLimit;
using CompanyEmployees.API.Extensions;
using CompanyEmployees.Presentation.Filters;
using Contracts.OtherContracts;
using Entities.DTO;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Options;
using NLog;
using Service;

var builder = WebApplication.CreateBuilder(args);

// NLog: Setup NLog for Dependency injection
LogManager.Setup().LoadConfigurationFromFile(string.Concat(Directory.GetCurrentDirectory(), "/nlog.config"));
builder.Services.ConfigureLoggerService();

builder.Services.AddAutoMapper(typeof(Program));
builder.Services.ConfigureCors();
builder.Services.ConfigureIISIntegration();
builder.Services.ConfigureRepositoryManager();
builder.Services.ConfigureServiceManager();
builder.Services.ConfigureSqlContext(builder.Configuration);
builder.Services.ConfigureVersioning();

<<<<<<< HEAD:CompanyEmployees.API/Program.cs
//rate limit
builder.Services.AddMemoryCache();//rate limiti ihtiyacı var oyuzden eklendi
builder.Services.ConfigureRateLimitingOptions(); 

builder.Services.AddAuthentication(); 
builder.Services.ConfigureIdentity(); 
builder.Services.ConfigureJWT(builder.Configuration); 
=======
builder.Services.AddAuthentication();
builder.Services.ConfigureIdentity();
builder.Services.ConfigureJWT(builder.Configuration);
builder.Services.AddJwtConfiguration(builder.Configuration);
builder.Services.ConfigureSwagger();
>>>>>>> binding_configuration_and_options_pattern:WebAPI/Program.cs

// patch işlemi için
NewtonsoftJsonPatchInputFormatter GetJsonPatchInputFormatter() => new ServiceCollection().AddLogging().AddMvc().AddNewtonsoftJson().Services.BuildServiceProvider()
.GetRequiredService<IOptions<MvcOptions>>().Value.InputFormatters.OfType<NewtonsoftJsonPatchInputFormatter>().First();

//data happer(veri şekillendirme) için
builder.Services.AddScoped<IDataShaper<EmployeeDto>, DataShaper<EmployeeDto>>();

builder.Services.AddScoped<ValidationFilterAttribute>();//eğer filter global olmayacaksa , her controller ve ya action metod için ayrı ayrı çağıracaksan scopped olarak ekle 

builder.Services.AddControllers(config =>
{

    config.CacheProfiles.Add("120SecondsDuration", new CacheProfile { Duration = 120 });
    config.InputFormatters.Insert(0, GetJsonPatchInputFormatter());
    // config.Filters.Add(new  ValidationFilterAttribute());// global filter ekleme yöntemi. Tüm kontrollerlar etkilenecek
})
//GetJsonPatchInputFormatter metodunu patch işlemi için ekledik. controller yapısını sunum katmanına taşıdık ve aktifleyebilmek için AddApplicationPart kullandık
.AddApplicationPart(typeof(CompanyEmployees.Presentation.AssemblyReference).Assembly).AddCustomCSVFormatter();


builder.Services.AddEndpointsApiExplorer();


// Apicontroller ile beraber gelen defauld  model  durumu  doğrulamasını  bastırıyoruz.
builder.Services.Configure<ApiBehaviorOptions>(options => options.SuppressModelStateInvalidFilter = true);


//middlewares
var app = builder.Build();

var logger = app.Services.GetRequiredService<ILoggerManager>();

app.ConfigureExceptionHandler(logger);

if (app.Environment.IsProduction()) app.UseHsts();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseForwardedHeaders(new ForwardedHeadersOptions { ForwardedHeaders = ForwardedHeaders.All });
app.UseIpRateLimiting(); 
app.UseCors("CorsPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.UseSwagger();

app.UseSwaggerUI(s =>
{
    s.SwaggerEndpoint("/swagger/v1/swagger.json", "Code Maze API v1");
    s.SwaggerEndpoint("/swagger/v2/swagger.json", "Code Maze API v2");
});

app.Run();
