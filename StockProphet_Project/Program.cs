using Microsoft.EntityFrameworkCore;
using StockProphet_Project.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<StocksContext>(
	  options => options.UseSqlServer(builder.Configuration.GetConnectionString("StocksConnstring")));
//AddSession
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromSeconds(10);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment()) {
	app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();
//呼叫跨頁面存取功能函式
app.UseSession();
app.MapControllerRoute(
	name: "default",

	pattern: "{controller=Home}/{action=Index}");

app.Run();
