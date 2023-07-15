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
    using Moq;
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

            var item = new Mock<ISchemaItem>();
            var schema = new GraphSchema();

            Assert.Throws<InvalidCastException>(() =>
            {
                validator.ValidateOrThrow(item.Object, schema);
            });
        }

        [Test]
        public void ObjectTypeIsInterface_ThrowsDeclarationException()
        {
            var validator = GraphArgumentValidator.Instance;

            var item = new Mock<IGraphArgument>();
            item.Setup(x => x.ObjectType).Returns(typeof(ISinglePropertyObject));

            var schema = new GraphSchema();
            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                validator.ValidateOrThrow(item.Object, schema);
            });
        }

        [Test]
        public void ObjectTypeIsNotInSchema_ThrowsDeclarationException()
        {
            var validator = GraphArgumentValidator.Instance;

            var item = new Mock<IGraphArgument>();
            item.Setup(x => x.ObjectType).Returns(typeof(TwoPropertyObject));
            item.Setup(x => x.Name).Returns("theName");

            var schema = new Mock<ISchema>();
            var typesCollection = new Mock<ISchemaTypeCollection>();

            // the tested type is not found
            typesCollection
                .Setup(x => x.FindGraphType(It.IsAny<Type>(), It.IsAny<TypeKind>()))
                .Returns(null as IGraphType);

            schema.Setup(x => x.KnownTypes).Returns(typesCollection.Object);

            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                validator.ValidateOrThrow(item.Object, schema.Object);
            });
        }
    }
}