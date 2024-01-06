// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Reflection;
using Identity.MongoDb.Core;
using Identity.MongoDb.Core.Domain;
using Microsoft.AspNetCore.Identity;

namespace Identity.MongoDb.Core.Test;

public class ApiConsistencyTest : ApiConsistencyTestBase
{
    protected override Assembly TargetAssembly => typeof(MongoIdentityUser).GetTypeInfo().Assembly;
}
