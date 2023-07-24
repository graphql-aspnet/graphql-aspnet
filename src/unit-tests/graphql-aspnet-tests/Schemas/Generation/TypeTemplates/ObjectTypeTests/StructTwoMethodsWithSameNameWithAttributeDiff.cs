﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Schemas.Generation.TypeTemplates.ObjectTypeTests
{
    using GraphQL.AspNet.Attributes;

    public struct StructTwoMethodsWithSameNameWithAttributeDiff
    {
        [GraphField]
        public string Method1()
        {
            return string.Empty;
        }

        [GraphField("MethodA")]
        public string Method1(int param1)
        {
            return string.Empty;
        }
    }
}