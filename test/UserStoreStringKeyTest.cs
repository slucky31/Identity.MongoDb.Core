// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Identity.MongoDb.Core.Domain;
using Identity.MongoDb.Core.Test.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using Xunit;

namespace Identity.MongoDb.Core.Test;

public class StringUser : MongoIdentityUser<string>
{
    public StringUser()
    {
        Id = Guid.NewGuid().ToString();
        UserName = Id;
    }
}

public class StringRole : MongoIdentityRole<string>
{
    public StringRole()
    {
        Id = Guid.NewGuid().ToString();
        Name = Id;
    }
}

[CollectionDefinition("UserStoreStringKeyTest")]
public class UserStoreStringKeyTest : SqlStoreTestBase<StringUser, StringRole, string>
{
    public UserStoreStringKeyTest(ScratchDatabaseFixture fixture)
        : base(fixture)
    {
    }

    [Fact]
    public void AddEntityFrameworkStoresCanInferKey()
    {
        var services = new ServiceCollection();
        services.AddLogging()
            .AddSingleton(new TestDbContext(new DbContextOptionsBuilder<TestDbContext>().Options));
        // This used to throw
        var builder = services.AddIdentity<StringUser, StringRole>().AddEntityFrameworkStores<TestDbContext>();

        var sp = services.BuildServiceProvider();
        using (var csope = sp.CreateScope())
        {
            Assert.NotNull(sp.GetRequiredService<UserManager<StringUser>>());
            Assert.NotNull(sp.GetRequiredService<RoleManager<StringRole>>());
        }
    }

    [Fact]
    public void AddEntityFrameworkStoresCanInferKeyWithGenericBase()
    {
        var services = new ServiceCollection();
        services.AddLogging()
            .AddSingleton(new TestDbContext(new DbContextOptionsBuilder<TestDbContext>().Options));
        // This used to throw
        var builder = services.AddIdentityCore<MongoIdentityUser<string>>().AddRoles<MongoIdentityRole<string>>().AddEntityFrameworkStores<TestDbContext>();

        var sp = services.BuildServiceProvider();
        using (var csope = sp.CreateScope())
        {
            Assert.NotNull(sp.GetRequiredService<UserManager<MongoIdentityUser<string>>>());
            Assert.NotNull(sp.GetRequiredService<RoleManager<MongoIdentityRole<string>>>());
        }
    }
}
