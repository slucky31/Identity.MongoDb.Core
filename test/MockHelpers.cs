// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute; 

namespace Identity.MongoDb.Core.Test;

public static class MockHelpers
{
    public static StringBuilder LogMessage = new StringBuilder();

    public static UserManager<TUser> MockUserManager<TUser>() where TUser : class
    {
        var store = Substitute.For<IUserStore<TUser>>();
        var mgr = Substitute.For<UserManager<TUser>>(store, null, null, null, null, null, null, null, null);
        mgr.UserValidators.Add(new UserValidator<TUser>());
        mgr.PasswordValidators.Add(new PasswordValidator<TUser>());
        return mgr;
    }

    public static RoleManager<TRole> MockRoleManager<TRole>(IRoleStore<TRole> store = null) where TRole : class
    {
        store = store ?? Substitute.For<IRoleStore<TRole>>();
        var roles = new List<IRoleValidator<TRole>>();
        roles.Add(new RoleValidator<TRole>());
        return Substitute.For<RoleManager<TRole>>(store, roles, MockLookupNormalizer(),
            new IdentityErrorDescriber(), null);
    }

    public static UserManager<TUser> TestUserManager<TUser>(IUserStore<TUser> store = null) where TUser : class
    {
        store = store ?? Substitute.For<IUserStore<TUser>>();
        var options = Substitute.For<IOptions<IdentityOptions>>();
        var idOptions = new IdentityOptions();
        idOptions.Lockout.AllowedForNewUsers = false;
        //options.(Arg.Any<string>())!.Returns(idOptions);
        var userValidators = new List<IUserValidator<TUser>>();
        var validator = Substitute.For<IUserValidator<TUser>>();
        userValidators.Add(validator);
        var pwdValidators = new List<PasswordValidator<TUser>>();
        pwdValidators.Add(new PasswordValidator<TUser>());
        var userManager = new UserManager<TUser>(store, options, new PasswordHasher<TUser>(),
            userValidators, pwdValidators, MockLookupNormalizer(),
            new IdentityErrorDescriber(), null,
            Substitute.For<ILogger < UserManager<TUser>>>());

        validator.ValidateAsync(userManager,Arg.Any<TUser>()).Returns(Task.FromResult(IdentityResult.Success));
        return userManager;
    }

    public static RoleManager<TRole> TestRoleManager<TRole>(IRoleStore<TRole> store = null) where TRole : class
    {
        store = store ?? Substitute.For<IRoleStore<TRole>>();
        var roles = new List<IRoleValidator<TRole>>();
        roles.Add(new RoleValidator<TRole>());
        return new RoleManager<TRole>(store, roles,
            MockLookupNormalizer(),
            new IdentityErrorDescriber(),
            null);
    }

    public static ILookupNormalizer MockLookupNormalizer()
    {
        var normalizerFunc = new Func<string, string>(i =>
        {
            if (i == null)
            {
                return null;
            }
            else
            {
                return Convert.ToBase64String(Encoding.UTF8.GetBytes(i)).ToUpperInvariant();
            }
        });
        var lookupNormalizer = Substitute.For<ILookupNormalizer>();
        lookupNormalizer.NormalizeName(Arg.Any<string>()).Returns((Func<NSubstitute.Core.CallInfo, string>)normalizerFunc);
        lookupNormalizer.NormalizeEmail(Arg.Any<string>()).Returns((Func<NSubstitute.Core.CallInfo, string>)normalizerFunc);
        return lookupNormalizer;
    }
}
