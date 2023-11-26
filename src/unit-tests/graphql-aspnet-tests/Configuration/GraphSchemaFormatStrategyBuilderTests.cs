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
    using GraphQL.AspNet.Configuration.Formatting;
    using NUnit.Framework;

    [TestFixture]
    public class GraphSchemaFormatStrategyBuilderTests
    {
        [Test]
        public void EnsureDefaults()
        {
            var strat = GraphSchemaFormatStrategyBuilder
                .Create()
                .Build();

            Assert.AreEqual(GraphNameFormatStrategy.ProperCase, strat.GraphTypeNameStrategy);
            Assert.AreEqual(GraphNameFormatStrategy.CamelCase, strat.FieldNameStrategy);
            Assert.AreEqual(GraphNameFormatStrategy.UpperCase, strat.EnumValueStrategy);
            Assert.AreEqual(NullabilityFormatStrategy.Default, strat.NullabilityStrategy);
        }

        [Test]
        public void WithGraphTypeNameFormat_UpdatesOnlyTypeNameStrategy()
        {
            var strat = GraphSchemaFormatStrategyBuilder
                .Create()
                .WithGraphTypeNameFormat(GraphNameFormatStrategy.NoChanges)
                .Build();

            Assert.AreEqual(GraphNameFormatStrategy.NoChanges, strat.GraphTypeNameStrategy);
            Assert.AreEqual(GraphNameFormatStrategy.CamelCase, strat.FieldNameStrategy);
            Assert.AreEqual(GraphNameFormatStrategy.UpperCase, strat.EnumValueStrategy);
        }

        [Test]
        public void WithFieldNameFormat_UpdatesOnlyFieldNameStrategy()
        {
            var strat = GraphSchemaFormatStrategyBuilder
                .Create()
                .WithFieldNameFormat(GraphNameFormatStrategy.NoChanges)
                .Build();

            Assert.AreEqual(GraphNameFormatStrategy.ProperCase, strat.GraphTypeNameStrategy);
            Assert.AreEqual(GraphNameFormatStrategy.NoChanges, strat.FieldNameStrategy);
            Assert.AreEqual(GraphNameFormatStrategy.UpperCase, strat.EnumValueStrategy);
        }

        [Test]
        public void WithEnumValueFormat_UpdatesOnlyEnumValueStrategy()
        {
            var strat = GraphSchemaFormatStrategyBuilder
                .Create()
                .WithEnumValueFormat(GraphNameFormatStrategy.NoChanges)
                .Build();

            Assert.AreEqual(GraphNameFormatStrategy.ProperCase, strat.GraphTypeNameStrategy);
            Assert.AreEqual(GraphNameFormatStrategy.CamelCase, strat.FieldNameStrategy);
            Assert.AreEqual(GraphNameFormatStrategy.NoChanges, strat.EnumValueStrategy);
        }

        [Test]
        public void WithRequiredObjects_UpdatesNullabilityStrategy()
        {

            var strat = GraphSchemaFormatStrategyBuilder
                .Create()
                .WithRequiredObjects()
                .Build();

            Assert.IsTrue(strat.NullabilityStrategy.HasFlag(NullabilityFormatStrategy.NonNullReferenceTypes));
        }

        [Test]
        public void WithRequiredStrings_UpdatesNullabilityStrategy()
        {

            var strat = GraphSchemaFormatStrategyBuilder
                .Create()
                .WithRequiredStrings()
                .Build();

            Assert.IsTrue(strat.NullabilityStrategy.HasFlag(NullabilityFormatStrategy.NonNullStrings));
        }

        [Test]
        public void WithRequiredLists_UpdatesNullabilityStrategy()
        {

            var strat = GraphSchemaFormatStrategyBuilder
                .Create()
                .WithRequiredLists()
                .Build();

            Assert.IsTrue(strat.NullabilityStrategy.HasFlag(NullabilityFormatStrategy.NonNullLists));
        }
    }
}