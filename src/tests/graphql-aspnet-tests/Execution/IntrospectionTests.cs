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
    using System.Linq;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Internal.Introspection.Model;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.CommonHelpers;
    using GraphQL.AspNet.Tests.Execution.IntrospectionTestData;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;

    [TestFixture]
    public class IntrospectionTests
    {
        [Test]
        public void IntrospectedSchema_PropertyCheck()
        {
            var serverBuilder = new TestServerBuilder();
            serverBuilder.AddGraphType<string>();

            var server = serverBuilder.Build();

            var schema = new IntrospectedSchema(server.Schema);
            schema.Rebuild();

            Assert.AreEqual(server.Schema.Name, schema.Name);
            Assert.IsNotNull(schema.FindGraphType(Constants.ScalarNames.STRING));
            Assert.IsNotNull(schema.FindIntrospectedType(Constants.ScalarNames.STRING));
            Assert.AreEqual(11, schema.KnownTypes.Count());

            Assert.IsNotNull(schema.QueryType);
            Assert.IsNull(schema.MutationType);
            Assert.IsNull(schema.SubscriptionType);
            Assert.IsNotNull(schema.Directives);
            Assert.AreEqual(2, schema.Directives.Count()); // skip , include
        }

        [Test]
        public void IntrospectedInputValueType_PropertyCheck()
        {
            var serverBuilder = new TestServerBuilder(TestOptions.UseCodeDeclaredNames)
                .AddGraphType<SodaCanBuildingController>();

            var server = serverBuilder.Build();
            var schema = new IntrospectedSchema(server.Schema);
            schema.Rebuild();

            var graphType = schema.FindGraphType("Query_buildings") as IObjectGraphType;
            var spected = schema.FindIntrospectedType("Query_buildings");

            Assert.IsNotNull(graphType);
            Assert.AreEqual(graphType.Name, spected.Name);
            Assert.AreEqual(graphType.Description, spected.Description);
            Assert.AreEqual(graphType.Kind, spected.Kind);

            Assert.AreEqual(1, spected.Fields.Count);

            var expectedField = graphType.Fields[nameof(SodaCanBuildingController.AddNewBuilding)];
            var field = spected.Fields[0];
            Assert.AreEqual(expectedField.Name, field.Name);
            Assert.AreEqual(1, field.Arguments.Count);

            var expectedArg = expectedField.Arguments["building"];
            var arg1 = field.Arguments[0];
            Assert.IsNotNull(arg1);
            Assert.AreEqual(expectedArg.Name, arg1.Name);
            Assert.AreEqual(expectedArg.Description, arg1.Description);
            Assert.AreEqual(expectedArg.DefaultValue, arg1.DefaultValue);

            // the type SodaCanBuildingData is used as input type "BuildngInput" for arg1
            var introspectedInputType = schema.FindIntrospectedType("BuildingInput");
            Assert.IsNotNull(introspectedInputType);
            Assert.AreEqual(introspectedInputType, arg1.IntrospectedGraphType);
            Assert.AreEqual(TypeKind.INPUT_OBJECT, introspectedInputType.Kind);

            Assert.AreEqual(3, introspectedInputType.InputFields.Count);

            var inputField1 = introspectedInputType.InputFields.Single(x => x.Name == "Name");
            Assert.AreEqual(null, inputField1.Description);
            Assert.AreEqual(null, inputField1.DefaultValue);
            Assert.AreEqual(TypeKind.SCALAR, inputField1.IntrospectedGraphType.Kind);

            var inputField2 = introspectedInputType.InputFields.Single(x => x.Name == "Address");
            Assert.AreEqual(null, inputField2.Description);
            Assert.AreEqual(null, inputField2.DefaultValue);
            Assert.AreEqual(TypeKind.SCALAR, inputField2.IntrospectedGraphType.Kind);

            var inputField3 = introspectedInputType.InputFields.Single(x => x.Name == "Capacity");
            Assert.AreEqual(null, inputField3.Description);
            Assert.AreEqual(null, inputField3.DefaultValue);

            Assert.AreEqual(TypeKind.NON_NULL, inputField3.IntrospectedGraphType.Kind);
            Assert.AreEqual(TypeKind.ENUM, inputField3.IntrospectedGraphType.OfType.Kind);
        }

        [Test]
        public void IntrospectedScalar_PropertyCheck()
        {
            var serverBuilder = new TestServerBuilder();
            serverBuilder.AddGraphType<string>();
            var server = serverBuilder.Build();

            var schema = new IntrospectedSchema(server.Schema);
            var scalar = GraphQLProviders.ScalarProvider.RetrieveScalar(typeof(string));

            var spected = new IntrospectedType(scalar);
            spected.Initialize(schema);

            Assert.AreEqual(scalar.Name, spected.Name);
            Assert.AreEqual(scalar.Description, spected.Description);
            Assert.AreEqual(scalar.Kind, spected.Kind);

            Assert.IsNull(spected.Fields);
            Assert.IsNull(spected.Interfaces);
            Assert.IsNull(spected.PossibleTypes);
            Assert.IsNull(spected.EnumValues);
            Assert.IsNull(spected.InputFields);
            Assert.IsNull(spected.OfType);
        }

        [Test]
        public void Introspected_NotNullType_PropertyCheck()
        {
            var serverBuilder = new TestServerBuilder();
            var server = serverBuilder.AddGraphType<string>()
                .Build();

            var schema = new IntrospectedSchema(server.Schema);
            schema.Rebuild();

            var scalar = GraphQLProviders.ScalarProvider.RetrieveScalar(typeof(string));

            var spected = schema.FindIntrospectedType(scalar);

            var wrapped = Introspection.WrapBaseTypeWithModifiers(spected, MetaGraphTypes.IsNotNull);

            Assert.AreEqual(TypeKind.NON_NULL, wrapped.Kind);

            Assert.IsNull(wrapped.Fields);
            Assert.IsNull(wrapped.Interfaces);
            Assert.IsNull(wrapped.PossibleTypes);
            Assert.IsNull(wrapped.EnumValues);
            Assert.IsNull(wrapped.InputFields);
            Assert.IsNull(wrapped.Name);
            Assert.IsNull(wrapped.Description);
            Assert.AreEqual(spected, wrapped.OfType);
        }

        [Test]
        public void Introspected_NotNullType_WhenSuppliedANotNullType_ThrowsException()
        {
            var serverBuilder = new TestServerBuilder();
            var server = serverBuilder.AddGraphType<string>()
                .Build();

            var schema = new IntrospectedSchema(server.Schema);
            schema.Rebuild();

            var scalar = GraphQLProviders.ScalarProvider.RetrieveScalar(typeof(string));

            var spected = schema.FindIntrospectedType(scalar);

            var wrapped = Introspection.WrapBaseTypeWithModifiers(spected, MetaGraphTypes.IsNotNull);

            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                var _ = new IntrospectedType(wrapped, TypeKind.NON_NULL);
            });
        }

        [Test]
        public void Introspected_ListType_PropertyCheck()
        {
            var serverBuilder = new TestServerBuilder();
            var server = serverBuilder.AddGraphType<string>()
                .Build();

            var schema = new IntrospectedSchema(server.Schema);
            schema.Rebuild();

            var scalar = GraphQLProviders.ScalarProvider.RetrieveScalar(typeof(string));

            var spected = schema.FindIntrospectedType(scalar);

            var wrapped = Introspection.WrapBaseTypeWithModifiers(spected, MetaGraphTypes.IsList);

            Assert.AreEqual(TypeKind.LIST, wrapped.Kind);

            Assert.IsNull(wrapped.Fields);
            Assert.IsNull(wrapped.Interfaces);
            Assert.IsNull(wrapped.PossibleTypes);
            Assert.IsNull(wrapped.EnumValues);
            Assert.IsNull(wrapped.InputFields);
            Assert.IsNull(wrapped.Name);
            Assert.IsNull(wrapped.Description);
            Assert.AreEqual(spected, wrapped.OfType);
        }

        [Test]
        public void Introspected_ListType_NotNullList_PropertyCheck()
        {
            var serverBuilder = new TestServerBuilder();
            var server = serverBuilder.AddGraphType<string>()
                .Build();

            var schema = new IntrospectedSchema(server.Schema);
            schema.Rebuild();

            var scalar = GraphQLProviders.ScalarProvider.RetrieveScalar(typeof(string));

            var spected = schema.FindIntrospectedType(scalar);

            var wrapped = Introspection.WrapBaseTypeWithModifiers(spected, GraphTypeExpression.RequiredList);
            Assert.AreEqual(TypeKind.NON_NULL, wrapped.Kind);

            var unwrappedList = wrapped.OfType;
            Assert.AreEqual(TypeKind.LIST, unwrappedList.Kind);

            var unwrappedItemType = unwrappedList.OfType;
            Assert.AreEqual(spected, unwrappedItemType);
        }

        [Test]
        public void Introspected_ListType_NotNullType_NotNullList_PropertyCheck()
        {
            var serverBuilder = new TestServerBuilder();
            var server = serverBuilder.AddGraphType<string>()
                .Build();

            var schema = new IntrospectedSchema(server.Schema);
            schema.Rebuild();

            var scalar = GraphQLProviders.ScalarProvider.RetrieveScalar(typeof(string));

            var spected = schema.FindIntrospectedType(scalar);

            var wrapped = Introspection.WrapBaseTypeWithModifiers(spected, GraphTypeExpression.RequiredListRequiredItem);
            Assert.AreEqual(TypeKind.NON_NULL, wrapped.Kind);

            var unwrappedList = wrapped.OfType;
            Assert.AreEqual(TypeKind.LIST, unwrappedList.Kind);

            var unwrappedItemType = unwrappedList.OfType;
            Assert.AreEqual(TypeKind.NON_NULL, unwrappedItemType.Kind);

            var itemType = unwrappedItemType.OfType;
            Assert.AreEqual(spected, itemType);
        }

        [Test]
        public void IntrospectedEnum_PropertyCheck()
        {
            var serverBuilder = new TestServerBuilder(TestOptions.UseCodeDeclaredNames);
            var server = serverBuilder
                .AddGraphType<IntrospectableEnum>()
                .Build();

            var template = TemplateHelper.CreateEnumTemplate<IntrospectableEnum>();
            var schema = new IntrospectedSchema(server.Schema);
            schema.Rebuild();

            var enumGraphType = server.CreateGraphType(typeof(IntrospectableEnum), TypeKind.ENUM).GraphType as IEnumGraphType;

            Assert.IsNotNull(enumGraphType);

            var spected = schema.FindIntrospectedType(enumGraphType);

            Assert.AreEqual(enumGraphType.Name, spected.Name);
            Assert.AreEqual(enumGraphType.Description, spected.Description);
            Assert.AreEqual(enumGraphType.Kind, spected.Kind);

            Assert.IsNotNull(spected.EnumValues);

            var expected1 = template.Values[0];
            var expected2 = template.Values[1];

            var val1 = spected.EnumValues[0];
            var val2 = spected.EnumValues[1];

            Assert.AreEqual(expected1.Name, val1.Name);
            Assert.AreEqual(expected1.Description, val1.Description);
            Assert.AreEqual(expected1.IsDeprecated, val1.IsDeprecated);
            Assert.AreEqual(expected1.DeprecationReason, val1.DeprecationReason);

            Assert.AreEqual(expected2.Name, val2.Name);
            Assert.AreEqual(expected2.Description, val2.Description);
            Assert.AreEqual(expected2.IsDeprecated, val2.IsDeprecated);
            Assert.AreEqual(expected2.DeprecationReason, val2.DeprecationReason);

            Assert.IsNull(spected.Fields);
            Assert.IsNull(spected.Interfaces);
            Assert.IsNull(spected.PossibleTypes);
            Assert.IsNull(spected.InputFields);
            Assert.IsNull(spected.OfType);
        }

        [Test]
        public void IntrospectedObject_PropertyCheck()
        {
            var serverBuilder = new TestServerBuilder(TestOptions.UseCodeDeclaredNames);
            var server = serverBuilder
                .AddGraphType<IntrospectableObject>()
                .Build();

            var template = TemplateHelper.CreateObjectTemplate<IntrospectableObject>();

            var schema = new IntrospectedSchema(server.Schema);
            schema.Rebuild();

            var ogt = server.Schema.KnownTypes.FindGraphType(typeof(IntrospectableObject)) as IObjectGraphType;
            var spected = schema.FindIntrospectedType(ogt);

            Assert.AreEqual(ogt.Name, spected.Name);
            Assert.AreEqual(ogt.Description, spected.Description);
            Assert.AreEqual(ogt.Kind, spected.Kind);

            Assert.IsNotNull(spected.Fields);
            Assert.AreEqual(3, spected.Fields.Count);

            var expected0 = template.FieldTemplates[$"[type]/{nameof(IntrospectableObject)}/{nameof(IntrospectableObject.Method1)}"];
            var expected1 = template.FieldTemplates[$"[type]/{nameof(IntrospectableObject)}/{nameof(IntrospectableObject.Method2)}"];
            var expected2 = template.FieldTemplates[$"[type]/{nameof(IntrospectableObject)}/{nameof(IntrospectableObject.Prop1)}"];

            var field0 = spected.Fields.FirstOrDefault(x => x.Name == nameof(IntrospectableObject.Method1));
            var field1 = spected.Fields.FirstOrDefault(x => x.Name == nameof(IntrospectableObject.Method2));
            var field2 = spected.Fields.FirstOrDefault(x => x.Name == nameof(IntrospectableObject.Prop1));

            Assert.IsNotNull(field0);
            Assert.AreEqual(field0.Name, expected0.Name);
            Assert.AreEqual(nameof(TwoPropertyObject), field0.IntrospectedGraphType.Name);
            Assert.AreEqual(field0.Description, expected0.Description);
            Assert.AreEqual(field0.IsDeprecated, expected0.IsDeprecated);
            Assert.AreEqual(field0.DeprecationReason, expected0.DeprecationReason);
            Assert.AreEqual(2, field0.Arguments.Count);

            var arg1 = field0.Arguments[0];
            var arg2 = field0.Arguments[1];

            Assert.IsNotNull(arg1);
            Assert.AreEqual(expected0.Arguments[0].Name, arg1.Name);
            Assert.AreEqual(expected0.Arguments[0].Description, arg1.Description);

            var expectedDefault = $"\"{expected0.Arguments[0].DefaultValue}\"";
            Assert.AreEqual(expectedDefault, arg1.DefaultValue);

            Assert.IsNotNull(arg2);
            Assert.AreEqual(expected0.Arguments[1].Name, arg2.Name);
            Assert.AreEqual(expected0.Arguments[1].Description, arg2.Description);
            Assert.AreEqual(expected0.Arguments[1].DefaultValue?.ToString(), arg2.DefaultValue);

            Assert.IsNotNull(field1);
            Assert.AreEqual(field1.Name, expected1.Name);
            Assert.AreEqual(nameof(TwoPropertyObjectV2), field1.IntrospectedGraphType.Name);
            Assert.AreEqual(field1.Description, expected1.Description);
            Assert.AreEqual(field1.IsDeprecated, expected1.IsDeprecated);
            Assert.AreEqual(field1.DeprecationReason, expected1.DeprecationReason);
            Assert.AreEqual(0, field1.Arguments.Count);

            Assert.IsNotNull(field2);
            Assert.AreEqual(field2.Name, expected2.Name);
            Assert.AreEqual(TypeKind.NON_NULL, field2.IntrospectedGraphType.Kind);
            Assert.AreEqual(TypeKind.SCALAR, field2.IntrospectedGraphType.OfType.Kind);
            Assert.AreEqual(Constants.ScalarNames.LONG, field2.IntrospectedGraphType.OfType.Name);
            Assert.AreEqual(field2.Description, expected2.Description);
            Assert.AreEqual(field2.IsDeprecated, expected2.IsDeprecated);
            Assert.AreEqual(field2.DeprecationReason, expected2.DeprecationReason);
            Assert.AreEqual(0, field2.Arguments.Count);

            // the not null wrapper should all be null
            Assert.IsNull(field2.IntrospectedGraphType.Fields);
            Assert.IsNull(field2.IntrospectedGraphType.EnumValues);
            Assert.IsNull(field2.IntrospectedGraphType.Interfaces);
            Assert.IsNull(field2.IntrospectedGraphType.PossibleTypes);
            Assert.IsNull(field2.IntrospectedGraphType.InputFields);
            Assert.IsNull(field2.IntrospectedGraphType.Name);
            Assert.IsNull(field2.IntrospectedGraphType.Description);

            // objects shoudl always return an interface collection even if empty
            Assert.IsNotNull(spected.Interfaces);
            Assert.AreEqual(0, spected.Interfaces.Count);

            Assert.IsNull(spected.EnumValues);
            Assert.IsNull(spected.PossibleTypes);
            Assert.IsNull(spected.InputFields);
            Assert.IsNull(spected.OfType);
        }

        [Test]
        public void IntrospectedUnion_PropertyCheck()
        {
            var serverBuilder = new TestServerBuilder();
            var server = serverBuilder.AddGraphType<SodaFountainController>()
                .Build();

            var proxy = new SodaTypeUnionProxy();
            var schema = new IntrospectedSchema(server.Schema);
            schema.Rebuild();

            var spected = schema.FindIntrospectedType(proxy.Name);
            spected.Initialize(schema);

            Assert.AreEqual(proxy.Name, spected.Name);
            Assert.AreEqual(proxy.Description, spected.Description);
            Assert.AreEqual(TypeKind.UNION, spected.Kind);
            Assert.AreEqual(2, spected.PossibleTypes.Count);
            Assert.IsTrue(spected.PossibleTypes.Any(x => x.Name == "SodaTypeA"));
            Assert.IsTrue(spected.PossibleTypes.Any(x => x.Name == "SodaTypeB"));

            Assert.IsNull(spected.Fields);
            Assert.IsNull(spected.Interfaces);
            Assert.IsNull(spected.EnumValues);
            Assert.IsNull(spected.InputFields);
            Assert.IsNull(spected.OfType);
        }

        [Test]
        public void IntrospectedVirtualType_HasATypeNameMetaField()
        {
            var serverBuilder = new TestServerBuilder(TestOptions.UseCodeDeclaredNames)
                .AddGraphType<SodaCanBuildingController>();

            var server = serverBuilder.Build();
            var schema = new IntrospectedSchema(server.Schema);
            schema.Rebuild();

            var graphType = schema.FindGraphType("Query_buildings") as IObjectGraphType;

            var typeNameField = graphType.Fields.FirstOrDefault(x => x.Name == Constants.ReservedNames.TYPENAME_FIELD);
            Assert.IsNotNull(typeNameField);
        }

        [Test]
        public async Task Schema_QueryAndMutationTypeNames_ReturnsValidData()
        {
            var serverBuilder = new TestServerBuilder();
            var server = serverBuilder.AddGraphType<SodaCanController>()
                .Build();

            var builder = server.CreateQueryContextBuilder();

            // need to resolve type field now
            builder.AddQueryText(@"{
                               __schema
                              {
                                 queryType { name }
                                 mutationType { name }
                              }
                            }");

            var response = await server.RenderResult(builder);

            var output =
                @"{
                    ""data"": {
                        ""__schema"" : {
                            ""queryType"" : {""name"": ""Query""},
                            ""mutationType"" : null
                        }
                    }
                }";

            CommonAssertions.AreEqualJsonStrings(output, response);
        }

        [Test]
        public async Task SingleType_NameAndFields_ObjectType_ReturnsValidData()
        {
            var serverBuilder = new TestServerBuilder();
            var server = serverBuilder.AddGraphType<SodaCanController>()
                .Build();

            var builder = server.CreateQueryContextBuilder();

            // need to resolve type field now
            builder.AddQueryText(@"{
                                   __type(name: ""SodaCan"")
                                  {
                                    name
                                    description
                                    kind
                                    fields{
                                        name
                                        description
                                        isDeprecated
                                        type {
                                            name
                                            description
                                            kind
                                        }
                                    }
                                  }
                            }");

            var response = await server.RenderResult(builder);

            var output = @"{
                            ""data"": {
                              ""__type"": {
                                        ""name"": ""SodaCan"",
                                        ""description"": null,
                                        ""kind"": ""OBJECT"",
                                        ""fields"": [
                                        {
                                            ""name"": ""brand"",
                                            ""description"": null,
                                            ""isDeprecated"": false,
                                            ""type"": {
                                                ""name"": ""String"",
                                                ""description"": ""A UTF-8 encoded string of characters."",
                                                ""kind"": ""SCALAR""
                                            }
                                        },
                                        {
                                            ""name"": ""id"",
                                            ""description"": null,
                                            ""isDeprecated"": false,
                                            ""type"": {
                                                ""name"": null,
                                                ""description"": null,
                                                ""kind"": ""NON_NULL""
                                            }
                                        }
                                        ]
                                    }
                            }
                         }";

            CommonAssertions.AreEqualJsonStrings(output, response);
        }

        [Test]
        public async Task SingleType_InputScalarsOnFields_ObjectType_ReturnsValidData()
        {
            var serverBuilder = new TestServerBuilder();
            var server = serverBuilder.AddGraphType<SodaCanController>()
                .Build();

            var builder = server.CreateQueryContextBuilder();

            // need to resolve type field now
            builder.AddQueryText(@"{
                               __type(name: ""Query_Sodas"")
                              {
                                name
                                kind
                                fields{
                                    name
                                    args {
                                        name
                                        type {name kind ofType {name kind}}
                                    }
                                }
                              }
                            }");

            var response = await server.RenderResult(builder);

            var output = @"
                            {
                            ""data"" : {
                                  ""__type"": {
                                            ""name"": ""Query_Sodas"",
                                            ""kind"": ""OBJECT"",
                                            ""fields"": [
                                            {
                                                ""name"": ""retrieveSoda"",
                                                ""args"": [
                                                {
                                                    ""name"": ""id"",
                                                    ""type"": {
                                                        ""name"": null,
                                                        ""kind"": ""NON_NULL"",
                                                        ""ofType"": {
                                                            ""name"": ""Int"",
                                                            ""kind"": ""SCALAR""
                                                        }
                                                    }
                                                }
                                                ]
                                            },
                                            {
                                                ""name"": ""canCount"",
                                                ""args"": [
                                                {
                                                    ""name"": ""id"",
                                                    ""type"": {
                                                        ""name"": null,
                                                        ""kind"": ""NON_NULL"",
                                                        ""ofType"": {
                                                            ""name"": ""Int"",
                                                            ""kind"": ""SCALAR""
                                                        }
                                                    }
                                                }
                                                ]
                                            }
                                            ]
                                        }
                                }
                            }
                            ";

            CommonAssertions.AreEqualJsonStrings(output, response);
        }

        [Test]
        public async Task SingleType_InputObjectOnFields_ObjectType_ReturnsValidData()
        {
            var serverBuilder = new TestServerBuilder();
            var server = serverBuilder.AddGraphType<SodaBottleController>().Build();
            var builder = server.CreateQueryContextBuilder();

            // need to resolve type field now
            builder.AddQueryText(@"{
                               __type(name: ""Query_Bottles"")
                              {
                                name
                                kind
                                fields{
                                    name
                                    args {
                                        name
                                        type {
                                            name
                                            kind
                                            inputFields{
                                                name
                                                description
                                                type {name}
                                            }
                                        }
                                    }
                                }
                              }
                            }");

            var response = await server.RenderResult(builder);

            var output = @"
                            {
                            ""data"": {
                              ""__type"": {
                                        ""name"": ""Query_Bottles"",
                                        ""kind"": ""OBJECT"",
                                        ""fields"": [
                                        {
                                            ""name"": ""retrieveSodaBottles"",
                                            ""args"": [
                                            {
                                                ""name"": ""bottleData"",
                                                ""type"": {
                                                    ""name"": ""BottleSearch"",
                                                    ""kind"": ""INPUT_OBJECT"",
                                                    ""inputFields"": [
                                                        {
                                                            ""name"": ""name"",
                                                            ""description"": null,
                                                            ""type"": {""name"": ""String""},
                                                        },
                                                        {
                                                            ""name"": ""size"",
                                                            ""description"": null,
                                                            ""type"": {""name"": ""Int""},
                                                        }
                                                    ]
                                                }
                                            }
                                            ]
                                        }
                                        ]
                                    }
                                }
                             }
                            ";

            CommonAssertions.AreEqualJsonStrings(output, response);
        }

        [Test]
        public async Task SingleType_EnumType_ReturnsValidData()
        {
            var serverBuilder = new TestServerBuilder();

            // contains an enum CapacityType on SodaCanBuildingData
            var server = serverBuilder.AddGraphType<SodaCanBuildingController>()
                .AddSchemaBuilderAction(o =>
                {
                    o.Options.ResponseOptions.ExposeExceptions = true;
                })
                .Build();
            var builder = server.CreateQueryContextBuilder();

            // need to resolve type field now
            builder.AddQueryText(@"{
                               __type(name: ""CapacityType"")
                              {
                                name
                                kind
                                enumValues (includeDeprecated: true) {name description isDeprecated deprecationReason}
                                }
                            }");

            var response = await server.RenderResult(builder);

            var output = @"
                            {
                            ""data"": {
                                  ""__type"": {
                                            ""name"": ""CapacityType"",
                                            ""kind"" : ""ENUM"",
                                            ""enumValues"": [
                                            {
                                                ""name"": ""SMALL"",
                                                ""description"": ""A small room"",
                                                ""isDeprecated"": false,
                                                ""deprecationReason"" : null,
                                            },
                                            {
                                                ""name"": ""MEDIUM"",
                                                ""description"": null,
                                                ""isDeprecated"": false,
                                                ""deprecationReason"" : null,
                                            },
                                            {
                                                ""name"": ""LARGE"",
                                                ""description"": null,
                                                ""isDeprecated"": true,
                                                ""deprecationReason"" : ""Room too big"",
                                            },
                                        ]}
                                    }
                                }";

            CommonAssertions.AreEqualJsonStrings(output, response);
        }

        [Test]
        public async Task SingleType_ScalarType_ReturnsValidData()
        {
            var serverBuilder = new TestServerBuilder();

            // contains an enum CapacityType on SodaCanBuildingData
            var server = serverBuilder.AddGraphType<SodaCanBuildingController>().Build();
            var builder = server.CreateQueryContextBuilder();

            // need to resolve type field now
            builder.AddQueryText(@"{
                               __type(name: ""Int"")
                              {
                                name  kind
                                }
                            }");

            var response = await server.RenderResult(builder);

            var output = @"
                            {
                                ""data"": {
                                   ""__type"": {
                                            ""name"": ""Int"",
                                            ""kind"" : ""SCALAR"",
                                        }
                                }
                            }";

            CommonAssertions.AreEqualJsonStrings(output, response);
        }

        [Test]
        public async Task SingleType_InterfaceType_PossibleTypes_ReturnsValidData()
        {
            var serverBuilder = new TestServerBuilder();

            // contains an enum CapacityType on SodaCanBuildingData
            var server = serverBuilder.AddGraphType<SodaCan2>()
                .AddGraphType<ICan>()
                .Build();

            var builder = server.CreateQueryContextBuilder();

            // need to resolve type field now
            builder.AddQueryText(@"{
                               __type(name: ""ICan"")
                              {
                                name
                                kind
                                possibleTypes { name kind }
                              }
                            }");

            var response = await server.RenderResult(builder);

            var output = @"
                            {
                                ""data"": {
                                      ""__type"": {
                                                ""name"": ""ICan"",
                                                ""kind"" : ""INTERFACE"",
                                                ""possibleTypes"": [{""name"" : ""SodaCan2"", ""kind"": ""OBJECT""}]
                                        }
                                }
                            }";

            CommonAssertions.AreEqualJsonStrings(output, response);
        }

        [Test]
        public async Task SingleType_ObjectType_Interfaces_ReturnsValidData()
        {
            var serverBuilder = new TestServerBuilder();

            // contains an enum CapacityType on SodaCanBuildingData
            var server = serverBuilder.AddGraphType<SodaCan2>()
                .AddGraphType<ICan>()
                .Build();

            var builder = server.CreateQueryContextBuilder();

            // need to resolve type field now
            builder.AddQueryText(@"{
                               __type(name: ""SodaCan2"")
                              {
                                name
                                kind
                                interfaces { name kind }
                                }
                            }");

            var response = await server.RenderResult(builder);

            var output = @"
                            {
                                ""data"": {
                                      ""__type"": {
                                                ""name"": ""SodaCan2"",
                                                ""kind"" : ""OBJECT"",
                                                ""interfaces"": [{""name"" : ""ICan"", ""kind"" : ""INTERFACE""}]
                                        }
                                }
                            }";

            CommonAssertions.AreEqualJsonStrings(output, response);
        }

        [Test]
        public async Task SingleType_WrappedTypeOnField_ReturnsValidData()
        {
            var serverBuilder = new TestServerBuilder();
            var server = serverBuilder.AddGraphType<VendingMachineController>().Build();
            var builder = server.CreateQueryContextBuilder();

            // need to resolve type field now
            builder.AddQueryText(@"{
                               __type(name: ""Query_Vending"")
                              {
                                name
                                fields{
                                    name
                                    type {
                                        kind
                                        ofType {
                                            kind
                                            ofType {name kind }
                                        }
                                    }
                                }
                              }
                            }");

            var response = await server.RenderResult(builder);

            var output = @"
                        {
                        ""data"": {
                              ""__type"": {
                                        ""name"": ""Query_Vending"",
                                        ""fields"": [
                                        {
                                            ""name"": ""cansPerRow"",
                                            ""type"": {
                                                ""kind"": ""LIST"",
                                                ""ofType"": {
                                                    ""kind"": ""NON_NULL"",
                                                    ""ofType"": {
                                                        ""name"": ""Int"",
                                                        ""kind"": ""SCALAR""
                                                    }
                                                }
                                            }
                                        }]
                                    }
                                }
                            }";

            CommonAssertions.AreEqualJsonStrings(output, response);
        }

        [Test]
        public async Task Schema_TypeNameAndKind_ReturnsValidData()
        {
            var serverBuilder = new TestServerBuilder();
            var server = serverBuilder.AddGraphType<SodaCanController>().Build();
            var builder = server.CreateQueryContextBuilder();

            builder.AddQueryText(@"{
                               __schema
                              {
                                 types { name kind }
                              }
                            }");

            var response = await server.RenderResult(builder);

            var output = @"
                        {
                        ""data"": {
                              ""__schema"": {
                                ""types"": [
                                  { ""name"": ""__DirectiveLocation"", ""kind"": ""ENUM"" },
                                  { ""name"": ""__InputValue"", ""kind"": ""OBJECT"" },
                                  { ""name"": ""__Directive"", ""kind"": ""OBJECT"" },
                                  { ""name"": ""__Type"", ""kind"": ""OBJECT"" },
                                  { ""name"": ""__TypeKind"", ""kind"": ""ENUM"" },
                                  { ""name"": ""__Schema"", ""kind"": ""OBJECT"" },
                                  { ""name"": ""__Field"", ""kind"": ""OBJECT"" },
                                  { ""name"": ""__EnumValue"", ""kind"": ""OBJECT"" },
                                  { ""name"": ""SodaCan"", ""kind"": ""OBJECT"" },
                                  { ""name"": ""Int"", ""kind"": ""SCALAR"" },
                                  { ""name"": ""String"", ""kind"": ""SCALAR"" },
                                  { ""name"": ""Boolean"", ""kind"": ""SCALAR"" },
                                  { ""name"": ""Query_Sodas"", ""kind"": ""OBJECT"" },
                                  { ""name"": ""Query"", ""kind"": ""OBJECT"" }
                                ]
                              }
                            }
                        }";

            CommonAssertions.AreEqualJsonStrings(output, response);
        }

        [Test]
        public async Task Schema_TypeNameAndKind_ThroughAFragment_ReturnsValidData()
        {
            var serverBuilder = new TestServerBuilder();
            var server = serverBuilder.AddGraphType<SodaCanController>().Build();
            var builder = server.CreateQueryContextBuilder();

            builder.AddQueryText(@"
                            {
                               __schema
                              {
                                 types {
                                    ...FullType
                                }
                              }
                            }
                             fragment FullType on __Type {
                                  kind
                                  name
                            }");

            var response = await server.RenderResult(builder);

            var output = @"
                        {
                            ""data"": {
                                ""__schema"": {
                                    ""types"": [
                                      { ""name"": ""__DirectiveLocation"", ""kind"": ""ENUM"" },
                                      { ""name"": ""__InputValue"", ""kind"": ""OBJECT"" },
                                      { ""name"": ""__Directive"", ""kind"": ""OBJECT"" },
                                      { ""name"": ""__Type"", ""kind"": ""OBJECT"" },
                                      { ""name"": ""__TypeKind"", ""kind"": ""ENUM"" },
                                      { ""name"": ""__Schema"", ""kind"": ""OBJECT"" },
                                        { ""name"": ""__Field"", ""kind"": ""OBJECT"" },
                                        { ""name"": ""__EnumValue"", ""kind"": ""OBJECT"" },
                                        { ""name"": ""String"", ""kind"": ""SCALAR"" },
                                        { ""name"": ""Boolean"", ""kind"": ""SCALAR"" },
                                        { ""name"": ""Int"", ""kind"": ""SCALAR"" },
                                        { ""name"": ""SodaCan"", ""kind"": ""OBJECT"" },
                                        { ""name"": ""Query_Sodas"", ""kind"": ""OBJECT"" },
                                        { ""name"": ""Query"", ""kind"": ""OBJECT"" },
                                        ]
                                }
                            }
                        }";

            CommonAssertions.AreEqualJsonStrings(output, response);
        }

        [Test]
        public async Task Schema_EnumValues_ReturnsValidData()
        {
            var serverBuilder = new TestServerBuilder();
            var server = serverBuilder.AddGraphType<IntrospectableEnum>()
                .Build();
            var builder = server.CreateQueryContextBuilder();

            builder.AddQueryText(@"
                            {
                               __type(name: ""IntrospectableEnum"")
                              {
                                kind
                                  name
                                  enumValues (includeDeprecated: true) { name }
                              }
                            }");

            var response = await server.RenderResult(builder);
            var output = @"
                        {
                            ""data"": {
                                ""__type"": {
                                    ""kind"": ""ENUM"",
                                    ""name"": ""IntrospectableEnum"",
                                    ""enumValues"": [
                                        {
                                            ""name"" : ""VALUE1""
                                        },
                                        {
                                            ""name"" : ""VALUE2""
                                        }
                                    ]
                                }
                            }
                        }";

            CommonAssertions.AreEqualJsonStrings(output, response);
        }

        [Test]
        public async Task Schema_FieldYieldsAUnion_ReturnsValidData()
        {
            var serverBuilder = new TestServerBuilder();
            var server = serverBuilder.AddGraphType<SodaFountainController>().Build();
            var builder = server.CreateQueryContextBuilder();

            builder.AddQueryText(@"{
                               __schema
                              {
                                 types { name kind }
                              }
                            }");

            var response = await server.RenderResult(builder);

            var output = @"
                        {
                         ""data"": {
                              ""__schema"": {
                                        ""types"": [
                                            { ""name"": ""__DirectiveLocation"", ""kind"": ""ENUM"" },
                                            { ""name"": ""__InputValue"", ""kind"": ""OBJECT"" },
                                            { ""name"": ""__Directive"", ""kind"": ""OBJECT"" },
                                            { ""name"": ""__Type"", ""kind"": ""OBJECT"" },
                                            { ""name"": ""__TypeKind"", ""kind"": ""ENUM"" },
                                            { ""name"": ""__Schema"", ""kind"": ""OBJECT"" },
                                            { ""name"": ""__Field"", ""kind"": ""OBJECT"" },
                                            { ""name"": ""__EnumValue"", ""kind"": ""OBJECT"" },
                                            { ""name"": ""String"", ""kind"": ""SCALAR"" },
                                            { ""name"": ""Boolean"", ""kind"": ""SCALAR"" },
                                            { ""name"": ""SodaTypeA"", ""kind"": ""OBJECT"" },
                                            { ""name"": ""SodaTypeB"", ""kind"": ""OBJECT"" },
                                            { ""name"": ""SodaTypes"", ""kind"": ""UNION"" },
                                            { ""name"": ""Query_Fountain"", ""kind"": ""OBJECT"" },
                                            { ""name"": ""Query"", ""kind"": ""OBJECT"" },
                                        ]
                                    }
                            }
                        }
                        ";

            CommonAssertions.AreEqualJsonStrings(output, response);
        }

        [Test]
        public async Task SingleType_FieldYieldsAUnion_UnionDeliversPossibleTypes()
        {
            var serverBuilder = new TestServerBuilder();
            var server = serverBuilder.AddGraphType<SodaFountainController>().Build();
            var builder = server.CreateQueryContextBuilder();

            builder.AddQueryText(@"
                            {
                               __type(name : ""SodaTypes"")
                              {
                                 name kind possibleTypes { name kind } fields
                              }
                            }");

            var response = await server.RenderResult(builder);

            var output = @"
                        {
                        ""data"": {
                              ""__type"": {
                                    ""name"": ""SodaTypes"",
                                    ""kind"": ""UNION"",
                                    ""possibleTypes"": [
                                        {
                                            ""name"": ""SodaTypeA"",
                                            ""kind"": ""OBJECT""
                                        },
                                        {
                                            ""name"": ""SodaTypeB"",
                                            ""kind"": ""OBJECT""
                                        }
                                    ],
                                    ""fields"" : null
                                }
                           }
                       } ";

            CommonAssertions.AreEqualJsonStrings(output, response);
        }
    }
}