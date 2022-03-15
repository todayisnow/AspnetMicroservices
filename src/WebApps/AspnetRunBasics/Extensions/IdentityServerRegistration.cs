using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Threading.Tasks;

namespace AspnetRunBasics.Extensions
{

    public static class IdentityServerRegistration
    {
        public static IServiceCollection AddIdentityServerServices(this IServiceCollection services, IConfiguration Configuration)
        {
            IdentityModelEventSource.ShowPII = true;
            //services.Configure<CookiePolicyOptions>(options =>
            //{
            //    options.MinimumSameSitePolicy = SameSiteMode.Unspecified;
            //    options.Secure = CookieSecurePolicy.SameAsRequest;
            //    options.OnAppendCookie = cookieContext =>
            //        AuthenticationHelpers.CheckSameSite(cookieContext.Context, cookieContext.CookieOptions);
            //    options.OnDeleteCookie = cookieContext =>
            //        AuthenticationHelpers.CheckSameSite(cookieContext.Context, cookieContext.CookieOptions);
            //});
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;

                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultForbidScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignOutScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })

                    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme,
                        options =>
                        {
                            options.Cookie.Name = "AspWebApp";
                            options.AccessDeniedPath = "/denied";
                        })
                    .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
                    {
                        options.CallbackPath = "/signin-oidc";
                        options.Authority = Configuration["IdentityServer:Uri"];

                        //dev only
                        options.RequireHttpsMetadata = false;
                        options.ClientId = "aspnetRunBasics_client";
                        options.ClientSecret = "secret";

                        options.ResponseType = "code id_token";
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            NameClaimType = JwtClaimTypes.Name,
                            RoleClaimType = JwtClaimTypes.Role
                        };
                        options.Events = new OpenIdConnectEvents
                        {
                            OnMessageReceived = context => OnMessageReceived(context),

                            OnRedirectToIdentityProvider = context => OnRedirectToIdentityProvider(context, Configuration["IdentityServer:RedirectUri"]),
                            OnRemoteFailure = ctx =>
                            {
                                using var loggerFactory = LoggerFactory.Create(builder =>
                                {
                                    builder.SetMinimumLevel(LogLevel.Information);
                                    builder.AddConsole();
                                    builder.AddEventSourceLogger();
                                });
                                var logger = loggerFactory.CreateLogger("Startup");
                                logger.LogInformation("Hello World 2");
                                logger.LogInformation(ctx.Failure.Message);

                                if (ctx.Failure.InnerException != null)
                                    logger.LogInformation(ctx.Failure.InnerException.Message);
                                // React to the error here. See the notes below.
                                return Task.CompletedTask;
                            }
                        };

                        //options.Scope.Add("openid"); come automatically
                        // options.Scope.Add("profile");
                        options.Scope.Add("address");
                        options.Scope.Add("email");
                        options.Scope.Add("profile");
                        options.Scope.Add("roles");

                        options.Scope.Add("offline_access");

                        //options.ClaimActions.DeleteClaim("sid");
                        //options.ClaimActions.DeleteClaim("idp");
                        //options.ClaimActions.DeleteClaim("s_hash");
                        //options.ClaimActions.DeleteClaim("auth_time");
                        options.ClaimActions.MapJsonKey("role", "role", "role");

                        options.Scope.Add("catalogAPI");
                        options.Scope.Add("orderAPI");
                        options.Scope.Add("OcelotApiGw");
                        options.Scope.Add("basketAPI");


                        options.SaveTokens = true;
                        options.GetClaimsFromUserInfoEndpoint = true;



                    });
            return services;
        }
        private static Task OnMessageReceived(MessageReceivedContext context)
        {
            context.Properties.IsPersistent = true;
            context.Properties.ExpiresUtc = new DateTimeOffset(DateTime.Now.AddHours(12));
            // context.ProtocolMessage
            return Task.CompletedTask;
        }

        private static Task OnRedirectToIdentityProvider(RedirectContext context, string ruri)
        {
            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Information);
                builder.AddConsole();
                builder.AddEventSourceLogger();
            });
            var logger = loggerFactory.CreateLogger("Startup");
            logger.LogInformation("Hello World");
            //becasue ngix internal call convert to http
            context.ProtocolMessage.RedirectUri = ruri;


            return Task.CompletedTask;
        }
    }
}

