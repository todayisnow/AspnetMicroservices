using AspnetRunBasics.HttpHandlers;
using AspnetRunBasics.Services;
using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using System;
using System.Net;
using System.Threading.Tasks;

namespace AspnetRunBasics
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {



            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                                       | SecurityProtocolType.Tls11
                                       | SecurityProtocolType.Tls12
                                       | SecurityProtocolType.Tls13;

            //services.AddTransient<LoggingDelegatingHandler>();
            IdentityModelEventSource.ShowPII = true;

            services.AddTransient<AuthenticationDelegatingHandler>();

            services.AddHttpClient<ICatalogService, CatalogService>(c =>
            {
                c.BaseAddress = new Uri(Configuration["ApiSettings:GatewayAddress"]);
                c.DefaultRequestHeaders.Clear();
                c.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/json");


            }).AddHttpMessageHandler<AuthenticationDelegatingHandler>();
            //.AddHttpMessageHandler<LoggingDelegatingHandler>()
            //.AddPolicyHandler(GetRetryPolicy())
            //.AddPolicyHandler(GetCircuitBreakerPolicy());

            services.AddHttpClient<IBasketService, BasketService>(c =>
            {
                c.BaseAddress = new Uri(Configuration["ApiSettings:GatewayAddress"]);
                c.DefaultRequestHeaders.Clear();
                c.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/json");


            }).AddHttpMessageHandler<AuthenticationDelegatingHandler>();
            //.AddHttpMessageHandler<LoggingDelegatingHandler>()
            //.AddPolicyHandler(GetRetryPolicy())
            //.AddPolicyHandler(GetCircuitBreakerPolicy());

            services.AddHttpClient<IOrderService, OrderService>(c =>
            {
                c.BaseAddress = new Uri(Configuration["ApiSettings:GatewayAddress"]);
                c.DefaultRequestHeaders.Clear();
                c.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/json");


            }).AddHttpMessageHandler<AuthenticationDelegatingHandler>();
            //.AddHttpMessageHandler<LoggingDelegatingHandler>()
            //.AddPolicyHandler(GetRetryPolicy())
            //.AddPolicyHandler(GetCircuitBreakerPolicy());


            services.AddHttpClient("IDPClient", client =>
            {
                client.BaseAddress = new Uri(Configuration["IdentityServer:Uri"]);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/json");
            });
            //services.AddSingleton(new ClientCredentialsTokenRequest
            //{                                                
            //    Address = "https://localhost:5005/connect/token",
            //    ClientId = "movieClient",
            //    ClientSecret = "secret",
            //    Scope = "movieAPI"
            //});
            services.AddHttpContextAccessor();



            services.AddRazorPages();

            services.Configure<CookiePolicyOptions>(options =>
            {
                options.MinimumSameSitePolicy = Microsoft.AspNetCore.Http.SameSiteMode.Unspecified;
                options.Secure = CookieSecurePolicy.SameAsRequest;
                options.OnAppendCookie = cookieContext =>
                    AuthenticationHelpers.CheckSameSite(cookieContext.Context, cookieContext.CookieOptions);
                options.OnDeleteCookie = cookieContext =>
                    AuthenticationHelpers.CheckSameSite(cookieContext.Context, cookieContext.CookieOptions);
            });
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = "oidc";

                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultForbidScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignOutScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })

                    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme,
                        options =>
                        {
                            options.Cookie.Name = "AspWebApp";
                        })
                    .AddOpenIdConnect("oidc", options =>
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

            services.AddHsts(opt =>
            {
                opt.Preload = true;
                opt.IncludeSubDomains = true;
                opt.MaxAge = TimeSpan.FromDays(365);

                //options.Security.HstsConfigureAction?.Invoke(opt);
            });

            //services.AddHealthChecks()
            //    .AddUrlGroup(new Uri(Configuration["ApiSettings:GatewayAddress"]), "Ocelot API Gw", HealthStatus.Degraded);
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
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }



            // app.UseHsts();


            //app.UsePathBase("");

            //// Add custom security headers
            //app.UseSecurityHeaders(new List<string> { "fonts.googleapis.com",
            //"fonts.gstatic.com",
            //"www.gravatar.com" });



            app.UseStaticFiles();







            //app.UseForwardedHeaders(new ForwardedHeadersOptions
            //{
            //    ForwardedHeaders = ForwardedHeaders.XForwardedProto
            //});
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();

                //endpoints.MapHealthChecks("/hc", new HealthCheckOptions()
                //{
                //    Predicate = _ => true,
                //    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                //});
            });
        }


        //private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        //{
        //    // In this case will wait for
        //    //  2 ^ 1 = 2 seconds then
        //    //  2 ^ 2 = 4 seconds then
        //    //  2 ^ 3 = 8 seconds then
        //    //  2 ^ 4 = 16 seconds then
        //    //  2 ^ 5 = 32 seconds

        //    return HttpPolicyExtensions
        //        .HandleTransientHttpError()
        //        .WaitAndRetryAsync(
        //            retryCount: 5,
        //            sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
        //            onRetry: (exception, retryCount, context) =>
        //            {
        //                Log.Error($"Retry {retryCount} of {context.PolicyKey} at {context.OperationKey}, due to: {exception}.");
        //            });
        //}

        //private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
        //{
        //    return HttpPolicyExtensions
        //        .HandleTransientHttpError()
        //        .CircuitBreakerAsync(
        //            handledEventsAllowedBeforeBreaking: 5,
        //            durationOfBreak: TimeSpan.FromSeconds(30)
        //        );
        //}
    }
}
