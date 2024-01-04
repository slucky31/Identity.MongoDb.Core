// Licensed to the .NET Foundation under one or more agreements.
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
    private readonly string _connection = "mongodb+srv://api-rest-dev:YTTu6dYjRqFhX4zC@dev.dvd91.azure.mongodb.net/";
    private readonly string _databaseName = "idmongodb_tests_" + DateTimeOffset.Now.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);

    public ScratchDatabaseFixture()
    {      
    }

    public string ConnectionString => _connection;
    public string DatabaseName => _databaseName;

    public void Dispose()
    {
        var client = new MongoClient(_connection);

        // TODO : manage Async and iterator
        var databases = client.ListDatabaseNames();
        var databasesNames = databases.ToList();
        var databaseToDelete = databasesNames.Where(item => item.Contains("idmongodb_tests_", StringComparison.InvariantCultureIgnoreCase)).ToList();
        foreach (var database in databaseToDelete)
        {
            client.DropDatabaseAsync(database);
        }
    }
}
