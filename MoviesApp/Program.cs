
using Domin.Model;
using infrastructure.Data;
using infrastructure.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MoviesApp.Security;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog early
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .MinimumLevel.Debug()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();


builder.Services.AddControllersWithViews();
builder.Services.AddTransient<IClaimsTransformation, RoleClaimsTransformation>();

builder.Services.AddDbContext<DB>(options => options.UseSqlServer(
    builder.Configuration.GetConnectionString("Myconnection")
));

builder.Services.AddIdentity<AppUser, IdentityRole>(option => {

    option.Lockout.AllowedForNewUsers = true;
    option.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    option.Lockout.MaxFailedAccessAttempts = 5;
})
    .AddEntityFrameworkStores<DB>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Accounts/Login";
    options.AccessDeniedPath = "/Home/AccessDenied";
});

builder.Services.AddScoped<IMoviesService, MoviesService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<ImageFileServices>();
builder.Services.AddScoped<IActorService, ActorService>();
builder.Services.AddScoped<IGenreServices, GenreService>();
builder.Services.AddScoped<IProducerService, ProducerService>();
builder.Services.AddScoped<IQualityServices, QualityServices>();
builder.Services.AddScoped<infrastructure.ViewComponents.NotificationViewComponent>();
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 1_073_741_824; // 1 GB
});

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 1_073_741_824; // 1 GB
});

builder.Services.AddSignalR(options => {
    options.EnableDetailedErrors = true;
});

// Register AuditLog service
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<IWatchHistoryService, WatchHistoryService>();

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



app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Accounts}/{action=Login}/{id?}"
);

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Movie}/{action=Index}/{id?}"
);

// Map SignalR hub for real-time notifications
app.MapHub<infrastructure.Hubs.NotificationHub>("/notificationHub");

// 9. Default route

// 10. Seed the database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await Seeding.Initialize(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred seeding the DB.");
    }
}

app.Run();
