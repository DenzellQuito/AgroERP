using AgroERP.Data;
using AgroERP.Data.AccesoDatos;
using AgroERP.Data.Seed;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.ConfigureApplicationCookie(options =>
{
    // Fuerza el rebote a tu nuevo controlador si alguien intenta entrar sin login
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied"; // Opcional, para cuando usemos roles
});
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<DAContabilidad>();
builder.Services.AddScoped<DACompras>();
builder.Services.AddScoped<DAAlmacen>();
builder.Services.AddScoped<DACalidad>();
builder.Services.AddScoped<DADistribucion>();
builder.Services.AddScoped<DAMaestros>();
builder.Services.AddScoped<DABI>();
builder.Services.AddHttpClient<DAPlanificacion>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Forecast:BaseUrl"] ?? "http://localhost:8000");
    client.Timeout = TimeSpan.FromSeconds(30);
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await PlanCuentaSeed.EjecutarAsync(db);

    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
    const string email = "admin@agroerp.com";
    if (await userManager.FindByEmailAsync(email) == null)
    {
        var result = await userManager.CreateAsync(
            new IdentityUser { UserName = email, Email = email, EmailConfirmed = true },
            "Admin123!");

        if (!result.Succeeded)
            Console.WriteLine("ERROR SEED ADMIN: " +
                string.Join(" | ", result.Errors.Select(e => e.Description)));
        else
            Console.WriteLine("Seed: usuario admin creado.");
    }
}
app.Run();
