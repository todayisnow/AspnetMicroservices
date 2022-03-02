using AspnetRunBasics.HttpHandlers;
using AspnetRunBasics.Services;
using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using System;
using System.Net;

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

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })

                    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
                    {

                        options.AccessDeniedPath = "/Account/denied";
                    })
                    .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
                    {
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



            //services.AddHealthChecks()
            //    .AddUrlGroup(new Uri(Configuration["ApiSettings:GatewayAddress"]), "Ocelot API Gw", HealthStatus.Degraded);
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
            app.UseHttpsRedirection();
            app.UseStaticFiles();

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
