﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Engine.TypeMakers.TestData
{
    using System;
    using Microsoft.AspNetCore.Authorization;

    [AttributeUsage(AttributeTargets.Property)]
    public class PropAuthorizeAttribute : AuthorizeAttribute
    {
    }
}