﻿using IdentityServer4;
using IdentityServer4.Models;
using System.Collections.Generic;

namespace IdentityServer
{
    public class Config
    {
        public static IEnumerable<Client> Clients =>
            new Client[]
            {
                   new Client
                   {
                        ClientId = "testClient",
                        AllowedGrantTypes = GrantTypes.ClientCredentials,
                        ClientSecrets =
                        {
                            new Secret("secret".Sha256())
                        },
                        AllowedScopes = { "basketAPI", "catalogAPI", "orderAPI" , "OcelotApiGw" }
                   },
                   new Client
                   {
                       ClientId = "aspnetRunBasics_client",
                       ClientName = "AspnetRun Basics Razor App",
                       AllowedGrantTypes = GrantTypes.Hybrid,
                       RequirePkce = false,
                       AllowRememberConsent = false,
                       RedirectUris = new List<string>()
                       {
                           "https://localhost:5006/signin-oidc"
                       },
                       PostLogoutRedirectUris = new List<string>()
                       {
                           "https://localhost:5006/signout-callback-oidc"
                       },
                       ClientSecrets = new List<Secret>
                       {
                           new Secret("secret".Sha256())
                       },
                       AllowedScopes = new List<string>
                       {
                           IdentityServerConstants.StandardScopes.OpenId,
                           IdentityServerConstants.StandardScopes.Profile,
                           IdentityServerConstants.StandardScopes.Address,
                           IdentityServerConstants.StandardScopes.Email,

                           "basketAPI",
                           "catalogAPI",
                           "orderAPI",
                           "OcelotApiGw",
                           "roles"
                       },
                       AllowOfflineAccess = true,
                       AlwaysIncludeUserClaimsInIdToken = true,
                       UpdateAccessTokenClaimsOnRefresh = true,

                   }
            };

        public static IEnumerable<ApiScope> ApiScopes =>
           new ApiScope[]
           {
               new ApiScope("basketAPI", "Basket API"),
               new ApiScope("catalogAPI", "Catalog API"),
               new ApiScope("orderAPI", "Order API"),
               new ApiScope("OcelotApiGw", "Ocelot API Gateway")

           };

        public static IEnumerable<ApiResource> ApiResources =>
          new ApiResource[]
          {
               //new ApiResource("movieAPI", "Movie API")
          };

        public static IEnumerable<IdentityResource> IdentityResources =>
          new IdentityResource[]
          {
              new IdentityResources.OpenId(),
              new IdentityResources.Profile(),
              new IdentityResources.Address(),
              new IdentityResources.Email(),

              new IdentityResource(
                    "roles",
                    "Your role(s)",
                    new List<string>() { "role" })
          };

        //public static List<TestUser> TestUsers =>
        //    new List<TestUser>
        //    {
        //        new TestUser
        //        {
        //            SubjectId = "5BE86359-073C-434B-AD2D-A3932222DABE",
        //            Username = "mehmet",
        //            Password = "swn",
        //            Claims = new List<Claim>
        //            {
        //                new Claim(JwtClaimTypes.GivenName, "mehmet"),
        //                new Claim(JwtClaimTypes.FamilyName, "ozkaya")
        //            }
        //        }
        //    };
    }
}
