// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Schemas
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using GraphQL.AspNet.Defaults.TypeMakers;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Schemas.TypeSystem.TypeCollections;
    using GraphQL.AspNet.Tests.CommonHelpers;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;
    using GraphQL.AspNet.Tests.Schemas.GraphTypeCollectionTestData;
    using GraphQL.AspNet.Tests.Schemas.SchemaTestData;
    using NUnit.Framework;

    [TestFixture]
    public class SchemaTypeCollectionTests
    {
        public enum RandomEnum
        {
            Value0,
            Value1,
            Value2,
        }

        private IGraphType MakeGraphType(Type type, TypeKind kind)
        {
            var testServer = new TestServerBuilder().Build();
            var maker = GraphQLProviders.GraphTypeMakerProvider.CreateTypeMaker(testServer.Schema, kind);
            return maker.CreateGraphType(type).GraphType;
        }

        private IGraphField MakeGraphField(IGraphTypeFieldTemplate fieldTemplate)
        {
            var testServer = new TestServerBuilder().Build();
            var maker = new GraphFieldMaker(testServer.Schema);
            return maker.CreateField(fieldTemplate).Field;
        }

        [Test]
        public void EnsureGraphType_Enum_IsAddedCorrectly()
        {
            var collection = new SchemaTypeCollection();

            var graphType = this.MakeGraphType(typeof(RandomEnum), TypeKind.ENUM) as IEnumGraphType;
            collection.EnsureGraphType(graphType, typeof(RandomEnum));
            Assert.AreEqual(1, collection.Count);
            Assert.IsTrue(collection.Contains(typeof(RandomEnum)));
        }

        [Test]
        public void EnsureGraphType_Scalar_IsAddedCorrectly()
        {
            var collection = new SchemaTypeCollection();

            var scalar = this.MakeGraphType(typeof(int), TypeKind.SCALAR) as IScalarGraphType;
            collection.EnsureGraphType(scalar, typeof(int));

            Assert.AreEqual(1, collection.Count);
            Assert.IsTrue(collection.Contains(typeof(int)));
        }

        [Test]
        public void EnsureGraphType_ScalarTwice_EndsUpInScalarCollectionOnce()
        {
            var collection = new SchemaTypeCollection();

            var scalar = this.MakeGraphType(typeof(int), TypeKind.SCALAR) as IScalarGraphType;
            collection.EnsureGraphType(scalar);
            collection.EnsureGraphType(scalar);

            Assert.AreEqual(1, collection.Count);
            Assert.IsTrue(collection.Contains(GraphQLProviders.ScalarProvider.RetrieveScalar(typeof(int))));
            Assert.IsTrue(collection.Contains(GraphQLProviders.ScalarProvider.RetrieveScalar(Constants.ScalarNames.INT)));
            Assert.IsTrue(collection.Contains(typeof(int)));
        }

        [Test]
        public void EnsureGraphType_TwoScalar_EndsUpInScalarCollectionOnceEach()
        {
            var collection = new SchemaTypeCollection();

            var scalar = this.MakeGraphType(typeof(int), TypeKind.SCALAR) as IScalarGraphType;
            collection.EnsureGraphType(scalar);

            scalar = this.MakeGraphType(typeof(long), TypeKind.SCALAR) as IScalarGraphType;
            collection.EnsureGraphType(scalar);

            Assert.AreEqual(2, collection.Count);
            Assert.IsTrue(collection.Contains(GraphQLProviders.ScalarProvider.RetrieveScalar(typeof(int))));
            Assert.IsTrue(collection.Contains(GraphQLProviders.ScalarProvider.RetrieveScalar(Constants.ScalarNames.INT)));
            Assert.IsTrue(collection.Contains(GraphQLProviders.ScalarProvider.RetrieveScalar(typeof(long))));
            Assert.IsTrue(collection.Contains(GraphQLProviders.ScalarProvider.RetrieveScalar(Constants.ScalarNames.LONG)));
        }

        [Test]
        public void EnsureGraphType_Directive_OfCorrectKind_WorksAsIntended()
        {
            var collection = new SchemaTypeCollection();
            var graphType = this.MakeGraphType(typeof(SimpleDirective), TypeKind.DIRECTIVE) as IDirectiveGraphType;

            collection.EnsureGraphType(graphType, typeof(SimpleDirective));
            Assert.AreEqual(1, collection.Count);
        }

        [Test]
        public void EnsureGraphType_Directive_OfIncorrectConcreteType_ThrowsException()
        {
            var collection = new SchemaTypeCollection();
            var graphType = this.MakeGraphType(typeof(SimpleDirective), TypeKind.DIRECTIVE) as IDirectiveGraphType;

            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                collection.EnsureGraphType(graphType, typeof(SchemaTypeCollectionTests));
            });
        }

        [Test]
        public void EnsureGraphType_Directive_FromConcreteType_ThrowsException()
        {
            var collection = new SchemaTypeCollection();
            var graphType = this.MakeGraphType(typeof(SimpleDirective), TypeKind.DIRECTIVE) as IDirectiveGraphType;

            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                collection.EnsureGraphType(graphType, typeof(SchemaTypeCollectionTests));
            });
        }

        [Test]
        public void EnsureGraphType_DictionaryTK_ThrowsExeption()
        {
            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                // shoudl be an impossible situation but just in case someone
                // passes an invalid concrete type with an valid graph type.
                var graphType = this.MakeGraphType(typeof(PlainOldTestObject), TypeKind.OBJECT) as IObjectGraphType;

                var collection = new SchemaTypeCollection();
                collection.EnsureGraphType(graphType, typeof(Dictionary<string, int>));
            });
        }

        [Test]
        public void EnsureGraphType_WhenObjectTypeIsAddedTwice_WithADifferentAssociatedType_ThrowsException()
        {
            var collection = new SchemaTypeCollection();
            var graphType = this.MakeGraphType(typeof(PlainOldTestObject), TypeKind.OBJECT) as IObjectGraphType;

            collection.EnsureGraphType(graphType, typeof(PlainOldTestObject));
            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                collection.EnsureGraphType(graphType, typeof(TwoPropertyObject));
            });
        }

        [Test]
        public void EnsureGraphType_WhenObjectTypeIsAddedTwice_WithSampleAssociatedType_OperationisIdempodent()
        {
            var collection = new SchemaTypeCollection();
            var graphType = this.MakeGraphType(typeof(PlainOldTestObject), TypeKind.OBJECT) as IObjectGraphType;

            collection.EnsureGraphType(graphType, typeof(PlainOldTestObject));

            collection.EnsureGraphType(graphType, typeof(PlainOldTestObject));
            Assert.AreEqual(1, collection.Count);

            collection.EnsureGraphType(graphType, typeof(PlainOldTestObject));
            Assert.AreEqual(1, collection.Count);

            collection.EnsureGraphType(graphType, typeof(PlainOldTestObject));
            Assert.AreEqual(1, collection.Count);
        }

        [Test]
        public void EnsureGraphType_OverloadedAddsForEqualValueScalars_Succeeds()
        {
            var collection = new SchemaTypeCollection();
            var intScalar = this.MakeGraphType(typeof(int), TypeKind.SCALAR) as IScalarGraphType;
            var nullableIntScalar = this.MakeGraphType(typeof(int?), TypeKind.SCALAR) as IScalarGraphType;

            collection.EnsureGraphType(intScalar, typeof(int));
            collection.EnsureGraphType(nullableIntScalar, typeof(int?));

            Assert.AreEqual(1, collection.Count);
        }

        [Test]
        public void QueueExtension_WhenAddedBeforeType_IsAssignedAfterTypeIsAdded()
        {
            var collection = new SchemaTypeCollection();
            var template = GraphQLProviders.TemplateProvider.ParseType(typeof(TypeExtensionController)) as IGraphControllerTemplate;

            var graphType = this.MakeGraphType(typeof(TwoPropertyObject), TypeKind.OBJECT) as IObjectGraphType;

            // queue the extension
            var extension = template.Extensions.FirstOrDefault();
            var field = this.MakeGraphField(extension);
            collection.EnsureGraphFieldExtension(typeof(TwoPropertyObject), field);

            // ensure no types exists or were added
            Assert.AreEqual(0, collection.Count);
            Assert.AreEqual(1, collection.QueuedExtensionFieldCount);

            var fieldCount = graphType.Fields.Count;

            // ensure the type is added, the queue is emptied and hte total fields on the graph type
            // is correct
            collection.EnsureGraphType(graphType, typeof(TwoPropertyObject));
            Assert.AreEqual(1, collection.Count);
            Assert.AreEqual(0, collection.QueuedExtensionFieldCount);
            Assert.AreEqual(fieldCount + 1, graphType.Fields.Count);
        }

        [Test]
        public void QueueExtension_WhenAddedAfterType_IsAddedToTheTypeAndNotQueued()
        {
            var collection = new SchemaTypeCollection();
            var template = GraphQLProviders.TemplateProvider.ParseType(typeof(TypeExtensionController)) as IGraphControllerTemplate;

            var twoObjectGraphType = this.MakeGraphType(typeof(TwoPropertyObject), TypeKind.OBJECT) as IObjectGraphType;

            var fieldCount = twoObjectGraphType.Fields.Count;
            collection.EnsureGraphType(twoObjectGraphType, typeof(TwoPropertyObject));

            // queue the extension
            var extension = template.Extensions.FirstOrDefault();
            var field = this.MakeGraphField(extension);
            collection.EnsureGraphFieldExtension(typeof(TwoPropertyObject), field);

            // ensure the type is added, the queue remains empty and the total fields on the graph type
            // is correct
            Assert.AreEqual(1, collection.Count);
            Assert.AreEqual(0, collection.QueuedExtensionFieldCount);
            Assert.AreEqual(fieldCount + 1, twoObjectGraphType.Fields.Count);
        }
    }
}