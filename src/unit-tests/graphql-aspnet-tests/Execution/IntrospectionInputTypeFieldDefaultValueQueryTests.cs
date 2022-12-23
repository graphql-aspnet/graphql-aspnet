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
    using GraphQL.AspNet.Interfaces.Execution.Response;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.Execution.TestData.IntrospecetionInputFieldTestData;
    using GraphQL.AspNet.Tests.Framework;
    using NUnit.Framework;

    [TestFixture]
    public class IntrospectionInputTypeFieldDefaultValueQueryTests
    {
        private static List<object[]> _testData;

        static IntrospectionInputTypeFieldDefaultValueQueryTests()
        {
            _testData = new List<object[]>();

            _testData.Add(new object[] { typeof(NotRequiredNotSetIntObject), "0" });
            _testData.Add(new object[] { typeof(NotRequiredSetIntObject), "2345" });
            _testData.Add(new object[] { typeof(RequiredIntObject), null });

            _testData.Add(new object[] { typeof(NotRequiredNotSetNullableIntObject), "null" });
            _testData.Add(new object[] { typeof(NotRequiredSetNullableIntObject), "567" });
            _testData.Add(new object[] { typeof(RequiredNullableIntObject), null });

            _testData.Add(new object[] { typeof(NotRequiredNonNullableSetNullableIntObject), "3" });
            _testData.Add(new object[] { typeof(NotRequiredNonNullableNotSetNullableIntObject), "<exception>" });
            _testData.Add(new object[] { typeof(RequiredNonNullableNullableIntObject), null });

            _testData.Add(new object[] { typeof(NotRequiredNotSetDoubleObject), "0" });
            _testData.Add(new object[] { typeof(NotRequiredSetDoubleObject), "1.2345" });
            _testData.Add(new object[] { typeof(RequiredDoubleObject), null });

            _testData.Add(new object[] { typeof(NotRequiredNotSetClassObject), "null" });
            _testData.Add(new object[] { typeof(NotRequiredSetClassObject), "{ Property1: \"prop 1 default\" Property2: 38 }" });
            _testData.Add(new object[] { typeof(NotRequiredNonNullableNotSetClassObject), "<exception>" });
            _testData.Add(new object[] { typeof(RequiredClassObject), null });

            _testData.Add(new object[] { typeof(NotRequiredNotSetDateTimeObject), $"\"{new DateTime(0).ToRfc3339String()}\"" });
            _testData.Add(new object[] { typeof(RequiredDateTimeObject), null });

            _testData.Add(new object[] { typeof(NotRequiredNotSetDatetimeOffsetObject), $"\"{new DateTimeOffset(0, TimeSpan.Zero).ToRfc3339String()}\"" });
            _testData.Add(new object[] { typeof(NotRequiredSetDatetimeOffsetObject), $"\"{new DateTimeOffset(2022, 8, 20, 11, 59, 0, TimeSpan.Zero).ToRfc3339String()}\"" });
            _testData.Add(new object[] { typeof(RequiredDateTimeOffsetObject), null });

            _testData.Add(new object[] { typeof(NotRequiredNotSetGraphIdObject), "null" });
            _testData.Add(new object[] { typeof(NotRequiredSetGraphIdObject), "\"abc\"" });
            _testData.Add(new object[] { typeof(RequiredGraphIdObject), null });

            _testData.Add(new object[] { typeof(NotRequiredNotSetStringObject), "null" });
            _testData.Add(new object[] { typeof(NotRequiredSetStringObject), "\"prop 1 default set\"" });
            _testData.Add(new object[] { typeof(NotRequiredNonNullableSetStringObject), "\"default string value\"" });
            _testData.Add(new object[] { typeof(NotRequiredNonNullableNotSetStringObject), "<exception>" });
            _testData.Add(new object[] { typeof(RequiredStringObject), null });

            _testData.Add(new object[] { typeof(NotRequiredNotSetStructObject), "{ TestStructProp1: 0 TestStructProp2: null }" });
            _testData.Add(new object[] { typeof(NotRequiredSetStructObject), "{ TestStructProp1: 89 TestStructProp2: \"default value set 89\" }" });
            _testData.Add(new object[] { typeof(RequiredStructObject), null });

            _testData.Add(new object[] { typeof(NotRequiredNotSetGuidObject), $"\"{Guid.Empty}\"" });
            _testData.Add(new object[] { typeof(NotRequiredSetGuidObject), $"\"033979ae-0955-4ef6-8a37-50bf0359601f\"" });
            _testData.Add(new object[] { typeof(RequiredGuidObject), null });

            _testData.Add(new object[] { typeof(NotRequiredSetInvalidValueEnumObject), "<exception>" });
            _testData.Add(new object[] { typeof(NotRequiredNotSetInvalidDefaultValueEnumObject), "<exception>" });
            _testData.Add(new object[] { typeof(NotRequiredNotSetDefaultNotDefinedLabelEnumObject), "<exception>" });
            _testData.Add(new object[] { typeof(NotRequiredNotSetValidDefaultEnumObject), nameof(InputEnum.Value1) });
            _testData.Add(new object[] { typeof(NotRequiredSetEnumObject), nameof(InputEnum.Value2) });
            _testData.Add(new object[] { typeof(RequiredEnumObject), null });

            _testData.Add(new object[] { typeof(StructNotRequiredNotSetIntObject), "0" });
            _testData.Add(new object[] { typeof(StructNotRequiredSetIntObject), "5" });
            _testData.Add(new object[] { typeof(StructRequiredIntObject), null });

            _testData.Add(new object[] { typeof(StructNotRequiredSetClassObject), "{ Property1: \"struct set prop value\" Property2: 99 }" });
            _testData.Add(new object[] { typeof(StructNotRequiredNotSetClassObject), "null" });
            _testData.Add(new object[] { typeof(StructRequiredClassObject), null });

            _testData.Add(new object[] { typeof(NotRequiredSetListIntObject), "[1, 2, 3, 4]" });
            _testData.Add(new object[] { typeof(NotRequiredNotSetListIntObject), "null" });
            _testData.Add(new object[] { typeof(RequiredListIntObject), null });
        }

        [TestCaseSource(nameof(_testData))]
        public async Task Execute(Type inputType, string expectedDefaultValue)
        {
            var serverBuilder = new TestServerBuilder(TestOptions.UseCodeDeclaredNames)
                .AddType(inputType, TypeKind.INPUT_OBJECT);

            if (expectedDefaultValue == "<exception>")
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
            var query = response.Data.Fields["query"] as IQueryResponseFieldSet;
            var inputFields = query.Fields["inputFields"] as IQueryResponseItemList;
            var inputField = inputFields.Items[0] as IQueryResponseFieldSet;
            var defaultValue = inputField.Fields["defaultValue"] as IQueryResponseSingleValue;
            Assert.AreEqual(expectedDefaultValue, defaultValue?.Value);
        }
    }
}