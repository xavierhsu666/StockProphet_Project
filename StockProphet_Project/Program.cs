using Microsoft.EntityFrameworkCore;
using StockProphet_Project.Models;
using System.Text.Encodings.Web;

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews()
         .AddJsonOptions(options => {
             //不要改大小寫
             options.JsonSerializerOptions.PropertyNamingPolicy = null;
             //排版
             options.JsonSerializerOptions.WriteIndented = true;
             options.JsonSerializerOptions.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;

         });
builder.Services.AddCors(options => {
	options.AddPolicy(name: MyAllowSpecificOrigins,
					  policy => {
						  policy.WithOrigins("https://mops.twse.com.tw/nas/t21/sii/t21sc03_113_2_0.html")
											 .AllowAnyHeader().AllowAnyMethod();
					  });
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
app.UseCors(MyAllowSpecificOrigins);

app.UseAuthorization();
app.UseSession();
app.MapControllerRoute(
	name: "default",
//pattern: "{controller=StockModel}/{action=predictindex}");
pattern: "{controller=Member}/{action=MyPredictResult}/{id?}");
//pattern: "{controller=StockModel}/{action=testBuild}");


app.Run();
