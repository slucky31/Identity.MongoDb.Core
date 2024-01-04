// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Identity.MongoDb.Core.Test;

public static class DbUtil
{
    public static IServiceCollection ConfigureDbServices<TContext>(
        string connectionString, string databaseName,
        IServiceCollection services = null) where TContext : DbContext
    {
        if (services == null)
        {
            services = new ServiceCollection();
        }

        services.AddHttpContextAccessor();

        services.AddDbContext<TContext>(options =>
        {
            options                
                .UseMongoDB(connectionString, databaseName);                
        });

        return services;
    }

    public static TContext Create<TContext>(string connectionString, string databaseName, IServiceCollection services = null) where TContext : DbContext
    {
        var serviceProvider = ConfigureDbServices<TContext>(connectionString, databaseName, services).BuildServiceProvider();
        return serviceProvider.GetRequiredService<TContext>();
    }

}
