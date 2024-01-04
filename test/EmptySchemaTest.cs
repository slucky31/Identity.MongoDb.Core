// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Identity.MongoDb.Core.Test.Utilities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Identity.MongoDb.Core.Test;

public class EmptySchemaTest : IClassFixture<ScratchDatabaseFixture>
{
    private readonly ApplicationBuilder _builder;

    public EmptySchemaTest(ScratchDatabaseFixture fixture)
    {
        var services = new ServiceCollection();
        services
            .AddLogging()
            .AddSingleton<IConfiguration>(new ConfigurationBuilder().Build())
            .AddDbContext<EmptyDbContext>(o =>
                o.UseMongoDB(fixture.ConnectionString, fixture.DatabaseName)
                    )
            .AddIdentity<IdentityUser, IdentityRole>(o =>
            {
                // Versions >= 10 are empty
                o.Stores.SchemaVersion = new Version(11, 0);
            })
            .AddEntityFrameworkStores<EmptyDbContext>();

        _builder = new ApplicationBuilder(services.BuildServiceProvider());
        using var scope = _builder.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EmptyDbContext>();
        
    }


}
