﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Tests.Configuration
{
    using System.Collections.Generic;
    using System.Linq;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Directives.Global;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.Configuration.SchemaItemExtensionTestData;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;
    using NSubstitute;
    using NUnit.Framework;

    [TestFixture]
    public class SchemaItemFilterExtensionTests
    {
        [Test]
        public void IsField_ByType()
        {
            var server = new TestServerBuilder()
                .AddType<TwoPropertyObject>()
                .Build();

            var foundItems = new List<ISchemaItem>();

            foreach (var item in server.Schema.AllSchemaItems())
            {
                if (item.IsField<TwoPropertyObject>("property1"))
                    foundItems.Add(item);
            }

            Assert.AreEqual(1, foundItems.Count);
        }

        [Test]
        public void IsField_ByName()
        {
            var server = new TestServerBuilder()
                .AddType<TwoPropertyObject>()
                .Build();

            var foundItems = new List<ISchemaItem>();

            foreach (var item in server.Schema.AllSchemaItems())
            {
                if (item.IsField("TwoPropertyObject", "property1"))
                    foundItems.Add(item);
            }

            Assert.AreEqual(1, foundItems.Count);
        }

        [Test]
        public void IsField_ByType_ForInputField()
        {
            var server = new TestServerBuilder()
                .AddType<TwoPropertyObject>(TypeKind.INPUT_OBJECT)
                .Build();

            var foundItems = new List<ISchemaItem>();

            foreach (var item in server.Schema.AllSchemaItems())
            {
                if (item.IsField<TwoPropertyObject>("property1", graphTypeKind: TypeKind.INPUT_OBJECT))
                    foundItems.Add(item);
            }

            Assert.AreEqual(1, foundItems.Count);
        }

        [Test]
        public void IsField_ByName_ForInputField()
        {
            var server = new TestServerBuilder()
                .AddType<TwoPropertyObject>(TypeKind.INPUT_OBJECT)
                .Build();

            var foundItems = new List<ISchemaItem>();

            foreach (var item in server.Schema.AllSchemaItems())
            {
                if (item.IsField("Input_TwoPropertyObject", "property1"))
                    foundItems.Add(item);
            }

            Assert.AreEqual(1, foundItems.Count);
        }

        [Test]
        public void IsObjectGraphType()
        {
            var server = new TestServerBuilder()
                .AddType<TwoPropertyObject>()
                .Build();

            var foundItems = new List<ISchemaItem>();

            foreach (var item in server.Schema.AllSchemaItems())
            {
                if (item.IsObjectGraphType<TwoPropertyObject>())
                    foundItems.Add(item);
            }

            Assert.AreEqual(1, foundItems.Count);
        }

        [Test]
        public void IsObjectGraphType_ByName()
        {
            var server = new TestServerBuilder()
                .AddType<TwoPropertyObject>()
                .Build();

            var foundItems = new List<ISchemaItem>();

            foreach (var item in server.Schema.AllSchemaItems())
            {
                if (item.IsObjectGraphType("TwoPropertyObject"))
                    foundItems.Add(item);
            }

            Assert.AreEqual(1, foundItems.Count);
        }

        [Test]
        public void IsGraphType()
        {
            var server = new TestServerBuilder()
                .AddType<TwoPropertyObject>()
                .Build();

            var foundItems = new List<ISchemaItem>();

            foreach (var item in server.Schema.AllSchemaItems())
            {
                if (item.IsGraphType<TwoPropertyObject>(TypeKind.OBJECT))
                    foundItems.Add(item);
            }

            Assert.AreEqual(1, foundItems.Count);
        }

        [Test]
        public void IsGraphType_ByName()
        {
            var server = new TestServerBuilder()
                .AddType<TwoPropertyObject>()
                .Build();

            var foundItems = new List<ISchemaItem>();

            foreach (var item in server.Schema.AllSchemaItems())
            {
                if (item.IsGraphType("TwoPropertyObject"))
                    foundItems.Add(item);
            }

            Assert.AreEqual(1, foundItems.Count);
        }

        [Test]
        public void IsGraphType_ForUnmatchedKind()
        {
            var server = new TestServerBuilder()
                .AddType<TwoPropertyObject>()
                .Build();

            var foundItems = new List<ISchemaItem>();

            foreach (var item in server.Schema.AllSchemaItems())
            {
                if (item.IsGraphType<TwoPropertyObject>(TypeKind.INPUT_OBJECT))
                    foundItems.Add(item);
            }

            Assert.AreEqual(0, foundItems.Count);
        }

        [Test]
        public void IsEnumValue()
        {
            var server = new TestServerBuilder()
                .AddType<EnumForSelecting>()
                .Build();

            var foundItems = new List<ISchemaItem>();

            foreach (var item in server.Schema.AllSchemaItems())
            {
                if (item.IsEnumValue(EnumForSelecting.Value2))
                    foundItems.Add(item);
            }

            Assert.AreEqual(1, foundItems.Count);
        }

        [Test]
        public void IsDirective()
        {
            var server = new TestServerBuilder()
                .AddType<TwoPropertyObject>()
                .Build();

            var foundItems = new List<ISchemaItem>();

            foreach (var item in server.Schema.AllSchemaItems(true))
            {
                if (item.IsDirective())
                    foundItems.Add(item);
            }

            // skip and include
            Assert.AreEqual(4, foundItems.Count);
            Assert.IsTrue(foundItems.Any(x => x.Name == Constants.ReservedNames.INCLUDE_DIRECTIVE));
            Assert.IsTrue(foundItems.Any(x => x.Name == Constants.ReservedNames.SKIP_DIRECTIVE));
            Assert.IsTrue(foundItems.Any(x => x.Name == Constants.ReservedNames.DEPRECATED_DIRECTIVE));
            Assert.IsTrue(foundItems.Any(x => x.Name == Constants.ReservedNames.SPECIFIED_BY_DIRECTIVE));
        }

        [Test]
        public void IsDirectiveArgument()
        {
            var server = new TestServerBuilder()
                .Build();

            var foundItems = new List<ISchemaItem>();

            foreach (var item in server.Schema.AllSchemaItems(true))
            {
                if (item.IsDirectiveArgument<IncludeDirective>("if"))
                    foundItems.Add(item);
            }

            // skip and include
            Assert.AreEqual(1, foundItems.Count);
            Assert.IsTrue(foundItems.Any(x => x is IGraphArgument a && a.Parent.Name == Constants.ReservedNames.INCLUDE_DIRECTIVE));
        }

        [TestCase("@include")]
        [TestCase("include")]
        public void IsDirectiveArgument_ByType(string directiveName)
        {
            var server = new TestServerBuilder()
                .Build();

            var foundItems = new List<ISchemaItem>();

            foreach (var item in server.Schema.AllSchemaItems(true))
            {
                if (item.IsDirectiveArgument(directiveName, "if"))
                    foundItems.Add(item);
            }

            // skip and include
            Assert.AreEqual(1, foundItems.Count);
            Assert.IsTrue(foundItems.Any(x => x is IGraphArgument a && a.Parent.Name == Constants.ReservedNames.INCLUDE_DIRECTIVE));
        }

        [Test]
        public void NullSchemaItem_IsConsideredVirtual()
        {
            ISchemaItem item = null;
            Assert.IsTrue(item.IsVirtualItem());
        }

        [Test]
        public void ISchema_IsConsideredNotVirtual()
        {
            var schema = Substitute.For<ISchema>();
            Assert.IsFalse(schema.IsVirtualItem());
        }

        [Test]
        public void IsArgument_Any()
        {
            var server = new TestServerBuilder()
            .AddGraphQL(o =>
            {
                o.AddGraphType<MethodObject>();
            })
            .Build();

            var foundItems = new List<ISchemaItem>();

            foreach (var item in server.Schema.AllSchemaItems(true))
            {
                if (item.IsArgument<MethodObject>("name", "whichName"))
                    foundItems.Add(item);
            }

            Assert.AreEqual(1, foundItems.Count);
            Assert.IsTrue(foundItems.Any(x => x.Name == "whichName"));
        }
    }
}