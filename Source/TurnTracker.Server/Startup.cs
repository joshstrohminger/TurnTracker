using System;
using System.IO;
using System.Linq;
using System.Net;
using AutoMapper;
using CSharpFunctionalExtensions;
using Fido2NetLib;
using JetBrains.Annotations;
using Lib.Net.Http.WebPush;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using TurnTracker.Common;
using TurnTracker.Data;
using TurnTracker.Domain.Authorization;
using TurnTracker.Domain.Configuration;
using TurnTracker.Domain.HealthChecks;
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
                .AddNewtonsoftJson();
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

            services.AddHealthChecks()
                .AddCheck<DatabaseHealthCheck>("Database");

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

                // Repair any activities that have no participants by creating a single participant from the activity owner
                var (_, repairFailed, repairError) = serviceScope.ServiceProvider.GetRequiredService<ITurnService>().CreateMissingParticipants();
                if (repairFailed)
                {
                    throw new Exception($"Failed to create missing participants: {repairError}");
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

            app.UseHealthChecks("/health");

            // rewrite / to /index.html
            app.UseDefaultFiles();

            // serve static files for the SPA
            app.UseSpaStaticFiles();

            // use auth and mvc
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints => endpoints.MapControllers());

            // temporary logging to discover where some requests are coming from
            app.Use(async (context, next) =>
            {
                if (context.Request.Path.StartsWithSegments("/admin") || context.Request.Method.Equals(WebRequestMethods.Http.Post, StringComparison.OrdinalIgnoreCase))
                {
                    var headers = string.Join('\n',
                        context.Request.Headers.Select(kvp => $"    {kvp.Key}: {kvp.Value}"));
                    using var reader = new StreamReader(context.Request.Body);
                    var body = await reader.ReadToEndAsync();
                    logger.LogWarning($"{context.Request.Method}: {context.Request.Path}\nHeaders:\n{headers}\nBody:\n{body}");
                }

                await next.Invoke();
            });

            // only GET methods are supported beyond this point since we should only be returning the default page when we get to app.useSpa
            app.Use(async (context, next) =>
            {
                if (!context.Request.Method.Equals(WebRequestMethods.Http.Get, StringComparison.OrdinalIgnoreCase))
                {
                    context.Response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
                    context.Response.Headers[HeaderNames.Allow] = WebRequestMethods.Http.Get;
                    return;
                }

                await next.Invoke();
            });

            // serve the default spa page if nothing else consumes the request
            app.UseSpa(_ => {});

            logger.LogInformation($"Started version '{AppHelper.Version}'");
        }
    }
}
