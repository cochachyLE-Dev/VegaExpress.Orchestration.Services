using Akavache;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

using System.Text;
using System.Text.Json;
using VegaExpress.Worker.Core.Common;
using VegaExpress.Worker.Core.Models.Settings;
using VegaExpress.Worker.Core.Persistence;
using VegaExpress.Worker.Core.Persistence.Contracts;
using VegaExpress.Worker.Core.Services;
using VegaExpress.Worker.Core.Services.Contracts;

namespace VegaExpress.Worker.Client.Extentions
{
    public static class ConfigureContainer
    {
        public static void ConfigureSwagger(this IApplicationBuilder app)
        {
            app.UseSwagger();

            app.UseSwaggerUI(setupAction =>
            {
                setupAction.SwaggerEndpoint("/swagger/OpenAPISpecification/swagger.json", "VegaExoress.Worker API");
                setupAction.RoutePrefix = "OpenAPI";
            });
        }
    }
    public static class DependencyInjection
    {
        public static IServiceCollection AddCurrentUserService(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            return services;
        }
        public static void AddDbContext(this IServiceCollection serviceCollection, IConfiguration configuration)
        {
            serviceCollection.AddDbContext<IRepositoryDbContext, RepositoryDbContext>(o => o.UseNpgsql(configuration.GetConnectionString("StorageDB")));
        }
        public static void AddScopedServices(this IServiceCollection serviceCollection)
        {            
            serviceCollection.AddScoped<IIdentityDbContext>(provider => provider.GetService<IdentityDbContext>()!);
            serviceCollection.AddScoped<IRepositoryDbContext>(provider => provider.GetService<RepositoryDbContext>()!);
            serviceCollection.AddScoped<ILocalStorageService, LocalStorageService>();
        }
        public static void AddSingletonServices(this IServiceCollection serviceCollection)
        {
            BlobCache.ApplicationName = "Client";
            serviceCollection.AddSingleton(typeof(IBlobCache), BlobCache.LocalMachine);
        }
        public static void AddTransientServices(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IDateTimeService, DateTimeService>();
            serviceCollection.AddTransient<IAccountService, AccountService>();
        }
        public static void AddIdentityService(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<JWTSettings>(configuration.GetSection("JWTSettings"));
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(o =>
                {
                    o.RequireHttpsMetadata = false;
                    o.SaveToken = false;
                    o.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero,
                        ValidIssuer = configuration["JWTSettings:Issuer"],
                        ValidAudience = configuration["JWTSettings:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWTSettings:Key"]!))
                    };
                    o.Events = new JwtBearerEvents()
                    {
                        OnAuthenticationFailed = context =>
                        {
                            if (context.Exception is SecurityTokenExpiredException)
                            {
                                context.Response.StatusCode = 401;
                                context.Response.ContentType = "application/json";
                                context.Response.Headers.Add("Token-Expired", "true");
                                var result = JsonSerializer.Serialize(new Response(StatusCode.PermissionDenied, "Token Expired"));
                                return context.Response.WriteAsync(result);
                            }
                            else
                            {
                                context.NoResult();
                                context.Response.StatusCode = 500;
                                context.Response.ContentType = "text/plain";
                                var result = JsonSerializer.Serialize(new Response(StatusCode.InvalidArgument, context.Exception.ToString()));
                                return context.Response.WriteAsync(result);
                            }
                        },
                        OnChallenge = context =>
                        {
                            context.HandleResponse();
                            context.Response.StatusCode = 401;
                            context.Response.ContentType = "application/json";
                            var result = JsonSerializer.Serialize(new Response(StatusCode.Unauthenticated, "You are not Authorized"));
                            return context.Response.WriteAsync(result);
                        },
                        OnForbidden = context =>
                        {
                            context.Response.StatusCode = 403;
                            context.Response.ContentType = "application/json";
                            var result = JsonSerializer.Serialize(new Response(StatusCode.PermissionDenied, "You are not authorized to access this resource"));
                            return context.Response.WriteAsync(result);
                        },
                    };
                });
        }

        public static void AddSwaggerOpenAPI(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSwaggerGen(setupAction =>
            {
                setupAction.SwaggerDoc(
                    "OpenAPISpecification",
                    new OpenApiInfo()
                    {
                        Title = "VegaExpress.Worker Services WebAPI",
                        Version = "1",
                        Description = "..."
                    });

                setupAction.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Description = $"Input your Bearer token in this format - Bearer token to access this API",
                });
                setupAction.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer",
                            },
                        }, new List<string>()
                    },
                });
            });
        }
    }
}
