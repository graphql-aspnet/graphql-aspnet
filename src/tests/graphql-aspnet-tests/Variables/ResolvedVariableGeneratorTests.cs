// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Variables
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Interfaces.Variables;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Variables.ResolvedVariableTestData;
    using GraphQL.AspNet.Variables;
    using NUnit.Framework;

    [TestFixture]
    public class ResolvedVariableGeneratorTests
    {
        public async Task<IResolvedVariableCollection> CreateResolvedVariableCollection(string queryText,  string jsonDoc)
        {
            var server = new TestServerBuilder()
                .AddGraphType<VariableTestController>()
                .Build();

            var plan = await server.CreateQueryPlan(queryText);

            if (plan.Messages.Count > 0)
            {
                Assert.Fail(plan.Messages[0].Message);
            }

            var operation = plan.Operations.First().Value;

            var resolver = new ResolvedVariableGenerator(server.Schema, operation);

            var variableSet = InputVariableCollection.FromJsonDocument(jsonDoc);

            return resolver.Resolve(variableSet);
        }

        [Test]
        public async Task FlatInputObject()
        {
            var result = await this.CreateResolvedVariableCollection(
                @"query ($var1: Input_VariableTestObject){
                            singleObjectTest(arg1: $var1)
                } ",
                @"{
                    ""var1"" : {
                        ""stringProperty"" : ""stringValue1"",
                        ""floatProperty"": 12.345,
                        ""boolProperty"":  true,
                        ""enumProperty"": ""value2""
                    }
                }");

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);

            var item = result["var1"].Value as VariableTestObject;
            Assert.IsNotNull(item);
            Assert.AreEqual("stringValue1", item.StringProperty);
            Assert.AreEqual(12.345f, item.FloatProperty);
            Assert.AreEqual(true, item.BoolProperty);
            Assert.AreEqual(VariableTestEnum.Value2, item.EnumProperty);
        }

        [Test]
        public async Task NestedInputObject()
        {
            var result = await this.CreateResolvedVariableCollection(
                @"query ($var1: Input_NestedVariableTestObject){
                            nestedObjectTest(arg1: $var1)
                } ",
                @"{
                    ""var1"" : {
                        ""object1Property"" : {
                            ""stringProperty"" : ""stringValue1"",
                            ""floatProperty"": 12.345,
                            ""boolProperty"":  true,
                            ""enumProperty"": ""value2""
                        },
                        ""object2Property"" : {
                            ""stringProperty"" : ""stringValue2"",
                            ""floatProperty"": 56.789,
                            ""boolProperty"":  true,
                            ""enumProperty"": ""value3""
                        },
                    }
                }");

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);

            var item = result["var1"].Value as NestedVariableTestObject;
            Assert.IsNotNull(item);

            var prop1 = item.Object1Property;
            var prop2 = item.Object2Property;

            Assert.IsNotNull(prop1);
            Assert.AreEqual("stringValue1", prop1.StringProperty);
            Assert.AreEqual(12.345f, prop1.FloatProperty);
            Assert.AreEqual(true, prop1.BoolProperty);
            Assert.AreEqual(VariableTestEnum.Value2, prop1.EnumProperty);

            Assert.IsNotNull(prop2);
            Assert.AreEqual("stringValue2", prop2.StringProperty);
            Assert.AreEqual(56.789f, prop2.FloatProperty);
            Assert.AreEqual(true, prop2.BoolProperty);
            Assert.AreEqual(VariableTestEnum.Value3, prop2.EnumProperty);
        }

        [Test]
        public async Task DoubleNestedObjectSet()
        {
            var result = await this.CreateResolvedVariableCollection(
                @"query ($var1: Input_SuperNestedVariableTestObject){
                            superNestedObjectTest(arg1: $var1 arg2: null)
                } ",
                @"{
                      ""var1"": {
                        ""nestedObject1Property"": {
                          ""object1Property"": {
                            ""stringProperty"": ""var1_nested1_obj1"",
                            ""floatProperty"": 12.345,
                            ""boolProperty"": true,
                            ""enumProperty"": ""Value2""
                          },
                          ""object2Property"": {
                            ""stringProperty"": ""var1_nested1_obj2"",
                            ""floatProperty"": 34.567,
                            ""boolProperty"": true,
                            ""enumProperty"": ""Value3""
                          }
                        },
                        ""nestedObject2Property"": {
                          ""object1Property"": {
                            ""stringProperty"": ""var1_nested2_obj1"",
                            ""floatProperty"": 45.678,
                            ""boolProperty"": true,
                            ""enumProperty"": ""Value2""
                          },
                          ""object2Property"": {
                            ""stringProperty"": ""var1_nested2_obj2"",
                            ""floatProperty"": 56.789,
                            ""boolProperty"": true,
                            ""enumProperty"": ""Value3""
                          }
                        },
                        ""flatObject1Property"": {
                          ""stringProperty"": ""var1_flat1"",
                          ""floatProperty"": 67.89,
                          ""boolProperty"": false,
                          ""enumProperty"": ""Value2""
                        },
                        ""flatObject2Property"": {
                          ""stringProperty"": ""var1_flat2"",
                          ""floatProperty"": 89.012,
                          ""boolProperty"": true,
                          ""enumProperty"": ""Value1""
                        }
                      }
                    }");

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);

            var item = result["var1"].Value as SuperNestedVariableTestObject;
            Assert.IsNotNull(item);

            var nestedObject1 = item.NestedObject1Property;
            var nestedObject2 = item.NestedObject2Property;
            var flatObject1 = item.FlatObject1Property;
            var flatObject2 = item.FlatObject2Property;

            Assert.IsNotNull(nestedObject1);
            Assert.IsNotNull(nestedObject1.Object1Property);
            Assert.AreEqual("var1_nested1_obj1", nestedObject1.Object1Property.StringProperty);
            Assert.AreEqual(12.345f, nestedObject1.Object1Property.FloatProperty);
            Assert.AreEqual(true, nestedObject1.Object1Property.BoolProperty);
            Assert.AreEqual(VariableTestEnum.Value2, nestedObject1.Object1Property.EnumProperty);

            Assert.IsNotNull(nestedObject1.Object2Property);
            Assert.AreEqual("var1_nested1_obj2", nestedObject1.Object2Property.StringProperty);
            Assert.AreEqual(34.567f, nestedObject1.Object2Property.FloatProperty);
            Assert.AreEqual(true, nestedObject1.Object2Property.BoolProperty);
            Assert.AreEqual(VariableTestEnum.Value3, nestedObject1.Object2Property.EnumProperty);

            Assert.IsNotNull(nestedObject2);
            Assert.IsNotNull(nestedObject2.Object1Property);
            Assert.AreEqual("var1_nested2_obj1", nestedObject2.Object1Property.StringProperty);
            Assert.AreEqual(45.678f, nestedObject2.Object1Property.FloatProperty);
            Assert.AreEqual(true, nestedObject2.Object1Property.BoolProperty);
            Assert.AreEqual(VariableTestEnum.Value2, nestedObject2.Object1Property.EnumProperty);

            Assert.IsNotNull(nestedObject2.Object2Property);
            Assert.AreEqual("var1_nested2_obj2", nestedObject2.Object2Property.StringProperty);
            Assert.AreEqual(56.789f, nestedObject2.Object2Property.FloatProperty);
            Assert.AreEqual(true, nestedObject2.Object2Property.BoolProperty);
            Assert.AreEqual(VariableTestEnum.Value3, nestedObject2.Object2Property.EnumProperty);

            Assert.IsNotNull(flatObject1);
            Assert.AreEqual("var1_flat1", flatObject1.StringProperty);
            Assert.AreEqual(67.89f, flatObject1.FloatProperty);
            Assert.AreEqual(false, flatObject1.BoolProperty);
            Assert.AreEqual(VariableTestEnum.Value2, flatObject1.EnumProperty);

            Assert.IsNotNull(flatObject2);
            Assert.AreEqual("var1_flat2", flatObject2.StringProperty);
            Assert.AreEqual(89.012f, flatObject2.FloatProperty);
            Assert.AreEqual(true, flatObject2.BoolProperty);
            Assert.AreEqual(VariableTestEnum.Value1, flatObject2.EnumProperty);
        }

        [Test]
        public async Task ListAsProperty()
        {
            var result = await this.CreateResolvedVariableCollection(
                @"query ($var1: Input_VariableListObject){
                            listAsPropertyOfItem(arg1: $var1)
                } ",
                @"{
                    ""var1"" : {
                        ""items"" : [5, 10, 18, 22, -1]
                    }
                }");

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);

            var item = result["var1"].Value as VariableListObject;
            Assert.IsNotNull(item?.Items);
            CollectionAssert.AreEqual(new[] { 5, 10, 18, 22, -1 }, item.Items);
        }

        [Test]
        public async Task ListAsDirectInput()
        {
            var result = await this.CreateResolvedVariableCollection(
                @"query ($var1: [Int!]){
                            listAsDirectInput(arg1: $var1)
                } ",
                @"{
                    ""var1"" : [5,10, -12, 22, -1]
                }");

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);

            var item = result["var1"].Value as IEnumerable<int>;
            Assert.IsNotNull(item);
            CollectionAssert.AreEqual(new[] { 5, 10, -12, 22, -1 }, item);
        }

        [Test]
        public async Task ListOfListAsDirectInput()
        {
            var result = await this.CreateResolvedVariableCollection(
                @"query ($var1: [[Int!]]){
                            listOfListAsDirectInput(arg1: $var1)
                } ",
                @"{
                    ""var1"" : [[5],[10, 18], [-12, 22, -1]]
                }");

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);

            var item = result["var1"].Value as IEnumerable<IEnumerable<int>>;
            Assert.IsNotNull(item);
            CollectionAssert.AreEqual(new[] { new[] { 5 }, new[] { 10, 18 }, new[] { -12, 22, -1 } }, item);
        }

        [Test]
        public async Task AlternateListOfListDeclaration()
        {
            var result = await this.CreateResolvedVariableCollection(
                @"query ($var1: [[Int!]]){
                            listOfListAlternateDeclarationAsDirectInput(arg1: $var1)
                } ",
                @"{
                    ""var1"" : [[5],[10, 18], [-12, 22, -1]]
                }");

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);

            var item = result["var1"].Value as IEnumerable<IEnumerable<int>>;
            Assert.IsNotNull(item);
            CollectionAssert.AreEqual(new[] { new[] { 5 }, new[] { 10, 18 }, new[] { -12, 22, -1 } }, item);
        }

        [Test]
        public async Task ListOfListOfListOfListDeclaration_WIthAlteratingInterfaces()
        {
            var result = await this.CreateResolvedVariableCollection(
                @"query ($var1: [[[[Int!]]]]){
                            listOfListV4(arg1: $var1)
                } ",
                @"{
                    ""var1"" : [[[[5]]], [[[6, 7], [8, 9]], [[10], [11], [12, 13, 14]]]]
                }");

            var expected = new[]
            {
                new[]
                {
                    new[]
                    {
                        new int[] { 5, },
                    },
                },
                new[]
                {
                    new[]
                    {
                        new[] { 6, 7, },
                        new[] { 8, 9, },
                    },
                    new[]
                    {
                        new[] { 10, },
                        new[] { 11, },
                        new[] { 12, 13, 14, },
                    },
                },
            };

            Assert.IsNotNull(result);

            var item = result["var1"].Value as IEnumerable<IEnumerable<IEnumerable<IEnumerable<int>>>>;
            Assert.IsNotNull(item);
            CollectionAssert.AreEqual(expected, item);
        }

        [Test]
        public void KeysReturnsExpectedKeys()
        {
            var collection = new ResolvedVariableCollection();
            collection.AddVariable(new ResolvedVariable("key1", new GraphTypeExpression("BOB"), "bob"));
            collection.AddVariable(new ResolvedVariable("key2", new GraphTypeExpression("BOB"), "bob2"));

            var keys = collection.Keys;
            Assert.AreEqual(2, keys.Count());
            Assert.IsTrue(keys.Contains("key1"));
            Assert.IsTrue(keys.Contains("key2"));
        }

        [Test]
        public void ContainsKey_ReturnsExpectedTruthiness()
        {
            var collection = new ResolvedVariableCollection();
            collection.AddVariable(new ResolvedVariable("key1", new GraphTypeExpression("BOB"), "bob"));

            Assert.IsTrue(collection.ContainsKey("key1"));
            Assert.IsFalse(collection.ContainsKey("key2"));
        }

        [Test]
        public void ValuesReturnsExpectedValues()
        {
            var collection = new ResolvedVariableCollection();
            var val1 = new ResolvedVariable("key1", new GraphTypeExpression("BOB"), "bob");
            var val2 = new ResolvedVariable("key2", new GraphTypeExpression("BOB"), "bob2");
            collection.AddVariable(val1);
            collection.AddVariable(val2);

            var values = collection.Values;
            Assert.AreEqual(2, values.Count());
            Assert.IsTrue(values.Contains(val1));
            Assert.IsTrue(values.Contains(val2));
        }
    }
}
