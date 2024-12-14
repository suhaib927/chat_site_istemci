using chat_site_istemci.Entities;
using chat_site_istemci.Services;
using HaircutProject.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.FileProviders;
using System.Reflection;


var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
builder.Services.AddScoped<IChatService, ChatService>();

builder.Services.AddDbContext<DatabaseContext>(opts => {
    opts.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services
      .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
      .AddCookie(opts =>
      {
          opts.Cookie.Name = ".chat_site_istemci.auth";
          opts.ExpireTimeSpan = TimeSpan.FromDays(7);
          opts.SlidingExpiration = false;
          opts.LoginPath = "/Account/Login";
          opts.LogoutPath = "/Account/Logout";
          opts.AccessDeniedPath = "/Home/AccessDenied";
      });

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
    pattern: "{controller=Chat}/{action=Index}/{id?}");

app.Run();
