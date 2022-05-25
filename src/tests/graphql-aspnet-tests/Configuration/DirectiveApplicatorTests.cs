// *************************************************************
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
    using GraphQL.AspNet.Interfaces.Configuration;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;
    using NUnit.Framework;

    [TestFixture]
    [FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
    public class DirectiveApplicatorTests
    {
        [Test]
        public void AllMatchedSchemaItems_HaveSeperateInstancesOfApplyDirective()
        {
            var matchedSchemaItems = new List<ISchemaItem>();
            object[] CreateArgs(ISchemaItem item)
            {
                matchedSchemaItems.Add(item);
                return new object[0];
            }

            var schema = new TestServerBuilder()
                .AddGraphType<TwoPropertyObject>()
                .Build()
                .Schema;

            var schema1 = new TestServerBuilder()
                 .AddGraphType<TwoPropertyObject>()
                 .Build()
                 .Schema;

            var applicator = new DirectiveApplicator("testDirective");
            applicator
                .WithArguments(CreateArgs)
                .Where(x => x is IGraphField);

            ((ISchemaConfigurationExtension)applicator).Configure(schema);

            for (var i = 0; i < matchedSchemaItems.Count; i++)
            {
                for (var j = i + 1; j < matchedSchemaItems.Count; j++)
                {
                    if (object.ReferenceEquals(matchedSchemaItems[i], matchedSchemaItems[j]))
                        Assert.Fail("Two schema items share an instance of an applied directive");
                }
            }
        }

        [Test]
        public void LastCallToWithArgumentsIsKept_ForAllItems()
        {
            var matchedSchemaItems = new List<ISchemaItem>();
            object[] CreateArgs(ISchemaItem item)
            {
                throw new System.Exception("shouldn't be called");
            }

            object[] CreateArgsOther(ISchemaItem item)
            {
                matchedSchemaItems.Add(item);
                return new object[0];
            }

            var schema = new TestServerBuilder()
                .AddGraphType<TwoPropertyObject>()
                .Build()
                .Schema;

            var applicator = new DirectiveApplicator("testDirective");
            applicator
                .WithArguments(CreateArgs)
                .WithArguments(new object[0])
                .WithArguments(CreateArgsOther)
                .Where(x => x is IGraphField);

            ((ISchemaConfigurationExtension)applicator).Configure(schema);

            // count would be greater than zero fi and only if the last
            // supplied function was executed and any fields were found
            Assert.IsTrue(matchedSchemaItems.Count > 0);
        }

        [Test]
        public void ConstantSuppliedArgumentsAreUsed_ForAllMatchedItems()
        {
            var argSet = new object[] { 1, 2, "bob" };

            var schema = new TestServerBuilder()
                .AddGraphType<TwoPropertyObject>()
                .Build()
                .Schema;

            var applicator = new DirectiveApplicator("testDirective");
            applicator
                .WithArguments(argSet)
                .Where(x => x is IGraphField);

            ((ISchemaConfigurationExtension)applicator).Configure(schema);

            foreach (var item in schema.AllSchemaItems())
            {
                if (item is IGraphField gf)
                {
                    var directive = item.AppliedDirectives.FirstOrDefault(x => x.DirectiveName == "testDirective");
                    if (directive != null)
                        Assert.AreEqual(argSet, directive.Arguments);
                }
            }
        }

        [Test]
        public void ClearRemovesDefaultFilters()
        {
            var schema = new TestServerBuilder()
                .AddGraphType<TwoPropertyObject>()
                .Build()
                .Schema;

            var applicator = new DirectiveApplicator("testDirective");
            applicator.Where(x => x is IGraphType);

            ((ISchemaConfigurationExtension)applicator).Configure(schema);

            // with teh default filter cleared introspection items etc would be
            // matched resulting in a higher count of fields effected
            applicator = new DirectiveApplicator("testDirective1");
            applicator
                .Clear()
                .Where(x => x is IGraphType);

            ((ISchemaConfigurationExtension)applicator).Configure(schema);

            var itemsEffectedWithDefaultFilter = schema.AllSchemaItems()
                .Where(x => x is IGraphType && x.AppliedDirectives.FirstOrDefault(y => y.DirectiveName == "testDirective") != null)
                .Count();

            var itemsEffectedWithOutDefaultFilter = schema.AllSchemaItems()
                .Where(x => x is IGraphType && x.AppliedDirectives.FirstOrDefault(y => y.DirectiveName == "testDirective1") != null)
                .Count();

            Assert.IsTrue(itemsEffectedWithOutDefaultFilter > itemsEffectedWithDefaultFilter);
        }
    }
}