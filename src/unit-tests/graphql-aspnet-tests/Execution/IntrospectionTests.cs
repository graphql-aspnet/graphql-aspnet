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
    using System.Linq;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Engine;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Schemas.TypeSystem.Introspection.Model;
    using GraphQL.AspNet.Tests.Common.CommonHelpers;
    using GraphQL.AspNet.Tests.Execution.TestData;
    using GraphQL.AspNet.Tests.Execution.TestData.IntrospectionTestData;
    using GraphQL.AspNet.Tests.Framework;
    using NUnit.Framework;

    [TestFixture]
    public class IntrospectionTests
    {
        [Test]
        public void IntrospectedSchema_PropertyCheck()
        {
            var serverBuilder = new TestServerBuilder();
            serverBuilder.AddType<string>();

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
            Assert.IsNotNull(schema.DeclaredDirectives);
            Assert.AreEqual(server.Schema.Description, schema.Description);

            // skip , include, deprecated, specifiedBy
            Assert.AreEqual(4, schema.DeclaredDirectives.Count());
        }

        [Test]
        public void IntrospectedInputValueType_PropertyCheck()
        {
            var serverBuilder = new TestServerBuilder(TestOptions.UseCodeDeclaredNames)
                .AddType<SodaCanBuildingController>();

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
            Assert.AreEqual(Constants.QueryLanguage.NULL, arg1.DefaultValue);

            // the type SodaCanBuildingData is used as input type "BuildngInput" for arg1
            var introspectedInputType = schema.FindIntrospectedType("BuildingInput");
            Assert.IsNotNull(introspectedInputType);
            Assert.AreEqual(introspectedInputType, arg1.IntrospectedGraphType);
            Assert.AreEqual(TypeKind.INPUT_OBJECT, introspectedInputType.Kind);

            Assert.AreEqual(3, introspectedInputType.InputFields.Count);

            var inputField1 = introspectedInputType.InputFields.Single(x => x.Name == "Name");
            Assert.AreEqual(null, inputField1.Description);
            Assert.AreEqual(Constants.QueryLanguage.NULL, inputField1.DefaultValue);
            Assert.AreEqual(TypeKind.SCALAR, inputField1.IntrospectedGraphType.Kind);

            var inputField2 = introspectedInputType.InputFields.Single(x => x.Name == "Address");
            Assert.AreEqual(null, inputField2.Description);
            Assert.AreEqual(Constants.QueryLanguage.NULL, inputField2.DefaultValue);
            Assert.AreEqual(TypeKind.SCALAR, inputField2.IntrospectedGraphType.Kind);

            var inputField3 = introspectedInputType.InputFields.Single(x => x.Name == "Capacity");
            Assert.AreEqual(null, inputField3.Description);
            Assert.AreEqual(((CapacityType)0).ToString(), inputField3.DefaultValue);

            Assert.AreEqual(TypeKind.NON_NULL, inputField3.IntrospectedGraphType.Kind);
            Assert.AreEqual(TypeKind.ENUM, inputField3.IntrospectedGraphType.OfType.Kind);
        }

        [Test]
        public void IntrospectedScalar_PropertyCheck()
        {
            var serverBuilder = new TestServerBuilder();
            serverBuilder.AddType<string>();
            var server = serverBuilder.Build();

            var schema = new IntrospectedSchema(server.Schema);
            var scalar = schema.Schema.KnownTypes.FindGraphType(typeof(string));

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
            var server = serverBuilder.AddType<string>()
                .Build();

            var schema = new IntrospectedSchema(server.Schema);
            schema.Rebuild();

            var scalar = schema.Schema.KnownTypes.FindGraphType(typeof(string));

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
            var server = serverBuilder.AddType<string>()
                .Build();

            var schema = new IntrospectedSchema(server.Schema);
            schema.Rebuild();

            var scalar = schema.Schema.KnownTypes.FindGraphType(typeof(string));

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
            var server = serverBuilder.AddType<string>()
                .Build();

            var schema = new IntrospectedSchema(server.Schema);
            schema.Rebuild();

            var scalar = schema.Schema.KnownTypes.FindGraphType(typeof(string));

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
            var server = serverBuilder.AddType<string>()
                .Build();

            var schema = new IntrospectedSchema(server.Schema);
            schema.Rebuild();

            var scalar = schema.Schema.KnownTypes.FindGraphType(typeof(string));

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
            var server = serverBuilder.AddType<string>()
                .Build();

            var schema = new IntrospectedSchema(server.Schema);
            schema.Rebuild();

            var scalar = schema.Schema.KnownTypes.FindGraphType(typeof(string));

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
                .AddType<IntrospectableEnum>()
                .Build();

            var template = GraphQLTemplateHelper.CreateEnumTemplate<IntrospectableEnum>();
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

            Assert.AreEqual(expected2.Name, val2.Name);
            Assert.AreEqual(expected2.Description, val2.Description);

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
                .AddType<IntrospectableObject>()
                .Build();

            var template = GraphQLTemplateHelper.CreateObjectTemplate<IntrospectableObject>();

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
            Assert.AreEqual(0, field1.Arguments.Count);

            Assert.IsNotNull(field2);
            Assert.AreEqual(field2.Name, expected2.Name);
            Assert.AreEqual(TypeKind.NON_NULL, field2.IntrospectedGraphType.Kind);
            Assert.AreEqual(TypeKind.SCALAR, field2.IntrospectedGraphType.OfType.Kind);
            Assert.AreEqual(Constants.ScalarNames.LONG, field2.IntrospectedGraphType.OfType.Name);
            Assert.AreEqual(field2.Description, expected2.Description);
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
            var server = serverBuilder.AddType<SodaFountainController>()
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
                .AddType<SodaCanBuildingController>();

            var server = serverBuilder.Build();
            var schema = new IntrospectedSchema(server.Schema);
            schema.Rebuild();

            var graphType = schema.FindGraphType("Query_buildings") as IObjectGraphType;

            var typeNameField = graphType.Fields.FirstOrDefault(x => x.Name == Constants.ReservedNames.TYPENAME_FIELD);
            Assert.IsNotNull(typeNameField);
        }

        [Test]
        public async Task Schema_Description_ReturnsValidData()
        {
            var serverBuilder = new TestServerBuilder<TestSchemaWithDescription>();
            var server = serverBuilder.AddType<SodaCanController>()
                .Build();

            var builder = server.CreateQueryContextBuilder();

            builder.AddQueryText(@"{
                               __schema
                              {
                                 description
                              }
                            }");

            var response = await server.RenderResult(builder);

            var output =
                @"{
                    ""data"": {
                        ""__schema"" : {
                            ""description"" : ""My Test Description""
                        }
                    }
                }";

            CommonAssertions.AreEqualJsonStrings(output, response);
        }

        [Test]
        public async Task Schema_QueryAndMutationTypeNames_ReturnsValidData()
        {
            var serverBuilder = new TestServerBuilder();
            var server = serverBuilder.AddType<SodaCanController>()
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
            var server = serverBuilder.AddType<SodaCanController>()
                .Build();

            var builder = server.CreateQueryContextBuilder();

            // need to resolve type field now
            builder.AddQueryText(@"{
                                   __type(name: ""SodaCan"")
                                  {
                                    name
                                    description
                                    kind
                                    specifiedByURL
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
                                        ""specifiedByURL"": null,
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
            var server = serverBuilder.AddType<SodaCanController>()
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
            var server = serverBuilder.AddType<SodaBottleController>().Build();
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
            var server = serverBuilder.AddType<SodaCanBuildingController>()
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
            var server = serverBuilder.AddType<SodaCanBuildingController>().Build();
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
            var server = serverBuilder.AddType<SodaCan2>()
                .AddType<ICan>()
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
            var server = serverBuilder.AddType<SodaCan2>()
                .AddType<ICan>()
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
            var server = serverBuilder.AddType<VendingMachineController>().Build();
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
            var server = serverBuilder.AddType<SodaCanController>().Build();
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
            var server = serverBuilder.AddType<SodaCanController>().Build();
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
            var server = serverBuilder.AddType<IntrospectableEnum>()
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
            var server = serverBuilder.AddType<SodaFountainController>().Build();
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
            var server = serverBuilder.AddType<SodaFountainController>().Build();
            var builder = server.CreateQueryContextBuilder();

            builder.AddQueryText(@"
                            {
                               __type(name : ""SodaTypes"")
                              {
                                 name kind possibleTypes { name kind }
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
                                    ]
                                }
                           }
                       } ";

            CommonAssertions.AreEqualJsonStrings(output, response);
        }

        [Test]
        public async Task DeprecatedLateBoundEnumValue_ReturnsTrueDeprecationFlag()
        {
            var serverBuilder = new TestServerBuilder();
            var server = serverBuilder.AddGraphQL(o =>
            {
                o.AddGraphType<IntrospectableEnum>();
                o.ApplyDirective("deprecated")
                .ToItems(schemaItem =>
                      schemaItem != null
                        && schemaItem is IEnumValue ev
                        && ev.Parent.ObjectType == typeof(IntrospectableEnum)
                        && Convert.ToInt32(ev.DeclaredValue) == (int)IntrospectableEnum.Value1);
            })
            .Build();

            var builder = server.CreateQueryContextBuilder();

            builder.AddQueryText(@"
                            {
                               __type(name: ""IntrospectableEnum"")
                              {
                                kind
                                  name
                                  enumValues (includeDeprecated: true) { name isDeprecated }
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
                                            ""name"" : ""VALUE1"",
                                            ""isDeprecated"" : true
                                        },
                                        {
                                            ""name"" : ""VALUE2"",
                                            ""isDeprecated"" : true
                                        }
                                    ]
                                }
                            }
                        }";

            CommonAssertions.AreEqualJsonStrings(output, response);
        }

        [Test]
        public async Task DeprecatedLateBoundField_ReturnsTrueDeprecationFlag()
        {
            var serverBuilder = new TestServerBuilder();
            var server = serverBuilder.AddGraphQL(o =>
            {
                o.AddGraphType<TwoPropertyObject>();
                o.ApplyDirective("deprecated")
                .ToItems(schemaItem =>
                      schemaItem != null
                        && schemaItem is IGraphField gf
                        && gf.Parent is IObjectGraphType ogt
                        && ogt.ObjectType == typeof(TwoPropertyObject)
                        && gf.Name == "property2");
            })
            .Build();

            var builder = server.CreateQueryContextBuilder();

            builder.AddQueryText(@"
                            {
                               __type(name: ""TwoPropertyObject"")
                              {
                                kind
                                  name
                                  fields (includeDeprecated: true) { name isDeprecated }
                              }
                            }");

            var response = await server.RenderResult(builder);
            var output = @"
                        {
                            ""data"": {
                                ""__type"": {
                                    ""kind"": ""OBJECT"",
                                    ""name"": ""TwoPropertyObject"",
                                    ""fields"": [
                                        {
                                            ""name"" : ""property1"",
                                            ""isDeprecated"" : false
                                        },
                                        {
                                            ""name"" : ""property2"",
                                            ""isDeprecated"" : true
                                        }
                                    ]
                                }
                            }
                        }";

            CommonAssertions.AreEqualJsonStrings(output, response);
        }

        [Test]
        public async Task SpecifiedByLateBound_PopulateSpecifiedByURL()
        {
            var serverBuilder = new TestServerBuilder();
            var server = serverBuilder.AddGraphQL(o =>
            {
                o.AddGraphType<TwoPropertyObject>();
                o.ApplyDirective("specifiedBy")
                .WithArguments("http://somesite")
                .ToItems(schemaItem =>
                      schemaItem != null
                        && schemaItem.Name == Constants.ScalarNames.STRING);
            })
            .Build();

            var builder = server.CreateQueryContextBuilder();

            builder.AddQueryText(@"
                            {
                               __type(name: ""String"")
                              {
                                kind
                                name
                                specifiedByURL
                              }
                            }");

            var response = await server.RenderResult(builder);
            var output = @"
                        {
                            ""data"": {
                                ""__type"": {
                                    ""kind"": ""SCALAR"",
                                    ""name"": ""String"",
                                    ""specifiedByURL"": ""http://somesite""
                                }
                            }
                        }";

            CommonAssertions.AreEqualJsonStrings(output, response);
        }

        [Test]
        public async Task SpecifiedByLateBound_WithAtSymbol_PopulateSpecifiedByURL()
        {
            var serverBuilder = new TestServerBuilder();
            var server = serverBuilder.AddGraphQL(o =>
            {
                o.AddGraphType<TwoPropertyObject>();
                o.ApplyDirective("@specifiedBy")
                .WithArguments("http://somesite")
                .ToItems(schemaItem =>
                      schemaItem != null
                        && schemaItem.Name == Constants.ScalarNames.STRING);
            })
            .Build();

            var builder = server.CreateQueryContextBuilder();

            builder.AddQueryText(@"
                            {
                               __type(name: ""String"")
                              {
                                kind
                                name
                                specifiedByURL
                              }
                            }");

            var response = await server.RenderResult(builder);
            var output = @"
                        {
                            ""data"": {
                                ""__type"": {
                                    ""kind"": ""SCALAR"",
                                    ""name"": ""String"",
                                    ""specifiedByURL"": ""http://somesite""
                                }
                            }
                        }";

            CommonAssertions.AreEqualJsonStrings(output, response);
        }

        [Test]
        public async Task SpecifiedByEarlyBound_PopulateSpecifiedByURL()
        {
            var serverBuilder = new TestServerBuilder();
            var server = serverBuilder.AddGraphQL(o =>
            {
                o.AddGraphType<CustomSpecifiedScalar>();
                o.AddGraphType<ObjectWithCustomScalar>();
            })
            .Build();

            var builder = server.CreateQueryContextBuilder();

            builder.AddQueryText(@"
                            {
                               __type(name: ""MyCustomScalar"")
                              {
                                kind
                                name
                                specifiedByURL
                              }
                            }");

            var response = await server.RenderResult(builder);
            var output = @"
                        {
                            ""data"": {
                                ""__type"": {
                                    ""kind"": ""SCALAR"",
                                    ""name"": ""MyCustomScalar"",
                                    ""specifiedByURL"": ""http://someSiteViaAttribute""
                                }
                            }
                        }";

            CommonAssertions.AreEqualJsonStrings(output, response);
        }

        [Test]
        public async Task SpecifiedByUrl_OnNonScalarType_ReturnsNull()
        {
            var serverBuilder = new TestServerBuilder();
            var server = serverBuilder.AddGraphQL(o =>
            {
                o.AddGraphType<TwoPropertyObject>();
                o.ApplyDirective("specifiedBy")
                .WithArguments("http://somesite")
                .ToItems(schemaItem =>
                      schemaItem != null
                        && schemaItem.Name == Constants.ScalarNames.STRING);
            })
            .Build();

            var builder = server.CreateQueryContextBuilder();

            builder.AddQueryText(@"
                            {
                               __type(name: ""TwoPropertyObject"")
                              {
                                kind
                                name
                                specifiedByURL
                              }
                            }");

            var response = await server.RenderResult(builder);
            var output = @"
                        {
                            ""data"": {
                                ""__type"": {
                                    ""kind"": ""OBJECT"",
                                    ""name"": ""TwoPropertyObject"",
                                    ""specifiedByURL"": null
                                }
                            }
                        }";

            CommonAssertions.AreEqualJsonStrings(output, response);
        }

        [Test]
        public async Task RepeatableDirective_SetsRepeatableFlag()
        {
            var serverBuilder = new TestServerBuilder();
            var server = serverBuilder.AddGraphQL(o =>
            {
                o.AddDirective<ARepeatableDirective>();
                o.AddDirective<NonRepeatableDirective>();
            })
            .Build();

            var builder = server.CreateQueryContextBuilder();

            builder.AddQueryText(@"
                            {
                               __schema
                              {
                                directives {
                                    name
                                    isRepeatable
                                }
                              }
                            }");

            var response = await server.RenderResult(builder);
            var output = @"
                        {
                            ""data"": {
                                ""__schema"": {
                                ""directives"" : [
                                    {
                                      ""name"": ""nonRepeatable"",
                                      ""isRepeatable"": false
                                    },
                                    {
                                      ""name"": ""aRepeatable"",
                                      ""isRepeatable"": true
                                    },
                                    {
                                      ""name"": ""deprecated"",
                                      ""isRepeatable"": false
                                    },
                                    {
                                      ""name"": ""specifiedBy"",
                                      ""isRepeatable"": false
                                    },
                                    {
                                      ""name"": ""include"",
                                      ""isRepeatable"": false
                                    },
                                    {
                                      ""name"": ""skip"",
                                      ""isRepeatable"": false
                                    }
                               ]
                            }
                        }}";

            CommonAssertions.AreEqualJsonStrings(output, response);
        }

        [Test]
        public async Task WhenAnInterfaceImplementsAnotherInteface_ItsIndicatedOnIntrospectionData()
        {
            var serverBuilder = new TestServerBuilder();
            var server = serverBuilder.AddGraphQL(o =>
            {
                o.AddGraphType<IInterfaceA>();
                o.AddGraphType<IInterfaceB>();
            })
            .Build();

            var builder = server.CreateQueryContextBuilder();

            builder.AddQueryText(@"
                            {
                               __type(name: ""IInterfaceB"")
                              {
                                kind
                                name
                                fields (includeDeprecated: true) {
                                   name
                                }
                                interfaces {
                                    name
                                    kind
                                }
                              }
                            }");

            var response = await server.RenderResult(builder);
            var output = @"
                        {
                          ""data"": {
                            ""__type"": {
                                        ""kind"": ""INTERFACE"",
                              ""name"": ""IInterfaceB"",
                              ""fields"": [
                                {
                                    ""name"": ""interfaceBField""
                                },
                                {
                                    ""name"": ""interfaceAField""
                                }
                              ],
                              ""interfaces"": [
                                {
                                  ""name"": ""IInterfaceA"",
                                  ""kind"": ""INTERFACE""
                                }
                              ]
                            }
                         }
                      }";

            CommonAssertions.AreEqualJsonStrings(output, response);
        }

        [Test]
        public async Task WhenAnInterfaceIsImplementedOnAnotherInteface_ItsFieldsDoNotChange()
        {
            var serverBuilder = new TestServerBuilder();
            var server = serverBuilder.AddGraphQL(o =>
            {
                o.AddGraphType<IInterfaceA>();
                o.AddGraphType<IInterfaceB>();
            })
            .Build();

            var builder = server.CreateQueryContextBuilder();

            builder.AddQueryText(@"
                            {
                               __type(name: ""IInterfaceA"")
                              {
                                kind
                                name
                                fields (includeDeprecated: true) {
                                   name
                                }
                                interfaces {
                                    name
                                    kind
                                }
                              }
                            }");

            var response = await server.RenderResult(builder);
            var output = @"
                        {
                          ""data"": {
                            ""__type"": {
                                        ""kind"": ""INTERFACE"",
                              ""name"": ""IInterfaceA"",
                              ""fields"": [
                                {
                                    ""name"": ""interfaceAField""
                                }
                              ],
                              ""interfaces"": []
                            }
                         }
                      }";

            CommonAssertions.AreEqualJsonStrings(output, response);
        }

        [Test]
        public async Task WhenAnInterfaceIsImplementsAnotherInteface_ButThatInterfaceIsntIncluded_ItsFieldsAreNotAdded()
        {
            var serverBuilder = new TestServerBuilder();
            var server = serverBuilder.AddGraphQL(o =>
            {
                o.AddGraphType<IInterfaceB>();
            })
            .Build();

            // interfaceA isnt added those fields shouldnt be shown on B
            var builder = server.CreateQueryContextBuilder();

            builder.AddQueryText(@"
                            {
                               __type(name: ""IInterfaceB"")
                              {
                                kind
                                name
                                fields (includeDeprecated: true) {
                                   name
                                }
                                interfaces {
                                    name
                                    kind
                                }
                              }
                            }");

            var response = await server.RenderResult(builder);
            var output = @"
                        {
                          ""data"": {
                            ""__type"": {
                                        ""kind"": ""INTERFACE"",
                              ""name"": ""IInterfaceB"",
                              ""fields"": [
                                {
                                    ""name"": ""interfaceBField""
                                }
                              ],
                              ""interfaces"": []
                            }
                         }
                      }";

            CommonAssertions.AreEqualJsonStrings(output, response);
        }

        [Test]
        public async Task WhenAnExtensionExtendsAnInterface_InterfacesAndObjectsThatImplementItAreAlsoExtended()
        {
            var serverBuilder = new TestServerBuilder();
            var server = serverBuilder.AddGraphQL(o =>
            {
                o.AddGraphType<IInterfaceA>();
                o.AddGraphType<IInterfaceB>();
                o.AddGraphType<ObjectC>();
                o.AddController<IInterfaceAController>();
            })
            .Build();

            var builder = server.CreateQueryContextBuilder();

            builder.AddQueryText(@"
                {
                    interfaceA: __type(name: ""IInterfaceA"")
                    {
                        kind
                        name
                        fields (includeDeprecated: true) {
                            name
                        }
                        interfaces {
                            name
                            kind
                        }
                    }

                    interfaceB: __type(name: ""IInterfaceB"")
                    {
                        kind
                        name
                        fields (includeDeprecated: true) {
                            name
                        }
                        interfaces {
                            name
                            kind
                        }
                    }

                    objectC: __type(name: ""ObjectC"")
                    {
                        kind
                        name
                        fields (includeDeprecated: true) {
                            name
                        }
                        interfaces {
                            name
                            kind
                        }
                    }
                }");

            var response = await server.RenderResult(builder);
            var output = @"
                {
                  ""data"": {
                    ""interfaceA"": {
                      ""kind"": ""INTERFACE"",
                      ""name"": ""IInterfaceA"",
                      ""fields"": [
                        {
                            ""name"": ""interfaceAField""
                        },
                        {
                            ""name"": ""extendedFieldA""
                        }
                      ],
                      ""interfaces"": []
                    },
                    ""interfaceB"": {
                      ""kind"": ""INTERFACE"",
                      ""name"": ""IInterfaceB"",
                      ""fields"": [
                        {
                            ""name"": ""interfaceBField""
                        },
                        {
                            ""name"": ""interfaceAField""
                        },
                        {
                            ""name"": ""extendedFieldA""
                        }
                      ],
                      ""interfaces"": [
                        {
                          ""name"": ""IInterfaceA"",
                          ""kind"": ""INTERFACE""
                        }
                      ]
                    },
                    ""objectC"": {
                      ""kind"": ""OBJECT"",
                      ""name"": ""ObjectC"",
                      ""fields"": [
                        {
                                    ""name"": ""objectCField""
                        },
                        {
                                    ""name"": ""interfaceBField""
                        },
                        {
                                    ""name"": ""interfaceAField""
                        },
                        {
                                    ""name"": ""extendedFieldA""
                        }
                      ],
                      ""interfaces"": [
                        {
                          ""name"": ""IInterfaceB"",
                          ""kind"": ""INTERFACE""
                        },
                        {
                          ""name"": ""IInterfaceA"",
                          ""kind"": ""INTERFACE""
                        }
                      ]
                    }
                }
            }";

            CommonAssertions.AreEqualJsonStrings(output, response);
        }

        [Test]
        public async Task InputObject_WithFields_ValueChecks()
        {
            var serverBuilder = new TestServerBuilder();
            var server = serverBuilder.AddGraphQL(o =>
            {
                o.AddGraphType<InputTestObject>(TypeKind.INPUT_OBJECT);
            })
            .Build();

            var builder = server.CreateQueryContextBuilder();

            builder.AddQueryText(@"
                {
                    query: __type(name: ""InputObject"")
                    {
                        kind
                        name
                        description
                        fields(includeDeprecated: true) { name }
                        interfaces { name }
                        possibleTypes { name }
                        enumValues { name }
                        specifiedByURL
                        ofType { name }
                        inputFields{
                            name
                            type { name kind ofType { name kind } }
                            description
                            defaultValue
                        }
                    }
                }");

            var response = await server.RenderResult(builder);
            var expectedResponse = @"
            {
                ""data"": {
                    ""query"": {
                        ""kind"": ""INPUT_OBJECT"",
                        ""name"": ""InputObject"",
                        ""description"": ""input obj desc"",
                        ""fields"": null,
                        ""interfaces"" : null,
                        ""possibleTypes"": null,
                        ""enumValues"": null,
                        ""specifiedByURL"": null,
                        ""ofType"": null,
                        ""inputFields"": [
                            {
                                ""name"": ""notRequiredButSetId"",
                                ""type"": {
                                    ""name"": null,
                                    ""kind"": ""NON_NULL"",
                                    ""ofType"": {
                                      ""name"": ""Int"",
                                      ""kind"": ""SCALAR""
                                    }
                                },
                                ""description"" : ""not required but set int"",
                                ""defaultValue"": ""-1""
                            },
                            {
                                ""name"": ""requiredId"",
                                ""type"": {
                                    ""name"": null,
                                    ""kind"": ""NON_NULL"",
                                    ""ofType"": {
                                      ""name"": ""Int"",
                                      ""kind"": ""SCALAR""
                                    }
                                },
                                ""description"" : ""required int"",
                                ""defaultValue"": null
                            },
                            {
                                ""name"": ""requiredBool"",
                                ""type"": {
                                    ""name"": null,
                                    ""kind"": ""NON_NULL"",
                                    ""ofType"": {
                                      ""name"": ""Boolean"",
                                      ""kind"": ""SCALAR""
                                    }
                                },
                                ""description"" : ""required bool"",
                                ""defaultValue"": null
                            },
                            {
                                ""name"": ""unrequiredButTrueBool"",
                                ""type"": {
                                    ""name"": null,
                                    ""kind"": ""NON_NULL"",
                                    ""ofType"": {
                                      ""name"": ""Boolean"",
                                      ""kind"": ""SCALAR""
                                    }
                                },
                                ""description"" : ""unrequired but true bool"",
                                ""defaultValue"": ""true""
                            },
                            {
                                ""name"": ""twoPropWithDefaultValue"",
                                ""type"": {
                                      ""name"": ""Input_TwoPropertyObject"",
                                      ""kind"": ""INPUT_OBJECT"",
                                      ""ofType"" : null
                                 },
                                ""description"" : ""two prop with default value"",
                                ""defaultValue"": ""{ property1: \""strvalue\"" property2: 5 }""
                            },
                            {
                                ""name"": ""twoPropWithNoDefaultValue"",
                                ""type"": {
                                      ""name"": ""Input_TwoPropertyObject"",
                                      ""kind"": ""INPUT_OBJECT"",
                                      ""ofType"" : null
                                 },
                                ""description"" : ""two prop no default value"",
                                ""defaultValue"": ""null""
                            },
                            {
                                ""name"": ""requiredTwoProp"",
                                ""type"": {
                                      ""name"": ""Input_TwoPropertyObject"",
                                      ""kind"": ""INPUT_OBJECT"",
                                      ""ofType"" : null
                                },
                                ""description"" : ""required two prop"",
                                ""defaultValue"": null
                            }
                        ]
                    }
                }
            }";

            CommonAssertions.AreEqualJsonStrings(expectedResponse, response);
        }

        [Test]
        public async Task Description_OnTypeExtensionField_IsRetrievedViaIntrospection()
        {
            var server = new TestServerBuilder()
                .AddGraphController<TypeExtensionDescriptionController>()
                .Build();

            var builder = server.CreateQueryContextBuilder();
            builder.AddQueryText(@"
                {
                    __type(name: ""TwoPropertyObject"")
                    {
                        kind
                        name
                        fields(includeDeprecated: true) {
                            name
                            description
                        }
                    }
                }");

            // property3 is a type extension and has a declared description
            var response = await server.RenderResult(builder);
            var expectedResponse = @"
            {
                ""data"": {
                    ""__type"": {
                        ""kind"": ""OBJECT"",
                        ""name"": ""TwoPropertyObject"",
                        ""fields"": [
                            {
                                ""name"": ""property1"",
                                ""description"": null
                            },
                            {
                                ""name"": ""property2"",
                                ""description"": null
                            },
                            {
                                ""name"": ""property3"",
                                ""description"": ""Property3 is a boolean""
                            }
                        ]
                    }
                }
            }";

            CommonAssertions.AreEqualJsonStrings(expectedResponse, response);
        }

        [Test]
        public async Task Descriptions_OnInheritedInterfaces_AreRetrievedViaIntrospection()
        {
            var server = new TestServerBuilder()
                .AddType<InterfaceAForIntrospection>()
                .AddType<InterfaceBForIntrospection>()
                .Build();

            var builder = server.CreateQueryContextBuilder();
            builder.AddQueryText(@"
                {
                    __type(name: ""InterfaceBForIntrospection"")
                    {
                        kind
                        name
                        interfaces{ name }
                        fields(includeDeprecated: true) {
                            name
                            description
                        }
                    }
                }");

            // description for field A is declared on interface A
            // description for field B is declared on interface B
            // both descriptions hould show as part of the fields for interface B
            var response = await server.RenderResult(builder);
            var expectedResponse = @"
            {
                ""data"": {
                    ""__type"": {
                        ""kind"": ""INTERFACE"",
                        ""name"": ""InterfaceBForIntrospection"",
                        ""interfaces"" : [
                                {
                                    ""name"": ""InterfaceAForIntrospection""
                                }
                        ],
                        ""fields"": [
                            {
                                ""name"": ""fieldA"",
                                ""description"": ""Description of field A""
                            },
                            {
                                ""name"": ""fieldB"",
                                ""description"": ""Description of field B""
                            }
                        ]
                    }
                }
            }";

            CommonAssertions.AreEqualJsonStrings(expectedResponse, response);
        }
    }
}