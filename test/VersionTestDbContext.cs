// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Identity.MongoDb.Core.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using MongoDB.Bson;

namespace Identity.MongoDb.Core.Test;

public class EmptyDbContext : IdentityDbContext<MongoIdentityUser, MongoIdentityRole, ObjectId>
{
    public EmptyDbContext(DbContextOptions options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {        
            base.OnModelCreating(builder);        
    }
}
