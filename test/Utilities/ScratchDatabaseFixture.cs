﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.


using System;
using System.Globalization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Xunit;

namespace Identity.MongoDb.Core.Test.Utilities;

public sealed class ScratchDatabaseFixture: IDisposable
{

    public ScratchDatabaseFixture()
    {
        Configuration config = new Configuration();
        ConnectionString = config.Connection;
        DatabaseName = config.DatabaseName;
    }

    public string ConnectionString { get; private set; }
    public string DatabaseName { get; private set;  }

    public void Dispose()
    {
        var client = new MongoClient(ConnectionString);
        client.DropDatabaseAsync(DatabaseName);

        /*var databases = client.ListDatabaseNames();
        var databasesNames = databases.ToList();
        var databaseToDelete = databasesNames.Where(item => item.Contains("idmongodb_tests_", StringComparison.InvariantCultureIgnoreCase)).ToList();
        foreach (var database in databaseToDelete)
        {
            client.DropDatabaseAsync(database);
        }
        */
    }

    public void Update(string connectionString, string databaseName)
    {
        ConnectionString = connectionString;
        DatabaseName = databaseName;
    }
}
