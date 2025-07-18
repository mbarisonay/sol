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

// Serilog Ayarlarý...
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

// --- DÝÐER SERVÝS KAYITLARI BURADA BAÞLIYOR ---

builder.Services.AddControllersWithViews();

builder.Services.AddScoped(typeof(IEntityService<,>), typeof(EntityService<,>));
builder.Services.AddScoped<IApplicationUserService, ApplicationUserService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<ICabinReservationService, CabinReservationService>();
builder.Services.AddScoped<ICabinService, CabinService>();
builder.Services.AddScoped<ICityService, CityService>();
builder.Services.AddScoped<IDistrictService, DistrictService>();
builder.Services.AddScoped<ILogService, LogService>();

// --- KOD PARÇACIÐINI KOYACAÐINIZ EN ÝYÝ YER BURASI ---

// --- Firebase Baþlatma (Ortam Deðiþkeni Yöntemi) ---
try
{
    // 1. Firebase uygulamasýný baþlat. Kimlik bilgilerini ortam deðiþkeninden otomatik bulacak.
    FirebaseApp.Create();

    // 2. Firestore veritabaný baðlantýsýný tek bir kere oluþtur.
    // Proje ID'sini de ortam deðiþkeninden almasýný bekleyebiliriz veya garanti olmasý için belirtebiliriz.
    // Proje ID'niz "kabin-sistemi" idi.
    FirestoreDb firestoreDb = FirestoreDb.Create("kabin-sistemi");

    // 3. Bu tekil baðlantýyý (firestoreDb) tüm uygulamanýn kullanabilmesi için singleton olarak kaydet.
    builder.Services.AddSingleton(firestoreDb);

    // 4. Kendi Firebase servisimizi de kaydedelim.
    builder.Services.AddScoped<IFirebaseService, FirebaseService>();
}
catch (Exception ex)
{
    // Firebase baþlatýlýrken bir hata olursa, programýn çökmemesi için yakalayýp loglayalým.
    Console.WriteLine($"Firebase initialization failed: {ex.Message}");
    // Burada Serilog'u da kullanabilirsiniz.
}
// --- FIREBASE KURULUMU BÝTTÝ ---


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