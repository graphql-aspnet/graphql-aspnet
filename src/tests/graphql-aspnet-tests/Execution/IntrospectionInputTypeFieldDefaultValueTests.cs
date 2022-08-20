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
    public class IntrospectionInputTypeFieldDefaultValueTests
    {
        private static List<object[]> _testData;

        static IntrospectionInputTypeFieldDefaultValueTests()
        {
            _testData = new List<object[]>();

            _testData.Add(new object[] { typeof(NotRequiredIntObject), "0" });
            _testData.Add(new object[] { typeof(NotRequiredSetIntObject), "2345" });
            _testData.Add(new object[] { typeof(RequiredIntObject), null });

            _testData.Add(new object[] { typeof(NotRequiredNullableIntObject), "null" });
            _testData.Add(new object[] { typeof(NotRequiredSetNullableIntObject), "567" });
            _testData.Add(new object[] { typeof(RequiredNullableIntObject), null });

            _testData.Add(new object[] { typeof(NotRequiredDoubleObject), "0" });
            _testData.Add(new object[] { typeof(NotRequiredSetDoubleObject), "1.2345" });
            _testData.Add(new object[] { typeof(RequiredDoubleObject), null });

            _testData.Add(new object[] { typeof(NotRequiredClassObject), "null" });
            _testData.Add(new object[] { typeof(NotRequiredSetClassObject), "{ Property1: \"prop 1 default\" Property2: 38 }" });
            _testData.Add(new object[] { typeof(RequiredClassObject), null });

            _testData.Add(new object[] { typeof(NotRequiredDateTimeObject), $"\"{new DateTime(0).ToRfc3339String()}\"" });
            _testData.Add(new object[] { typeof(RequiredDateTimeObject), null });

            _testData.Add(new object[] { typeof(NotRequiredNotSetGraphIdObject), "null" });
            _testData.Add(new object[] { typeof(NotRequiredSetGraphIdObject), "\"abc\"" });
            _testData.Add(new object[] { typeof(RequiredGraphIdObject), null });

            _testData.Add(new object[] { typeof(NotRequiredStringObject), "null" });
            _testData.Add(new object[] { typeof(RequiredStringObject), null });
            _testData.Add(new object[] { typeof(NotRequiredNonNullableSetStringObject), "\"default string value\"" });
            _testData.Add(new object[] { typeof(NotRequiredNonNullableNotSetStringObject), "exception" });

            _testData.Add(new object[] { typeof(NotRequiredStructObject), "{ Property1: 0 Property2: null }" });
            _testData.Add(new object[] { typeof(RequiredStructObject), null });

            _testData.Add(new object[] { typeof(NotRequiredNotSetGuidObject), $"\"{Guid.Empty}\"" });
            _testData.Add(new object[] { typeof(NotRequiredSetGuidObject), $"\"033979ae-0955-4ef6-8a37-50bf0359601f\"" });
            _testData.Add(new object[] { typeof(RequiredGuidObject), null });
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