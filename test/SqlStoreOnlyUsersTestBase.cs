// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Globalization;
using System.Linq.Expressions;
using System.Security.Claims;
using Identity.MongoDb.Core.Domain;
using Identity.MongoDb.Core.Test.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Identity.MongoDb.Core.Test;

public abstract class SqlStoreOnlyUsersTestBase<TUser, TKey> : UserManagerSpecificationTestBase<TUser, TKey>, IClassFixture<ScratchDatabaseFixture>
    where TUser : MongoIdentityUser<TKey>, new()
    where TKey : IEquatable<TKey>
{
    private readonly ScratchDatabaseFixture _fixture;

    protected SqlStoreOnlyUsersTestBase(ScratchDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    public class TestUserDbContext : IdentityUserContext<TUser, TKey>
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

    protected override TUser CreateTestUser(string namePrefix = "", string email = "", string phoneNumber = "",
        bool lockoutEnabled = false, DateTimeOffset? lockoutEnd = default(DateTimeOffset?), bool useNamePrefixAsUserName = false)
    {
        return new TUser
        {
            UserName = useNamePrefixAsUserName ? namePrefix : string.Format(CultureInfo.InvariantCulture, "{0}{1}", namePrefix, Guid.NewGuid()),
            Email = email,
            PhoneNumber = phoneNumber,
            LockoutEnabled = lockoutEnabled,
            LockoutEnd = lockoutEnd
        };
    }

    protected override Expression<Func<TUser, bool>> UserNameEqualsPredicate(string userName) => u => u.UserName == userName;

#pragma warning disable CA1310 // Specify StringComparison for correctness
    protected override Expression<Func<TUser, bool>> UserNameStartsWithPredicate(string userName) => u => u.UserName.StartsWith(userName);
#pragma warning restore CA1310 // Specify StringComparison for correctness

    private TestUserDbContext CreateContext()
    {
        var db = DbUtil.Create<TestUserDbContext>(_fixture.ConnectionString, _fixture.DatabaseName);
        
        return db;
    }

    protected override object CreateTestContext()
    {
        return CreateContext();
    }

    protected override void AddUserStore(IServiceCollection services, object context = null)
    {
        services.AddSingleton<IUserStore<TUser>>(new UserOnlyStore<TUser, TestUserDbContext, TKey>((TestUserDbContext)context));
    }

    protected override void SetUserPasswordHash(TUser user, string hashedPassword)
    {
        user.PasswordHash = hashedPassword;
    }


    [Fact]
    public async Task DeleteUserRemovesTokensTest()
    {
        // Need fail if not empty?
        var userMgr = CreateManager();
        var user = CreateTestUser();
        IdentityResultAssert.IsSuccess(await userMgr.CreateAsync(user));
        IdentityResultAssert.IsSuccess(await userMgr.SetAuthenticationTokenAsync(user, "provider", "test", "value"));

        Assert.Equal("value", await userMgr.GetAuthenticationTokenAsync(user, "provider", "test"));

        IdentityResultAssert.IsSuccess(await userMgr.DeleteAsync(user));

        Assert.Null(await userMgr.GetAuthenticationTokenAsync(user, "provider", "test"));
    }

    [Fact]
    public void CanCreateUserUsingEF()
    {
        using (var db = CreateContext())
        {
            var user = CreateTestUser();
            db.Users.Add(user);
            db.SaveChanges();
            Assert.True(db.Users.Any(u => u.UserName == user.UserName));
            Assert.NotNull(db.Users.FirstOrDefault(u => u.UserName == user.UserName));
        }
    }

    [Fact]
    public async Task CanCreateUsingManager()
    {
        var manager = CreateManager();
        var user = CreateTestUser();
        IdentityResultAssert.IsSuccess(await manager.CreateAsync(user));
        IdentityResultAssert.IsSuccess(await manager.DeleteAsync(user));
    }

    private async Task LazyLoadTestSetup(TestUserDbContext db, TUser user)
    {
        var context = CreateContext();
        var manager = CreateManager(context);
        IdentityResultAssert.IsSuccess(await manager.CreateAsync(user));
        IdentityResultAssert.IsSuccess(await manager.AddLoginAsync(user, new UserLoginInfo("provider", user.Id.ToString(), "display")));
        Claim[] userClaims =
        {
                new Claim("Whatever", "Value"),
                new Claim("Whatever2", "Value2")
            };
        foreach (var c in userClaims)
        {
            IdentityResultAssert.IsSuccess(await manager.AddClaimAsync(user, c));
        }
    }

    [Fact]
    public async Task LoadFromDbFindByIdTest()
    {
        var db = CreateContext();
        var user = CreateTestUser();
        await LazyLoadTestSetup(db, user);

        db = CreateContext();
        var manager = CreateManager(db);

        var userById = await manager.FindByIdAsync(user.Id.ToString());
        Assert.Equal(2, (await manager.GetClaimsAsync(userById)).Count);
        Assert.Equal(1, (await manager.GetLoginsAsync(userById)).Count);
        Assert.Equal(2, (await manager.GetRolesAsync(userById)).Count);
    }

    [Fact]
    public async Task LoadFromDbFindByNameTest()
    {
        var db = CreateContext();
        var user = CreateTestUser();
        await LazyLoadTestSetup(db, user);

        db = CreateContext();
        var manager = CreateManager(db);
        var userByName = await manager.FindByNameAsync(user.UserName);
        Assert.Equal(2, (await manager.GetClaimsAsync(userByName)).Count);
        Assert.Equal(1, (await manager.GetLoginsAsync(userByName)).Count);
        Assert.Equal(2, (await manager.GetRolesAsync(userByName)).Count);
    }

    [Fact]
    public async Task LoadFromDbFindByLoginTest()
    {
        var db = CreateContext();
        var user = CreateTestUser();
        await LazyLoadTestSetup(db, user);

        db = CreateContext();
        var manager = CreateManager(db);
        var userByLogin = await manager.FindByLoginAsync("provider", user.Id.ToString());
        Assert.Equal(2, (await manager.GetClaimsAsync(userByLogin)).Count);
        Assert.Equal(1, (await manager.GetLoginsAsync(userByLogin)).Count);
        Assert.Equal(2, (await manager.GetRolesAsync(userByLogin)).Count);
    }

    [Fact]
    public async Task LoadFromDbFindByEmailTest()
    {
        var db = CreateContext();
        var user = CreateTestUser();
        user.Email = "fooz@fizzy.pop";
        await LazyLoadTestSetup(db, user);

        db = CreateContext();
        var manager = CreateManager(db);
        var userByEmail = await manager.FindByEmailAsync(user.Email);
        Assert.Equal(2, (await manager.GetClaimsAsync(userByEmail)).Count);
        Assert.Equal(1, (await manager.GetLoginsAsync(userByEmail)).Count);
        Assert.Equal(2, (await manager.GetRolesAsync(userByEmail)).Count);
    }
}
