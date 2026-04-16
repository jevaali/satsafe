using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using App.Application.Features.Alerts.UseCases;
using App.Application.Features.Conjunctions.UseCases;
using App.Application.Features.Dashboard.UseCases;
using App.Application.Features.Debris.UseCases;
using App.Application.Features.Notifications.UseCases;
using App.Application.Features.Operators.UseCases;
using App.Application.Features.Satellites.UseCases;
using App.Application.Features.Users.UseCases;
using App.Application.Interfaces;
using App.Application.Options;
using App.Infrastructure.ExternalServices;
using App.Application.Interfaces.IRepositories;
using App.Domain.Entities.Identity;
using App.Domain.Services;
using App.Infrastructure.Persistence;
using App.Infrastructure.Repositories;
using App.Infrastructure.Seeding;
using App.Infrastructure.UnitOfWork;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using WebApp.Infrastructure;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.SwaggerGen;
using WebApp;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                       throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

/*
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));
*/

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(
            connectionString,
            o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery))
        .ConfigureWarnings(w =>
            w.Throw(RelationalEventId.MultipleCollectionIncludeWarning))
        .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTrackingWithIdentityResolution);

    if (!builder.Environment.IsProduction())
    {
        options.EnableDetailedErrors()
            .EnableSensitiveDataLogging();
    }
});

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddScoped<AddSatelliteUseCase>();
builder.Services.AddScoped<UpdateSatelliteUseCase>();
builder.Services.AddScoped<DeleteSatelliteUseCase>();
builder.Services.AddScoped<AddOperatorUseCase>();
builder.Services.AddScoped<AddDebrisObjectUseCase>();
builder.Services.AddScoped<WebApp.UseCases.LoginUseCase>();
builder.Services.AddScoped<RegisterUserUseCase>();
builder.Services.AddScoped<RegisterOperatorUseCase>();
builder.Services.AddScoped<GetOperatorDashboardUseCase>();
builder.Services.AddScoped<GetOperatorSatellitesUseCase>();
builder.Services.AddScoped<GetSatelliteByIdUseCase>();
builder.Services.AddScoped<GetOperatorConjunctionsUseCase>();
builder.Services.AddScoped<GetConjunctionByIdUseCase>();
builder.Services.AddScoped<GetOperatorAlertsUseCase>();
builder.Services.AddScoped<GetAlertByIdUseCase>();
builder.Services.AddScoped<GetHomeStatsUseCase>();
builder.Services.AddScoped<AcknowledgeAlertUseCase>();
builder.Services.AddScoped<ResolveConjunctionUseCase>();
builder.Services.AddScoped<AcknowledgeConjunctionSideUseCase>();
builder.Services.AddScoped<RefreshConjunctionStatusesUseCase>();
builder.Services.AddScoped<GetMainListUseCase>();
builder.Services.AddScoped<RunConjunctionScreeningUseCase>();
builder.Services.AddScoped<GetSettingsUseCase>();
builder.Services.AddScoped<GetUserContextUseCase>();
builder.Services.AddScoped<UpdateOperatorSettingsUseCase>();
builder.Services.AddScoped<AddHelperUseCase>();
builder.Services.AddScoped<RemoveHelperUseCase>();
builder.Services.AddScoped<SaveNotificationsUseCase>();
builder.Services.AddScoped<UpdateAlertMessageUseCase>();
builder.Services.AddScoped<SaveDraftSatelliteUseCase>();
builder.Services.AddScoped<GetUserDraftsUseCase>();
builder.Services.AddScoped<DeleteDraftSatelliteUseCase>();
builder.Services.AddScoped<GetOperatorActionLogsUseCase>();

builder.Services.AddScoped<ImportTleFromCelesTrakUseCase>();
builder.Services.AddScoped<AddTleManuallyUseCase>();

builder.Services.Configure<CelesTrakOptions>(
    builder.Configuration.GetSection(CelesTrakOptions.Section));

builder.Services.AddHttpClient<ICelesTrakService, CelesTrakService>(client =>
{
    client.BaseAddress = new Uri("https://celestrak.org");
    client.DefaultRequestHeaders.Add("User-Agent", "SSA-Platform/1.0 (educational)");
    client.Timeout = TimeSpan.FromSeconds(15);
});

builder.Services.AddScoped<ConjunctionAnalysisService>();
builder.Services.AddScoped<SatelliteRiskScoringService>();
builder.Services.AddScoped<CalculateSatelliteRiskUseCase>();

builder.Services.AddScoped<ISatelliteRepository, SatelliteRepository>();
builder.Services.AddScoped<IDebrisObjectRepository, DebrisObjectRepository>();
builder.Services.AddScoped<IOperatorRepository, OperatorRepository>();
builder.Services.AddScoped<IUserOperatorRepository, UserOperatorRepository>();
builder.Services.AddScoped<IConjunctionEventRepository, ConjunctionEventRepository>();
builder.Services.AddScoped<IAlertRepository, AlertRepository>();
builder.Services.AddScoped<ISpaceObjectsRepository, SpaceObjectsRepository>();
builder.Services.AddScoped<INotificationSettingRepository, NotificationSettingRepository>();
builder.Services.AddScoped<IDraftSatelliteRepository, DraftSatelliteRepository>();
builder.Services.AddScoped<IRiskAssessmentRepository, RiskAssessmentRepository>();
builder.Services.AddScoped<IOperatorActionLogRepository, OperatorActionLogRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<UserOperatorRepository>();
builder.Services.AddScoped<LogOperatorActionFilter>();

