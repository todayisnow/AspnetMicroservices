using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
namespace AspnetRunBasics.HttpHandlers
{

    public class AuthenticationDelegatingHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthenticationDelegatingHandler(IHttpContextAccessor httpContextAccessor, IHttpClientFactory httpClientFactory)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _httpClientFactory = httpClientFactory;
        }
        public async Task RefreshToken()//More Investigation
        {
            var expat = await _httpContextAccessor.HttpContext.GetTokenAsync("expires_at");

            var dataExp = DateTime.Parse(expat, null, DateTimeStyles.RoundtripKind);

            if ((dataExp - DateTime.Now).TotalMinutes < 60)
            {
                var client = _httpClientFactory.CreateClient("IDPClient");

                var disco = await client.GetDiscoveryDocumentAsync();
                if (disco.IsError) throw new Exception(disco.Error);



                var rt = await _httpContextAccessor.HttpContext.GetTokenAsync("refresh_token");


                var tokenResult = client.RequestRefreshTokenAsync(new RefreshTokenRequest
                {
                    Address = disco.TokenEndpoint,
                    ClientId = "aspnetRunBasics_client",
                    ClientSecret = "secret",
                    RefreshToken = rt
                }).Result;


                if (!tokenResult.IsError)
                {
                    var oldIdToken = await _httpContextAccessor.HttpContext.GetTokenAsync("id_token");
                    var newAccessToken = tokenResult.AccessToken;
                    var newRefreshToken = tokenResult.RefreshToken;

                    var tokens = new List<AuthenticationToken>
                        {
                            new AuthenticationToken {Name = OpenIdConnectParameterNames.IdToken, Value = oldIdToken},
                            new AuthenticationToken
                            {
                                Name = OpenIdConnectParameterNames.AccessToken,
                                Value = newAccessToken
                            },
                            new AuthenticationToken
                            {
                                Name = OpenIdConnectParameterNames.RefreshToken,
                                Value = newRefreshToken
                            }
                        };

                    var expiresAt = DateTime.Now + TimeSpan.FromSeconds(tokenResult.ExpiresIn);
                    tokens.Add(new AuthenticationToken
                    {
                        Name = "expires_at",
                        Value = expiresAt.ToString("o", CultureInfo.InvariantCulture)
                    });

                    var info = await _httpContextAccessor.HttpContext.AuthenticateAsync("Cookies");
                    info.Properties.StoreTokens(tokens);
                    await _httpContextAccessor.HttpContext.SignInAsync("Cookies", info.Principal, info.Properties);
                }
            }
        }

        private readonly IHttpClientFactory _httpClientFactory;
        //private readonly ClientCredentialsTokenRequest _tokenRequest;

        //public AuthenticationDelegatingHandler(IHttpClientFactory httpClientFactory, ClientCredentialsTokenRequest tokenRequest)
        //{
        //    _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        //    _tokenRequest = tokenRequest ?? throw new ArgumentNullException(nameof(tokenRequest));
        //}

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            //var httpClient = _httpClientFactory.CreateClient("IDPClient");

            //var tokenResponse = await httpClient.RequestClientCredentialsTokenAsync(_tokenRequest);
            //if (tokenResponse.IsError)
            //{
            //    throw new HttpRequestException("Something went wrong while requesting the access token");
            //}
            //request.SetBearerToken(tokenResponse.AccessToken);

            var accessToken = await _httpContextAccessor
                .HttpContext.GetTokenAsync(OpenIdConnectParameterNames.AccessToken);

            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                await RefreshToken();
                request.SetBearerToken(accessToken);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
