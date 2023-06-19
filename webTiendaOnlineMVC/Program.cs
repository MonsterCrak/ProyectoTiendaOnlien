using Microsoft.AspNetCore.Authentication.Cookies;
using webTiendaOnlineMVC.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddSession();

builder.Services.AddHttpClient();


builder.Services.AddScoped<UrlShortener>();


//builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
//    .AddCookie(options =>
//    {
//        options.Cookie.Name = "CookieTiendaOnline";
//        options.Cookie.HttpOnly = true;
//        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
//        options.LoginPath = "/Acceso/Login"; // Ruta de inicio de sesión
//        options.AccessDeniedPath = "/Acceso/Denegado"; // Ruta de acceso denegado
//        options.SlidingExpiration = true;
//    });


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Acceso/Denegado");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseSession(new SessionOptions
{
    Cookie = new CookieBuilder
    {
        Name = "CookiesSesion", // Reemplaza con el nombre que desees para la cookie de sesión
        HttpOnly = true,
        IsEssential = true,
        SameSite = SameSiteMode.Strict,
        MaxAge = TimeSpan.FromMinutes(30) // Tiempo de expiración de la sesión (30 minutos en este ejemplo)
    }
});

app.UseAuthorization();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Vistas}/{action=ListaProductosVender}/{id?}");

app.Run();
