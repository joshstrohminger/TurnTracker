using System;
using System.Linq;
using AutoMapper;
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
                options.InputFormatters.Insert(0, new StringBodyInputFormatter());
            }).SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
            services.AddResponseCompression();

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddMaps(AppDomain.CurrentDomain.GetAssemblies().Where(x => x.GetName().Name.StartsWith("TurnTracker.")));
            });
            mapperConfig.AssertConfigurationIsValid();
            services.AddSingleton(mapperConfig.CreateMapper());

            var appSettingsSection = _configuration.GetSection("AppSettings");
            services.Configure<AppSettings>(appSettingsSection);
            var appSettings = appSettingsSection.Get<AppSettings>();

            services.AddDbContext<TurnContext>(options => options.UseSqlServer(_configuration.GetConnectionString("SQL")));

            services.AddAuthorization(x =>
            {
                x.AddPolicy(nameof(PolicyType.Refresh), policy => policy.RequireClaim(nameof(ClaimType.RefreshKey)));
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

            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ITurnService, TurnService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IResourceAuthorizationService, ResourceAuthorizationService>();
            services.AddScoped<IPushNotificationService, PushNotificationService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IOptions<AppSettings> appSettings, ILogger<Startup> logger)
        {
            using (var serviceScope = app.ApplicationServices
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                using var context = serviceScope.ServiceProvider.GetService<TurnContext>();
                context.Database.Migrate();

                if (appSettings.Value.Seed)
                {
                    logger.LogInformation("Seeding database");
                    var result = serviceScope.ServiceProvider.GetRequiredService<IUserService>().EnsureSeedUsers();
                    if (result.IsFailure)
                    {
                        throw new Exception($"Failed to seed users: {result.Error}");
                    }

                    result = serviceScope.ServiceProvider.GetRequiredService<ITurnService>().EnsureSeedActivities();
                    if (result.IsFailure)
                    {
                        throw new Exception($"Failed to seed activities: {result.Error}");
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

            //app.UseCors(x => x
            //    .AllowAnyOrigin()
            //    .AllowAnyMethod()
            //    .AllowAnyHeader()
            //    .AllowCredentials());
            app.UseResponseCompression();
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints => endpoints.MapControllers());

            logger.LogInformation($"Started version '{AppHelper.Version}'");
        }
    }
}
