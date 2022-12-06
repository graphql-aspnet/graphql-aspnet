// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Common.Extensions.ReflectionExtensionTestData
{
    using System;

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class MultiAllowedAttribute : Attribute
    {
    }
}