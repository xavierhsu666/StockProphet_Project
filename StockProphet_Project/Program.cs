using Microsoft.EntityFrameworkCore;
using StockProphet_Project.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<StocksContext>(
	  options => options.UseSqlServer(builder.Configuration.GetConnectionString("StocksConnstring")));
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment()) {
	app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
	name: "default",
	//pattern: "{controller=StockModel}/{action=predictindex}/{id?}");
	pattern: "{controller=StockModel}/{action=testBuild}");

app.Run();
