// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Tests.Directives
{
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Configuration.Exceptions;
    using GraphQL.AspNet.Tests.Directives.RepeatableDirectivesTestData;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;

    [TestFixture]
    public class RepeatableDirectiveTests
    {
        [Test]
        public void RepeatableDirective_WhenRepeated_IsAppliedMultipleTimes()
        {
            var directive = new RepeatableObjectDirective();

            var builder = new TestServerBuilder();
            builder.AddSingleton(directive);
            builder.AddGraphQL(o =>
            {
                o.AddGraphType<TwoPropertyObject>();
                o.ApplyDirective<RepeatableObjectDirective>()
                    .WithArguments("data1")
                    .ToItems(x => x.IsObjectGraphType<TwoPropertyObject>());

                o.ApplyDirective<RepeatableObjectDirective>()
                    .WithArguments("data2")
                    .ToItems(x => x.IsObjectGraphType<TwoPropertyObject>());
            });

            var server = builder.Build();

            Assert.AreEqual(2, directive.TotalApplications);
            Assert.AreEqual("data1", directive.SuppliedData[0]);
            Assert.AreEqual("data2", directive.SuppliedData[1]);
        }

        [Test]
        public void NonRepeatableDirective_WhenRepeated_ThrowsException()
        {
            var directive = new NonRepeatableObjectDirective();

            var builder = new TestServerBuilder();
            builder.AddSingleton(directive);
            builder.AddGraphQL(o =>
            {
                o.AddGraphType<TwoPropertyObject>();
                o.ApplyDirective<NonRepeatableObjectDirective>()
                    .WithArguments("data1")
                    .ToItems(x => x.IsObjectGraphType<TwoPropertyObject>());

                o.ApplyDirective<NonRepeatableObjectDirective>()
                    .WithArguments("data2")
                    .ToItems(x => x.IsObjectGraphType<TwoPropertyObject>());
            });

            Assert.Throws<SchemaConfigurationException>(() =>
            {
                var server = builder.Build();
            });
        }
    }
}