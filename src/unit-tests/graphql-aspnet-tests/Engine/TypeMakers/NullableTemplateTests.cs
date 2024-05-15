// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Engine.TypeMakers
{
    using System.Linq;
    using GraphQL.AspNet.Tests.Engine.TypeMakers.TestData;
    using GraphQL.AspNet.Tests.Framework;
    using NUnit.Framework;

    public class NullableTemplateTests : GraphTypeMakerTestBase
    {
        [Test]
        public void InfersCorrectTypes_NonNullInteger()
        {
            var server = new TestServerBuilder().Build();
            var template = GraphQLTemplateHelper.CreateObjectTemplate<NullableContextObject>();

            var field = template.FieldTemplates.FirstOrDefault(it => it.Name == "NonNullInteger");

            Assert.NotNull(field);
            Assert.AreEqual("Int!", field.TypeExpression.ToString());
        }

        [Test]
        public void InfersCorrectTypes_NullableInteger()
        {
            var server = new TestServerBuilder().Build();
            var template = GraphQLTemplateHelper.CreateObjectTemplate<NullableContextObject>();

            var field = template.FieldTemplates.FirstOrDefault(it => it.Name == "NullableInteger");

            Assert.NotNull(field);
            Assert.AreEqual("Int", field.TypeExpression.ToString());
        }

        [Test]
        public void InfersCorrectTypes_NonNullString()
        {
            var server = new TestServerBuilder().Build();
            var template = GraphQLTemplateHelper.CreateObjectTemplate<NullableContextObject>();

            var field = template.FieldTemplates.FirstOrDefault(it => it.Name == "NonNullString");

            Assert.NotNull(field);
            Assert.AreEqual("String!", field.TypeExpression.ToString());
        }

        [Test]
        public void InfersCorrectTypes_NullableString()
        {
            var server = new TestServerBuilder().Build();
            var template = GraphQLTemplateHelper.CreateObjectTemplate<NullableContextObject>();

            var field = template.FieldTemplates.FirstOrDefault(it => it.Name == "NullableString");

            Assert.NotNull(field);
            Assert.AreEqual("String", field.TypeExpression.ToString());
        }

        [Test]
        [Ignore("NullabilityInfo reflection of inner element is not available (May 2024)")]
        public void InfersCorrectTypes_NonNullListNonNullItems()
        {
            var server = new TestServerBuilder().Build();
            var template = GraphQLTemplateHelper.CreateObjectTemplate<NullableContextObject>();

            var nonNullString = template.FieldTemplates.FirstOrDefault(it => it.Name == "NonNullListNonNullItems");

            Assert.NotNull(nonNullString);
            Assert.AreEqual("[String!]!", nonNullString.TypeExpression.ToString());
        }

        [Test]
        [Ignore("NullabilityInfo reflection of inner element is not available (May 2024)")]
        public void InfersCorrectTypes_NullableListNonNullItems()
        {
            var server = new TestServerBuilder().Build();
            var template = GraphQLTemplateHelper.CreateObjectTemplate<NullableContextObject>();

            var nonNullString = template.FieldTemplates.FirstOrDefault(it => it.Name == "NullableListNonNullItems");

            Assert.NotNull(nonNullString);
            Assert.AreEqual("[String!]", nonNullString.TypeExpression.ToString());
        }

        [Test]
        public void InfersCorrectTypes_NonNullListNullableItems()
        {
            var server = new TestServerBuilder().Build();
            var template = GraphQLTemplateHelper.CreateObjectTemplate<NullableContextObject>();

            var nonNullString = template.FieldTemplates.FirstOrDefault(it => it.Name == "NonNullListNullableItems");

            Assert.NotNull(nonNullString);
            Assert.AreEqual("[String]!", nonNullString.TypeExpression.ToString());
        }

        [Test]
        public void InfersCorrectTypes_NullableListNullableItems()
        {
            var server = new TestServerBuilder().Build();
            var template = GraphQLTemplateHelper.CreateObjectTemplate<NullableContextObject>();

            var nonNullString = template.FieldTemplates.FirstOrDefault(it => it.Name == "NullableListNullableItems");

            Assert.NotNull(nonNullString);
            Assert.AreEqual("[String]", nonNullString.TypeExpression.ToString());
        }

        [Test]
        public void InfersCorrectTypes_NonNullMethod()
        {
            var server = new TestServerBuilder().Build();
            var template = GraphQLTemplateHelper.CreateObjectTemplate<NullableContextObject>();

            var nonNullString = template.FieldTemplates.FirstOrDefault(it => it.Name == "NonNullMethod");

            Assert.NotNull(nonNullString);
            Assert.AreEqual("String!", nonNullString.TypeExpression.ToString());
        }

        [Test]
        public void InfersCorrectTypes_NullableMethod()
        {
            var server = new TestServerBuilder().Build();
            var template = GraphQLTemplateHelper.CreateObjectTemplate<NullableContextObject>();

            var nonNullString = template.FieldTemplates.FirstOrDefault(it => it.Name == "NullableMethod");

            Assert.NotNull(nonNullString);
            Assert.AreEqual("String", nonNullString.TypeExpression.ToString());
        }
    }
}