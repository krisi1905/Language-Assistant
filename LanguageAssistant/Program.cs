
using LanguageAssistant.Data;
using LanguageAssistant.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// API услуги
builder.Services.AddHttpClient<DictionaryService>();
builder.Services.AddHttpClient<GeminiService>();

// База данни
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (builder.Environment.IsProduction())
    {
        var connectionString =
            builder.Configuration.GetConnectionString("PostgresConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "PostgresConnection is not configured.");
        }

        options.UseNpgsql(connectionString);
    }
    else
    {
        options.UseSqlServer(
            builder.Configuration.GetConnectionString("DefaultConnection"));
    }
});

// MVC
builder.Services.AddControllersWithViews();

var app = builder.Build();

if (app.Environment.IsProduction())
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        await db.Database.EnsureCreatedAsync();
    }
}


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
