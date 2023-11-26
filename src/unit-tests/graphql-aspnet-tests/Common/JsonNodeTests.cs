// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Common
{
    using System.Collections.Generic;
    using System.Text.Json.Nodes;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Common.JsonNodes;
    using GraphQL.AspNet.Tests.Common.CommonHelpers;
    using NUnit.Framework;

    [TestFixture]
    public class JsonNodeTests
    {
        [TestCase("var1", "value1", @"{ ""var1"" : ""value1"" }")]
        [TestCase("var1", 34, @"{ ""var1"" : 34 }")]
        [TestCase("var1.0.var2", "value1", @"{ ""var1"" : [{""var2"": ""value1"" }] } ")]
        [TestCase("var1.0.1.2.var2", "value1", @"{ ""var1"" : [[null,[null, null,{""var2"": ""value1"" }]]] } ")]
        [TestCase("var1.0.var2.1.var3.2.var4", "value1", @"{ ""var1"" : [{""var2"" : [null, {""var3"": [null, null, {""var4"": ""value1"" }]}]}] } ")]
        public void SetChildNodeValue_WithStringPaths(string queryPath, object newValue, string expectedJson)
        {
            var node = JsonNode.Parse("{}");

            JsonNodeExtensions.SetChildNodeValue(node, queryPath, JsonValue.Create(newValue));

            CommonAssertions.AreEqualJsonStrings(expectedJson, node.ToJsonString());
        }

        [TestCase(@"[""var1""]", "value1", @"{ ""var1"" : ""value1"" }")]
        [TestCase(@"[""var1""]", 34, @"{ ""var1"" : 34 }")]
        [TestCase(@"[""var1.0.var2""]", "value1", @"{ ""var1"" : [{""var2"": ""value1"" }] } ")]
        [TestCase(@"[""var1"", 0, ""var2""]", "value1", @"{ ""var1"" : [{""var2"": ""value1"" }] } ")]
        [TestCase(@"[""var1"", 0, 1, 2, ""var2""]", "value1", @"{ ""var1"" : [[null,[null, null,{""var2"": ""value1"" }]]] } ")]
        [TestCase(@"[""var1"", 0, ""1"", 2, ""var2""]", "value1", @"{ ""var1"" : [[null,[null, null,{""var2"": ""value1"" }]]] } ")]
        [TestCase(@"[""var1.0.var2.1.var3.2.var4""]", "value1", @"{ ""var1"" : [{""var2"" : [null, {""var3"": [null, null, {""var4"": ""value1"" }]}]}] } ")]
        public void SetChildNodeValue_WithArrayOfPaths(string queryPathArray, object newValue, string expectedJson)
        {
            var node = JsonNode.Parse("{}");

            var pathArray = JsonArray.Parse(queryPathArray) as JsonArray;

            JsonNodeExtensions.SetChildNodeValue(node, pathArray, JsonValue.Create(newValue));

            var actualJson = node.ToJsonString();
            CommonAssertions.AreEqualJsonStrings(expectedJson, actualJson);
        }

        [Test]
        public void SetChildNode_WithStringPath_WithoutDotPathParsing()
        {
            var node = JsonNode.Parse("{}");

            JsonNodeExtensions.SetChildNodeValue(node, "var1.0.var2.1.var3.2.var4", JsonValue.Create("value1"), false);

            var expectedJson = @"{
                ""var1.0.var2.1.var3.2.var4"": ""value1""
            }";

            var actualJson = node.ToJsonString();
            CommonAssertions.AreEqualJsonStrings(expectedJson, actualJson);
        }

        [Test]
        public void SetChildNode_WithArrayOfPaths_WithoutSingleItemParsing()
        {
            var node = JsonNode.Parse("{}");

            var pathArray = JsonNode.Parse(@"[""var1.0.var2.1.var3.2.var4""]") as JsonArray;

            JsonNodeExtensions.SetChildNodeValue(node, pathArray, JsonValue.Create("value1"), false);

            var expectedJson = @"{
                ""var1.0.var2.1.var3.2.var4"": ""value1""
            }";

            var actualJson = node.ToJsonString();
            CommonAssertions.AreEqualJsonStrings(expectedJson, actualJson);
        }

        [Test]
        public void SetChildNode_WithArrayOfPaths_WithInvalidValueMember_ThrowsException()
        {
            var node = JsonNode.Parse("{}");

            var pathArray = JsonNode.Parse(@"[""var1"", true]") as JsonArray;

            Assert.Throws<JsonNodeException>(() =>
            {
                JsonNodeExtensions.SetChildNodeValue(node, pathArray, JsonValue.Create("value1"), false);
            });
        }

        [Test]
        public void SetChildNode_WithArrayOfPaths_WithObjectMember_ThrowsException()
        {
            var node = JsonNode.Parse("{}");

            var pathArray = JsonNode.Parse(@"[""var1"", {""var2"" : 3}]") as JsonArray;

            Assert.Throws<JsonNodeException>(() =>
            {
                JsonNodeExtensions.SetChildNodeValue(node, pathArray, JsonValue.Create("value1"), false);
            });
        }

        [Test]
        public void SetChildNode_WithArrayOfPaths_WithArrayMember_ThrowsException()
        {
            var node = JsonNode.Parse("{}");

            var pathArray = JsonNode.Parse(@"[""var1"", [""var2""]]") as JsonArray;

            Assert.Throws<JsonNodeException>(() =>
            {
                JsonNodeExtensions.SetChildNodeValue(node, pathArray, JsonValue.Create("value1"), false);
            });
        }

        [Test]
        public void SetChildNode_WithRootNodeAsTerminalValue_ThrowsException()
        {
            var node = JsonValue.Parse("\"someString\"");

            var pathArray = new List<string>() { "var1", "var2" };

            Assert.Throws<JsonNodeException>(() =>
            {
                JsonNodeExtensions.SetChildNodeValue(node, pathArray, JsonValue.Create("value1"));
            });
        }

        [Test]
        public void SetChildNode_WithTerminalValueInPath_ThrowsException()
        {
            var node = JsonObject.Parse(@"{""var1"": {""var2"": ""someString""}}");

            var pathArray = new List<string>() { "var1", "var2", "var3", "var4" };

            Assert.Throws<JsonNodeException>(() =>
            {
                JsonNodeExtensions.SetChildNodeValue(node, pathArray, JsonValue.Create("value1"));
            });
        }

        [Test]
        public void AddSubNode_ToATerminalNode_ThrowsException()
        {
            var node = JsonObject.Parse("\"bob\"");

            Assert.Throws<JsonNodeException>(() =>
            {
                JsonNodeExtensions.AddSubNode(node, "var1", JsonValue.Create("value1"));
            });
        }

        [Test]
        public void AddSubNode_PropertyNameToDeclaredArray_ThrowsException()
        {
            var node = JsonObject.Parse("[]");

            Assert.Throws<JsonNodeException>(() =>
            {
                JsonNodeExtensions.AddSubNode(node, "var1", JsonValue.Create("value1"));
            });
        }
    }
}