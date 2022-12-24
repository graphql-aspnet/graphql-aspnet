// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Common.Extensions.AttributeTestData
{
    using System;

    [AttributeUsage(AttributeTargets.All, AllowMultiple =true)]
    public class InheritsFromBaseTestAttribute : BaseTestAttribute
    {
    }
}