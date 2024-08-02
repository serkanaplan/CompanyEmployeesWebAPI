using AspNetCoreRateLimit;
using CompanyEmployees.API.Extensions;
using CompanyEmployees.Presentation.Filters;
using Contracts;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Options;
using NLog;
using Service;
using Shared.DTO;

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

//rate limit
builder.Services.AddMemoryCache();//rate limiti ihtiyacı var oyuzden eklendi
builder.Services.ConfigureRateLimitingOptions(); 

// patch işlemi için
NewtonsoftJsonPatchInputFormatter GetJsonPatchInputFormatter() => new ServiceCollection().AddLogging().AddMvc().AddNewtonsoftJson().Services.BuildServiceProvider()
.GetRequiredService<IOptions<MvcOptions>>().Value.InputFormatters.OfType<NewtonsoftJsonPatchInputFormatter>().First();

//data happer(veri şekillendirme) için
builder.Services.AddScoped<IDataShaper<EmployeeDto>, DataShaper<EmployeeDto>>(); 

builder.Services.AddScoped<ValidationFilterAttribute>();//eğer filter global olmayacaksa , her controller ve ya action metod için ayrı ayrı çağıracaksan scopped olarak ekle 

//GetJsonPatchInputFormatter metodunu patch işlemi için ekledik. controller yapısını sunum katmanına taşıdık ve aktifleyebilmek için AddApplicationPart kullandık
builder.Services.AddControllers(config =>
{
    config.InputFormatters.Insert(0, GetJsonPatchInputFormatter());
    // config.Filters.Add(new  ValidationFilterAttribute());// global filter ekleme yöntemi. Tüm kontrollerlar etkilenecek
})
.AddApplicationPart(typeof(CompanyEmployees.Presentation.AssemblyReference).Assembly).AddCustomCSVFormatter();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();




// Apicontroller ile beraber gelen defauld  model  durumu  doğrulamasını  bastırıyoruz.
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});


//middlewares
var app = builder.Build();

var logger = app.Services.GetRequiredService<ILoggerManager>();

app.ConfigureExceptionHandler(logger);

if (app.Environment.IsProduction())
    app.UseHsts();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseForwardedHeaders(new ForwardedHeadersOptions { ForwardedHeaders = ForwardedHeaders.All });
app.UseIpRateLimiting(); 
app.UseCors("CorsPolicy");
app.UseAuthorization();

app.MapControllers();

app.Run();
