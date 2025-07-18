// Gerekli using direktifleri
using FirebaseAdmin;
using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using user_panel.Context;
using user_panel.Data;
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

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Serilog Ayarlar�...
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

// --- D��ER SERV�S KAYITLARI BURADA BA�LIYOR ---

builder.Services.AddControllersWithViews();

builder.Services.AddScoped(typeof(IEntityService<,>), typeof(EntityService<,>));
builder.Services.AddScoped<IApplicationUserService, ApplicationUserService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<ICabinReservationService, CabinReservationService>();
builder.Services.AddScoped<ICabinService, CabinService>();
builder.Services.AddScoped<ICityService, CityService>();
builder.Services.AddScoped<IDistrictService, DistrictService>();
builder.Services.AddScoped<ILogService, LogService>();

// --- KOD PAR�ACI�INI KOYACA�INIZ EN �Y� YER BURASI ---

// --- Firebase Ba�latma (Ortam De�i�keni Y�ntemi) ---
try
{
    // 1. Firebase uygulamas�n� ba�lat. Kimlik bilgilerini ortam de�i�keninden otomatik bulacak.
    FirebaseApp.Create();

    // 2. Firestore veritaban� ba�lant�s�n� tek bir kere olu�tur.
    // Proje ID'sini de ortam de�i�keninden almas�n� bekleyebiliriz veya garanti olmas� i�in belirtebiliriz.
    // Proje ID'niz "kabin-sistemi" idi.
    FirestoreDb firestoreDb = FirestoreDb.Create("kabin-sistemi");

    // 3. Bu tekil ba�lant�y� (firestoreDb) t�m uygulaman�n kullanabilmesi i�in singleton olarak kaydet.
    builder.Services.AddSingleton(firestoreDb);

    // 4. Kendi Firebase servisimizi de kaydedelim.
    builder.Services.AddScoped<IFirebaseService, FirebaseService>();
}
catch (Exception ex)
{
    // Firebase ba�lat�l�rken bir hata olursa, program�n ��kmemesi i�in yakalay�p loglayal�m.
    Console.WriteLine($"Firebase initialization failed: {ex.Message}");
    // Burada Serilog'u da kullanabilirsiniz.
}
// --- FIREBASE KURULUMU B�TT� ---


builder.Services.Configure<GoogleMapsSettings>(
    builder.Configuration.GetSection(GoogleMapsSettings.SectionName)
);

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSession();
app.UseRouting();
app.UseSerilogRequestLogging();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();