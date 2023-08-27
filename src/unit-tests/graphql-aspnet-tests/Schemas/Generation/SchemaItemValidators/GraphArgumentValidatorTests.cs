// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Schemas.Generation.SchemaItemValidators
{
    using System;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.Generation.SchemaItemValidators;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.Common.CommonHelpers;
    using GraphQL.AspNet.Tests.Common.Interfaces;
    using NSubstitute;
    using NUnit.Framework;

    [TestFixture]
    public class GraphArgumentValidatorTests
    {
        private enum EnumNotInSchema
        {
            value1,
            Value2,
        }

        [Test]
        public void NullArgument_ThrowsCastException()
        {
            var validator = GraphArgumentValidator.Instance;

            var item = Substitute.For<ISchemaItem>();
            var schema = new GraphSchema();

            Assert.Throws<InvalidCastException>(() =>
            {
                validator.ValidateOrThrow(item, schema);
            });
        }

        [Test]
        public void ObjectTypeIsInterface_ThrowsDeclarationException()
        {
            var validator = GraphArgumentValidator.Instance;

            var item = Substitute.For<IGraphArgument>();
            item.ObjectType.Returns(typeof(ISinglePropertyObject));
            item.Parent.Returns(null as ISchemaItem);

            var schema = new GraphSchema();
            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                validator.ValidateOrThrow(item, schema);
            });
        }

        [Test]
        public void ObjectTypeIsNotInSchema_ThrowsDeclarationException()
        {
            var validator = GraphArgumentValidator.Instance;

            var item = Substitute.For<IGraphArgument>();
            item.Parent.Returns(null as ISchemaItem);
            item.ObjectType.Returns(typeof(TwoPropertyObject));
            item.Name.Returns("theName");

            var schema = Substitute.For<ISchema>();
            var typesCollection = Substitute.For<ISchemaTypeCollection>();

            // the tested type is not found
            typesCollection
                .FindGraphType(Arg.Any<Type>(), Arg.Any<TypeKind>())
                .Returns(null as IGraphType);

            schema.KnownTypes.Returns(typesCollection);

            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                validator.ValidateOrThrow(item, schema);
            });
        }
    }
}