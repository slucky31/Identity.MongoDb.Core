// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Reflection.Emit;
using System.Security.Cryptography;
using Identity.MongoDb.Core.Domain;
using Identity.MongoDb.Core.Test.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Identity.MongoDb.Core.Test;

public class IntUser : MongoIdentityUser<int>
{
    public IntUser()
    {
        Id = RandomNumberGenerator.GetInt32(1000000);
        UserName = Guid.NewGuid().ToString();
    }
}

public class IntRole : MongoIdentityRole<int>
{
    public IntRole()
    {
        Id = RandomNumberGenerator.GetInt32(1000000);
        Name = Guid.NewGuid().ToString();
    }
}



[CollectionDefinition("UserStoreIntTest")]
public class UserStoreIntTest : SqlStoreTestBase<IntUser, IntRole, int>
{

    public UserStoreIntTest(ScratchDatabaseFixture fixture)
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
        var builder = services.AddIdentity<IntUser, IntRole>().AddEntityFrameworkStores<TestDbContext>();

        var sp = services.BuildServiceProvider();
        using (var csope = sp.CreateScope())
        {
            Assert.NotNull(sp.GetRequiredService<UserManager<IntUser>>());
            Assert.NotNull(sp.GetRequiredService<RoleManager<IntRole>>());
        }
    }

    [Fact]
    public void AddEntityFrameworkStoresCanInferKeyWithGenericBase()
    {
        var services = new ServiceCollection();
        services.AddLogging()
            .AddSingleton(new TestDbContext(new DbContextOptionsBuilder<TestDbContext>().Options));
        // This used to throw
        var builder = services.AddIdentityCore<MongoIdentityUser<int>>().AddRoles<MongoIdentityRole<int>>().AddEntityFrameworkStores<TestDbContext>();

        var sp = services.BuildServiceProvider();
        using (var csope = sp.CreateScope())
        {
            Assert.NotNull(sp.GetRequiredService<UserManager<MongoIdentityUser<int>>>());
            Assert.NotNull(sp.GetRequiredService<RoleManager<MongoIdentityRole<int>>>());
        }
    }

}
