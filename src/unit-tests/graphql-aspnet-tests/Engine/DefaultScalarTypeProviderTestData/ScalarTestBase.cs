// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Engine.DefaultScalarTypeProviderTestData
{
    using System;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Schemas.TypeSystem.Scalars;
    using Moq;

    public abstract class ScalarTestBase : IScalarGraphType
    {
        protected ScalarTestBase()
        {
            this.Kind = TypeKind.SCALAR;
            this.ValueType = ScalarValueType.Number;
            this.Publish = true;
            this.IsVirtual = false;
            this.ObjectType = typeof(ScalarDataType);
            this.InternalFullName = "myInternalName";
            this.Route = new SchemaItemPath(AspNet.Execution.SchemaItemCollections.Types, "myScalar");
            this.Name = "MyScalar";
            this.Description = "my description";
            this.AppliedDirectives = new AppliedDirectiveCollection(this);
            this.SpecifiedByUrl = null;

            this.SourceResolver = new Mock<ILeafValueResolver>().Object;
        }

        public virtual IScalarGraphType Clone(string newName)
        {
            newName = Validation.ThrowIfNullWhiteSpaceOrReturn(newName, nameof(newName));
            var newInstance = GlobalTypes.CreateScalarInstanceOrThrow(this.GetType()) as ScalarTestBase;
            newInstance.Name = newName;
            return newInstance;
        }

        public ScalarValueType ValueType { get; set; }

        public ILeafValueResolver SourceResolver { get; set; }

        public TypeKind Kind { get; set; }

        public bool Publish { get; set; }

        public bool IsVirtual { get; set; }

        public Type ObjectType { get; set; }

        public string InternalFullName { get; set; }

        public SchemaItemPath Route { get; set; }

        public IAppliedDirectiveCollection AppliedDirectives { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string SpecifiedByUrl { get; set; }

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
            return true;
        }
    }
}