using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using user_panel.Context;
using user_panel.Data; // This line is important
using user_panel.Services.Base;
using user_panel.Services.Entity.ApplicationUserServices;
using user_panel.Services.Entity.BookingServices;
using user_panel.Services.Entity.CabinReservationServices;
using user_panel.Services.Entity.CabinServices;
using user_panel.Services.Entity.CityServices;
using user_panel.Services.Entity.DistrictServices;
using user_panel.Services.Entity.LogServices;
using user_panel.Services.Firebase; 
using user_panel.Settings;



var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// THIS IS THE CORRECTED LINE. We are now using ApplicationUser.
builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration.ReadFrom.Configuration(context.Configuration)
                 .ReadFrom.Services(services)
                 .Enrich.FromLogContext();
});

Serilog.Debugging.SelfLog.Enable(msg =>
{
    File.AppendAllText("serilog-errors.txt", msg);
});




builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .AddUserSecrets<Program>()
    .AddEnvironmentVariables();

builder.Services.AddControllersWithViews();

builder.Services.AddScoped(typeof(IEntityService<,>), typeof(EntityService<,>));

builder.Services.AddScoped<IApplicationUserService, ApplicationUserService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<ICabinReservationService, CabinReservationService>();
builder.Services.AddScoped<ICabinService, CabinService>();
builder.Services.AddScoped<ICityService, CityService>();
builder.Services.AddScoped<IDistrictService, DistrictService>();
builder.Services.AddScoped<ILogService, LogService>();

builder.Services.Configure<GoogleMapsSettings>(
    builder.Configuration.GetSection(GoogleMapsSettings.SectionName)
);

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
var pathToKey = Path.Combine(Directory.GetCurrentDirectory(), "gizli-anahtar-dosyanizin-adi.json"); 
FirebaseApp.Create(new AppOptions()
{
    Credential = GoogleCredential.FromFile(pathToKey)
});
builder.Services.AddSingleton<IFirebaseService, FirebaseService>(); 

var app = builder.Build();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production,
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSession();
app.UseRouting();

app.UseSerilogRequestLogging();

// These two must be in this order
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();