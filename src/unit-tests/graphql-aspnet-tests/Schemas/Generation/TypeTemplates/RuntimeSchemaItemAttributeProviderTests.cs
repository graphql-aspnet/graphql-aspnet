// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Schemas.Generation.TypeTemplates
{
    using System;
    using System.Collections.Generic;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Interfaces.Schema.RuntimeDefinitions;
    using GraphQL.AspNet.Schemas.Generation.TypeTemplates;
    using GraphQL.AspNet.Tests.Middleware.FieldSecurityMiddlewareTestData;
    using GraphQL.AspNet.Tests.Schemas.Generation.TypeTemplates.RuntimeSchemaItemAttributeProviderTestData;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class RuntimeSchemaItemAttributeProviderTests
    {
        public delegate int NoParamDelegate();

        [Teacher]
        public int MethodWithTeacherAttribute()
        {
            return 0;
        }

        [Person]
        public int MethodWithPersonAttribute()
        {
            return 0;
        }

        public int MethodWithNoAttributes()
        {
            return 0;
        }

        [TestCase(typeof(TeacherAttribute), true)]
        [TestCase(typeof(PersonAttribute), true)]
        [TestCase(typeof(AuthorAttribute), false)]
        public void HasAttribute_DirectlyDefinedDoesProduceValue(Type typeToSearchFor, bool shouldBeFound)
        {
            // sanity check to ensure the world is working right and MethodInfo
            // as an attribute provider behaves as expected
            var mock = new Mock<IGraphQLResolvableSchemaItemDefinition>();

            var method = typeof(RuntimeSchemaItemAttributeProviderTests)
                .GetMethod(nameof(MethodWithTeacherAttribute));

            var wasFound = method.HasAttribute(typeToSearchFor);
            Assert.AreEqual(shouldBeFound, wasFound);
        }

        [TestCase(typeof(TeacherAttribute), true)]
        [TestCase(typeof(PersonAttribute), true)]
        [TestCase(typeof(AuthorAttribute), false)]
        public void HasAttribute_AllProvidedExternally(Type typeToSearchFor, bool shouldBeFound)
        {
            var mock = new Mock<IGraphQLResolvableSchemaItemDefinition>();

            var method = typeof(RuntimeSchemaItemAttributeProviderTests)
                .GetMethod(nameof(MethodWithNoAttributes));

            var del = method.CreateDelegate<NoParamDelegate>(this);
            mock.Setup(x => x.Resolver).Returns(del);

            // append the teacher attribute to a list in the provider
            // not as part of the method
            var attribs = new List<Attribute>();
            attribs.Add(new TeacherAttribute());
            mock.Setup(x => x.Attributes).Returns(attribs);

            var provider = new RuntimeSchemaItemAttributeProvider(mock.Object);

            var wasFound = provider.HasAttribute(typeToSearchFor);
            Assert.AreEqual(shouldBeFound, wasFound);
        }
    }
}