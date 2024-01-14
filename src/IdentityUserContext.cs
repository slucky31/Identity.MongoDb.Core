// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Linq;
using Identity.MongoDb.Core.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Bson;

namespace Identity.MongoDb.Core;

/// <summary>
/// Base class for the Entity Framework database context used for identity.
/// </summary>
/// <typeparam name="TUser">The type of the user objects.</typeparam>
public class IdentityUserContext<TUser> : IdentityUserContext<TUser, ObjectId> where TUser : MongoIdentityUser
{
    /// <summary>
    /// Initializes a new instance of <see cref="IdentityUserContext{TUser}"/>.
    /// </summary>
    /// <param name="options">The options to be used by a <see cref="DbContext"/>.</param>
    public IdentityUserContext(DbContextOptions options) : base(options) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="IdentityUserContext{TUser}" /> class.
    /// </summary>
    protected IdentityUserContext() { }
}

/// <summary>
/// Base class for the Entity Framework database context used for identity.
/// </summary>
/// <typeparam name="TUser">The type of user objects.</typeparam>
/// <typeparam name="TKey">The type of the primary key for users and roles.</typeparam>
public class IdentityUserContext<TUser, TKey> : IdentityUserContext<TUser, TKey, IdentityUserClaim<TKey>, IdentityUserLogin<TKey>, IdentityUserToken<TKey>>
    where TUser : MongoIdentityUser<TKey>
    where TKey : IEquatable<TKey>
{
    /// <summary>
    /// Initializes a new instance of the db context.
    /// </summary>
    /// <param name="options">The options to be used by a <see cref="DbContext"/>.</param>
    public IdentityUserContext(DbContextOptions options) : base(options) { }

    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    protected IdentityUserContext() { }
}

/// <summary>
/// Base class for the Entity Framework database context used for identity.
/// </summary>
/// <typeparam name="TUser">The type of user objects.</typeparam>
/// <typeparam name="TKey">The type of the primary key for users and roles.</typeparam>
/// <typeparam name="TUserClaim">The type of the user claim object.</typeparam>
/// <typeparam name="TUserLogin">The type of the user login object.</typeparam>
/// <typeparam name="TUserToken">The type of the user token object.</typeparam>
public abstract class IdentityUserContext<TUser, TKey, TUserClaim, TUserLogin, TUserToken> : DbContext
    where TUser : MongoIdentityUser<TKey>
    where TKey : IEquatable<TKey>
    where TUserClaim : IdentityUserClaim<TKey>
    where TUserLogin : IdentityUserLogin<TKey>
    where TUserToken : IdentityUserToken<TKey>
{
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="options">The options to be used by a <see cref="DbContext"/>.</param>
    protected IdentityUserContext(DbContextOptions options) : base(options) { }

    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    protected IdentityUserContext() { }

    /// <summary>
    /// Gets or sets the <see cref="DbSet{TEntity}"/> of Users.
    /// </summary>
    public virtual DbSet<TUser> Users { get; set; } = default!;

    private StoreOptions? GetStoreOptions() => this.GetService<IDbContextOptions>()
                        .Extensions.OfType<CoreOptionsExtension>()
                        .FirstOrDefault()?.ApplicationServiceProvider
                        ?.GetService<IOptions<IdentityOptions>>()
                        ?.Value?.Stores;

    private sealed class PersonalDataConverter : ValueConverter<string, string>
    {
        public PersonalDataConverter(IPersonalDataProtector protector) : base(s => protector.Protect(s), s => protector.Unprotect(s), default)
        { }
    }

    /// <summary>
    /// Configures the schema needed for the identity framework.
    /// </summary>
    /// <param name="builder">
    /// The builder being used to construct the model for this context.
    /// </param>
    protected override void OnModelCreating(ModelBuilder builder)
    {

    }


}
