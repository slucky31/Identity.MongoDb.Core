// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Identity.MongoDb.Core.Domain;
using Identity.MongoDb.Core.Test.Utilities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using Xunit;

namespace Identity.MongoDb.Core.Test;

[CollectionDefinition("UserOnlyCustomContextTest")]
public class UserOnlyCustomContextTest : IClassFixture<ScratchDatabaseFixture>
{
    private readonly ApplicationBuilder _builder;

    public class CustomContext : DbContext
    {
        public CustomContext(DbContextOptions options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<MongoIdentityUser>(b =>
            {
                b.HasKey(u => u.Id);                
                b.Property(u => u.ConcurrencyStamp).IsConcurrencyToken();
                b.Property(u => u.UserName).HasMaxLength(256);
                b.Property(u => u.NormalizedUserName).HasMaxLength(256);
                b.Property(u => u.Email).HasMaxLength(256);
                b.Property(u => u.NormalizedEmail).HasMaxLength(256);                
            });
            
        }
    }

    public UserOnlyCustomContextTest(ScratchDatabaseFixture fixture)
    {
        var services = new ServiceCollection();

        services
            .AddSingleton<IConfiguration>(new ConfigurationBuilder().Build())
            .AddDbContext<CustomContext>(o =>
                o.UseMongoDB(fixture.ConnectionString, fixture.DatabaseName)
                    )
            .AddIdentityCore<MongoIdentityUser>(o => { })
            .AddEntityFrameworkStores<CustomContext>();

        services.AddLogging();

        var provider = services.BuildServiceProvider();
        _builder = new ApplicationBuilder(provider);

        using (var scoped = provider.GetRequiredService<IServiceScopeFactory>().CreateScope())
        using (var db = scoped.ServiceProvider.GetRequiredService<CustomContext>())
        {
            
        }
    }

    [Fact]
    public async Task EnsureStartupUsageWorks()
    {
        var userStore = _builder.ApplicationServices.GetRequiredService<IUserStore<MongoIdentityUser>>();
        var userManager = _builder.ApplicationServices.GetRequiredService<UserManager<MongoIdentityUser>>();

        Assert.NotNull(userStore);
        Assert.NotNull(userManager);

        const string userName = "admin";
        const string password = "[PLACEHOLDER]-1a";
        var user = new MongoIdentityUser { UserName = userName };
        IdentityResultAssert.IsSuccess(await userManager.CreateAsync(user, password));
        IdentityResultAssert.IsSuccess(await userManager.DeleteAsync(user));
    }

}