builder.Services
    .AddIdentity<AppUser,  AppRole>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddDefaultUI()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear(); // => remove default claims
builder.Services
    .AddAuthentication()
    .AddCookie(options => { options.SlidingExpiration = true; })
    .AddJwtBearer(cfg =>
    {
        cfg.RequireHttpsMetadata = false; // TODO: set to true in production!
        cfg.SaveToken = true;
        cfg.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = builder.Configuration["JWT:Issuer"],
            ValidAudience = builder.Configuration["JWT:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"]!)),
            ClockSkew = TimeSpan.Zero // remove delay of token when expire
        };
    });


builder.Services.AddControllersWithViews(options =>
{
    options.ModelBinderProviders.Insert(0, new InvariantDoubleModelBinderProvider());
    options.Filters.Add<LogOperatorActionFilter>();
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

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsAllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod()
            .WithExposedHeaders("X-Version", "X-Version-Created-At");
    });
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

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
builder.Services.AddSwaggerGen();

// Add authorization policies

builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, ApiAuthorizationMiddlewareResultHandler>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("OperatorPolicy", policy =>
    {
        policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
        policy.RequireAuthenticatedUser();
        policy.RequireRole("Operator", "OperatorHelper");
    });

    options.AddPolicy("OperatorOnlyPolicy", policy =>
    {
        policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
        policy.RequireAuthenticatedUser();
        policy.RequireRole("Operator");
    });
});

var app = builder.Build();

SetupAppData(app, app.Environment, app.Configuration);

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var dbContext = services.GetRequiredService<AppDbContext>();
    var userManager = services.GetRequiredService<UserManager<AppUser>>();
    var roleManager = services.GetRequiredService<RoleManager<AppRole>>();
    
    //AppDataInit.MigrateDatabase(dbContext);
    //AppDataInit.SeedIdentity(userManager, roleManager);
    //AppDataInit.SeedAppData(dbContext, userManager);
    dbContext.ChangeTracker.Clear();

    var riskUseCase = services.GetRequiredService<CalculateSatelliteRiskUseCase>();
    await riskUseCase.CalculateForAllNewAsync();
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

app.UseCors("CorsAllowAll");

app.UseRouting();

app.UseAuthorization();


app.UseSwagger();
app.UseSwaggerUI(options =>
{
    var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
    foreach (var description in provider.ApiVersionDescriptions)
    {
        options.SwaggerEndpoint(
            $"/swagger/{description.GroupName}/swagger.json",
            description.GroupName.ToUpperInvariant()
        );
    }
    // serve from root
    // options.RoutePrefix = string.Empty;
});


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


static void SetupAppData(IApplicationBuilder app, IWebHostEnvironment env, IConfiguration configuration)
{
    using var serviceScope = ((IApplicationBuilder)app).ApplicationServices
        .GetRequiredService<IServiceScopeFactory>()
        .CreateScope();
    var logger = serviceScope.ServiceProvider.GetRequiredService<ILogger<IApplicationBuilder>>();

    using var context = serviceScope.ServiceProvider.GetRequiredService<AppDbContext>();

    WaitDbConnection(context, logger);

    using var userManager = serviceScope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
    using var roleManager = serviceScope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>();

    if (configuration.GetValue<bool>("DataInitialization:DropDatabase"))
    {
        logger.LogWarning("DropDatabase");
        AppDataInit.DeleteDatabase(context);
    }

    if (configuration.GetValue<bool>("DataInitialization:MigrateDatabase"))
    {
        logger.LogInformation("MigrateDatabase");
        AppDataInit.MigrateDatabase(context);
    }

    if (configuration.GetValue<bool>("DataInitialization:SeedIdentity"))
    {
        logger.LogInformation("SeedIdentity");
        AppDataInit.SeedIdentity(userManager, roleManager);
    }

    if (configuration.GetValue<bool>("DataInitialization:SeedData"))
    {
        logger.LogInformation("SeedData");
        AppDataInit.SeedAppData(context, userManager);
    }
}

static void WaitDbConnection(AppDbContext ctx, ILogger<IApplicationBuilder> logger)
{
    while (true)
    {
        try
        {
            ctx.Database.OpenConnection();
            ctx.Database.CloseConnection();
            return;
        }
        catch (Npgsql.PostgresException e)
        {
            logger.LogWarning("Checked postgres db connection. Got: {}", e.Message);

            if (e.Message.Contains("does not exist"))
            {
                logger.LogWarning("Applying migration, probably db is not there (but server is)");
                return;
            }

            logger.LogWarning("Waiting for db connection. Sleep 1 sec");
            System.Threading.Thread.Sleep(1000);
        }
    }
}
