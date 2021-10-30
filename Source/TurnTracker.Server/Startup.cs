using System;
using System.Linq;
using AutoMapper;
using CSharpFunctionalExtensions;
using Fido2NetLib;
using JetBrains.Annotations;
using Lib.Net.Http.WebPush;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TurnTracker.Common;
using TurnTracker.Data;
using TurnTracker.Domain.Authorization;
using TurnTracker.Domain.Configuration;
using TurnTracker.Domain.HostedServices;
using TurnTracker.Domain.Interfaces;
using TurnTracker.Domain.Services;
using TurnTracker.Server.Utilities;

namespace TurnTracker.Server
{
    public class Startup
    {
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            _env = env;
            _configuration = configuration;
        }
        
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors();
            services.AddMvc(options =>
            {
                options.InputFormatters.Insert(0, new BoolBodyInputFormatter());
                options.InputFormatters.Insert(0, new ByteBodyInputFormatter());
                options.InputFormatters.Insert(0, new StringBodyInputFormatter());
            })
                // newtonsoft is necessary if we want to use the fido2 library since it relies heavily on it
                .AddNewtonsoftJson()
                .SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
            services.AddResponseCompression();
            services.AddMemoryCache();

            services.Configure<Fido2Configuration>(_configuration.GetSection("fido2"));
            services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<Fido2Configuration>>().Value);
            services.AddTransient<IFido2, Fido2>();

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddMaps(AppDomain.CurrentDomain.GetAssemblies().Where(x => x.GetName().Name?.StartsWith("TurnTracker.") ?? false));
            });
            mapperConfig.AssertConfigurationIsValid();
            services.AddSingleton(mapperConfig.CreateMapper());

            var appSettingsSection = _configuration.GetSection("AppSettings");
            services.Configure<AppSettings>(appSettingsSection);
            var appSettings = appSettingsSection.Get<AppSettings>();

            services.AddDbContext<TurnContext>(options => options.UseSqlServer(_configuration.GetConnectionString("SQL")));

            services.AddAuthorization(x =>
            {
                x.AddPolicy(nameof(PolicyType.CanRefreshSession),
                    policy => policy.RequireClaim(nameof(ClaimType.RefreshKey)));
                x.AddPolicy(nameof(PolicyType.CanActOnNotification),
                    policy => policy
                        .RequireClaim(nameof(ClaimType.NotificationAction))
                        .RequireClaim(nameof(ClaimType.ParticipantId)));
            });

            // configure jwt authentication
            services.AddAuthentication(x =>
                {
                    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(x =>
                {
                    x.RequireHttpsMetadata = _env.IsProduction();
                    x.SaveToken = true;
                    x.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(appSettings.GetSecretBytes()),
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        ClockSkew = appSettings.JwtClockSkew
                    };
                });

            services.AddSpaStaticFiles(config =>
            {
                config.RootPath = "wwwroot";
            });

            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ITurnService, TurnService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IResourceAuthorizationService, ResourceAuthorizationService>();
            services.AddScoped<IPushSubscriptionService, PushSubscriptionService>();
            services.AddScoped<IPushNotificationService, PushNotificationService>();
            services.AddScoped<IPushNotificationActionService, PushNotificationActionService>();
            services.AddScoped<IWebAuthnService, WebAuthnService>();
            services.AddHttpClient<PushServiceClient>();
            services.AddHostedService<ActivityStatusChecker>();
            services.AddHostedService<Pruner>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        [UsedImplicitly]
        public void Configure(IApplicationBuilder app, IOptions<AppSettings> appSettings, ILogger<Startup> logger)
        {
            using (var serviceScope = app.ApplicationServices
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                using var context = serviceScope.ServiceProvider.GetRequiredService<TurnContext>();
                context.Database.Migrate();

                var (_, defaultUsersFailed, defaultUsersError) = serviceScope.ServiceProvider.GetRequiredService<IUserService>().EnsureDefaultUsers();
                if (defaultUsersFailed)
                {
                    throw new Exception($"Failed to ensure default users: {defaultUsersError}");
                }

                if (appSettings.Value.Seed)
                {
                    var (_, seedActivitiesFailed, seedActivitiesError) = serviceScope.ServiceProvider.GetRequiredService<ITurnService>().EnsureSeedActivitiesAsync().GetAwaiter().GetResult();
                    if (seedActivitiesFailed)
                    {
                        throw new Exception($"Failed to seed activities: {seedActivitiesError}");
                    }
                }
            }

            if (_env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
                app.UseHttpsRedirection();
            }

            app.UseResponseCompression();

            // rewrite / to /index.html
            app.UseDefaultFiles();

            // serve static files for the SPA
            app.UseSpaStaticFiles();

            // use auth and mvc
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints => endpoints.MapControllers());

            // serve the default spa page if nothing else consumes the request
            app.UseSpa(_ => {});

            logger.LogInformation($"Started version '{AppHelper.Version}'");
        }
    }
}
