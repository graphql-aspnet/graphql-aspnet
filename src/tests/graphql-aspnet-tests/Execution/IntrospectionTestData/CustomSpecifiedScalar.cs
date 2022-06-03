﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Tests.Execution.IntrospectionTestData
{
    using System;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.Schemas.TypeSystem.Scalars;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;

    [SpecifiedBy("http://someSiteViaAttribute")]
    public class CustomSpecifiedScalar : BaseScalarType
    {
        public CustomSpecifiedScalar()
            : base("MyCustomScalar", typeof(CustomSpecifiedScalarObjectItem))
        {
        }

        public override ScalarValueType ValueType => ScalarValueType.String;

        public override TypeCollection OtherKnownTypes { get; } = TypeCollection.Empty;

        public override object Resolve(ReadOnlySpan<char> data)
        {
            return null;
        }

        public override object Serialize(object item)
        {
            return null;
        }
    }
}