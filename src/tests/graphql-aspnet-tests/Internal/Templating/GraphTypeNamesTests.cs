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
    using System.Collections.Generic;
    using System.Linq;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Internal;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.Internal.Templating.GraphTypeNameTestData;
    using NUnit.Framework;

    [TestFixture]
    public class GraphTypeNamesTests
    {
        [Test]
        public void Item_WithAttribute_AndAttributeIsAnInvalidName_ThrowsException()
        {
            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                GraphTypeNames.ParseName<AttributedClassInvalidName>(TypeKind.OBJECT);
            });
        }

        [Test]
        public void GenericItem_WithNoAttribute_StoresMultipleNamesCorrectly()
        {
            var name = GraphTypeNames.ParseName<GenericClass<int, string>>(TypeKind.OBJECT);
            Assert.AreEqual("GenericClass_int_string_", name);

            name = GraphTypeNames.ParseName<GenericClass<int, int>>(TypeKind.OBJECT);
            Assert.AreEqual("GenericClass_int_int_", name);

            name = GraphTypeNames.ParseName<GenericClass<int, string>>(TypeKind.OBJECT);
            Assert.AreEqual("GenericClass_int_string_", name);
        }

        [Test]
        public void GenericItem_WithAttribute_ThrowsException()
        {
            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                var name = GraphTypeNames.ParseName<GenericClassWithAttribute<int, string>>(TypeKind.OBJECT);
            });
        }

        [Test]
        public void Item_WithAttribute_ReturnsAttributeDefinedName()
        {
            var name = GraphTypeNames.ParseName<AttributedClass>(TypeKind.OBJECT);
            Assert.AreEqual("AlternateName", name);
        }

        [Test]
        public void Item_ScalarParsesToScalarName_Always()
        {
            foreach (var scalar in GraphQLProviders.ScalarProvider)
            {
                var nameSet = new HashSet<string>();
                var concreteType = GraphQLProviders.ScalarProvider.RetrieveConcreteType(scalar.Name);
                foreach (var typeKind in Enum.GetValues(typeof(TypeKind)).Cast<TypeKind>())
                {
                    var name = GraphTypeNames.ParseName(concreteType, typeKind);
                    if (!nameSet.Contains(name))
                        nameSet.Add(name);

                    Assert.AreEqual(1, nameSet.Count);
                }
            }
        }

        [Test]
        public void Item_EnumParsesToEnumName_RegardlessOfTypeKind()
        {
            var nameSet = new HashSet<string>();
            foreach (var typeKind in Enum.GetValues(typeof(TypeKind)).Cast<TypeKind>())
            {
                var name = GraphTypeNames.ParseName(typeof(EnumNameTest), typeKind);
                if (!nameSet.Contains(name))
                    nameSet.Add(name);

                Assert.AreEqual(1, nameSet.Count);
            }
        }

        [Test]
        public void Item_EnumWithTypeNameAttribute_ParseesToEnumName_RegardlessOfTypeKind()
        {
            var nameSet = new HashSet<string>();
            foreach (var typeKind in Enum.GetValues(typeof(TypeKind)).Cast<TypeKind>())
            {
                var name = GraphTypeNames.ParseName(typeof(EnumNameTestWithTypeName), typeKind);
                if (!nameSet.Contains(name))
                    nameSet.Add(name);

                Assert.AreEqual("GreatEnum", name);
                Assert.AreEqual(1, nameSet.Count);
            }
        }

        [Test]
        public void Item_WithNoAttribute_ReturnsNameOfClass()
        {
            var name = GraphTypeNames.ParseName<NoAttributeClass>(TypeKind.OBJECT);
            Assert.AreEqual(nameof(NoAttributeClass), name);
        }

        [Test]
        public void Item_WithNoAttribute_StoredAsInputAndNormal_ReturnsNameOfClass()
        {
            var name = GraphTypeNames.ParseName<NoAttributeClass>(TypeKind.OBJECT);
            var inputName = GraphTypeNames.ParseName<NoAttributeClass>(TypeKind.INPUT_OBJECT);
            Assert.AreEqual(nameof(NoAttributeClass), name);
            Assert.AreEqual($"Input_{nameof(NoAttributeClass)}", inputName);
        }

        [Test]
        public void Item_ForceAssign_NewName_ReturnsNewName()
        {
            var name = GraphTypeNames.ParseName<NoAttributeClassForNewName>(TypeKind.OBJECT);
            Assert.AreEqual(nameof(NoAttributeClassForNewName), name);

            GraphTypeNames.AssignName(typeof(NoAttributeClassForNewName), TypeKind.OBJECT, "NewName123");
            name = GraphTypeNames.ParseName<NoAttributeClassForNewName>(TypeKind.OBJECT);
            Assert.AreEqual("NewName123", name);
        }

        // need tests for generic type names against graphtypename class
    }
}