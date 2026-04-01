using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using ACMSModel.Models;
using QTMSModel.Models;
using FMSModel.Models;
using LMSModel.Models;
using SQTWeb.Services.Quotations;
using SQTWeb.Navigation;
using COMMONModel.Models;
using UOMLib.DependencyInjection;
using SQTWeb.Services.Agents;
using SQTWeb.Services.Ports;
using SQTWeb.Services.QuotationStatus;
using SQTWeb.Services.UnitOfMeasure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLocalization(opts => opts.ResourcesPath = "Resources");
builder.Services.AddControllersWithViews(options =>
    {
        options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
    })
    .AddViewLocalization()
    .AddDataAnnotationsLocalization(opts =>
        opts.DataAnnotationLocalizerProvider = (_, factory) =>
            factory.Create(typeof(SQTWeb.SharedResource)));
builder.Services.AddRouting(options => options.LowercaseUrls = true);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMemoryCache();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/account/login";
        options.AccessDeniedPath = "/account/login";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

builder.Services.AddDbContext<ACMSContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("AcmsConnection")));

builder.Services.AddDbContextFactory<FMSContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("FMSConnection")));
builder.Services.AddDbContextFactory<COMMONContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("CommonConnection")));
builder.Services.AddDbContextFactory<QTMSContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("QTMSConnection")));
builder.Services.AddDbContextFactory<LMSContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("LMSConnection")));

builder.Services.AddUomServices(builder.Configuration);

builder.Services.AddSingleton<ICompanyService, CompanyService>();
builder.Services.AddSingleton<ICurrencyService, CurrencyService>();
builder.Services.AddSingleton<IQuotationStatusService, QuotationStatusService>();
builder.Services.AddSingleton<IChargeService, ChargeService>();
builder.Services.AddSingleton<IUnitOfMeasureService, UnitOfMeasureService>();
builder.Services.AddSingleton<IAgentService, AgentService>();
builder.Services.AddSingleton<IPortService, PortService>();
builder.Services.AddHostedService<SQTWeb.Services.CacheRefreshHostedService>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IPageContext, PageContext>();

builder.Services.AddScoped<IShipmentDetailCalculator, ShipmentDetailCalculator>();
builder.Services.AddScoped<IShipmentSummaryCalculator, ShipmentSummaryCalculator>();
builder.Services.AddScoped<IFreightChargeTierResolver, FreightChargeTierResolver>();
builder.Services.AddScoped<IFreightChargeCalculator, FreightChargeCalculator>();
builder.Services.AddScoped<IQuotationHeaderCalculator, QuotationHeaderCalculator>();
builder.Services.AddScoped<IQuotationCalculationOrchestrator, QuotationCalculationOrchestrator>();
builder.Services.AddScoped<IQuotationAppService, QuotationAppService>();

var app = builder.Build();

// Preload caches synchronously before first request — HostedService handles periodic refresh only
using (var scope = app.Services.CreateScope())
{
    var sp = scope.ServiceProvider;
    try
    {
        sp.GetRequiredService<ICompanyService>().GetOptionsAsync().GetAwaiter().GetResult();
        sp.GetRequiredService<ICurrencyService>().GetOptionsAsync().GetAwaiter().GetResult();
        sp.GetRequiredService<IQuotationStatusService>().GetOptionsAsync().GetAwaiter().GetResult();
        sp.GetRequiredService<IChargeService>().GetOptionsAsync().GetAwaiter().GetResult();
        sp.GetRequiredService<IUnitOfMeasureService>().GetOptionsAsync().GetAwaiter().GetResult();
        sp.GetRequiredService<IAgentService>().GetOptionsAsync().GetAwaiter().GetResult();
        sp.GetRequiredService<IPortService>().GetOptionsAsync().GetAwaiter().GetResult();
    }
    catch { /* startup preload failures are non-fatal */ }
}


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

var defaultCulture = "en";
var supportedCultures = new[] { "en", "th" };

var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture(defaultCulture)
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);

// If a user doesn't yet have a culture cookie, set one to the default (English)
// This ensures users without a cookie won't be forced to their browser's Accept-Language.
app.Use(async (context, next) =>
{
    var cookieName = CookieRequestCultureProvider.DefaultCookieName;
    if (!context.Request.Cookies.ContainsKey(cookieName))
    {
        context.Response.Cookies.Append(
            cookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(defaultCulture)),
            new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1), IsEssential = true, SameSite = SameSiteMode.Lax });
    }
    await next();
});

app.UseRequestLocalization(localizationOptions);

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

