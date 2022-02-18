using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;

namespace AspnetRunBasics.HttpHandlers
{
    public class TokenFilterAttribute : ActionFilterAttribute
    {


        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var expat = filterContext.HttpContext.GetTokenAsync("expires_at").Result;

            var dataExp = DateTime.Parse(expat, null, DateTimeStyles.RoundtripKind);

            if ((dataExp - DateTime.Now).TotalMinutes < 10)
            {
                var client = new HttpClient();


                var disco = client.GetDiscoveryDocumentAsync().Result;
                if (disco.IsError) throw new Exception(disco.Error);



                var rt = filterContext.HttpContext.GetTokenAsync("refresh_token").Result;


                var tokenResult = client.RequestRefreshTokenAsync(new RefreshTokenRequest
                {
                    Address = disco.TokenEndpoint,
                    ClientId = "aspnetRunBasics_client",
                    ClientSecret = "secret",
                    RefreshToken = rt
                }).Result;


                if (!tokenResult.IsError)
                {
                    var oldIdToken = filterContext.HttpContext.GetTokenAsync("id_token").Result;
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

                    var info = filterContext.HttpContext.AuthenticateAsync("Cookies").Result;
                    info.Properties.StoreTokens(tokens);
                    filterContext.HttpContext.SignInAsync("Cookies", info.Principal, info.Properties);
                }
            }
        }
    }
}
