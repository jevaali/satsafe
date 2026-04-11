using System.Globalization;
using App.Application.Interfaces;
using App.Application.Interfaces.IRepositories;
using App.Application.UseCases;
using App.Domain.Entities.Identity;
using App.Domain.Services;
using App.Infrastructure.Persistence;
using App.Infrastructure.Repositories;
using App.Infrastructure.Seeding;
using App.Infrastructure.UnitOfWork;
using Asp.Versioning;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using WebApp.Infrastructure;
using Microsoft.Extensions.Options;

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
builder.Services.AddScoped<AcknowledgeConjunctionSideUseCase>();
builder.Services.AddScoped<RefreshConjunctionStatusesUseCase>();
builder.Services.AddScoped<GetMainListUseCase>();
builder.Services.AddScoped<RunConjunctionScreeningUseCase>();
builder.Services.AddScoped<GetSettingsUseCase>();
builder.Services.AddScoped<UpdateOperatorSettingsUseCase>();
builder.Services.AddScoped<AddHelperUseCase>();
builder.Services.AddScoped<RemoveHelperUseCase>();
builder.Services.AddScoped<SaveNotificationsUseCase>();
builder.Services.AddScoped<UpdateAlertMessageUseCase>();
builder.Services.AddScoped<SaveDraftSatelliteUseCase>();
builder.Services.AddScoped<GetUserDraftsUseCase>();
builder.Services.AddScoped<DeleteDraftSatelliteUseCase>();

builder.Services.AddScoped<ConjunctionAnalysisService>();

builder.Services.AddScoped<ISatelliteRepository, SatelliteRepository>();
builder.Services.AddScoped<IDebrisObjectRepository, DebrisObjectRepository>();
builder.Services.AddScoped<IOperatorRepository, OperatorRepository>();
builder.Services.AddScoped<IUserOperatorRepository, UserOperatorRepository>();
builder.Services.AddScoped<IConjunctionEventRepository, ConjunctionEventRepository>();
builder.Services.AddScoped<IAlertRepository, AlertRepository>();
builder.Services.AddScoped<ISpaceObjectsRepository, SpaceObjectsRepository>();
builder.Services.AddScoped<INotificationSettingRepository, NotificationSettingRepository>();
builder.Services.AddScoped<IDraftSatelliteRepository, DraftSatelliteRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services
    .AddIdentity<AppUser,  AppRole>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddDefaultUI()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddControllersWithViews(options =>
{
    options.ModelBinderProviders.Insert(0, new InvariantDoubleModelBinderProvider());
});

var supportedCultures = builder.Configuration
    .GetSection("SupportedCultures")
    .GetChildren()
    .Select(x => new CultureInfo(x.Value!))
    .ToArray();

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    // datetime and currency support
    options.SupportedCultures = supportedCultures;
    // UI translated strings
    options.SupportedUICultures = supportedCultures;
    // if nothing is found, use this
    options.DefaultRequestCulture = new RequestCulture("et-EE", "et-EE");
    options.SetDefaultCulture("et-EE");

    options.RequestCultureProviders = new List<IRequestCultureProvider>
    {
        // Order is important, it's in which order they will be evaluated
        new QueryStringRequestCultureProvider(),
        new CookieRequestCultureProvider()
    };
});


var apiVersioningBuilder = builder.Services.AddApiVersioning(options =>
{
    options.ReportApiVersions = true;
    // in case of no explicit version
    options.DefaultApiVersion = new ApiVersion(1, 0);
});

apiVersioningBuilder.AddApiExplorer(options =>
{
    // add the versioned api explorer, which also adds IApiVersionDescriptionProvider service
    // note: the specified format code will format the version as "'v'major[.minor][-status]"
    options.GroupNameFormat = "'v'VVV";

    // note: this option is only necessary when versioning by url segment. the SubstitutionFormat
    // can also be used to control the format of the API version in route templates
    options.SubstituteApiVersionInUrl = true;
});


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var dbContext = services.GetRequiredService<AppDbContext>();
    var userManager = services.GetRequiredService<UserManager<AppUser>>();
    var roleManager = services.GetRequiredService<RoleManager<AppRole>>();
    
    AppDataInit.MigrateDatabase(dbContext);
    AppDataInit.SeedIdentity(userManager, roleManager);
    AppDataInit.SeedAppData(dbContext, userManager);
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


app.UseRequestLocalization(options: app.Services
    .GetService<IOptions<RequestLocalizationOptions>>()!.Value);


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