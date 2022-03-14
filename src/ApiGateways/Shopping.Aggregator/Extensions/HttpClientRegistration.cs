using Common.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using Serilog;
using Shopping.Aggregator.HttpHandlers;
using Shopping.Aggregator.Services;
using System;
using System.Net.Http;

namespace Shopping.Aggregator.Extensions
{
    public static class HttpClientRegistration
    {
        public static IServiceCollection AddHttpClientServices(this IServiceCollection services, IConfiguration Configuration)
        {
            services.AddTransient<LoggingDelegatingHandler>();
            services.AddTransient<AuthenticationDelegatingHandler>();


            services.AddHttpClient<ICatalogService, CatalogService>(c =>
                c.BaseAddress = new Uri(Configuration["ApiSettings:CatalogUrl"]))
                .AddHttpMessageHandler<AuthenticationDelegatingHandler>()
            .AddHttpMessageHandler<LoggingDelegatingHandler>()

             .AddPolicyHandler(GetRetryPolicy())
             .AddPolicyHandler(GetCircuitBreakerPolicy());

            services.AddHttpClient<IBasketService, BasketService>(c =>
                c.BaseAddress = new Uri(Configuration["ApiSettings:BasketUrl"]))
                .AddHttpMessageHandler<AuthenticationDelegatingHandler>()
            .AddHttpMessageHandler<LoggingDelegatingHandler>()

            .AddPolicyHandler(GetRetryPolicy())
            .AddPolicyHandler(GetCircuitBreakerPolicy());

            services.AddHttpClient<IOrderService, OrderService>(c =>
                c.BaseAddress = new Uri(Configuration["ApiSettings:OrderingUrl"]))
                .AddHttpMessageHandler<AuthenticationDelegatingHandler>()
            .AddHttpMessageHandler<LoggingDelegatingHandler>()
             .AddPolicyHandler(GetRetryPolicy())
             .AddPolicyHandler(GetCircuitBreakerPolicy());

            return services;
        }
        private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            // In this case will wait for
            //  2 ^ 1 = 2 seconds then
            //  2 ^ 2 = 4 seconds then
            //  2 ^ 3 = 8 seconds then
            //  2 ^ 4 = 16 seconds then
            //  2 ^ 5 = 32 seconds

            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(
                    retryCount: 5,
                    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (exception, retryCount, context) =>
                    {
                        Log.Error($"Retry {retryCount} of {context.PolicyKey} at {context.OperationKey}, due to: {exception.Exception.Message}.");
                    });
        }

        private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: 5,
                    durationOfBreak: TimeSpan.FromSeconds(30),
                    onBreak: (ex, breakDelay) =>
                    {

                        Log.Error(".Breaker logging: Breaking the circuit for "
                            + breakDelay.TotalMilliseconds + "ms! ..due to: " + ex.Exception.Message);
                    },
                    onReset: () => Log.Warning(".Breaker logging: Call ok! Closed the circuit again!"),
                    onHalfOpen: () => Log.Warning(".Breaker logging: Half-open: Next call is a trial!")
                );
        }
    }
}
