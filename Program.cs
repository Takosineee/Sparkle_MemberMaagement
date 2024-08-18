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
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();//��ʵ��UHttpContextAccessor
builder.Services.AddScoped<checkUserCourse>();
string dbConnectionString = configuration.GetConnectionString("GymConnection");
builder.Services.AddDbContext<GymContext>(
 options => options.UseSqlServer(builder.Configuration.GetConnectionString("GymConnection")));
builder.Services.AddSession();//��ʱҥ�session
var app = builder.Build();
app.UseSession();//�ҥ�session
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
