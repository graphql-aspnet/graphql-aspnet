// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Internal.Templating
{
    using System;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Defaults;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class CustomScalarTypeRegistrationTests
    {
        [Test]
        public void NoErrors_RegistersPropertly()
        {
            var provider = new DefaultScalarTypeProvider();
            var mockResolver = new Mock<IScalarValueResolver>();

            var mock = new Mock<IScalarGraphType>();
            mock.Setup(x => x.Name).Returns("MyCustomType");
            mock.Setup(x => x.Description).Returns(null as string);
            mock.Setup(x => x.ObjectType).Returns(typeof(CustomScalarTypeRegistrationTests));
            mock.Setup(x => x.OtherKnownTypes).Returns(TypeCollection.Empty);
            mock.Setup(x => x.Kind).Returns(TypeKind.SCALAR);
            mock.Setup(x => x.SourceResolver).Returns(mockResolver.Object);
            mock.Setup(x => x.ValueType).Returns(ScalarValueType.String);

            provider.RegisterCustomScalar(mock.Object);

            Assert.IsTrue(provider.IsScalar("MyCustomType"));
            Assert.IsTrue(provider.IsScalar(typeof(CustomScalarTypeRegistrationTests)));

            var foundScalar = provider.RetrieveScalar("MyCustomType");
            Assert.AreEqual(mock.Object, foundScalar);

            foundScalar = provider.RetrieveScalar(typeof(CustomScalarTypeRegistrationTests));
            Assert.AreEqual(mock.Object, foundScalar);

            var concrceteType = provider.RetrieveConcreteType("MyCustomType");
            Assert.AreEqual(typeof(CustomScalarTypeRegistrationTests), concrceteType);
        }

        [Test]
        public void InUseName_ThrowsException()
        {
            var provider = new DefaultScalarTypeProvider();
            var mockResolver = new Mock<IScalarValueResolver>();

            var mock = new Mock<IScalarGraphType>();
            mock.Setup(x => x.Name).Returns(Constants.ScalarNames.INT);
            mock.Setup(x => x.Description).Returns(null as string);
            mock.Setup(x => x.ObjectType).Returns(typeof(CustomScalarTypeRegistrationTests));
            mock.Setup(x => x.OtherKnownTypes).Returns(TypeCollection.Empty);
            mock.Setup(x => x.Kind).Returns(TypeKind.SCALAR);
            mock.Setup(x => x.SourceResolver).Returns(mockResolver.Object);

            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                provider.RegisterCustomScalar(mock.Object);
            });
        }

        [Test]
        public void NoName_ThrowsException()
        {
            var provider = new DefaultScalarTypeProvider();
            var mockResolver = new Mock<IScalarValueResolver>();

            var mock = new Mock<IScalarGraphType>();
            mock.Setup(x => x.Name).Returns(null as string);
            mock.Setup(x => x.Description).Returns(null as string);
            mock.Setup(x => x.ObjectType).Returns(typeof(CustomScalarTypeRegistrationTests));
            mock.Setup(x => x.ValueType).Returns(ScalarValueType.String);
            mock.Setup(x => x.OtherKnownTypes).Returns(TypeCollection.Empty);
            mock.Setup(x => x.Kind).Returns(TypeKind.SCALAR);
            mock.Setup(x => x.SourceResolver).Returns(mockResolver.Object);

            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                provider.RegisterCustomScalar(mock.Object);
            });
        }

        [Test]
        public void InvalidName_ThrowsException()
        {
            var provider = new DefaultScalarTypeProvider();
            var mockResolver = new Mock<IScalarValueResolver>();

            var mock = new Mock<IScalarGraphType>();
            mock.Setup(x => x.Name).Returns("__Bob");
            mock.Setup(x => x.Description).Returns(null as string);
            mock.Setup(x => x.ObjectType).Returns(typeof(CustomScalarTypeRegistrationTests));
            mock.Setup(x => x.ValueType).Returns(ScalarValueType.String);
            mock.Setup(x => x.OtherKnownTypes).Returns(TypeCollection.Empty);
            mock.Setup(x => x.Kind).Returns(TypeKind.SCALAR);
            mock.Setup(x => x.SourceResolver).Returns(mockResolver.Object);

            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                provider.RegisterCustomScalar(mock.Object);
            });
        }

        [Test]
        public void TypeAlreadyRegistered_ThrowsException()
        {
            var provider = new DefaultScalarTypeProvider();
            var mockResolver = new Mock<IScalarValueResolver>();

            var mock = new Mock<IScalarGraphType>();
            mock.Setup(x => x.Name).Returns("MyScalar");
            mock.Setup(x => x.Description).Returns(null as string);
            mock.Setup(x => x.ObjectType).Returns(typeof(string));
            mock.Setup(x => x.ValueType).Returns(ScalarValueType.String);
            mock.Setup(x => x.OtherKnownTypes).Returns(TypeCollection.Empty);
            mock.Setup(x => x.Kind).Returns(TypeKind.SCALAR);
            mock.Setup(x => x.SourceResolver).Returns(mockResolver.Object);

            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                provider.RegisterCustomScalar(mock.Object);
            });
        }

        [Test]
        public void NoTypeSupplied_ThrowsException()
        {
            var provider = new DefaultScalarTypeProvider();
            var mockResolver = new Mock<IScalarValueResolver>();

            var mock = new Mock<IScalarGraphType>();
            mock.Setup(x => x.Name).Returns("MyScalar");
            mock.Setup(x => x.Description).Returns(null as string);
            mock.Setup(x => x.ObjectType).Returns(null as Type);
            mock.Setup(x => x.ValueType).Returns(ScalarValueType.String);
            mock.Setup(x => x.OtherKnownTypes).Returns(TypeCollection.Empty);
            mock.Setup(x => x.Kind).Returns(TypeKind.SCALAR);
            mock.Setup(x => x.SourceResolver).Returns(mockResolver.Object);

            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                provider.RegisterCustomScalar(mock.Object);
            });
        }

        [Test]
        public void NoOtherTypeList_ThrowsException()
        {
            var provider = new DefaultScalarTypeProvider();
            var mockResolver = new Mock<IScalarValueResolver>();

            var mock = new Mock<IScalarGraphType>();
            mock.Setup(x => x.Name).Returns("MyScalar");
            mock.Setup(x => x.Description).Returns(null as string);
            mock.Setup(x => x.ValueType).Returns(ScalarValueType.String);
            mock.Setup(x => x.ObjectType).Returns(typeof(CustomScalarTypeRegistrationTests));
            mock.Setup(x => x.OtherKnownTypes).Returns(null as TypeCollection);
            mock.Setup(x => x.Kind).Returns(TypeKind.SCALAR);
            mock.Setup(x => x.SourceResolver).Returns(mockResolver.Object);

            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                provider.RegisterCustomScalar(mock.Object);
            });
        }

        [Test]
        public void OtherTypeInUse_ThrowsException()
        {
            var provider = new DefaultScalarTypeProvider();
            var mockResolver = new Mock<IScalarValueResolver>();

            var mock = new Mock<IScalarGraphType>();
            mock.Setup(x => x.Name).Returns("MyScalar");
            mock.Setup(x => x.Description).Returns(null as string);
            mock.Setup(x => x.ValueType).Returns(ScalarValueType.String);
            mock.Setup(x => x.ObjectType).Returns(typeof(CustomScalarTypeRegistrationTests));
            mock.Setup(x => x.OtherKnownTypes).Returns(new TypeCollection(typeof(string)));
            mock.Setup(x => x.Kind).Returns(TypeKind.SCALAR);
            mock.Setup(x => x.SourceResolver).Returns(mockResolver.Object);

            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                provider.RegisterCustomScalar(mock.Object);
            });
        }

        [Test]
        public void NullListForOtherTypes_ThrowsException()
        {
            var provider = new DefaultScalarTypeProvider();
            var mockResolver = new Mock<IScalarValueResolver>();

            var mock = new Mock<IScalarGraphType>();
            mock.Setup(x => x.Name).Returns("MyScalar");
            mock.Setup(x => x.Description).Returns(null as string);
            mock.Setup(x => x.ValueType).Returns(ScalarValueType.String);
            mock.Setup(x => x.ObjectType).Returns(typeof(CustomScalarTypeRegistrationTests));
            mock.Setup(x => x.OtherKnownTypes).Returns(null as TypeCollection);
            mock.Setup(x => x.Kind).Returns(TypeKind.SCALAR);
            mock.Setup(x => x.SourceResolver).Returns(mockResolver.Object);

            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                provider.RegisterCustomScalar(mock.Object);
            });
        }

        [Test]
        public void NameAlreadyRegisteredScalar_ThrowsException()
        {
            var provider = new DefaultScalarTypeProvider();
            var mockResolver = new Mock<IScalarValueResolver>();

            var mock = new Mock<IScalarGraphType>();
            mock.Setup(x => x.Name).Returns("Int");
            mock.Setup(x => x.Description).Returns(null as string);
            mock.Setup(x => x.ValueType).Returns(ScalarValueType.String);
            mock.Setup(x => x.ObjectType).Returns(typeof(CustomScalarTypeRegistrationTests));
            mock.Setup(x => x.OtherKnownTypes).Returns(TypeCollection.Empty);
            mock.Setup(x => x.Kind).Returns(TypeKind.SCALAR);
            mock.Setup(x => x.SourceResolver).Returns(mockResolver.Object);

            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                provider.RegisterCustomScalar(mock.Object);
            });
        }

        [Test]
        public void ObjectTypeAlreadyRegisteredScalar_ThrowsException()
        {
            var provider = new DefaultScalarTypeProvider();
            var mockResolver = new Mock<IScalarValueResolver>();

            var mock = new Mock<IScalarGraphType>();
            mock.Setup(x => x.Name).Returns("NewInt");
            mock.Setup(x => x.Description).Returns(null as string);
            mock.Setup(x => x.ValueType).Returns(ScalarValueType.String);
            mock.Setup(x => x.ObjectType).Returns(typeof(int));
            mock.Setup(x => x.OtherKnownTypes).Returns(TypeCollection.Empty);
            mock.Setup(x => x.Kind).Returns(TypeKind.SCALAR);
            mock.Setup(x => x.SourceResolver).Returns(mockResolver.Object);

            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                provider.RegisterCustomScalar(mock.Object);
            });
        }

        [Test]
        public void OtherObjectTypeAlreadyRegisteredScalar_ThrowsException()
        {
            var provider = new DefaultScalarTypeProvider();
            var mockResolver = new Mock<IScalarValueResolver>();

            var mock = new Mock<IScalarGraphType>();
            mock.Setup(x => x.Name).Returns("NewInt");
            mock.Setup(x => x.Description).Returns(null as string);
            mock.Setup(x => x.ValueType).Returns(ScalarValueType.String);
            mock.Setup(x => x.ObjectType).Returns(typeof(CustomScalarTypeRegistrationTests));
            mock.Setup(x => x.OtherKnownTypes).Returns(new TypeCollection(typeof(int)));
            mock.Setup(x => x.Kind).Returns(TypeKind.SCALAR);
            mock.Setup(x => x.SourceResolver).Returns(mockResolver.Object);

            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                provider.RegisterCustomScalar(mock.Object);
            });
        }

        [Test]
        public void TypeKindNotAScalar_ThrowsException()
        {
            var provider = new DefaultScalarTypeProvider();
            var mockResolver = new Mock<IScalarValueResolver>();

            var mock = new Mock<IScalarGraphType>();
            mock.Setup(x => x.Name).Returns("MyScalar");
            mock.Setup(x => x.Description).Returns(null as string);
            mock.Setup(x => x.ValueType).Returns(ScalarValueType.String);
            mock.Setup(x => x.ObjectType).Returns(typeof(CustomScalarTypeRegistrationTests));
            mock.Setup(x => x.OtherKnownTypes).Returns(TypeCollection.Empty);
            mock.Setup(x => x.Kind).Returns(TypeKind.OBJECT);
            mock.Setup(x => x.SourceResolver).Returns(mockResolver.Object);

            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                provider.RegisterCustomScalar(mock.Object);
            });
        }

        [Test]
        public void NoSourceResolver_ThrowsException()
        {
            var provider = new DefaultScalarTypeProvider();

            var mock = new Mock<IScalarGraphType>();
            mock.Setup(x => x.Name).Returns("MyScalar");
            mock.Setup(x => x.Description).Returns(null as string);
            mock.Setup(x => x.ValueType).Returns(ScalarValueType.String);
            mock.Setup(x => x.ObjectType).Returns(typeof(CustomScalarTypeRegistrationTests));
            mock.Setup(x => x.OtherKnownTypes).Returns(TypeCollection.Empty);
            mock.Setup(x => x.Kind).Returns(TypeKind.SCALAR);
            mock.Setup(x => x.SourceResolver).Returns(null as IScalarValueResolver);

            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                provider.RegisterCustomScalar(mock.Object);
            });
        }

        [Test]
        public void Enumerator_Check()
        {
            var provider = new DefaultScalarTypeProvider();

            var i = 0;
            foreach (var scalarType in provider)
                i++;

            Assert.AreNotEqual(0, i);
        }

        [Test]
        public void IsLeaf_NullType_IsFalse()
        {
            var provider = new DefaultScalarTypeProvider();

            var result = provider.IsLeaf(null);
            Assert.IsFalse(result);
        }
    }
}