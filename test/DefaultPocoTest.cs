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
using Xunit;

namespace Identity.MongoDb.Core.Test;

[CollectionDefinition("DefaultPocoTest")]
public class DefaultPocoTest : IClassFixture<ScratchDatabaseFixture>
{
    private readonly ApplicationBuilder _builder;


    public DefaultPocoTest(ScratchDatabaseFixture fixture)
    {
        var services = new ServiceCollection();

        services
            .AddSingleton<IConfiguration>(new ConfigurationBuilder().Build())
            .AddDbContext<IdentityDbContext>(o =>
                o.UseMongoDB(fixture.ConnectionString, fixture.DatabaseName))                    
            .AddIdentity<MongoIdentityUser, MongoIdentityRole>()
            .AddEntityFrameworkStores<IdentityDbContext>();

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
}
