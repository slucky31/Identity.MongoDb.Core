// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Globalization;
using System.Security.Cryptography;
using Identity.MongoDb.Core.Test.Utilities;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Identity.MongoDb.Core.Test;

[CollectionDefinition("CustomPocoTest")]
public class CustomPocoTest : IClassFixture<ScratchDatabaseFixture>
{

    private readonly ScratchDatabaseFixture _fixture;

    public CustomPocoTest(ScratchDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    public class User<TKey> where TKey : IEquatable<TKey>
    {
        public TKey Id { get; set; }
        public string UserName { get; set; }
    }

    public class CustomDbContext<TKey> : DbContext where TKey : IEquatable<TKey>
    {
        public CustomDbContext(DbContextOptions options) : base(options)
        { }

        public DbSet<User<TKey>> Users { get; set; }

    }

    [Fact]
    public async Task CanUpdateNameGuid()
    {

        using (var db = new CustomDbContext<Guid>(
            new DbContextOptionsBuilder().UseMongoDB(_fixture.ConnectionString, _fixture.DatabaseName).Options))
        {
            

            var oldName = Guid.NewGuid().ToString();
            var user = new User<Guid> { UserName = oldName, Id = Guid.NewGuid() };
            db.Users.Add(user);
            await db.SaveChangesAsync();
            var newName = Guid.NewGuid().ToString();
            user.UserName = newName;
            await db.SaveChangesAsync();
            Assert.Null(db.Users.SingleOrDefault(u => u.UserName == oldName));
            Assert.Equal(user, db.Users.Single(u => u.UserName == newName));

            
        }
    }

    [Fact]
    public async Task CanUpdateNameString()
    {
        
        using (var db = new CustomDbContext<string>(
            new DbContextOptionsBuilder().UseMongoDB(_fixture.ConnectionString, _fixture.DatabaseName).Options))
        {
            

            var oldName = Guid.NewGuid().ToString();
            var user = new User<string> { UserName = oldName, Id = Guid.NewGuid().ToString() };
            db.Users.Add(user);
            await db.SaveChangesAsync();
            var newName = Guid.NewGuid().ToString();
            user.UserName = newName;
            await db.SaveChangesAsync();
            Assert.Null(db.Users.SingleOrDefault(u => u.UserName == oldName));
            Assert.Equal(user, db.Users.Single(u => u.UserName == newName));

            
        }
    }

    [Fact]
    public async Task CanCreateUserInt()
    {
        using (var db = new CustomDbContext<int>(
            new DbContextOptionsBuilder().UseMongoDB(_fixture.ConnectionString, _fixture.DatabaseName).Options))
        {
            
            var user = new User<int>();
            user.Id = RandomNumberGenerator.GetInt32(1000000);
            db.Users.Add(user);
            await db.SaveChangesAsync();
            user.UserName = "Boo";
            await db.SaveChangesAsync();
            var fetch = db.Users.First(u => u.UserName == "Boo");
            Assert.Equal(user, fetch);

        }
    }

    [Fact]
    public async Task CanCreateUserIntViaSet()
    {
        using (var db = new CustomDbContext<int>(
            new DbContextOptionsBuilder().UseMongoDB(_fixture.ConnectionString, _fixture.DatabaseName).Options))
        {

            var user = new User<int>();
            user.Id = RandomNumberGenerator.GetInt32(1000000);
            var users = db.Set<User<int>>();
            users.Add(user);
            await db.SaveChangesAsync();
            user.UserName = "Boo2";
            await db.SaveChangesAsync();
            var fetch = users.First(u => u.UserName == "Boo2");
            Assert.Equal(user, fetch);

        }
    }

    [Fact]
    public async Task CanUpdateNameInt()
    {
        using (var db = new CustomDbContext<int>(
            new DbContextOptionsBuilder().UseMongoDB(_fixture.ConnectionString, _fixture.DatabaseName).Options))
        {

            var oldName = Guid.NewGuid().ToString();
            var user = new User<int> { Id = RandomNumberGenerator.GetInt32(1000000), UserName = oldName };
            db.Users.Add(user);
            await db.SaveChangesAsync();
            var newName = Guid.NewGuid().ToString();
            user.UserName = newName;
            await db.SaveChangesAsync();
            Assert.Null(db.Users.SingleOrDefault(u => u.UserName == oldName));
            Assert.Equal(user, db.Users.Single(u => u.UserName == newName));

        }
    }

    [Fact]
    public async Task CanUpdateNameIntWithSet()
    {
        using (var db = new CustomDbContext<int>(
            new DbContextOptionsBuilder().UseMongoDB(_fixture.ConnectionString, _fixture.DatabaseName).Options))
        {
            

            var oldName = Guid.NewGuid().ToString();
            var user = new User<int> { Id = RandomNumberGenerator.GetInt32(1000000), UserName = oldName };
            db.Set<User<int>>().Add(user);
            await db.SaveChangesAsync();
            var newName = Guid.NewGuid().ToString();
            user.UserName = newName;
            await db.SaveChangesAsync();
            Assert.Null(db.Set<User<int>>().SingleOrDefault(u => u.UserName == oldName));
            Assert.Equal(user, db.Set<User<int>>().Single(u => u.UserName == newName));

            
        }
    }
}
