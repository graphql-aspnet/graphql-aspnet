// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Defaults.TypeMakers.TestData
{
    using System;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.Schemas.TypeSystem.Scalars;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;

    [ApplyDirective(typeof(DirectiveWithArgs), 87, "scalar arg")]
    public class CustomScalarWithDirectives : BaseScalarType
    {
        public CustomScalarWithDirectives(
            IAppliedDirectiveCollection directives = null)
            : base("twoType", typeof(TwoPropertyObject), directives)
        {
        }

        public override object Resolve(ReadOnlySpan<char> data)
        {
            return null;
        }

        public override object Serialize(object item)
        {
            return null;
        }

        public override string Description => "desc";

        public override ScalarValueType ValueType => ScalarValueType.String;

        public override TypeCollection OtherKnownTypes { get; } = new TypeCollection();
    }
}