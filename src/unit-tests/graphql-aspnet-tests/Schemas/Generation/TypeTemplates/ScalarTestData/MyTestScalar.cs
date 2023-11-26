// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Schemas.Generation.TypeTemplates.ScalarTestData
{
    using System;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Directives.Global;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Schemas.TypeSystem.Scalars;
    using NSubstitute;

    [ApplyDirective("directive1")]
    public class MyTestScalar : IScalarGraphType
    {
        public MyTestScalar()
        {
            this.Kind = TypeKind.SCALAR;
            this.SpecifiedByUrl = "http://mytestsclar.org";
            this.ValueType = ScalarValueType.Boolean;
            this.Name = "myScalar";
            this.Description = "myScalar Desc";
            this.ItemPath = new ItemPath(ItemPathRoots.Types, this.Name);
            this.InternalName = "My.Test.Scalar";
            this.ObjectType = typeof(MyTestObject);
            this.SourceResolver = Substitute.For<ILeafValueResolver>();
            this.Publish = true;

            var dir = new AppliedDirectiveCollection(this);
            dir.Add(new AppliedDirective("myDirective", "arg1"));
            dir.Add(new AppliedDirective(typeof(SkipDirective), "argA"));
            this.AppliedDirectives = dir;
        }

        public ScalarValueType ValueType { get; }

        public ILeafValueResolver SourceResolver { get; set; }

        public string SpecifiedByUrl { get; set; }

        public TypeKind Kind { get; }

        public bool Publish { get; set; }

        public bool IsVirtual { get; }

        public Type ObjectType { get; }

        public string InternalName { get; }

        public ItemPath ItemPath { get; }

        public IAppliedDirectiveCollection AppliedDirectives { get; }

        public string Name { get; }

        public string Description { get; set; }

        public IGraphType Clone(string typeName = null)
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
    }
}