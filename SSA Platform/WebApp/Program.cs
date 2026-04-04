using App.Application.Interfaces;
using App.Application.Interfaces.IRepositories;
using App.Application.UseCases;
using App.Domain.Entities.Identity;
using App.Infrastructure.Persistence;
using App.Infrastructure.Repositories;
using App.Infrastructure.Seeding;
using App.Infrastructure.UnitOfWork;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                       throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddScoped<AddSatelliteUseCase>();
builder.Services.AddScoped<UpdateSatelliteUseCase>();
builder.Services.AddScoped<DeleteSatelliteUseCase>();
builder.Services.AddScoped<AddOperatorUseCase>();
builder.Services.AddScoped<AddDebrisObjectUseCase>();
builder.Services.AddScoped<RegisterOperatorUseCase>();
builder.Services.AddScoped<GetOperatorDashboardUseCase>();
builder.Services.AddScoped<GetHomeStatsUseCase>();
builder.Services.AddScoped<AcknowledgeAlertUseCase>();
builder.Services.AddScoped<ResolveConjunctionUseCase>();

builder.Services.AddScoped<ISatelliteRepository, SatelliteRepository>();
builder.Services.AddScoped<IDebrisObjectRepository, DebrisObjectRepository>();
builder.Services.AddScoped<IOperatorRepository, OperatorRepository>();
builder.Services.AddScoped<IUserOperatorRepository, UserOperatorRepository>();
builder.Services.AddScoped<IConjunctionEventRepository, ConjunctionEventRepository>();
builder.Services.AddScoped<IAlertRepository, AlertRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services
    .AddIdentity<AppUser,  AppRole>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddDefaultUI()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddControllersWithViews();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var dbContext = services.GetRequiredService<AppDbContext>();
    var userManager = services.GetRequiredService<UserManager<AppUser>>();
    var roleManager = services.GetRequiredService<RoleManager<AppRole>>();
    
    AppDataInit.MigrateDatabase(dbContext);
    AppDataInit.SeedIdentity(userManager, roleManager);
    AppDataInit.SeedAppData(dbContext);
}

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
        name: "area",
        pattern: "{area}/{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
    .WithStaticAssets();

app.Run();