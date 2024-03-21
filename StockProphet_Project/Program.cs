using Microsoft.EntityFrameworkCore;
using StockProphet_Project.Models;
using System.Text.Encodings.Web;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews()
         .AddJsonOptions(options => {
             //���n��j�p�g
             options.JsonSerializerOptions.PropertyNamingPolicy = null;
             //�ƪ�
             options.JsonSerializerOptions.WriteIndented = true;
             options.JsonSerializerOptions.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;

         });
builder.Services.AddDbContext<StocksContext>(
	  options => options.UseSqlServer(builder.Configuration.GetConnectionString("StocksConnstring")));
//AddSession
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
	options.IdleTimeout = TimeSpan.FromSeconds(1000);
	options.Cookie.HttpOnly = true;
	options.Cookie.IsEssential = true;
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();
app.UseSession();
app.MapControllerRoute(
	name: "default",
//pattern: "{controller=Home}/{action=Index}");
pattern: "{controller=Home}/{action=Index}/{id?}"); 
//pattern: "{controller=StockModel}/{action=testBuild}");

app.Run();
