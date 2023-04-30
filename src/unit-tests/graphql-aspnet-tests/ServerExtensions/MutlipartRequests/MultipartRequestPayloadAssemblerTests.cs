// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.ServerExtensions.MutlipartRequests
{
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using GraphQL.AspNet.Execution.Variables;
    using GraphQL.AspNet.Interfaces.Execution.Variables;
    using GraphQL.AspNet.ServerExtensions.MultipartRequests;
    using GraphQL.AspNet.ServerExtensions.MultipartRequests.Engine;
    using GraphQL.AspNet.ServerExtensions.MultipartRequests.Exceptions;
    using GraphQL.AspNet.ServerExtensions.MultipartRequests.Interfaces;
    using GraphQL.AspNet.ServerExtensions.MultipartRequests.Model;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.Internal;
    using Microsoft.Extensions.Primitives;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class MultipartRequestPayloadAssemblerTests
    {
        private MultiPartHttpFormPayloadParser CreateTestObject(
                string operationsField,
                string mapField,
                IMultipartRequestConfiguration config = null,
                (string FieldName, string FieldValue)[] additionalFields = null,
                (string FieldName, string FileName, string ContentType, string FileContents)[] files = null,
                string httpMethod = "POST",
                string contentType = "multipart/form-data")
        {
            var httpContext = new DefaultHttpContext();

            var fileCollection = new FormFileCollection();
            var fieldCollection = new Dictionary<string, StringValues>();

            if (operationsField != null)
                fieldCollection.Add(MultipartRequestConstants.Web.OPERATIONS_FORM_KEY, operationsField);

            if (mapField != null)
                fieldCollection.Add(MultipartRequestConstants.Web.MAP_FORM_KEY, mapField);

            if (additionalFields != null)
            {
                foreach (var kvp in additionalFields)
                    fieldCollection.Add(kvp.FieldName, kvp.FieldValue);
            }

            if (files != null)
            {
                foreach (var item in files)
                {
                    byte[] fileBytes = null;
                    if (item.FileContents != null)
                        fileBytes = Encoding.UTF8.GetBytes(item.FileContents);

                    var formFile = new FormFile(
                            fileBytes != null
                                ? new MemoryStream(fileBytes)
                                : Stream.Null,
                            0,
                            fileBytes?.Length ?? 0,
                            item.FieldName,
                            item.FileName);

                    formFile.Headers = new HeaderDictionary();
                    if (item.ContentType != null)
                        formFile.Headers.Add("Content-Type", item.ContentType);

                    fileCollection.Add(formFile);
                }
            }

            var form = new FormCollection(fieldCollection, fileCollection);
            httpContext.Request.ContentType = contentType;
            httpContext.Request.Method = httpMethod;
            httpContext.Request.Form = form;
            httpContext.Response.Body = new MemoryStream();

            return new MultiPartHttpFormPayloadParser(
                httpContext,
                new DefaultFileUploadScalarValueMaker(),
                config);
        }

        [Test]
        public async Task NoVariablesSingleQuery_NoFiles_ReturnsQueryInPayload()
        {
            var queryText = "query { field1 {field2 field3} }";
            var operations = @"
            {
                ""query"" : """ + queryText + @"""
            }";

            string map = null;

            var parser = this.CreateTestObject(operations, map);
            var payload = await parser.ParseAsync();

            Assert.IsNotNull(payload);
            Assert.AreEqual(1, payload.Count);
            Assert.IsFalse(payload.IsBatch);
            Assert.AreEqual(queryText, payload[0].Query);
            Assert.IsNull(payload[0].Variables);
        }

        [Test]
        public async Task VariablesOnSingleQuery_NoFiles_ReturnsQueryInPayload()
        {
            var queryText = "query { field1 {field2 field3} }";
            var operations = @"
            {
                ""query""     : """ + queryText + @""",
                ""variables"" : { ""var1"": ""value1"", ""var2"": 3 }
            }";

            string map = null;

            var parser = this.CreateTestObject(operations, map);
            var payload = await parser.ParseAsync();

            Assert.IsNotNull(payload);
            Assert.AreEqual(1, payload.Count);
            Assert.IsFalse(payload.IsBatch);
            Assert.AreEqual(queryText, payload[0].Query);
            Assert.AreEqual(2, payload[0].Variables.Count);

            var found1 = payload[0].Variables.TryGetVariable("var1", out var var1);
            var found2 = payload[0].Variables.TryGetVariable("var2", out var var2);

            Assert.IsTrue(found1);
            Assert.IsTrue(found2);
            Assert.AreEqual("\"value1\"", ((InputSingleValueVariable)var1).Value);
            Assert.AreEqual("3", ((InputSingleValueVariable)var2).Value);
        }

        [Test]
        public async Task MutliVariablesOnMultiQueries_NoFiles_ReturnsQueryInPayload()
        {
            var queryText1 = "query { field1 {field2 field3} }";
            var queryText2 = "query { field4 {field5 field6} }";
            var operations = @"[
                {
                    ""query""     : """ + queryText1 + @""",
                    ""variables"" : { ""var1"": ""value1"", ""var2"": 3 },
                    ""operationName"" : ""bob""
                },
                {
                    ""query""     : """ + queryText2 + @""",
                    ""variables"" : { ""var3"": ""value3"", ""var4"": 4 }
                }
            ]";

            string map = null;

            var parser = this.CreateTestObject(operations, map);
            var payload = await parser.ParseAsync();

            Assert.IsNotNull(payload);
            Assert.AreEqual(2, payload.Count);
            Assert.IsTrue(payload.IsBatch);

            var operation1 = payload[0];
            Assert.AreEqual(queryText1, operation1.Query);
            Assert.AreEqual("bob", operation1.OperationName);
            Assert.AreEqual(2, operation1.Variables.Count);

            var found1 = operation1.Variables.TryGetVariable("var1", out var var1);
            var found2 = operation1.Variables.TryGetVariable("var2", out var var2);

            Assert.IsTrue(found1);
            Assert.IsTrue(found2);
            Assert.AreEqual("\"value1\"", ((InputSingleValueVariable)var1).Value);
            Assert.AreEqual("3", ((InputSingleValueVariable)var2).Value);

            var operation2 = payload[1];
            Assert.AreEqual(queryText2, operation2.Query);
            Assert.IsNull(operation2.OperationName);
            Assert.AreEqual(2, operation2.Variables.Count);

            var found3 = operation2.Variables.TryGetVariable("var3", out var var3);
            var found4 = operation2.Variables.TryGetVariable("var4", out var var4);

            Assert.IsTrue(found3);
            Assert.IsTrue(found4);
            Assert.AreEqual("\"value3\"", ((InputSingleValueVariable)var3).Value);
            Assert.AreEqual("4", ((InputSingleValueVariable)var4).Value);
        }

        [Test]
        public async Task OperationNameOnSingleQuery_NoFiles_ReturnsQueryInPayload()
        {
            var queryText = "query { field1 {field2 field3} }";
            var operations = @"
            {
                ""query""     : """ + queryText + @""",
                ""operationName"" : ""bob"",
            }";

            string map = null;

            var parser = this.CreateTestObject(operations, map);
            var payload = await parser.ParseAsync();

            Assert.IsNotNull(payload);
            Assert.AreEqual(1, payload.Count);
            Assert.IsFalse(payload.IsBatch);
            Assert.AreEqual(queryText, payload[0].Query);
            Assert.IsNull(payload[0].Variables);
            Assert.AreEqual("bob", payload[0].OperationName);
        }

        [Test]
        public async Task SingleQuery_SingleFile_TopLevelVariable_AddedToQueryVariableCorrectly()
        {
            var queryText = "query { field1 {field2 field3} }";
            var operations = @"
            {
                ""query""     : """ + queryText + @""",
                ""variables"" : { ""var1"": null }
            }";

            var map = @"{ ""0"": [""variables"", ""var1""]}";

            var file = ("0", "myFile.txt", "text/plain", "testData");

            var parser = this.CreateTestObject(operations, map, files: new[] { file });
            var payload = await parser.ParseAsync();

            Assert.IsNotNull(payload);
            Assert.AreEqual(1, payload.Count);
            Assert.IsFalse(payload.IsBatch);
            Assert.AreEqual(queryText, payload[0].Query);

            var found1 = payload[0].Variables.TryGetVariable("var1", out var var1);
            Assert.IsTrue(found1);

            var fileVariable = var1 as InputFileUploadVariable;
            Assert.IsNotNull(fileVariable);
            Assert.AreEqual("myFile.txt", fileVariable.Value.FileName);
        }

        [Test]
        public async Task SingleQuery_SingleFile_NoMap_NotAddedToCollection()
        {
            var queryText = "query { field1 {field2 field3} }";
            var operations = @"
            {
                ""query""     : """ + queryText + @""",
                ""variables"" : { ""var1"": null }
            }";

            string map = null;

            var file = ("0", "myFile.txt", "text/plain", "testData");

            var parser = this.CreateTestObject(operations, map, files: new[] { file });
            var payload = await parser.ParseAsync();

            Assert.IsNotNull(payload);
            Assert.AreEqual(1, payload.Count);
            Assert.IsFalse(payload.IsBatch);
            Assert.AreEqual(queryText, payload[0].Query);

            var found1 = payload[0].Variables.TryGetVariable("var1", out var var1);
            Assert.IsTrue(found1);

            var fileVariable = var1 as IInputSingleValueVariable;
            Assert.IsNotNull(fileVariable);
            Assert.IsNull(fileVariable.Value);
        }

        [Test]
        public async Task SingleQuery_SingleFile_AsArrayMember_AddedToQueryVariableCorrectly()
        {
            var queryText = "query { field1 {field2 field3} }";
            var operations = @"
            {
                ""query""     : """ + queryText + @""",
                ""variables"" : { ""var1"": [null, null] }
            }";

            var map = @"{ ""0"": [""variables"", ""var1"", 1]}";

            var file = ("0", "myFile.txt", "text/plain", "testData");

            var parser = this.CreateTestObject(operations, map, files: new[] { file });
            var payload = await parser.ParseAsync();

            Assert.IsNotNull(payload);
            Assert.AreEqual(1, payload.Count);
            Assert.IsFalse(payload.IsBatch);
            Assert.AreEqual(queryText, payload[0].Query);

            var found1 = payload[0].Variables.TryGetVariable("var1", out var var1);
            Assert.IsTrue(found1);

            var array = var1 as IInputListVariable;
            Assert.IsNotNull(array);

            var element0 = array.Items[0] as IInputSingleValueVariable;
            var element1 = array.Items[1] as InputFileUploadVariable;
            Assert.IsNotNull(element0);
            Assert.IsNotNull(element1);

            Assert.AreEqual("myFile.txt", element1.Value.FileName);
        }

        [Test]
        public async Task SingleQuery_SingleFile_AsFieldOnArrayMember_AddedToQueryVariableCorrectly()
        {
            var queryText = "query { field1 {field2 field3} }";
            var operations = @"
            {
                ""query""     : """ + queryText + @""",
                ""variables"" : { ""var1"": [null, { ""var2"": null } ] }
            }";

            var map = @"{ ""0"": [""variables"", ""var1"", 1, ""var2""]}";

            var file = ("0", "myFile.txt", "text/plain", "testData");

            var parser = this.CreateTestObject(operations, map, files: new[] { file });
            var payload = await parser.ParseAsync();

            Assert.IsNotNull(payload);
            Assert.AreEqual(1, payload.Count);
            Assert.IsFalse(payload.IsBatch);
            Assert.AreEqual(queryText, payload[0].Query);

            var found1 = payload[0].Variables.TryGetVariable("var1", out var var1);
            Assert.IsTrue(found1);

            var array = var1 as IInputListVariable;
            Assert.IsNotNull(array);

            var element0 = array.Items[0] as IInputSingleValueVariable;
            var element1 = array.Items[1] as IInputFieldSetVariable;
            Assert.IsNotNull(element0);
            Assert.IsNotNull(element1);

            Assert.IsTrue(element1.Fields.ContainsKey("var2"));
            var element2 = element1.Fields["var2"] as InputFileUploadVariable;

            Assert.IsNotNull(element2);
            Assert.AreEqual("myFile.txt", element2.Value.FileName);
        }

        [Test]
        public async Task SingleQuery_SingleFile_DeepMixedPath_AddedToQueryVariableCorrectly()
        {
            var queryText = "query { field1 {field2 field3} }";
            var operations = @"
            {
                ""query""     : """ + queryText + @""",
                ""variables"" : { ""var1"": [null, { ""var2"": {""45"": [null, null, {""var4"": [null] }] } } ] }
            }";

            var map = @"{ ""0"": [""variables"", ""var1"", 1, ""var2"", 45, 2, ""var4"", 0]}";

            var file = ("0", "myFile.txt", "text/plain", "testData");

            var parser = this.CreateTestObject(operations, map, files: new[] { file });
            var payload = await parser.ParseAsync();

            Assert.IsNotNull(payload);

            var found1 = payload[0].Variables.TryGetVariable("var1", out var var1);
            Assert.IsTrue(found1);

            var element = var1 as IInputListVariable;
            Assert.IsNotNull(element);

            var element0 = element.Items[1] as IInputFieldSetVariable;
            var element1 = element0.Fields["var2"] as IInputFieldSetVariable;
            var element2 = element1.Fields["45"] as IInputListVariable;
            var element3 = element2.Items[2] as IInputFieldSetVariable;
            var element4 = element3.Fields["var4"] as IInputListVariable;
            var element5 = element4.Items[0] as InputFileUploadVariable;

            Assert.IsNotNull(element5);
            Assert.AreEqual("myFile.txt", element5.Value.FileName);
        }

        [Test]
        public async Task SingleQuery_MultipleFiles_AsArrayMember_AddedToQueryVariableCorrectly()
        {
            var queryText = "query { field1 {field2 field3} }";
            var operations = @"
            {
                ""query""     : """ + queryText + @""",
                ""variables"" : { ""var1"": [null, null] }
            }";

            var map = @"{ 
                ""0"": [""variables"", ""var1"", 1],
                ""bob"": [""variables"", ""var1"", 0]
            }";

            var file = ("0", "myFile.txt", "text/plain", "testData");
            var fileBob = ("bob", "bobFile.txt", "text/plain", "testDataBob");
            var parser = this.CreateTestObject(operations, map, files: new[] { file, fileBob });
            var payload = await parser.ParseAsync();

            Assert.IsNotNull(payload);
            Assert.AreEqual(1, payload.Count);
            Assert.IsFalse(payload.IsBatch);
            Assert.AreEqual(queryText, payload[0].Query);

            var found1 = payload[0].Variables.TryGetVariable("var1", out var var1);
            Assert.IsTrue(found1);

            var array = var1 as IInputListVariable;
            Assert.IsNotNull(array);

            var element0 = array.Items[0] as InputFileUploadVariable;
            var element1 = array.Items[1] as InputFileUploadVariable;
            Assert.IsNotNull(element0);
            Assert.IsNotNull(element1);

            Assert.AreEqual("bobFile.txt", element0.Value.FileName);
            Assert.AreEqual("myFile.txt", element1.Value.FileName);
        }

        [Test]
        public async Task BatchQuery_SingleFile_TopLevelVariable_AddedToQueryVariableCorrectly()
        {
            var queryText1 = "query { field1 {field2 field3} }";
            var queryText2 = "query { field4 {field5 field6} }";
            var operations = @"[
                {
                    ""query""     : """ + queryText1 + @""",
                    ""variables"" : { ""var1"": ""value1"", ""var2"": 3 },
                    ""operationName"" : ""bob""
                },
                {
                    ""query""     : """ + queryText2 + @""",
                    ""variables"" : { ""var3"": ""value3"", ""var4"": null }
                }
            ]";

            var map = @"{ ""0"": [1, ""variables"", ""var4""]}";

            var file = ("0", "myFile.txt", "text/plain", "testData");

            var parser = this.CreateTestObject(operations, map, files: new[] { file });
            var payload = await parser.ParseAsync();

            Assert.IsNotNull(payload);
            Assert.AreEqual(2, payload.Count);
            Assert.IsTrue(payload.IsBatch);

            var operation1 = payload[0];
            Assert.AreEqual(queryText1, operation1.Query);
            Assert.AreEqual("bob", operation1.OperationName);
            Assert.AreEqual(2, operation1.Variables.Count);

            var found1 = operation1.Variables.TryGetVariable("var1", out var var1);
            var found2 = operation1.Variables.TryGetVariable("var2", out var var2);

            Assert.IsTrue(found1);
            Assert.IsTrue(found2);
            Assert.AreEqual("\"value1\"", ((InputSingleValueVariable)var1).Value);
            Assert.AreEqual("3", ((InputSingleValueVariable)var2).Value);

            var operation2 = payload[1];
            Assert.AreEqual(queryText2, operation2.Query);
            Assert.IsNull(operation2.OperationName);
            Assert.AreEqual(2, operation2.Variables.Count);

            var found3 = operation2.Variables.TryGetVariable("var3", out var var3);
            var found4 = operation2.Variables.TryGetVariable("var4", out var var4);

            Assert.IsTrue(found3);
            Assert.IsTrue(found4);
            Assert.AreEqual("\"value3\"", ((InputSingleValueVariable)var3).Value);

            var var4AsFile = var4 as InputFileUploadVariable;
            Assert.IsNotNull(var4AsFile);
            Assert.AreEqual("myFile.txt", var4AsFile.Value.FileName);
        }

        [Test]
        public async Task BatchQuery_MultipleFiles_DeepNesting_AddedToQueryVariableCorrectly()
        {
            var queryText0 = "query { field1 {field2 field3} }";
            var queryText1 = "query { field4 {field5 field6} }";
            var operations = @"[
                {
                    ""query""     : """ + queryText0 + @""",
                    ""variables"" : { ""var1"": [{""var2"": [null, null, null] }, {""var3"": [null, null, null] }] },
                    ""operationName"" : ""bob""
                },
                {
                    ""query""     : """ + queryText1 + @""",
                    ""variables"" : { ""var4"": [[[null, null],[null, null]],[[null, null],[null, null]]] }
                }
            ]";

            var map = @"{
                ""var3-1"": [0, ""variables"", ""var1"", 1, ""var3"",""1""],
                ""var4-1-0-1"": [1, ""variables"", ""var4"", ""1"",0, 1]
            }";

            var fileVar3 = ("var3-1", "var3-1.txt", "text/plain", "testData");
            var fileVar4 = ("var4-1-0-1", "var4-1-0-1.txt", "text/plain", "testData");

            var parser = this.CreateTestObject(operations, map, files: new[] { fileVar4, fileVar3 });
            var payload = await parser.ParseAsync();

            Assert.IsNotNull(payload);
            Assert.AreEqual(2, payload.Count);
            Assert.IsTrue(payload.IsBatch);

            var operation0 = payload[0];
            Assert.AreEqual(queryText0, operation0.Query);
            Assert.AreEqual("bob", operation0.OperationName);
            var foundVar1 = operation0.Variables.TryGetVariable("var1", out var var1);
            Assert.IsTrue(foundVar1);
            var var1Index0 = (var1 as IInputListVariable).Items[1] as IInputFieldSetVariable;
            var var3Array = var1Index0.Fields["var3"] as IInputListVariable;
            var var3_1_File = var3Array.Items[1] as InputFileUploadVariable;
            Assert.AreEqual("var3-1.txt", var3_1_File.Value.FileName);

            var operation1 = payload[1];
            Assert.AreEqual(queryText1, operation1.Query);
            Assert.AreEqual(null, operation1.OperationName);

            var foundVar4 = operation1.Variables.TryGetVariable("var4", out var var4);
            Assert.IsTrue(foundVar4);

            var var41Array = (var4 as IInputListVariable).Items[1] as IInputListVariable;
            var var410Array = var41Array.Items[0] as IInputListVariable;
            var var4_101_file = var410Array.Items[1] as InputFileUploadVariable;
            Assert.AreEqual("var4-1-0-1.txt", var4_101_file.Value.FileName);
        }

        [TestCase(@"{ ""0"": ""variables.var1""}")] // string
        [TestCase(@"{ ""0"": [""variables.var1""] }")] // array with one element is treated like a string
        [TestCase(@"{ ""0"": [""variables"", ""var1""] }")] // array with multiple elements is treated seperately
        public async Task SingleQuery_SingleFile_TopLevelVariable_AddedToQueryVariableCorrectly(string mapParts)
        {
            var queryText = "query { field1 {field2 field3} }";
            var operations = @"
            {
                ""query""     : """ + queryText + @""",
                ""variables"" : { ""var1"": null }
            }";

            var map = mapParts;

            var file = ("0", "myFile.txt", "text/plain", "testData");

            var parser = this.CreateTestObject(operations, map, files: new[] { file });
            var payload = await parser.ParseAsync();

            Assert.IsNotNull(payload);
            Assert.AreEqual(1, payload.Count);
            Assert.IsFalse(payload.IsBatch);
            Assert.AreEqual(queryText, payload[0].Query);

            var found1 = payload[0].Variables.TryGetVariable("var1", out var var1);
            Assert.IsTrue(found1);

            var fileVariable = var1 as InputFileUploadVariable;
            Assert.IsNotNull(fileVariable);
            Assert.AreEqual("myFile.txt", fileVariable.Value.FileName);
        }

        [Test]
        public async Task SingleQuery_SingleFile_AsArrayMember_StringMapPath_AddedToQueryVariableCorrectly()
        {
            var queryText = "query { field1 {field2 field3} }";
            var operations = @"
            {
                ""query""     : """ + queryText + @""",
                ""variables"" : { ""var1"": [null,null] }
            }";

            var map = @"{ ""0"": ""variables.var1.1""}";

            var file = ("0", "myFile.txt", "text/plain", "testData");

            var parser = this.CreateTestObject(operations, map, files: new[] { file });
            var payload = await parser.ParseAsync();

            Assert.IsNotNull(payload);
            Assert.AreEqual(1, payload.Count);
            Assert.IsFalse(payload.IsBatch);
            Assert.AreEqual(queryText, payload[0].Query);

            var found1 = payload[0].Variables.TryGetVariable("var1", out var var1);
            Assert.IsTrue(found1);

            var arrayVar = var1 as IInputListVariable;
            Assert.IsNotNull(arrayVar);

            var element0 = arrayVar.Items[0] as IInputSingleValueVariable;
            var element1 = arrayVar.Items[1] as InputFileUploadVariable;
            Assert.IsNull(element0.Value);

            Assert.IsNotNull(element1);

            Assert.AreEqual("myFile.txt", element1.Value.FileName);
        }

        [Test]
        public void InvalidJsonForOperationsKey_ThrowsException()
        {
            var operations = @"
                {
                    ""query""     : ""query doesnt matter"",
                    ""variables"" : { ""var1"":  },
                    ""operation"" : ""bob""
                }";

            var map = @"{ ""0"": ""variables.var1""}";

            var file = ("0", "myFile.txt", "text/plain", "testData");

            var parser = this.CreateTestObject(operations, map, files: new[] { file });

            var ex = Assert.ThrowsAsync<InvalidMultiPartOperationException>(parser.ParseAsync);

            // ensure the error contains the correct failed index
            Assert.IsTrue(ex.InnerException is JsonException);
        }

        [Test]
        public void InvalidOperationName_SingleQuery_ThrowsException()
        {
            var operations = @"
                {
                    ""query""     : ""query doesnt matter"",
                    ""variables"" : { ""var1"": null },
                    ""operationName"" : 45
                }";

            var map = @"{ ""0"": ""variables.var1""}";

            var file = ("0", "myFile.txt", "text/plain", "testData");

            var parser = this.CreateTestObject(operations, map, files: new[] { file });

            var ex = Assert.ThrowsAsync<InvalidMultiPartOperationException>(parser.ParseAsync);
        }

        [Test]
        public void InvalidOperationName_BatchQuery_ThrowsException()
        {
            var operations = @"[
                {
                    ""query""     : ""query doesnt matter"",
                    ""variables"" : { ""var1"": [{""var2"": [null, null, null] }, {""var3"": [null, null, null] }] },
                    ""operationName"" : ""bob""
                },
                {
                    ""query""     : ""query doesnt matter"",
                    ""variables"" : { ""var4"": [[[null, null],[null, null]],[[null, null],[null, null]]] },
                    ""operationName""     : 45,
                }
            ]";

            var file = ("0", "myFile.txt", "text/plain", "testData");

            var parser = this.CreateTestObject(operations, null, files: new[] { file });

            var ex = Assert.ThrowsAsync<InvalidMultiPartOperationException>(parser.ParseAsync);

            // ensure the error contains the correct failed index
            Assert.IsTrue(ex.Message.Contains("1"));
        }

        // map is not an object
        [TestCase(@"95")]

        //// no segments
        [TestCase(@"{ ""0"": """"}")]
        [TestCase(@"{ ""0"": []}")]
        [TestCase(@"{ ""0"": null}")]

        // only 1 segment
        [TestCase(@"{ ""0"": ""variables""}")]
        [TestCase(@"{ ""0"": [""variables""]}")]

        // segment is not a string or number
        [TestCase(@"{ ""0"": [""variables"", true]}")]

        //// not a terminal variable
        [TestCase(@"{ ""0"": ""variables.var1""}", @"{ ""var1"": {""var2"": null } }")]
        [TestCase(@"{ ""0"": [""variables"", ""var1""]}", @"{ ""var1"": {""var2"": null } }")]

        //// variable value is not provided as null
        [TestCase(@"{ ""0"": ""variables.var1""}", @"{ ""var1"": 35 }")]
        [TestCase(@"{ ""0"": [""variables"", ""var1""]}", @"{ ""var1"": 36 }")]

        //// file reference doesnt exist
        [TestCase(@"{ ""notAFile"": ""variables.var1""}")]
        public void InvalidMapValue_SingleQuery_ThrowsException(string map, string customVariablestring = null)
        {
            var variables = customVariablestring ?? @"{ ""var1"": null }";
            var operations = @"
                {
                    ""query""     : ""query doesnt matter"",
                    ""variables"" : " + variables + @"
                }";

            var file = ("0", "myFile.txt", "text/plain", "testData");

            var parser = this.CreateTestObject(operations, map, files: new[] { file });

            var ex = Assert.ThrowsAsync<InvalidMultiPartMapException>(parser.ParseAsync);
        }

        // map is not an object
        [TestCase(@"95")]

        // no segments
        [TestCase(@"{ ""0"": """"}")]
        [TestCase(@"{ ""0"": []}")]
        [TestCase(@"{ ""0"": null}")]

        // only 1 segment
        [TestCase(@"{ ""0"": ""variables""}")]
        [TestCase(@"{ ""0"": [""variables""]}")]
        [TestCase(@"{ ""0"": ""0""}")]
        [TestCase(@"{ ""0"": 0}")]
        [TestCase(@"{ ""0"": [""0""]}")]
        [TestCase(@"{ ""0"": [0]}")]

        // segment is not a string or number
        [TestCase(@"{ ""0"": [0, ""variables"", true]}")]

        // not a terminal variable
        [TestCase(@"{ ""0"": ""0.variables.var1""}", @"{ ""var1"": {""var2"": null } }")]
        [TestCase(@"{ ""0"": [0, ""variables"", ""var1""]}", @"{ ""var1"": {""var2"": null } }")]

        // terminal variable value not provided as null
        [TestCase(@"{ ""0"": ""0.variables.var1""}", @"{ ""var1"": 35 }")]
        [TestCase(@"{ ""0"": [0, ""variables"", ""var1""]}", @"{ ""var1"": 23 }")]

        // file reference doesnt exist
        [TestCase(@"{ ""notAFile"": ""0.variables.var1""}")]
        public void InvalidMapValue_BatchQuery_ThrowsException(string map, string customVariablestring = null)
        {
            var variables = customVariablestring ?? @"{ ""var1"": null }";
            var operations = @"[
                {
                    ""query""     : ""query doesnt matter"",
                    ""variables"" : " + variables + @"
                },
                {
                    ""query""     : ""query doesnt matter"",
                    ""variables"" : { ""var1"": null }
                }]";

            var file = ("0", "myFile.txt", "text/plain", "testData");

            var parser = this.CreateTestObject(operations, map, files: new[] { file });
            var ex = Assert.ThrowsAsync<InvalidMultiPartMapException>(parser.ParseAsync);
        }

        [Test]
        public async Task NoVariablesCollectionDefinedOnTargetOfMap_VariablesCollectionIsAdded()
        {
            var queryText = "query { field1 {field2 field3} }";
            var operations = @"
            {
                ""query""     : """ + queryText + @"""
            }";

            string map = @"{""0"": [""variables"", ""var1""] }";

            var file = ("0", "myFile.txt", "text/plain", "testData");

            var parser = this.CreateTestObject(operations, map, files: new[] { file });
            var payload = await parser.ParseAsync();

            Assert.IsFalse(payload.IsBatch);
            Assert.AreEqual(1, payload.Count);

            var data = payload[0];
            Assert.AreEqual(queryText, data.Query);
            Assert.IsNull(data.OperationName);

            Assert.IsNotNull(data.Variables);

            var found = data.Variables.TryGetVariable("var1", out var var1);
            Assert.IsTrue(found);

            Assert.IsTrue(var1 is InputFileUploadVariable);
            var fileUploadVar = var1 as InputFileUploadVariable;

            Assert.AreEqual("myFile.txt", fileUploadVar.Value.FileName);
        }

        [Test]
        public async Task NoRootLevelVariableDefinedOnTargetOfMap_VariableIsAdded()
        {
            var queryText = "query { field1 {field2 field3} }";
            var operations = @"
            {
                ""query""     : """ + queryText + @""",
                ""variables"": {}
            }";

            string map = @"{""0"": [""variables"", ""var1""] }";

            var file = ("0", "myFile.txt", "text/plain", "testData");

            var parser = this.CreateTestObject(operations, map, files: new[] { file });
            var payload = await parser.ParseAsync();

            Assert.IsFalse(payload.IsBatch);
            Assert.AreEqual(1, payload.Count);

            var data = payload[0];
            Assert.AreEqual(queryText, data.Query);
            Assert.IsNull(data.OperationName);

            Assert.IsNotNull(data.Variables);

            var found = data.Variables.TryGetVariable("var1", out var var1);
            Assert.IsTrue(found);

            Assert.IsTrue(var1 is InputFileUploadVariable);
            var fileUploadVar = var1 as InputFileUploadVariable;

            Assert.AreEqual("myFile.txt", fileUploadVar.Value.FileName);
        }

        [Test]
        public async Task MissingVariableInNestedSequence_VariableIsAdded()
        {
            var queryText = "query { field1 {field2 field3} }";
            var operations = @"
            {
                ""query""     : """ + queryText + @""",
                ""variables"": {""var1"": {""var2"" : {""var3"" : null } } }
            }";

            string map = @"{""0"": [""variables"", ""var1"", ""var4"", ""var3""] }";

            var file = ("0", "myFile.txt", "text/plain", "testData");

            var parser = this.CreateTestObject(operations, map, files: new[] { file });
            var payload = await parser.ParseAsync();

            var data = payload[0];

            Assert.IsFalse(payload.IsBatch);
            Assert.AreEqual(1, payload.Count);
        }

        [Test]
        public void InvalidJsonAsMap_ThrowsException()
        {
            var queryText = "query { field1 {field2 field3} }";
            var operations = @"
            {
                ""query""     : """ + queryText + @""",
                ""variables"" : { ""var1"": null }
            }";

            string map = @"{ ""0"": [""variables"", }";

            var file = ("0", "myFile.txt", "text/plain", "testData");

            var parser = this.CreateTestObject(operations, map, files: new[] { file });

            Assert.ThrowsAsync<InvalidMultiPartMapException>(parser.ParseAsync);
        }

        [TestCase("[\"variables\", \"var1\", 0]", 1, 0)]
        [TestCase("[\"variables\", \"var1\", 1]", 2, 1)]
        [TestCase("[\"variables\", \"var1\", 3]", 4, 3)]
        [TestCase("[\"variables\", \"var1\", 200]", 201, 200)]

        [TestCase("\"variables.var1.0\"", 1, 0)]
        public async Task SingleQuery_SingleFile_AsArrayMember_OnEmptyArray_IndexIsAddedQueryVariableCorrectly(
            string mapText,
            int totalExpectedElements,
            int placedAtElement)
        {
            var queryText = "query { field1 {field2 field3} }";
            var operations = @"
            {
                ""query""     : """ + queryText + @""",
                ""variables"" : { ""var1"": [] }
            }";

            var map = @"{ ""0"": " + mapText + @" }";

            var file = ("0", "myFile.txt", "text/plain", "testData");

            var parser = this.CreateTestObject(operations, map, files: new[] { file });
            var payload = await parser.ParseAsync();

            Assert.IsNotNull(payload);
            Assert.AreEqual(1, payload.Count);
            Assert.IsFalse(payload.IsBatch);
            Assert.AreEqual(queryText, payload[0].Query);

            var found1 = payload[0].Variables.TryGetVariable("var1", out var var1);
            Assert.IsTrue(found1);

            var arrayVar = var1 as IInputListVariable;
            Assert.IsNotNull(arrayVar);

            Assert.AreEqual(totalExpectedElements, arrayVar.Items.Count);

            for (var i = 0; i < arrayVar.Items.Count; i++)
            {
                if (i == placedAtElement)
                {
                    var fileElement = arrayVar.Items[i] as InputFileUploadVariable;
                    Assert.IsNotNull(fileElement);
                    Assert.AreEqual("myFile.txt", fileElement.Value.FileName);
                }
                else
                {
                    var addedNullElement = arrayVar.Items[0] as IInputSingleValueVariable;
                    Assert.IsNotNull(addedNullElement);
                    Assert.IsNull(addedNullElement.Value);
                }
            }
        }

        [Test]
        public void UsingTheReservedFileKey_InAMap_ThrowsException()
        {
            var operations = @"
                {
                    ""query""     : ""query doesnt matter"",
                    ""variables"" : { ""var1"": null },
                    ""operation"" : ""bob""
                }";

            var key = MultipartRequestConstants.Protected.FILE_MARKER_PREFIX;
            var map = @"{ """ + key + @"1"": ""variables.var1""}";

            var file = ("0", "myFile.txt", "text/plain", "testData");

            var parser = this.CreateTestObject(operations, map, files: new[] { file });

            var ex = Assert.ThrowsAsync<InvalidMultiPartMapException>(parser.ParseAsync);
        }
    }
}