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
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Interfaces.Internal;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Schemas.TypeSystem.TypeCollections;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Schemas.SchemaTestData.InterfaceRegistrationTestData;
    using NUnit.Framework;
    using GraphQL.AspNet.Schemas.Generation;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.Generation.TypeTemplates;
    using Microsoft.AspNetCore.Hosting.Server;
    using GraphQL.AspNet.Tests.CommonHelpers;

    [TestFixture]
    [FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
    public class SchemaTypeCollectionInterefaceImplementationTests
    {
        public static object[] _interfaceExtensionsWithObjects;
        public static object[] _interfaceAndObjectExtensions;

        static SchemaTypeCollectionInterefaceImplementationTests()
        {
            var list = GetPermutations(Enumerable.Range(1, 6), 6);
            _interfaceExtensionsWithObjects = list
                .Select(x => x.Cast<object>().ToArray())
                .Cast<object>()
                .ToArray();

            list = GetPermutations(Enumerable.Range(1, 7), 7);
            _interfaceAndObjectExtensions = list
                .Select(x => x.Cast<object>().ToArray())
                .Cast<object>()
                .ToArray();
        }

        private static IEnumerable<IEnumerable<T>> GetPermutations<T>(IEnumerable<T> list, int length)
        {
            if (length == 1)
                return list.Select(t => new T[] { t });

            return GetPermutations(list, length - 1)
                .SelectMany(
                    t =>
                        list.Where(e => !t.Contains(e)),
                    (t1, t2) =>
                        t1.Concat(new T[] { t2 }));
        }

        private SchemaTypeCollection _collection;
        private IInterfaceGraphType _ipastry;
        private IInterfaceGraphType _idonut;
        private IGraphField _hasSugarFieldExtension;
        private IGraphField _hasGlazeFieldExtension;
        private IGraphField _hasDoubleGlazeFieldExtension;
        private IObjectGraphType _pastry;
        private IObjectGraphType _donut;

        public SchemaTypeCollectionInterefaceImplementationTests()
        {
            _collection = new SchemaTypeCollection();

            // master interfaces
            _ipastry = this.MakeGraphType(typeof(IPastry), TypeKind.INTERFACE) as IInterfaceGraphType;
            _idonut = this.MakeGraphType(typeof(IDonut), TypeKind.INTERFACE) as IInterfaceGraphType;
            _pastry = this.MakeGraphType(typeof(Pastry), TypeKind.OBJECT) as IObjectGraphType;
            _donut = this.MakeGraphType(typeof(Donut), TypeKind.OBJECT) as IObjectGraphType;

            // type extension
            var template = new GraphControllerTemplate(typeof(PastryExtensionController)) as IGraphControllerTemplate;
            template.Parse();
            template.ValidateOrThrow();

            var hasSugarTemplate = template.Extensions.FirstOrDefault(x => x.DeclaredName == nameof(PastryExtensionController.HasSugarExtension));
            var hasGlazeTemplate = template.Extensions.FirstOrDefault(x => x.DeclaredName == nameof(PastryExtensionController.HasGlazeExtension));
            var hasDoubleGlazeTemplate = template.Extensions.FirstOrDefault(x => x.DeclaredName == nameof(PastryExtensionController.HasDoubleGlazeExtension));
            _hasSugarFieldExtension = this.MakeGraphField(hasSugarTemplate);
            _hasGlazeFieldExtension = this.MakeGraphField(hasGlazeTemplate);
            _hasDoubleGlazeFieldExtension = this.MakeGraphField(hasDoubleGlazeTemplate);

            // preflight check to ensure that donut hasnt somehow gained
            // the fields that should be added even though it inherits from ipastry
            Assert.AreEqual(2, _idonut.Fields.Count);
            Assert.IsNotNull(_idonut.Fields.FirstOrDefault(x => x.Name == "flavor"));
            Assert.IsNotNull(_idonut.Fields.FirstOrDefault(x => x.Name == "__typename"));
        }

        private IGraphType MakeGraphType(Type type, TypeKind kind)
        {
            var testServer = new TestServerBuilder().Build();

            var factory = testServer.CreateMakerFactory();

            var template = factory.MakeTemplate(type, kind);
            var maker = factory.CreateTypeMaker(type, kind);
            return maker.CreateGraphType(template).GraphType;
        }

        private IGraphField MakeGraphField(IGraphFieldTemplate fieldTemplate)
        {
            var testServer = new TestServerBuilder().Build();

            var factory = testServer.CreateMakerFactory();

            var maker = factory.CreateFieldMaker();
            return maker.CreateField(fieldTemplate).Field;
        }

        [TestCase(1, 2)]
        [TestCase(2, 1)]
        public void InterfaceFieldInhertance_Scenarios(
            int positionToAddIPastry,
            int positionToAddIDonut)
        {
            for (var i = 1; i <= 2; i++)
            {
                if (positionToAddIPastry == i)
                    _collection.EnsureGraphType(_ipastry, typeof(IPastry));
                else if (positionToAddIDonut == i)
                    _collection.EnsureGraphType(_idonut, typeof(IDonut));
            }

            // Donut Fields: __typeName, Flavor, Name
            Assert.AreEqual(3, _idonut.Fields.Count);
            Assert.IsNotNull(_idonut.Fields["flavor"]);
            Assert.IsNotNull(_idonut.Fields["name"]);
            Assert.IsNotNull(_idonut.Fields["__typename"]);

            // Pastry Fields: __typename, Name
            Assert.AreEqual(2, _ipastry.Fields.Count);
            Assert.IsNotNull(_ipastry.Fields["name"]);
            Assert.IsNotNull(_ipastry.Fields["__typename"]);
        }

        [TestCase(1, 2, 3)]
        [TestCase(1, 3, 2)]
        [TestCase(2, 1, 3)]
        [TestCase(2, 3, 1)]
        [TestCase(3, 1, 2)]
        [TestCase(3, 2, 1)]
        public void InterfaceExtension_Scenarios(
            int positionToAddIPastry,
            int positionToAddIDonut,
            int positiontoAddHasSugarField)
        {
            // add the interfaces and the extension in each concievable order
            // to ensure that the data makes it to the expected places by the end
            for (var i = 1; i <= 3; i++)
            {
                if (positionToAddIPastry == i)
                    _collection.EnsureGraphType(_ipastry, typeof(IPastry));
                else if (positionToAddIDonut == i)
                    _collection.EnsureGraphType(_idonut, typeof(IDonut));
                else if (positiontoAddHasSugarField == i)
                    _collection.EnsureGraphFieldExtension(typeof(IPastry), _hasSugarFieldExtension);
            }

            // __typeName, Flavor, Name, hasSugar
            Assert.AreEqual(4, _idonut.Fields.Count);
            Assert.IsNotNull(_idonut.Fields["flavor"]);
            Assert.IsNotNull(_idonut.Fields["name"]);
            Assert.IsNotNull(_idonut.Fields["hasSugar"]);
            Assert.IsNotNull(_idonut.Fields["__typename"]);

            // __typename, Name, hasSugar
            Assert.AreEqual(3, _ipastry.Fields.Count);
            Assert.IsNotNull(_ipastry.Fields["name"]);
            Assert.IsNotNull(_ipastry.Fields["hasSugar"]);
            Assert.IsNotNull(_ipastry.Fields["__typename"]);
        }

        [TestCase(1, 2, 3, 4, 5, 6)]
        [TestCase(1, 6, 5, 3, 4, 2)]
        [TestCase(1, 5, 6, 4, 3, 2)]
        [TestCase(2, 3, 1, 4, 5, 6)]
        [TestCase(2, 5, 6, 3, 4, 1)]
        [TestCase(2, 5, 3, 4, 6, 1)]
        [TestCase(3, 4, 2, 6, 5, 1)]
        [TestCase(3, 4, 1, 2, 6, 5)]
        [TestCase(4, 2, 3, 6, 1, 5)]
        [TestCase(4, 5, 6, 3, 2, 1)]
        [TestCase(5, 1, 4, 2, 6, 3)]
        [TestCase(5, 4, 1, 2, 3, 6)]
        [TestCase(6, 5, 4, 3, 2, 1)]
        [TestCase(6, 5, 2, 3, 4, 1)]
        public void InterfaceExtensionWithConcreteObject_Scenarios(
            int positionToAddIPastry,
            int positionToAddIDonut,
            int positionToAddDonut,
            int positionToAddPastry,
            int positiontoAddSugarField,
            int positionToAddGlazeField)
        {
            // add the interfaces and the extension in each concievable order
            // to ensure that the data makes it to the expected places by the end
            for (var i = 1; i <= 6; i++)
            {
                if (positionToAddIPastry == i)
                    _collection.EnsureGraphType(_ipastry, typeof(IPastry));
                else if (positionToAddIDonut == i)
                    _collection.EnsureGraphType(_idonut, typeof(IDonut));
                else if (positionToAddDonut == i)
                    _collection.EnsureGraphType(_donut, typeof(Donut));
                else if (positionToAddPastry == i)
                    _collection.EnsureGraphType(_pastry, typeof(Pastry));
                else if (positiontoAddSugarField == i)
                    _collection.EnsureGraphFieldExtension(typeof(IPastry), _hasSugarFieldExtension);
                else if (positionToAddGlazeField == i)
                    _collection.EnsureGraphFieldExtension(typeof(IDonut), _hasGlazeFieldExtension);
            }

            // IDonut: __typeName, Flavor, Name, hasSugar, hasGlaze
            Assert.AreEqual(5, _idonut.Fields.Count);
            Assert.IsNotNull(_idonut.Fields["flavor"]);
            Assert.IsNotNull(_idonut.Fields["name"]);
            Assert.IsNotNull(_idonut.Fields["hasSugar"]);
            Assert.IsNotNull(_idonut.Fields["hasGlaze"]);
            Assert.IsNotNull(_idonut.Fields["__typename"]);

            // Donut: __typeName, Flavor, Name, hasSugar, hasGlaze
            Assert.AreEqual(5, _donut.Fields.Count);
            Assert.IsNotNull(_donut.Fields["flavor"]);
            Assert.IsNotNull(_donut.Fields["name"]);
            Assert.IsNotNull(_donut.Fields["hasSugar"]);
            Assert.IsNotNull(_donut.Fields["hasGlaze"]);
            Assert.IsNotNull(_donut.Fields["__typename"]);

            // IPastry: __typename, Name, hasSugar
            Assert.AreEqual(3, _ipastry.Fields.Count);
            Assert.IsNotNull(_ipastry.Fields["name"]);
            Assert.IsNotNull(_ipastry.Fields["hasSugar"]);
            Assert.IsNotNull(_ipastry.Fields["__typename"]);

            // Pastry: __typename, Name, hasSugar
            Assert.AreEqual(3, _pastry.Fields.Count);
            Assert.IsNotNull(_pastry.Fields["name"]);
            Assert.IsNotNull(_pastry.Fields["hasSugar"]);
            Assert.IsNotNull(_pastry.Fields["__typename"]);
        }

        [TestCase(1, 2, 3, 4, 5, 6, 7)]
        [TestCase(1, 2, 3, 5, 4, 6, 7)]
        [TestCase(1, 2, 3, 5, 4, 7, 6)]
        [TestCase(1, 4, 3, 5, 2, 7, 6)]
        [TestCase(1, 5, 6, 4, 2, 7, 3)]
        [TestCase(2, 1, 3, 4, 5, 6, 7)]
        [TestCase(2, 3, 4, 6, 1, 5, 7)]
        [TestCase(3, 4, 1, 2, 5, 6, 7)]
        [TestCase(3, 5, 4, 1, 2, 6, 7)]
        [TestCase(4, 2, 3, 5, 1, 7, 6)]
        [TestCase(4, 1, 2, 5, 3, 6, 7)]
        [TestCase(5, 2, 3, 1, 4, 7, 6)]
        [TestCase(5, 3, 4, 1, 2, 7, 6)]
        [TestCase(6, 5, 1, 4, 7, 2, 3)]
        [TestCase(6, 5, 4, 7, 1, 3, 2)]
        [TestCase(7, 6, 5, 4, 3, 2, 1)]
        [TestCase(7, 2, 5, 3, 4, 6, 1)]
        public void InterfaceAndObjectTypeExtensions_Scenarios(
            int positionToAddIPastry,
            int positionToAddIDonut,
            int positionToAddDonut,
            int positionToAddPastry,
            int positiontoAddSugarField,
            int positionToAddGlazeField,
            int positiontoAddDoubleGlazeField)
        {
            // Tests the various scenarios of when a type extension, an OBJECT graph type and an INTERFACE
            // graph type may be registered to ensure that all graph types contain all extensions by the end
            // of the inclusion process regardless of the order encountered
            for (var i = 1; i <= 7; i++)
            {
                if (positionToAddIPastry == i)
                    _collection.EnsureGraphType(_ipastry, typeof(IPastry));
                else if (positionToAddIDonut == i)
                    _collection.EnsureGraphType(_idonut, typeof(IDonut));
                else if (positionToAddDonut == i)
                    _collection.EnsureGraphType(_donut, typeof(Donut));
                else if (positionToAddPastry == i)
                    _collection.EnsureGraphType(_pastry, typeof(Pastry));
                else if (positiontoAddSugarField == i)
                    _collection.EnsureGraphFieldExtension(typeof(IPastry), _hasSugarFieldExtension);
                else if (positionToAddGlazeField == i)
                    _collection.EnsureGraphFieldExtension(typeof(IDonut), _hasGlazeFieldExtension);
                else if (positiontoAddDoubleGlazeField == i)
                    _collection.EnsureGraphFieldExtension(typeof(Donut), _hasDoubleGlazeFieldExtension);
            }

            // IDonut: __typeName, Flavor, Name, hasSugar, hasGlaze
            Assert.AreEqual(5, _idonut.Fields.Count);
            Assert.IsNotNull(_idonut.Fields["flavor"]);
            Assert.IsNotNull(_idonut.Fields["name"]);
            Assert.IsNotNull(_idonut.Fields["hasSugar"]);
            Assert.IsNotNull(_idonut.Fields["hasGlaze"]);
            Assert.IsNotNull(_idonut.Fields["__typename"]);

            // Donut: __typeName, Flavor, Name, hasSugar, hasGlaze, hasDoubleGlaze
            Assert.AreEqual(6, _donut.Fields.Count);
            Assert.IsNotNull(_donut.Fields["flavor"]);
            Assert.IsNotNull(_donut.Fields["name"]);
            Assert.IsNotNull(_donut.Fields["hasSugar"]);
            Assert.IsNotNull(_donut.Fields["hasGlaze"]);
            Assert.IsNotNull(_donut.Fields["hasDoubleGlaze"]);
            Assert.IsNotNull(_donut.Fields["__typename"]);

            // IPastry: __typename, Name, hasSugar
            Assert.AreEqual(3, _ipastry.Fields.Count);
            Assert.IsNotNull(_ipastry.Fields["name"]);
            Assert.IsNotNull(_ipastry.Fields["hasSugar"]);
            Assert.IsNotNull(_ipastry.Fields["__typename"]);

            // Pastry: __typename, Name, hasSugar
            Assert.AreEqual(3, _pastry.Fields.Count);
            Assert.IsNotNull(_pastry.Fields["name"]);
            Assert.IsNotNull(_pastry.Fields["hasSugar"]);
            Assert.IsNotNull(_pastry.Fields["__typename"]);
        }
    }
}