using Gym.Models;
using Microsoft.Build.Experimental.ProjectCache;
using Microsoft.EntityFrameworkCore;
using System.Configuration;
using Gym.Service;

var builder = WebApplication.CreateBuilder(args);
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();
// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();//手動註冊HttpContextAccessor
builder.Services.AddScoped<checkUserCourse>();
string dbConnectionString = configuration.GetConnectionString("GymConnection");
builder.Services.AddDbContext<GymContext>(
 options => options.UseSqlServer(builder.Configuration.GetConnectionString("GymConnection")));
builder.Services.AddSession();//手動啟用session
var app = builder.Build();
app.UseSession();//啟用session
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=CoachDB}/{action=MyCourse}/{id?}");
    endpoints.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Admin}/{action=Students}/{id?}");
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

});

app.Run();
