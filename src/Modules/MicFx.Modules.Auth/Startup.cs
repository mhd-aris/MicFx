using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using MicFx.Core.Modularity;
using MicFx.SharedKernel.Modularity;
using MicFx.SharedKernel.Interfaces;
using MicFx.Modules.Auth.Data;
using MicFx.Modules.Auth.Domain.Entities;
using MicFx.Modules.Auth.Domain.Configuration;
using MicFx.Modules.Auth.Services;

namespace MicFx.Modules.Auth
{
    /// <summary>
    /// Startup class untuk Auth module - Versi yang disederhanakan
    /// </summary>
    public class AuthStartup : ModuleStartupBase
    {
        public override IModuleManifest Manifest { get; } = new Manifest();

        private AuthConfig _config = new AuthConfig();

        public AuthStartup() : base(null)
        {
        }

        protected override void ConfigureModuleServices(IServiceCollection services)
        {
            // 1. Bind configuration dari appsettings.json atau gunakan default
            var serviceProvider = services.BuildServiceProvider();
            var configuration = serviceProvider.GetService<IConfiguration>();
            configuration?.GetSection("MicFx:Auth").Bind(_config);

            // Register config sebagai singleton
            services.AddSingleton(_config);

            // 2. Configure Database Context - menggunakan shared connection string
            services.AddDbContext<AuthDbContext>(options =>
            {
                var connectionString = configuration?.GetConnectionString("DefaultConnection");
                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new InvalidOperationException("Shared connection string 'DefaultConnection' not found in configuration.");
                }
                options.UseSqlServer(connectionString);
            });

            // 3. Configure ASP.NET Core Identity dengan konfigurasi
            services.AddIdentity<User, Role>(options =>
            {
                // Password settings dari config
                options.Password.RequireDigit = _config.Password.RequireDigit;
                options.Password.RequiredLength = _config.Password.RequiredLength;
                options.Password.RequireNonAlphanumeric = _config.Password.RequireNonAlphanumeric;
                options.Password.RequireUppercase = _config.Password.RequireUppercase;
                options.Password.RequireLowercase = _config.Password.RequireLowercase;
                options.Password.RequiredUniqueChars = _config.Password.RequiredUniqueChars;

                // Lockout settings dari config
                options.Lockout.DefaultLockoutTimeSpan = _config.Lockout.DefaultLockoutTimeSpan;
                options.Lockout.MaxFailedAccessAttempts = _config.Lockout.MaxFailedAccessAttempts;
                options.Lockout.AllowedForNewUsers = _config.Lockout.AllowedForNewUsers;

                // User settings
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = false;
            })
            .AddEntityFrameworkStores<AuthDbContext>()
            .AddDefaultTokenProviders();

            // 4. Configure Authentication Cookies dengan konfigurasi lengkap
            services.ConfigureApplicationCookie(options =>
            {
                // Path configuration
                options.LoginPath = "/auth/login";
                options.LogoutPath = "/auth/logout";
                options.AccessDeniedPath = "/auth/access-denied";
                options.ReturnUrlParameter = "returnUrl";
                
                // Cookie configuration dari AuthConfig
                options.ExpireTimeSpan = _config.Cookie.ExpireTimeSpan;
                options.SlidingExpiration = _config.Cookie.SlidingExpiration;
                options.Cookie.Name = _config.Cookie.CookieName;
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.SameAsRequest;
                
                // Event handlers untuk proper redirect handling
                options.Events.OnRedirectToLogin = context =>
                {
                    var returnUrl = context.Request.Path + context.Request.QueryString;
                    context.Response.Redirect($"{options.LoginPath}?returnUrl={Uri.EscapeDataString(returnUrl)}");
                    return Task.CompletedTask;
                };
                
                options.Events.OnRedirectToAccessDenied = context =>
                {
                    var returnUrl = context.Request.Path + context.Request.QueryString;
                    context.Response.Redirect($"{options.AccessDeniedPath}?returnUrl={Uri.EscapeDataString(returnUrl)}");
                    return Task.CompletedTask;
                };
            });

            // 5. Configure Authorization Policies
            services.ConfigureAuthorizationPolicies(_config);

            // 6. Register Services
            services.AddScoped<IAuthService, AuthService>();
            services.AddHostedService<AuthDatabaseInitializer>();
            
            // 7. Register Module Seeder untuk data initialization
            services.AddScoped<IModuleSeeder, AuthModuleSeeder>();
            
            // 8. Register Health Check
            services.AddHealthChecks()
                .AddCheck<AuthHealthCheck>("auth_module", tags: new[] { "auth", "database", "identity" });
            
            // Note: Admin Navigation Contributor akan di-register otomatis oleh AdminModuleScanner
        }

        protected override void ConfigureModuleEndpoints(Microsoft.AspNetCore.Routing.IEndpointRouteBuilder endpoints)
        {
            // Gunakan auto-mapping dari base class, tidak perlu hardcode routes
            // Route patterns akan dibuat otomatis berdasarkan _config.RoutePrefix
        }
    }
}