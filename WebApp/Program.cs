using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using RestSharp.Authenticators;
using RestSharp;
using Serilog;
using System.Text;
using System.Threading.RateLimiting;
using WebApp.Models.Auth;
using WebApp.Models.Dtos.CategoryDtos;
using WebApp.Models.Dtos.UserDtos;
using WebApp.Models.Dtos.WordDtos;
using WebApp.Profiles;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
//builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();


builder.Services.AddHttpContextAccessor();


// -----------****----------- Restsharp Implementation -----------****-----------
var restClientOptions = new RestClientOptions(builder.Configuration.GetSection("ApiBaseUrl").Value!);
builder.Services.AddSingleton<RestClientOptions>(restClientOptions); // options di for login eg. methods need just base url
builder.Services.AddScoped<RestClient>(provider =>
{
    var clientOptions = restClientOptions;

    var httpContextAccessor = provider.GetRequiredService<IHttpContextAccessor>();
    var httpContext = httpContextAccessor.HttpContext;
    if (httpContext != null)
    {
        var accessToken = httpContext.Request.Cookies["access_token"];
        if (!string.IsNullOrEmpty(accessToken)) clientOptions.Authenticator = new JwtAuthenticator(accessToken);
    }
    return new RestClient(clientOptions);
});



// -----------****----------- JWT Implementation -----------****-----------
TokenOptions tokenOptions = builder.Configuration.GetSection("TokenOptions").Get<TokenOptions>()!;
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    }).AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidIssuer = tokenOptions.Issuer,
            ValidAudience = tokenOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenOptions.SecurityKey))
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (context.Request.Cookies.ContainsKey("access_token"))
                {
                    context.Token = context.Request.Cookies["access_token"];
                }
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                // Token yoksa veya geçersizse bu metot çalýþýr
                context.Response.Redirect("/Auth/Login");
                context.HandleResponse(); // Response'u middleware'in iþlemesini engelle
                return Task.CompletedTask;
            }
        };
    });


// -----------****----------- Rate Limiter Implementation -----------****-----------
var _policyName = "sliding";
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddSlidingWindowLimiter(policyName: _policyName, slidingOptions =>
    {
        slidingOptions.PermitLimit = 10;
        slidingOptions.Window = TimeSpan.FromSeconds(15);
        slidingOptions.SegmentsPerWindow = 4;
        slidingOptions.QueueLimit = 2;
        slidingOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });
});


// ****** Logger Injection ******
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();
builder.Host.UseSerilog();


// ****** FluentValidation Injection ****** 
builder.Services.AddValidatorsFromAssemblyContaining<LoginByEmailDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<UserCreateDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CategoryCreateDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CategoryUpdateDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<WordCreateDtoValidator>();
 

// ****** AutoMapper Injection ****** 
builder.Services.AddAutoMapper(typeof(MappingProfiles));


var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseRateLimiter();

app.UseSerilogRequestLogging();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "admin",
    pattern: "portal/{controller=Portal}/{action=Index}/{id?}");

app.Run();