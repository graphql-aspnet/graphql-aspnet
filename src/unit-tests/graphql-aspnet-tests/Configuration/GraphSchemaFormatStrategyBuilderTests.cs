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
            var strat = (SchemaFormatStrategy)SchemaFormatStrategyBuilder
                .Create()
                .Build();

            Assert.AreEqual(SchemaItemNameFormatOptions.ProperCase, strat.GraphTypeNameStrategy);
            Assert.AreEqual(SchemaItemNameFormatOptions.CamelCase, strat.FieldNameStrategy);
            Assert.AreEqual(SchemaItemNameFormatOptions.UpperCase, strat.EnumValueStrategy);
            Assert.AreEqual(TypeExpressionNullabilityFormatRules.Default, strat.NullabilityStrategy);
        }

        [Test]
        public void WithGraphTypeNameFormat_UpdatesOnlyTypeNameStrategy()
        {
            var strat = (SchemaFormatStrategy)SchemaFormatStrategyBuilder
                .Create()
                .WithGraphTypeNameFormat(SchemaItemNameFormatOptions.NoChanges)
                .Build();

            Assert.AreEqual(SchemaItemNameFormatOptions.NoChanges, strat.GraphTypeNameStrategy);
            Assert.AreEqual(SchemaItemNameFormatOptions.CamelCase, strat.FieldNameStrategy);
            Assert.AreEqual(SchemaItemNameFormatOptions.UpperCase, strat.EnumValueStrategy);
        }

        [Test]
        public void WithFieldNameFormat_UpdatesOnlyFieldNameStrategy()
        {
            var strat = (SchemaFormatStrategy)SchemaFormatStrategyBuilder
                .Create()
                .WithFieldNameFormat(SchemaItemNameFormatOptions.NoChanges)
                .Build();

            Assert.AreEqual(SchemaItemNameFormatOptions.ProperCase, strat.GraphTypeNameStrategy);
            Assert.AreEqual(SchemaItemNameFormatOptions.NoChanges, strat.FieldNameStrategy);
            Assert.AreEqual(SchemaItemNameFormatOptions.UpperCase, strat.EnumValueStrategy);
        }

        [Test]
        public void WithEnumValueFormat_UpdatesOnlyEnumValueStrategy()
        {
            var strat = (SchemaFormatStrategy)SchemaFormatStrategyBuilder
                .Create()
                .WithEnumValueFormat(SchemaItemNameFormatOptions.NoChanges)
                .Build();

            Assert.AreEqual(SchemaItemNameFormatOptions.ProperCase, strat.GraphTypeNameStrategy);
            Assert.AreEqual(SchemaItemNameFormatOptions.CamelCase, strat.FieldNameStrategy);
            Assert.AreEqual(SchemaItemNameFormatOptions.NoChanges, strat.EnumValueStrategy);
        }

        [Test]
        public void ClearNullabilityRules_UpdatesNullabilityStrategy()
        {
            var strat = (SchemaFormatStrategy)SchemaFormatStrategyBuilder
                .Create()
                .ClearNullabilityRules()
                .Build();

            Assert.AreEqual(TypeExpressionNullabilityFormatRules.None, strat.NullabilityStrategy);
        }

        [Test]
        public void WithRequiredObjects_UpdatesNullabilityStrategy()
        {
            var strat = (SchemaFormatStrategy)SchemaFormatStrategyBuilder
                .Create()
                .WithRequiredObjects()
                .Build();

            Assert.IsTrue(strat.NullabilityStrategy.HasFlag(TypeExpressionNullabilityFormatRules.NonNullIntermediateTypes));
            Assert.IsTrue(strat.NullabilityStrategy.HasFlag(TypeExpressionNullabilityFormatRules.NonNullReferenceTypes));
        }

        [Test]
        public void WithRequiredStrings_UpdatesNullabilityStrategy()
        {
            var strat = (SchemaFormatStrategy)SchemaFormatStrategyBuilder
                .Create()
                .WithRequiredStrings()
                .Build();

            Assert.IsTrue(strat.NullabilityStrategy.HasFlag(TypeExpressionNullabilityFormatRules.NonNullIntermediateTypes));
            Assert.IsTrue(strat.NullabilityStrategy.HasFlag(TypeExpressionNullabilityFormatRules.NonNullStrings));
        }

        [Test]
        public void WithRequiredLists_UpdatesNullabilityStrategy()
        {
            var strat = (SchemaFormatStrategy)SchemaFormatStrategyBuilder
                .Create()
                .WithRequiredLists()
                .Build();

            Assert.IsTrue(strat.NullabilityStrategy.HasFlag(TypeExpressionNullabilityFormatRules.NonNullIntermediateTypes));
            Assert.IsTrue(strat.NullabilityStrategy.HasFlag(TypeExpressionNullabilityFormatRules.NonNullLists));
        }
    }
}