// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Schemas.GlobalTypesTestData
{
    using System;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Schemas.TypeSystem.Scalars;
    using GraphQL.AspNet.Tests.Common.CommonHelpers;
    using NSubstitute;

    public class NullAppliedDirectivesScalar : IScalarGraphType
    {
        public NullAppliedDirectivesScalar()
        {
            this.Name = "ValidName";
            this.ValueType = ScalarValueType.String;
            this.SourceResolver = Substitute.For<ILeafValueResolver>();
            this.ObjectType = typeof(TwoPropertyStruct);
            this.AppliedDirectives = null;
        }

        public ScalarValueType ValueType { get; }

        public ILeafValueResolver SourceResolver { get; set; }

        public string SpecifiedByUrl { get; set; }

        public TypeKind Kind => TypeKind.SCALAR;

        public bool Publish { get; set; }

        public bool IsVirtual { get; }

        public Type ObjectType { get; }

        public string InternalName { get; }

        public SchemaItemPath Route { get; }

        public IAppliedDirectiveCollection AppliedDirectives { get; }

        public string Name { get; }

        public string Description { get; set; }

        public object Resolve(ReadOnlySpan<char> data)
        {
            throw new NotImplementedException();
        }

        public object Serialize(object item)
        {
            throw new NotImplementedException();
        }

        public string SerializeToQueryLanguage(object item)
        {
            throw new NotImplementedException();
        }

        public bool ValidateObject(object item)
        {
            throw new NotImplementedException();
        }

        public IGraphType Clone(string typeName)
        {
            throw new NotImplementedException();
        }
    }
}