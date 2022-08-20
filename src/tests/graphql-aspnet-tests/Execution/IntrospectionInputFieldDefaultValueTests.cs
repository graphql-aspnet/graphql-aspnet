// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Response;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.Execution.IntrospecetionInputFieldTestData;
    using GraphQL.AspNet.Tests.Framework;
    using NUnit.Framework;

    [TestFixture]
    public class IntrospectionInputFieldDefaultValueTests
    {
        private static List<object[]> _testData;

        static IntrospectionInputFieldDefaultValueTests()
        {
            _testData = new List<object[]>();

            _testData.Add(new object[] { typeof(NotRequiredIntObject), "0" });
            _testData.Add(new object[] { typeof(RequiredIntObject), null });

            _testData.Add(new object[] { typeof(NotRequiredClassObject), "null" });
            _testData.Add(new object[] { typeof(NotRequiredSetClassObject), "{ Property1: \"prop 1 default\" Property2: 38 }" });
            _testData.Add(new object[] { typeof(RequiredClassObject), null });

            _testData.Add(new object[] { typeof(NotRequiredDateTimeObject), $"\"{new DateTime(0).ToRfc3339String()}\"" });
            _testData.Add(new object[] { typeof(RequiredDateTimeObject), null });

            _testData.Add(new object[] { typeof(NotRequiredNotSetGraphIdObject), "\"\"" });
            _testData.Add(new object[] { typeof(NotRequiredSetGraphIdObject), "\"abc\"" });
            _testData.Add(new object[] { typeof(RequiredGraphIdObject), null });

            _testData.Add(new object[] { typeof(NotRequiredStringObject), "null" });
            _testData.Add(new object[] { typeof(RequiredStringObject), null });
            _testData.Add(new object[] { typeof(NotRequiredNonNullableSetStringObject), "\"\"" });
            _testData.Add(new object[] { typeof(NotRequiredNonNullableNotSetStringObject), "exception" });

            _testData.Add(new object[] { typeof(NotRequiredStructObject), "{ Property1: 0 Property2: null }" });
            _testData.Add(new object[] { typeof(RequiredStructObject), null });
        }

        [TestCaseSource(nameof(_testData))]
        public async Task Execute(Type inputType, string expectedDefaultValue)
        {
            var serverBuilder = new TestServerBuilder(TestOptions.UseCodeDeclaredNames)
                .AddType(inputType, TypeKind.INPUT_OBJECT);

            if (expectedDefaultValue == "exception")
            {
                Assert.Throws<GraphTypeDeclarationException>(() =>
                {
                    serverBuilder.Build();
                });

                return;
            }

            var server = serverBuilder.Build();
            var builder = server.CreateQueryContextBuilder();

            builder.AddQueryText(@"
                {
                    query: __type(name: ""Input_" + inputType.Name + @""")
                    {
                        inputFields{
                            defaultValue
                        }
                    }
                }");

            var response = await server.ExecuteQuery(builder);
            Assert.AreEqual(0, response.Messages.Count);
            var query = response.Data.Fields["query"] as IResponseFieldSet;
            var inputFields = query.Fields["inputFields"] as IResponseList;
            var inputField = inputFields.Items[0] as IResponseFieldSet;
            var defaultValue = inputField.Fields["defaultValue"] as IResponseSingleValue;
            Assert.AreEqual(expectedDefaultValue, defaultValue?.Value);
        }
    }
}