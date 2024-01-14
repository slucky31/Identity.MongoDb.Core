// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Identity.MongoDb.Core.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;

namespace Identity.MongoDb.Core;

/// <summary>
/// Represents a new instance of a persistence store for users, using the default implementation
/// of <see cref="MongoIdentityUser{TKey}"/> with a string as a primary key.
/// </summary>
public class UserStore : UserStore<MongoIdentityUser<ObjectId>>
{
    /// <summary>
    /// Constructs a new instance of <see cref="UserStore"/>.
    /// </summary>
    /// <param name="context">The <see cref="DbContext"/>.</param>
    /// <param name="describer">The <see cref="IdentityErrorDescriber"/>.</param>
    public UserStore(DbContext context, IdentityErrorDescriber? describer = null) : base(context, describer) { }
}

/// <summary>
/// Creates a new instance of a persistence store for the specified user type.
/// </summary>
/// <typeparam name="TUser">The type representing a user.</typeparam>
public class UserStore<TUser> : UserStore<TUser, MongoIdentityRole, DbContext, ObjectId>
    where TUser : MongoIdentityUser<ObjectId>, new()
{
    /// <summary>
    /// Constructs a new instance of <see cref="UserStore{TUser}"/>.
    /// </summary>
    /// <param name="context">The <see cref="DbContext"/>.</param>
    /// <param name="describer">The <see cref="IdentityErrorDescriber"/>.</param>
    public UserStore(DbContext context, IdentityErrorDescriber? describer = null) : base(context, describer) { }
}

/// <summary>
/// Represents a new instance of a persistence store for the specified user and role types.
/// </summary>
/// <typeparam name="TUser">The type representing a user.</typeparam>
/// <typeparam name="TRole">The type representing a role.</typeparam>
/// <typeparam name="TContext">The type of the data context class used to access the store.</typeparam>
public class UserStore<TUser, TRole, TContext> : UserStore<TUser, TRole, TContext, ObjectId>
    where TUser : MongoIdentityUser<ObjectId>
    where TRole : MongoIdentityRole<ObjectId>
    where TContext : DbContext
{
    /// <summary>
    /// Constructs a new instance of <see cref="UserStore{TUser, TRole, TContext}"/>.
    /// </summary>
    /// <param name="context">The <see cref="DbContext"/>.</param>
    /// <param name="describer">The <see cref="IdentityErrorDescriber"/>.</param>
    public UserStore(TContext context, IdentityErrorDescriber? describer = null) : base(context, describer) { }
}

/// <summary>
/// Represents a new instance of a persistence store for the specified user and role types.
/// </summary>
/// <typeparam name="TUser">The type representing a user.</typeparam>
/// <typeparam name="TRole">The type representing a role.</typeparam>
/// <typeparam name="TContext">The type of the data context class used to access the store.</typeparam>
/// <typeparam name="TKey">The type of the primary key for a role.</typeparam>
public class UserStore<TUser, TRole, TContext, TKey> : UserStore<TUser, TRole, TContext, TKey, IdentityUserClaim<TKey>, IdentityUserRole<TKey>, IdentityUserLogin<TKey>, IdentityUserToken<TKey>, IdentityRoleClaim<TKey>>
    where TUser : MongoIdentityUser<TKey>
    where TRole : MongoIdentityRole<TKey>
    where TContext : DbContext
    where TKey : IEquatable<TKey>
{
    /// <summary>
    /// Constructs a new instance of <see cref="UserStore{TUser, TRole, TContext, TKey}"/>.
    /// </summary>
    /// <param name="context">The <see cref="DbContext"/>.</param>
    /// <param name="describer">The <see cref="IdentityErrorDescriber"/>.</param>
    public UserStore(TContext context, IdentityErrorDescriber? describer = null) : base(context, describer) { }
}

