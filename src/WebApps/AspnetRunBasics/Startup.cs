using AspnetRunBasics.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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


            services.AddHttpClientServices(Configuration);


            services.AddHttpContextAccessor();



            services.AddRazorPages();

            services.AddIdentityServerServices(Configuration);

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

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            if (env.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                //   app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            var options = new ForwardedHeadersOptions()
            {
                ForwardedHeaders = ForwardedHeaders.All
            };
            options.KnownProxies.Clear();
            options.KnownNetworks.Clear();
            app.UseForwardedHeaders(options);
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
