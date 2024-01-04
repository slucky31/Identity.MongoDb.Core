using System.Globalization;
using Identity.MongoDb.Core.Test;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

var _connection = "mongodb+srv://api-rest-dev:YTTu6dYjRqFhX4zC@dev.dvd91.azure.mongodb.net/";
var _databaseName = "identity_mongo_db_core_tests_" + DateTimeOffset.Now.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<EmptyDbContext>(options =>
            options
                .UseMongoDB(_connection, _databaseName)
                .EnableDetailedErrors(true)
        );

var app = builder.Build();

app.Run();

#pragma warning disable S1118 // Utility classes should not have public constructors
public partial class Program { }
#pragma warning restore S1118 // Utility classes should not have public constructors