/// <summary>
/// Represents a new instance of a persistence store for the specified user and role types.
/// </summary>
/// <typeparam name="TUser">The type representing a user.</typeparam>
/// <typeparam name="TRole">The type representing a role.</typeparam>
/// <typeparam name="TContext">The type of the data context class used to access the store.</typeparam>
/// <typeparam name="TKey">The type of the primary key for a role.</typeparam>
/// <typeparam name="TUserClaim">The type representing a claim.</typeparam>
/// <typeparam name="TUserRole">The type representing a user role.</typeparam>
/// <typeparam name="TUserLogin">The type representing a user external login.</typeparam>
/// <typeparam name="TUserToken">The type representing a user token.</typeparam>
/// <typeparam name="TRoleClaim">The type representing a role claim.</typeparam>
public class UserStore<TUser, TRole, TContext, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TKey, TUserClaim, TUserRole, TUserLogin, TUserToken, TRoleClaim> :
    UserStoreBase<TUser, TRole, TKey, TUserClaim, TUserRole, TUserLogin, TUserToken, TRoleClaim>,
    IProtectedUserStore<TUser>
    where TUser : MongoIdentityUser<TKey>
    where TRole : MongoIdentityRole<TKey>
    where TContext : DbContext
    where TKey : IEquatable<TKey>
    where TUserClaim : IdentityUserClaim<TKey>, new()
    where TUserRole : IdentityUserRole<TKey>, new()
    where TUserLogin : IdentityUserLogin<TKey>, new()
    where TUserToken : IdentityUserToken<TKey>, new()
    where TRoleClaim : IdentityRoleClaim<TKey>, new()
{
    /// <summary>
    /// Creates a new instance of the store.
    /// </summary>
    /// <param name="context">The context used to access the store.</param>
    /// <param name="describer">The <see cref="IdentityErrorDescriber"/> used to describe store errors.</param>
    public UserStore(TContext context, IdentityErrorDescriber? describer = null) : base(describer ?? new IdentityErrorDescriber())
    {
        ArgumentNullException.ThrowIfNull(context);
        Context = context;
    }

    /// <summary>
    /// Gets the database context for this store.
    /// </summary>
    public virtual TContext Context { get; private set; }

    private DbSet<TUser> UsersDbSet { get { return Context.Set<TUser>(); } }
    private DbSet<TRole> Roles { get { return Context.Set<TRole>(); } }
    

    /// <summary>
    /// Gets or sets a flag indicating if changes should be persisted after CreateAsync, UpdateAsync and DeleteAsync are called.
    /// </summary>
    /// <value>
    /// True if changes should be automatically persisted, otherwise false.
    /// </value>
    public bool AutoSaveChanges { get; set; } = true;

    public override IQueryable<TUser> Users => UsersDbSet;

    /// <summary>Saves the current store.</summary>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
    protected Task SaveChanges(CancellationToken cancellationToken)
    {
        return AutoSaveChanges ? Context.SaveChangesAsync(cancellationToken) : Task.CompletedTask;
    }

    /// <summary>
    /// Creates the specified <paramref name="user"/> in the user store.
    /// </summary>
    /// <param name="user">The user to create.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="IdentityResult"/> of the creation operation.</returns>
    public override async Task<IdentityResult> CreateAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(user);
        Context.Add(user);
        await SaveChanges(cancellationToken);
        return IdentityResult.Success;
    }

    /// <summary>
    /// Updates the specified <paramref name="user"/> in the user store.
    /// </summary>
    /// <param name="user">The user to update.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="IdentityResult"/> of the update operation.</returns>
    public override async Task<IdentityResult> UpdateAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(user);

        Context.Attach(user);
        user.ConcurrencyStamp = Guid.NewGuid().ToString();
        Context.Update(user);
        try
        {
            await SaveChanges(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return IdentityResult.Failed(ErrorDescriber.ConcurrencyFailure());
        }
        return IdentityResult.Success;
    }

    /// <summary>
    /// Deletes the specified <paramref name="user"/> from the user store.
    /// </summary>
    /// <param name="user">The user to delete.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="IdentityResult"/> of the update operation.</returns>
    public override async Task<IdentityResult> DeleteAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(user);

        Context.Remove(user);
        try
        {
            await SaveChanges(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return IdentityResult.Failed(ErrorDescriber.ConcurrencyFailure());
        }
        return IdentityResult.Success;
    }

    /// <summary>
    /// Finds and returns a user, if any, who has the specified <paramref name="userId"/>.
    /// </summary>
    /// <param name="userId">The user ID to search for.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>
    /// The <see cref="Task"/> that represents the asynchronous operation, containing the user matching the specified <paramref name="userId"/> if it exists.
    /// </returns>
    public override Task<TUser?> FindByIdAsync(string userId, CancellationToken cancellationToken = default(CancellationToken))
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        var id = ConvertIdFromString(userId);
        return UsersDbSet.FindAsync(new object?[] { id }, cancellationToken).AsTask();
    }

    /// <summary>
    /// Finds and returns a user, if any, who has the specified normalized user name.
    /// </summary>
    /// <param name="normalizedUserName">The normalized user name to search for.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>
    /// The <see cref="Task"/> that represents the asynchronous operation, containing the user matching the specified <paramref name="normalizedUserName"/> if it exists.
    /// </returns>
    public override Task<TUser?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken = default(CancellationToken))
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        return Users.FirstOrDefaultAsync(u => u.NormalizedUserName == normalizedUserName, cancellationToken);
    }

    /// <summary>
    /// A navigation property for the users the store contains.
    /// </summary>
 

    /// <summary>
    /// Return a role with the normalized name if it exists.
    /// </summary>
    /// <param name="normalizedRoleName">The normalized role name.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The role if it exists.</returns>
    protected override Task<TRole?> FindRoleAsync(string normalizedRoleName, CancellationToken cancellationToken)
    {
        return Roles.SingleOrDefaultAsync(r => r.NormalizedName == normalizedRoleName, cancellationToken);
    }

    /// <summary>
    /// Return a user role for the userId and roleId if it exists.
    /// </summary>
    /// <param name="userId">The user's id.</param>
    /// <param name="roleId">The role's id.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The user role if it exists.</returns>
    protected override async Task<TUserRole?> FindUserRoleAsync(TKey userId, TKey roleId, CancellationToken cancellationToken)
    {        
        var users = await Users.ToListAsync(cancellationToken);
        var user = users.Where(x => x.Id.Equals(userId) && x.Roles.Any(r => r.Equals(roleId))).FirstOrDefault();

        if (user is null) 
        { 
            return null;
        }
        
        return user.Roles
            .Where(r => r.Equals(roleId))
            .Select(x => new TUserRole
                {
                    UserId = userId,
                    RoleId = roleId
                })
            .FirstOrDefault();
    }

    /// <summary>
    /// Return a user with the matching userId if it exists.
    /// </summary>
    /// <param name="userId">The user's id.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The user if it exists.</returns>
    protected override Task<TUser?> FindUserAsync(TKey userId, CancellationToken cancellationToken)
    {
        return Users.SingleOrDefaultAsync(u => u.Id.Equals(userId), cancellationToken);
    }

    /// <summary>
    /// Return a user login with the matching userId, provider, providerKey if it exists.
    /// </summary>
    /// <param name="userId">The user's id.</param>
    /// <param name="loginProvider">The login provider name.</param>
    /// <param name="providerKey">The key provided by the <paramref name="loginProvider"/> to identify a user.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The user login if it exists.</returns>
    protected override async Task<TUserLogin?> FindUserLoginAsync(TKey userId, string loginProvider, string providerKey, CancellationToken cancellationToken)
    {
        Expression<Func<TUser, bool>> predicate = x => x.Id.Equals(userId) && x.Logins.Any(l => l.LoginProvider == loginProvider && l.ProviderKey == providerKey);
        
        var user = await Users.Where(predicate).FirstOrDefaultAsync(cancellationToken);
        
        return (TUserLogin)user.GetUserLogin(loginProvider, providerKey);
    }

    /// <summary>
    /// Return a user login with  provider, providerKey if it exists.
    /// </summary>
    /// <param name="loginProvider">The login provider name.</param>
    /// <param name="providerKey">The key provided by the <paramref name="loginProvider"/> to identify a user.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The user login if it exists.</returns>
    protected override async Task<TUserLogin?> FindUserLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken)
    {
        // TODO : MongoDB.Driver.Linq.ExpressionNotSupportedException : Expression not supported: i.Logins.AsQueryable().
        var users = await Users.ToListAsync(cancellationToken);

        if (users is null || users.Count == 0) 
        {
            return null;
        }

        var user = users.Where(x => x.Logins.Any(l => l.LoginProvider == loginProvider && l.ProviderKey == providerKey)).FirstOrDefault();

        if (user is null)
        {
            return null;
        }

        return (TUserLogin)user.GetUserLogin(loginProvider, providerKey);        
    }

    /// <summary>
    /// Adds the given <paramref name="normalizedRoleName"/> to the specified <paramref name="user"/>.
    /// </summary>
    /// <param name="user">The user to add the role to.</param>
    /// <param name="normalizedRoleName">The role to add.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
    public override async Task AddToRoleAsync(TUser user, string normalizedRoleName, CancellationToken cancellationToken = default(CancellationToken))
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(user);
        if (string.IsNullOrWhiteSpace(normalizedRoleName))
        {
            //TODO : Custom errors ?
            throw new ArgumentException("ValueCannotBeNullOrEmpty", nameof(normalizedRoleName));
        }

        var roleEntity = await FindRoleAsync(normalizedRoleName, cancellationToken);
        if (roleEntity == null)
        {
            //TODO : Custom errors ?
            throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "RoleNotFound", normalizedRoleName));
        }

        user.AddRole(roleEntity.Id);
    }

    /// <summary>
    /// Removes the given <paramref name="normalizedRoleName"/> from the specified <paramref name="user"/>.
    /// </summary>
    /// <param name="user">The user to remove the role from.</param>
    /// <param name="normalizedRoleName">The role to remove.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
    public override async Task RemoveFromRoleAsync(TUser user, string normalizedRoleName, CancellationToken cancellationToken = default(CancellationToken))
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(user);
        if (string.IsNullOrWhiteSpace(normalizedRoleName))
        {
            //TODO : Custom errors ?
            throw new ArgumentException("ValueCannotBeNullOrEmpty", nameof(normalizedRoleName));
        }
        var roleEntity = await FindRoleAsync(normalizedRoleName, cancellationToken);
        if (roleEntity != null)
        {
            user.RemoveRole(roleEntity.Id);
        }
    }

    /// <summary>
    /// Retrieves the roles the specified <paramref name="user"/> is a member of.
    /// </summary>
    /// <param name="user">The user whose roles should be retrieved.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>A <see cref="Task{TResult}"/> that contains the roles the user is a member of.</returns>
    public override async Task<IList<string>> GetRolesAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(user);

        if (user.Roles.Any())
        {
            var roles =  await this.Roles.Where(x => user.Roles.Contains(x.Id)).ToListAsync(cancellationToken);
            return roles.Select(r => r.Name).ToList();                                               
        }

        return new List<string>(0);
    }

    /// <summary>
    /// Returns a flag indicating if the specified user is a member of the give <paramref name="normalizedRoleName"/>.
    /// </summary>
    /// <param name="user">The user whose role membership should be checked.</param>
    /// <param name="normalizedRoleName">The role to check membership of</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>A <see cref="Task{TResult}"/> containing a flag indicating if the specified user is a member of the given group. If the
    /// user is a member of the group the returned value with be true, otherwise it will be false.</returns>
    public override async Task<bool> IsInRoleAsync(TUser user, string normalizedRoleName, CancellationToken cancellationToken = default(CancellationToken))
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(user);
        if (string.IsNullOrWhiteSpace(normalizedRoleName))
        {
            //TODO : Custom errors ?
            throw new ArgumentException("ValueCannotBeNullOrEmpty", nameof(normalizedRoleName));
        }
        var role = await FindRoleAsync(normalizedRoleName, cancellationToken);
        if (role != null)
        {
            var userRole = await FindUserRoleAsync(user.Id, role.Id, cancellationToken);
            return userRole != null;
        }
        return false;
    }

    /// <summary>
    /// Get the claims associated with the specified <paramref name="user"/> as an asynchronous operation.
    /// </summary>
    /// <param name="user">The user whose claims should be retrieved.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>A <see cref="Task{TResult}"/> that contains the claims granted to a user.</returns>
    public override Task<IList<Claim>> GetClaimsAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(user);        

        IList<Claim> claims = user.Claims.Select(x => x.ToClaim()).ToList();
        return Task.FromResult(claims);
    }

    /// <summary>
    /// Adds the <paramref name="claims"/> given to the specified <paramref name="user"/>.
    /// </summary>
    /// <param name="user">The user to add the claim to.</param>
    /// <param name="claims">The claim to add to the user.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
    public override Task AddClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken = default(CancellationToken))
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNull(claims);
        foreach (var claim in claims)
        {
            var result = user.AddClaim(claim);
            if (!result)
            {
                Task.FromResult(false);
            }
        }
        return Task.CompletedTask;
    }

    /// <summary>
    /// Replaces the <paramref name="claim"/> on the specified <paramref name="user"/>, with the <paramref name="newClaim"/>.
    /// </summary>
    /// <param name="user">The user to replace the claim on.</param>
    /// <param name="claim">The claim replace.</param>
    /// <param name="newClaim">The new claim replacing the <paramref name="claim"/>.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
    public override async Task ReplaceClaimAsync(TUser user, Claim claim, Claim newClaim, CancellationToken cancellationToken = default(CancellationToken))
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNull(claim);
        ArgumentNullException.ThrowIfNull(newClaim);
                
        user.ReplaceClaim(claim, newClaim);
        
    }

    /// <summary>
    /// Removes the <paramref name="claims"/> given from the specified <paramref name="user"/>.
    /// </summary>
    /// <param name="user">The user to remove the claims from.</param>
    /// <param name="claims">The claim to remove.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
    public override Task RemoveClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken = default(CancellationToken))
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNull(claims);
        foreach (var claim in claims)
        {
            var result = user.RemoveClaim(claim);
            if (result is false)
            {
                return Task.FromResult(false);
            }
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Adds the <paramref name="login"/> given to the specified <paramref name="user"/>.
    /// </summary>
    /// <param name="user">The user to add the login to.</param>
    /// <param name="login">The login to add to the user.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
    public override Task AddLoginAsync(TUser user, UserLoginInfo login,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNull(login);
        
        return Task.FromResult(user.AddLogin(login));
    }

    /// <summary>
    /// Removes the <paramref name="loginProvider"/> given from the specified <paramref name="user"/>.
    /// </summary>
    /// <param name="user">The user to remove the login from.</param>
    /// <param name="loginProvider">The login to remove from the user.</param>
    /// <param name="providerKey">The key provided by the <paramref name="loginProvider"/> to identify a user.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
    public override async Task RemoveLoginAsync(TUser user, string loginProvider, string providerKey,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(user);

        var entry = await FindUserLoginAsync(user.Id, loginProvider, providerKey, cancellationToken);
        if (entry != null)
        {
            user.RemoveLogin(new UserLoginInfo(loginProvider, providerKey, entry.ProviderDisplayName));
        }
    }

    /// <summary>
    /// Retrieves the associated logins for the specified <param ref="user"/>.
    /// </summary>
    /// <param name="user">The user whose associated logins to retrieve.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>
    /// The <see cref="Task"/> for the asynchronous operation, containing a list of <see cref="UserLoginInfo"/> for the specified <paramref name="user"/>, if any.
    /// </returns>
    public override Task<IList<UserLoginInfo>> GetLoginsAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(user);
        var userId = user.Id;

        IList<UserLoginInfo> logins = user.Logins.Select(l => new UserLoginInfo(l.LoginProvider, l.ProviderKey, l.ProviderDisplayName)).ToList();
        return Task.FromResult(logins);
    }

    /// <summary>
    /// Retrieves the user associated with the specified login provider and login provider key.
    /// </summary>
    /// <param name="loginProvider">The login provider who provided the <paramref name="providerKey"/>.</param>
    /// <param name="providerKey">The key provided by the <paramref name="loginProvider"/> to identify a user.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>
    /// The <see cref="Task"/> for the asynchronous operation, containing the user, if any which matched the specified login provider and key.
    /// </returns>
    public override async Task<TUser?> FindByLoginAsync(string loginProvider, string providerKey,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        var users = await Users.ToListAsync(cancellationToken);
        if (users is null || users.Count == 0)
        {
            return null;
        }

        var user = users.Where(x => x.Logins.Any(l => l.LoginProvider == loginProvider && l.ProviderKey == providerKey)).FirstOrDefault();
        return user;
        
    }

    /// <summary>
    /// Gets the user, if any, associated with the specified, normalized email address.
    /// </summary>
    /// <param name="normalizedEmail">The normalized email address to return the user for.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>
    /// The task object containing the results of the asynchronous lookup operation, the user if any associated with the specified normalized email address.
    /// </returns>
    public override Task<TUser?> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default(CancellationToken))
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        return Users.SingleOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail, cancellationToken);
    }

    /// <summary>
    /// Retrieves all users with the specified claim.
    /// </summary>
    /// <param name="claim">The claim whose users should be retrieved.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>
    /// The <see cref="Task"/> contains a list of users, if any, that contain the specified claim.
    /// </returns>
    public override async Task<IList<TUser>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken = default(CancellationToken))
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(claim);

        var users = await Users.ToListAsync(cancellationToken);
        
        return users.Where(x => x.Claims.Any(c => c.ClaimValue == claim.Value && c.ClaimType == claim.Type)).ToList();     
    }

    /// <summary>
    /// Retrieves all users in the specified role.
    /// </summary>
    /// <param name="normalizedRoleName">The role whose users should be retrieved.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>
    /// The <see cref="Task"/> contains a list of users, if any, that are in the specified role.
    /// </returns>
    public override async Task<IList<TUser>> GetUsersInRoleAsync(string normalizedRoleName, CancellationToken cancellationToken = default(CancellationToken))
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentException.ThrowIfNullOrEmpty(normalizedRoleName);

        var role = await FindRoleAsync(normalizedRoleName, cancellationToken);

        if (role is not null)
        {
            return await Users.Where(user => user.Roles.Contains(role.Id)).ToListAsync(cancellationToken);            
        }
        return new List<TUser>();
    }

    /// <summary>
    /// Find a user token if it exists.
    /// </summary>
    /// <param name="user">The token owner.</param>
    /// <param name="loginProvider">The login provider for the token.</param>
    /// <param name="name">The name of the token.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The user token if it exists.</returns>
    protected override Task<TUserToken?> FindTokenAsync(TUser user, string loginProvider, string name, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(user);

        var token = user.GetToken(loginProvider, name);
        return Task.FromResult((TUserToken)token);
    }       

    /// <summary>
    /// Add a new user token.
    /// </summary>
    /// <param name="token">The token to be added.</param>
    /// <returns></returns>
    protected override async Task AddUserTokenAsync(TUserToken token)
    {        
        this.ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(token);

        var user = await FindUserAsync(token.UserId, CancellationToken.None);
        user.AddToken(token);        
    }

    /// <summary>
    /// Remove a new user token.
    /// </summary>
    /// <param name="token">The token to be removed.</param>
    /// <returns></returns>
    protected override async Task RemoveUserTokenAsync(TUserToken token)
    {
        this.ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(token);

        var user = await FindUserAsync(token.UserId, CancellationToken.None);
        user.RemoveToken(token);
    }
}
