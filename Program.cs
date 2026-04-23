using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Project_BD.Database;

// Resolve the correct root so static files (CSS/JS) always load,
// whether launched via Visual Studio or from the bin/ output folder.
static string ResolveContentRoot()
{
    // Walk up from AppContext.BaseDirectory until we find a folder containing wwwroot
    var dir = new DirectoryInfo(AppContext.BaseDirectory);
    while (dir != null)
    {
        if (Directory.Exists(Path.Combine(dir.FullName, "wwwroot")))
            return dir.FullName;
        dir = dir.Parent;
    }
    // Fallback: current working directory
    return Directory.GetCurrentDirectory();
}

var contentRoot = ResolveContentRoot();

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = contentRoot,
    WebRootPath = Path.Combine(contentRoot, "wwwroot")
});

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddHttpContextAccessor();

// Registering connection string using school class

builder.Services.AddDbContext<E_db>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("con")));
var app = builder.Build();

// Seed Database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<E_db>();
        DbSeeder.Seed(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred seeding the DB.");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Only redirect to HTTPS if not in development or if explicitly configured
// This avoids the "Failed to determine the https port" warning during local debugging/runs
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
