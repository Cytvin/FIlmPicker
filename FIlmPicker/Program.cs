using FIlmPicker.Data;
using FIlmPicker.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity.UI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
var kinopoiskAPIKey = builder.Configuration.GetValue<string>("KinopoiskAPIKey") ?? throw new InvalidOperationException("String 'KinopoiskAPIKey' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();
builder.Services.AddTransient<APIService>(s => new APIService(kinopoiskAPIKey));
builder.Services.AddTransient<DatabaseService>();
builder.Services.AddTransient<IEmailSender, EmaiSenderService>(s => new EmaiSenderService("", ""));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

CultureInfo ruCulture = new CultureInfo("ru-RU");
ruCulture.NumberFormat.NumberDecimalSeparator = ".";
ruCulture.NumberFormat.CurrencyDecimalSeparator = ".";

app.UseRequestLocalization(new RequestLocalizationOptions
{
    SupportedCultures = new[] { ruCulture }
});

app.UseHttpsRedirection();
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();