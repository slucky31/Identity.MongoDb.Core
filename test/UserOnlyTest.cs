// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations.Schema;
using Identity.MongoDb.Core.Domain;
using Identity.MongoDb.Core.Test.Utilities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.EntityFrameworkCore.Extensions;
using Xunit;

namespace Identity.MongoDb.Core.Test;

[CollectionDefinition("UserOnlyTest")]
public class UserOnlyTest : IClassFixture<ScratchDatabaseFixture>
{
    [Column("blog_id")]
    private readonly ApplicationBuilder _builder;

    public class TestUserDbContext : IdentityUserContext<MongoIdentityUser>
    {
        public TestUserDbContext(DbContextOptions options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            if (modelBuilder != null)
            {
                modelBuilder.Entity<IdentityUserLogin<string>>().HasKey(x => x.UserId);
                modelBuilder.Entity<IdentityUserLogin<string>>().Property(x => x.UserId).HasElementName("_id");

                modelBuilder.Entity<IdentityUserToken<string>>().HasKey(x => x.UserId);
                modelBuilder.Entity<IdentityUserToken<string>>().Property(x => x.UserId).HasElementName("_id");
            }
        }
    }

    public UserOnlyTest(ScratchDatabaseFixture fixture)
    {
        var services = new ServiceCollection();

        services
            .AddSingleton<IConfiguration>(new ConfigurationBuilder().Build())
            .AddDbContext<TestUserDbContext>(
                o => o.UseMongoDB(fixture.ConnectionString, fixture.DatabaseName)
                    )
            .AddIdentityCore<MongoIdentityUser>(o => { })
            .AddEntityFrameworkStores<TestUserDbContext>();

        services.AddLogging();

        var provider = services.BuildServiceProvider();
        _builder = new ApplicationBuilder(provider);        
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

    [Fact]
    public async Task FindByEmailThrowsWithTwoUsersWithSameEmail()
    {
        var userStore = _builder.ApplicationServices.GetRequiredService<IUserStore<MongoIdentityUser>>();
        var manager = _builder.ApplicationServices.GetRequiredService<UserManager<MongoIdentityUser>>();

        Assert.NotNull(userStore);
        Assert.NotNull(manager);

        var userA = new MongoIdentityUser(Guid.NewGuid().ToString());
        userA.Email = "dupe@dupe.com";
        const string password = "[PLACEHOLDER]-1a";
        IdentityResultAssert.IsSuccess(await manager.CreateAsync(userA, password));
        var userB = new MongoIdentityUser(Guid.NewGuid().ToString());
        userB.Email = "dupe@dupe.com";
        IdentityResultAssert.IsSuccess(await manager.CreateAsync(userB, password));
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await manager.FindByEmailAsync("dupe@dupe.com"));
    }
}
