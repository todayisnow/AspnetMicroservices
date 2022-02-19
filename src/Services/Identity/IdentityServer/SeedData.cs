// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityModel;
using IdentityServer.Data;
using IdentityServer.Models;
using IdentityServer4.EntityFramework.Mappers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Linq;
using System.Security.Claims;

namespace IdentityServer
{
    public class SeedData
    {
        public static void EnsureSeedData(string connectionString)
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddDbContext<ApplicationDbContext>(options =>
               options.UseSqlServer(connectionString));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            using (var serviceProvider = services.BuildServiceProvider())
            {
                using (var scope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
                {
                    var context = scope.ServiceProvider.GetService<ApplicationDbContext>();
                    context.Database.Migrate();

                    var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                    var alice = userMgr.FindByNameAsync("alice").Result;
                    if (alice == null)
                    {
                        alice = new ApplicationUser
                        {
                            UserName = "alice",
                            Email = "AliceSmith@email.com",
                            EmailConfirmed = true,
                            EmployeeId = "2",
                            IsEnabled = true
                        };
                        var result = userMgr.CreateAsync(alice, "Pass123$").Result;
                        if (!result.Succeeded)
                        {
                            throw new Exception(result.Errors.First().Description);
                        }

                        result = userMgr.AddClaimsAsync(alice, new Claim[]{
                            new Claim(JwtClaimTypes.Name, "Alice Smith"),
                            new Claim(JwtClaimTypes.GivenName, "Alice"),
                            new Claim(JwtClaimTypes.FamilyName, "Smith"),
                            new Claim(JwtClaimTypes.WebSite, "http://alice.com"),
                            new Claim(JwtClaimTypes.Role, "admin")
                        }).Result;
                        if (!result.Succeeded)
                        {
                            throw new Exception(result.Errors.First().Description);
                        }
                        Log.Debug("alice created");
                    }
                    else
                    {
                        Log.Debug("alice already exists");
                    }

                    var bob = userMgr.FindByNameAsync("bob").Result;
                    if (bob == null)
                    {
                        bob = new ApplicationUser
                        {
                            UserName = "bob",
                            Email = "BobSmith@email.com",
                            EmailConfirmed = true,
                            EmployeeId = "1",
                            IsEnabled = true
                        };
                        var result = userMgr.CreateAsync(bob, "Pass123$").Result;
                        if (!result.Succeeded)
                        {
                            throw new Exception(result.Errors.First().Description);
                        }

                        result = userMgr.AddClaimsAsync(bob, new Claim[]{
                            new Claim(JwtClaimTypes.Name, "Bob Smith"),
                            new Claim(JwtClaimTypes.GivenName, "Bob"),
                            new Claim(JwtClaimTypes.FamilyName, "Smith"),
                            new Claim(JwtClaimTypes.WebSite, "http://bob.com"),
                            new Claim("location", "somewhere"),
                            new Claim(JwtClaimTypes.Role, "admin"),
                            new Claim(JwtClaimTypes.Role, "user")
                        }).Result;
                        if (!result.Succeeded)
                        {
                            throw new Exception(result.Errors.First().Description);
                        }
                        Log.Debug("bob created");
                    }
                    else
                    {
                        Log.Debug("bob already exists");
                    }

                    Log.Debug("Clients being populated");
                    foreach (var client in Config.Clients.ToList())
                    {
                        if (context.Clients.Any(a => a.ClientId == client.ClientId))
                        {
                            context.Clients.Remove(context.Clients.First(a => a.ClientId == client.ClientId));

                        }

                        context.Clients.Add(client.ToEntity());
                    }
                    context.SaveChanges();

                    Log.Debug("Clients already populated");



                    Log.Debug("IdentityResources being populated");
                    foreach (var resource in Config.IdentityResources.ToList())
                    {
                        if (context.IdentityResources.Any(a => a.Name == resource.Name))
                        {
                            context.IdentityResources.Remove(context.IdentityResources.First(a => a.Name == resource.Name));

                        }

                        context.IdentityResources.Add(resource.ToEntity());
                    }
                    context.SaveChanges();
                    Log.Debug("IdentityResources already populated");



                    Log.Debug("ApiScopes being populated");
                    foreach (var resource in Config.ApiScopes.ToList())
                    {
                        if (context.ApiScopes.Any(a => a.Name == resource.Name))
                        {
                            context.ApiScopes.Remove(context.ApiScopes.First(a => a.Name == resource.Name));

                        }

                        context.ApiScopes.Add(resource.ToEntity());
                    }
                    context.SaveChanges();

                    Log.Debug("ApiScopes already populated");

                }
            }
        }
    }
}
