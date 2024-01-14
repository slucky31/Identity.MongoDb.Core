using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Security.Claims;
using System;
using System.Linq;
using MongoDB.Bson;

namespace Identity.MongoDb.Core.Domain;


public class MongoIdentityUser : MongoIdentityUser<ObjectId>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MongoIdentityUser" /> type.
    /// </summary>
    public MongoIdentityUser() : this(null)
    {
        Id = ObjectId.GenerateNewId();
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="MongoIdentityUser" /> type.
    /// </summary>
    /// <param name="userName">The user name.</param>
    public MongoIdentityUser(string userName) : base(userName)
    {
        Id = ObjectId.GenerateNewId();
    }
}

/// <summary>
///     Represents a user in the identity system.
/// </summary>
/// <typeparam name="TKey">The type of the primary key or the user.</typeparam>
public class MongoIdentityUser<TKey> : IdentityUser<TKey>
    where TKey : IEquatable<TKey>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MongoIdentityUser{TKey}" /> type.
    /// </summary>
    public MongoIdentityUser() : this(null)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="MongoIdentityUser{TKey}" /> type.
    /// </summary>
    /// <param name="userName">The user name.</param>
    public MongoIdentityUser(string userName) : base(userName)
    {
        this.Roles = new List<TKey>();
        this.Claims = new List<MongoClaim>();
        this.Logins = new List<IdentityUserLogin<TKey>>();
        this.Tokens = new List<IdentityUserToken<TKey>>();
    }

    /// <summary>
    ///     The IDs of the roles of the user.
    /// </summary>
    public IList<TKey> Roles { get; set; }

    /// <summary>
    ///     The claims of the user.
    /// </summary>
    public IList<MongoClaim> Claims { get; set; }

    /// <summary>
    ///     The logins of the user.
    /// </summary>
    public IList<IdentityUserLogin<TKey>> Logins { get; set; }

    /// <summary>
    ///     The authentication tokens of the user.
    /// </summary>
    public IList<IdentityUserToken<TKey>> Tokens { get; set; }

    /// <summary>
    ///     Tries to get a user login for the provided parameters.
    /// </summary>
    /// <param name="loginProvider"></param>
    /// <param name="providerKey"></param>
    /// <returns></returns>
    public virtual IdentityUserLogin<TKey> GetUserLogin(string loginProvider, string providerKey)
    {
        return this.Logins.FirstOrDefault(x => x.LoginProvider == loginProvider && x.ProviderKey == providerKey);        
    }

    /// <summary>
    ///     Tries to get a user token for the provided parameters.
    /// </summary>
    /// <param name="loginProvider"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public virtual IdentityUserToken<TKey> GetToken(string loginProvider, string name)
    {
        return this.Tokens.FirstOrDefault(x => x.LoginProvider == loginProvider && x.Name == name);        
    }

    /// <summary>
    ///     Adds a claim to a the user.
    /// </summary>
    /// <param name="claim">The claim to add.</param>
    /// <returns>Returns <c>true</c> if the claim was successfully added.</returns>
    public virtual bool AddClaim(Claim claim)
    {
        ArgumentNullException.ThrowIfNull(claim);

        // Prevent adding duplicate claims.
        bool hasClaim = this.Claims.Any(x => x.ClaimValue == claim.Value && x.ClaimType == claim.Type);
        if (hasClaim)
        {
            return false;
        }

        this.Claims.Add(new MongoClaim(claim));

        return true;
    }

    /// <summary>
    ///     Removes a claim from the user.
    /// </summary>
    /// <param name="claim">The claim to remove.</param>
    /// <returns>Returns <c>true</c> if the claim was successfully removed.</returns>
    public virtual bool RemoveClaim(Claim claim)
    {
        ArgumentNullException.ThrowIfNull(claim);

        bool hasClaim = this.Claims.Any(x => x.ClaimValue == claim.Value && x.ClaimType == claim.Type);
        if (!hasClaim)
        {
            return false;
        }

        this.Claims.Remove(new MongoClaim(claim));

        return true;
    }

    /// <summary>
    ///     Replaces a claim of the user.
    /// </summary>
    /// <param name="claim">The claim to replace.</param>
    /// <param name="newClaim">The new claim to set.</param>
    /// <returns>Returns <c>true</c> if the claim was successfully replaced.</returns>
    public virtual bool ReplaceClaim(Claim claim, Claim newClaim)
    {
        RemoveClaim(claim);
        AddClaim(newClaim);

        return true;
    }

    /// <summary>
    ///     Adds a role to a the user.
    /// </summary>
    /// <param name="roleId">The id of the role add.</param>
    /// <returns>Returns <c>true</c> if the role was successfully added.</returns>
    public virtual bool AddRole(TKey roleId)
    {
        ArgumentNullException.ThrowIfNull(roleId);
        if (roleId.Equals(default))
        {
            throw new ArgumentNullException(nameof(roleId));
        }

        // Prevent adding duplicate roles.
        if (this.Roles.Contains(roleId))
        {
            return false;
        }

        this.Roles.Add(roleId);
        return true;
    }

    /// <summary>
    ///     Removes a role from the user.
    /// </summary>
    /// <param name="roleId">The id of the role to remove.</param>
    /// <returns>Returns <c>true</c> if the role was successfully removed.</returns>
    public virtual bool RemoveRole(TKey roleId)
    {
        ArgumentNullException.ThrowIfNull(roleId);
        if (roleId.Equals(default))
        {
            throw new ArgumentNullException(nameof(roleId));
        }

        TKey id = this.Roles.FirstOrDefault(e => e.Equals(roleId));
        if (id == null || id.Equals(default))
        {
            return false;
        }

        this.Roles.Remove(roleId);
        return true;

    }

    /// <summary>
    ///     Adds a login to a the user.
    /// </summary>
    /// <param name="login">The login to add.</param>
    /// <returns>Returns <c>true</c> if the login was successfully added.</returns>
    public virtual bool AddLogin(UserLoginInfo login)
    {
        ArgumentNullException.ThrowIfNull(login);

        // Prevent adding duplicate logins.
        bool hasLogin = this.Logins.Any(x => x.LoginProvider == login.LoginProvider && x.ProviderKey == login.ProviderKey);
        if (hasLogin)
        {
            return false;
        }

        var userLogin = new IdentityUserLogin<TKey>()
        {
            LoginProvider = login.LoginProvider,
            ProviderKey = login.ProviderKey,
            ProviderDisplayName = login.ProviderDisplayName
        };
        this.Logins.Add(userLogin);

        return true;
    }

    /// <summary>
    ///     Removes a login from the user.
    /// </summary>
    /// <param name="login">The login to remove.</param>
    /// <returns>Returns <c>true</c> if the login was successfully removed.</returns>
    public virtual bool RemoveLogin(UserLoginInfo login)
    {
        ArgumentNullException.ThrowIfNull(login);

        var userLogin = this.Logins.FirstOrDefault(x => x.LoginProvider == login.LoginProvider && x.ProviderKey == login.ProviderKey);
        if (userLogin is null)
        {
            return false;
        }

        this.Logins.Remove(userLogin);
        return true;
    }

    /// <summary>
    ///     Adds a token to a the user.
    /// </summary>
    /// <param name="token">The token to add.</param>
    /// <returns>Returns <c>true</c> if the token was successfully added.</returns>
    public virtual bool AddToken(IdentityUserToken<TKey> token)
    {
        ArgumentNullException.ThrowIfNull(token);

        // Prevent adding duplicate tokens.
        bool hasToken = this.Tokens.Any(x => x.LoginProvider == token.LoginProvider && x.Name == token.Name && x.Value == token.Value);
        if (hasToken)
        {
            return false;
        }
        
        this.Tokens.Add(token);

        return true;
    }

    /// <summary>
    ///     Removes a token from the user.
    /// </summary>
    /// <param name="token">The token to remove.</param>
    /// <returns>Returns <c>true</c> if the token was successfully removed.</returns>
    public virtual bool RemoveToken(IdentityUserToken<TKey> token)
    {
        ArgumentNullException.ThrowIfNull(token);

        var userToken = this.Tokens.FirstOrDefault(x => x.LoginProvider == token.LoginProvider && x.Name == token.Name);
        if (userToken is null)
        {
            return false;
        }

        this.Tokens.Remove(userToken);
        return true;
    }
}
