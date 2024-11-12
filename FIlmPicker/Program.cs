using FIlmPicker.Data;
using FIlmPicker.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity.UI.Services;

var builder = WebApplication.CreateBuilder(args);

string connectionString;
string kinopoiskAPIKey;
string smtpLogin;
string smtpPassword;
string smtpServer;
string smtpPort;
string smtpSenderName;
string smtpSenderEmail;

if (builder.Environment.IsDevelopment())
{
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    kinopoiskAPIKey = builder.Configuration.GetValue<string>("KinopoiskAPIKey") ?? throw new InvalidOperationException("String 'KinopoiskAPIKey' not found.");
    smtpServer = "";
    smtpPort = "";
    smtpSenderName = "";
    smtpSenderEmail = "";
    smtpLogin = "";
    smtpPassword = "";
}
else
{
    connectionString = Environment.GetEnvironmentVariable("SQL_CONNECTION_STRING") ?? throw new InvalidOperationException("Environment Variable 'SQL_CONNECTION_STRING' not found.");
    kinopoiskAPIKey = Environment.GetEnvironmentVariable("KINOPOISK_API_KEY") ?? throw new InvalidOperationException("Environment Variable 'KINOPOISK_API_KEY' not found.");
    smtpServer = Environment.GetEnvironmentVariable("SMTP_SERVER") ?? throw new InvalidOperationException("Environment Variable 'SMTP_SERVER' not found.");
    smtpPort = Environment.GetEnvironmentVariable("SMTP_PORT") ?? throw new InvalidOperationException("Environment Variable 'SMTP_PORT' not found.");
    smtpSenderName = Environment.GetEnvironmentVariable("SMTP_SENDER_NAME") ?? throw new InvalidOperationException("Environment Variable 'SMTP_SENDER_NAME' not found.");
    smtpSenderEmail = Environment.GetEnvironmentVariable("SMTP_SENDER_EMAIL") ?? throw new InvalidOperationException("Environment Variable 'SMTP_SENDER_EMAIL' not found.");
    smtpLogin = Environment.GetEnvironmentVariable("SMTP_LOGIN") ?? throw new InvalidOperationException("Environment Variable 'SMTP_LOGIN' not found.");
    smtpPassword = Environment.GetEnvironmentVariable("SMTP_PASSWORD") ?? throw new InvalidOperationException("Environment Variable 'SMTP_PASSWORD' not found.");
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.User.RequireUniqueEmail = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddErrorDescriber<RussianIdentityErrorDescriber>();

builder.Services.AddControllersWithViews();
builder.Services.AddTransient(s => new APIService(kinopoiskAPIKey));
builder.Services.AddTransient<DatabaseService>();
builder.Services.AddTransient<IEmailSender, EmaiSenderService>(s => new EmaiSenderService(smtpServer, Convert.ToInt32(smtpPort), smtpLogin, smtpPassword, smtpSenderName, smtpSenderEmail));

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