using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProductsCatalogWeb.Data;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<ProductsCatalogWebContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ProductsCatalogWebContext") ?? throw new InvalidOperationException("Connection string 'ProductsCatalogWebContext' not found.")));

//Add Distributed in-memory cache
builder.Services.AddDistributedMemoryCache();

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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
