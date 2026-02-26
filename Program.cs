using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Project_BD.Database;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    // This helps the app find the correct root folder even if run from the bin folder
    ContentRootPath = Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"))
        ? Directory.GetCurrentDirectory()
        : AppContext.BaseDirectory,
    WebRootPath = "wwwroot"
});

// Add services to the container.
builder.Services.AddControllersWithViews();

// Registering connection string using school class

builder.Services.AddDbContext<E_db>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("con")));
var app = builder.Build();

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

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
