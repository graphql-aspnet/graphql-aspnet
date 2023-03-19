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
    using System.Text.Json;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Execution.Variables;
    using GraphQL.AspNet.Interfaces.Execution.Variables;
    using GraphQL.AspNet.ServerExtensions.MultipartRequests;
    using GraphQL.AspNet.ServerExtensions.MultipartRequests.Exceptions;
    using GraphQL.AspNet.ServerExtensions.MultipartRequests.Interfaces;
    using GraphQL.AspNet.ServerExtensions.MultipartRequests.Model;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class MultipartRequestPayloadAssemblerTests
    {
        [Test]
        public async Task NoVariablesSingleQuery_NoFiles_ReturnsQueryInPayload()
        {
            var queryText = "query { field1 {field2 field3} }";
            var operations = @"
            {
                ""query"" : """ + queryText + @"""
            }";

            string map = null;
            var files = new Dictionary<string, FileUpload>();

            var assembler = new MultipartRequestPayloadAssembler();
            var payload = await assembler.AssemblePayload(operations, map, files);

            Assert.IsNotNull(payload);
            Assert.AreEqual(1, payload.QueriesToExecute.Count);
            Assert.IsFalse(payload.IsBatch);
            Assert.AreEqual(queryText, payload.QueriesToExecute[0].Query);
            Assert.AreEqual(InputVariableCollection.Empty, payload.QueriesToExecute[0].Variables);
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
            var files = new Dictionary<string, FileUpload>();

            var assembler = new MultipartRequestPayloadAssembler();
            var payload = await assembler.AssemblePayload(operations, map, files);

            Assert.IsNotNull(payload);
            Assert.AreEqual(1, payload.QueriesToExecute.Count);
            Assert.IsFalse(payload.IsBatch);
            Assert.AreEqual(queryText, payload.QueriesToExecute[0].Query);
            Assert.AreEqual(2, payload.QueriesToExecute[0].Variables.Count);

            var found1 = payload.QueriesToExecute[0].Variables.TryGetVariable("var1", out var var1);
            var found2 = payload.QueriesToExecute[0].Variables.TryGetVariable("var2", out var var2);

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
                    ""operation"" : ""bob""
                },
                {
                    ""query""     : """ + queryText2 + @""",
                    ""variables"" : { ""var3"": ""value3"", ""var4"": 4 }
                }
            ]";

            string map = null;
            var files = new Dictionary<string, FileUpload>();

            var assembler = new MultipartRequestPayloadAssembler();
            var payload = await assembler.AssemblePayload(operations, map, files);

            Assert.IsNotNull(payload);
            Assert.AreEqual(2, payload.QueriesToExecute.Count);
            Assert.IsTrue(payload.IsBatch);

            var operation1 = payload.QueriesToExecute[0];
            Assert.AreEqual(queryText1, operation1.Query);
            Assert.AreEqual("bob", operation1.OperationName);
            Assert.AreEqual(2, operation1.Variables.Count);

            var found1 = operation1.Variables.TryGetVariable("var1", out var var1);
            var found2 = operation1.Variables.TryGetVariable("var2", out var var2);

            Assert.IsTrue(found1);
            Assert.IsTrue(found2);
            Assert.AreEqual("\"value1\"", ((InputSingleValueVariable)var1).Value);
            Assert.AreEqual("3", ((InputSingleValueVariable)var2).Value);

            var operation2 = payload.QueriesToExecute[1];
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
                ""operation"" : ""bob"",
            }";

            string map = null;
            var files = new Dictionary<string, FileUpload>();

            var assembler = new MultipartRequestPayloadAssembler();
            var payload = await assembler.AssemblePayload(operations, map, files);

            Assert.IsNotNull(payload);
            Assert.AreEqual(1, payload.QueriesToExecute.Count);
            Assert.IsFalse(payload.IsBatch);
            Assert.AreEqual(queryText, payload.QueriesToExecute[0].Query);
            Assert.AreEqual(0, payload.QueriesToExecute[0].Variables.Count);
            Assert.AreEqual("bob", payload.QueriesToExecute[0].OperationName);
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

            var file = new FileUpload("0", new Mock<IFileUploadStreamContainer>().Object, "text/plain", "myFile.txt");

            var files = new Dictionary<string, FileUpload>();
            files.Add(file.MapKey, file);

            var assembler = new MultipartRequestPayloadAssembler();
            var payload = await assembler.AssemblePayload(operations, map, files);

            Assert.IsNotNull(payload);
            Assert.AreEqual(1, payload.QueriesToExecute.Count);
            Assert.IsFalse(payload.IsBatch);
            Assert.AreEqual(queryText, payload.QueriesToExecute[0].Query);

            var found1 = payload.QueriesToExecute[0].Variables.TryGetVariable("var1", out var var1);
            Assert.IsTrue(found1);

            var fileVariable = var1 as InputFileUploadVariable;
            Assert.IsNotNull(fileVariable);
            Assert.AreEqual(file, fileVariable.Value);
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

            var file = new FileUpload("0", new Mock<IFileUploadStreamContainer>().Object, "text/plain", "myFile.txt");

            var files = new Dictionary<string, FileUpload>();
            files.Add(file.MapKey, file);

            var assembler = new MultipartRequestPayloadAssembler();
            var payload = await assembler.AssemblePayload(operations, map, files);

            Assert.IsNotNull(payload);
            Assert.AreEqual(1, payload.QueriesToExecute.Count);
            Assert.IsFalse(payload.IsBatch);
            Assert.AreEqual(queryText, payload.QueriesToExecute[0].Query);

            var found1 = payload.QueriesToExecute[0].Variables.TryGetVariable("var1", out var var1);
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

            var file = new FileUpload("0", new Mock<IFileUploadStreamContainer>().Object, "text/plain", "myFile.txt");

            var files = new Dictionary<string, FileUpload>();
            files.Add(file.MapKey, file);

            var assembler = new MultipartRequestPayloadAssembler();
            var payload = await assembler.AssemblePayload(operations, map, files);

            Assert.IsNotNull(payload);
            Assert.AreEqual(1, payload.QueriesToExecute.Count);
            Assert.IsFalse(payload.IsBatch);
            Assert.AreEqual(queryText, payload.QueriesToExecute[0].Query);

            var found1 = payload.QueriesToExecute[0].Variables.TryGetVariable("var1", out var var1);
            Assert.IsTrue(found1);

            var array = var1 as IInputListVariable;
            Assert.IsNotNull(array);

            var element0 = array.Items[0] as IInputSingleValueVariable;
            var element1 = array.Items[1] as InputFileUploadVariable;
            Assert.IsNotNull(element0);
            Assert.IsNotNull(element1);

            Assert.AreEqual(file, element1.Value);
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

            var file = new FileUpload("0", new Mock<IFileUploadStreamContainer>().Object, "text/plain", "myFile.txt");

            var files = new Dictionary<string, FileUpload>();
            files.Add(file.MapKey, file);

            var assembler = new MultipartRequestPayloadAssembler();
            var payload = await assembler.AssemblePayload(operations, map, files);

            Assert.IsNotNull(payload);
            Assert.AreEqual(1, payload.QueriesToExecute.Count);
            Assert.IsFalse(payload.IsBatch);
            Assert.AreEqual(queryText, payload.QueriesToExecute[0].Query);

            var found1 = payload.QueriesToExecute[0].Variables.TryGetVariable("var1", out var var1);
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
            Assert.AreEqual(file, element2.Value);
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

            var file = new FileUpload("0", new Mock<IFileUploadStreamContainer>().Object, "text/plain", "myFile.txt");

            var files = new Dictionary<string, FileUpload>();
            files.Add(file.MapKey, file);

            var assembler = new MultipartRequestPayloadAssembler();
            var payload = await assembler.AssemblePayload(operations, map, files);

            Assert.IsNotNull(payload);

            var found1 = payload.QueriesToExecute[0].Variables.TryGetVariable("var1", out var var1);
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
            Assert.AreEqual(file, element5.Value);
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

            var file = new FileUpload("0", new Mock<IFileUploadStreamContainer>().Object, "text/plain", "myFile.txt");
            var fileBob = new FileUpload("bob", new Mock<IFileUploadStreamContainer>().Object, "text/plain", "myFile.txt");

            var files = new Dictionary<string, FileUpload>();
            files.Add(file.MapKey, file);
            files.Add(fileBob.MapKey, fileBob);

            var assembler = new MultipartRequestPayloadAssembler();
            var payload = await assembler.AssemblePayload(operations, map, files);

            Assert.IsNotNull(payload);
            Assert.AreEqual(1, payload.QueriesToExecute.Count);
            Assert.IsFalse(payload.IsBatch);
            Assert.AreEqual(queryText, payload.QueriesToExecute[0].Query);

            var found1 = payload.QueriesToExecute[0].Variables.TryGetVariable("var1", out var var1);
            Assert.IsTrue(found1);

            var array = var1 as IInputListVariable;
            Assert.IsNotNull(array);

            var element0 = array.Items[0] as InputFileUploadVariable;
            var element1 = array.Items[1] as InputFileUploadVariable;
            Assert.IsNotNull(element0);
            Assert.IsNotNull(element1);

            Assert.AreEqual(fileBob, element0.Value);
            Assert.AreEqual(file, element1.Value);
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
                    ""operation"" : ""bob""
                },
                {
                    ""query""     : """ + queryText2 + @""",
                    ""variables"" : { ""var3"": ""value3"", ""var4"": null }
                }
            ]";

            var map = @"{ ""0"": [1, ""variables"", ""var4""]}";

            var file = new FileUpload("0", new Mock<IFileUploadStreamContainer>().Object, "text/plain", "myFile.txt");
            var files = new Dictionary<string, FileUpload>();
            files.Add(file.MapKey, file);

            var assembler = new MultipartRequestPayloadAssembler();
            var payload = await assembler.AssemblePayload(operations, map, files);

            Assert.IsNotNull(payload);
            Assert.AreEqual(2, payload.QueriesToExecute.Count);
            Assert.IsTrue(payload.IsBatch);

            var operation1 = payload.QueriesToExecute[0];
            Assert.AreEqual(queryText1, operation1.Query);
            Assert.AreEqual("bob", operation1.OperationName);
            Assert.AreEqual(2, operation1.Variables.Count);

            var found1 = operation1.Variables.TryGetVariable("var1", out var var1);
            var found2 = operation1.Variables.TryGetVariable("var2", out var var2);

            Assert.IsTrue(found1);
            Assert.IsTrue(found2);
            Assert.AreEqual("\"value1\"", ((InputSingleValueVariable)var1).Value);
            Assert.AreEqual("3", ((InputSingleValueVariable)var2).Value);

            var operation2 = payload.QueriesToExecute[1];
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
            Assert.AreEqual(file, var4AsFile.Value);
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
                    ""operation"" : ""bob""
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

            var fileVar3 = new FileUpload("var3-1", new Mock<IFileUploadStreamContainer>().Object, "text/plain", "myFile.txt");
            var fileVar4 = new FileUpload("var4-1-0-1", new Mock<IFileUploadStreamContainer>().Object, "text/plain", "myFile.txt");
            var files = new Dictionary<string, FileUpload>();
            files.Add(fileVar3.MapKey, fileVar3);
            files.Add(fileVar4.MapKey, fileVar4);

            var assembler = new MultipartRequestPayloadAssembler();
            var payload = await assembler.AssemblePayload(operations, map, files);

            Assert.IsNotNull(payload);
            Assert.AreEqual(2, payload.QueriesToExecute.Count);
            Assert.IsTrue(payload.IsBatch);

            var operation0 = payload.QueriesToExecute[0];
            Assert.AreEqual(queryText0, operation0.Query);
            Assert.AreEqual("bob", operation0.OperationName);
            var foundVar1 = operation0.Variables.TryGetVariable("var1", out var var1);
            Assert.IsTrue(foundVar1);
            var var1Index0 = (var1 as IInputListVariable).Items[1] as IInputFieldSetVariable;
            var var3Array = var1Index0.Fields["var3"] as IInputListVariable;
            var var3_1_File = var3Array.Items[1] as InputFileUploadVariable;
            Assert.AreEqual(fileVar3, var3_1_File.Value);

            var operation1 = payload.QueriesToExecute[1];
            Assert.AreEqual(queryText1, operation1.Query);
            Assert.AreEqual(null, operation1.OperationName);

            var foundVar4 = operation1.Variables.TryGetVariable("var4", out var var4);
            Assert.IsTrue(foundVar4);

            var var41Array = (var4 as IInputListVariable).Items[1] as IInputListVariable;
            var var410Array = var41Array.Items[0] as IInputListVariable;
            var var4_101_file = var410Array.Items[1] as InputFileUploadVariable;
            Assert.AreEqual(fileVar4, var4_101_file.Value);
        }

        [Test]
        public async Task SingleQuery_SingleFile_TopLevelVariable_StringMapPath_AddedToQueryVariableCorrectly()
        {
            var queryText = "query { field1 {field2 field3} }";
            var operations = @"
            {
                ""query""     : """ + queryText + @""",
                ""variables"" : { ""var1"": null }
            }";

            var map = @"{ ""0"": ""variables.var1""}";

            var file = new FileUpload("0", new Mock<IFileUploadStreamContainer>().Object, "text/plain", "myFile.txt");

            var files = new Dictionary<string, FileUpload>();
            files.Add(file.MapKey, file);

            var assembler = new MultipartRequestPayloadAssembler();
            var payload = await assembler.AssemblePayload(operations, map, files);

            Assert.IsNotNull(payload);
            Assert.AreEqual(1, payload.QueriesToExecute.Count);
            Assert.IsFalse(payload.IsBatch);
            Assert.AreEqual(queryText, payload.QueriesToExecute[0].Query);

            var found1 = payload.QueriesToExecute[0].Variables.TryGetVariable("var1", out var var1);
            Assert.IsTrue(found1);

            var fileVariable = var1 as InputFileUploadVariable;
            Assert.IsNotNull(fileVariable);
            Assert.AreEqual(file, fileVariable.Value);
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

            var file = new FileUpload("0", new Mock<IFileUploadStreamContainer>().Object, "text/plain", "myFile.txt");

            var files = new Dictionary<string, FileUpload>();
            files.Add(file.MapKey, file);

            var assembler = new MultipartRequestPayloadAssembler();
            var payload = await assembler.AssemblePayload(operations, map, files);

            Assert.IsNotNull(payload);
            Assert.AreEqual(1, payload.QueriesToExecute.Count);
            Assert.IsFalse(payload.IsBatch);
            Assert.AreEqual(queryText, payload.QueriesToExecute[0].Query);

            var found1 = payload.QueriesToExecute[0].Variables.TryGetVariable("var1", out var var1);
            Assert.IsTrue(found1);

            var arrayVar = var1 as IInputListVariable;
            Assert.IsNotNull(arrayVar);

            var element0 = arrayVar.Items[0] as IInputSingleValueVariable;
            var element1 = arrayVar.Items[1] as InputFileUploadVariable;
            Assert.IsNull(element0.Value);

            Assert.IsNotNull(element1);

            Assert.AreEqual(file, element1.Value);
        }

        [Test]
        public void NoQueryKeyOnSingleObject_ThrowsException()
        {
            var operations = @"
            {
                ""variables"" : { ""var1"": null }
            }";

            var map = @"{ ""0"": ""variables.var1""}";

            var file = new FileUpload("0", new Mock<IFileUploadStreamContainer>().Object, "text/plain", "myFile.txt");

            var files = new Dictionary<string, FileUpload>();
            files.Add(file.MapKey, file);

            var assembler = new MultipartRequestPayloadAssembler();

            Assert.ThrowsAsync<InvalidMultiPartOperationException>(async () =>
            {
                await assembler.AssemblePayload(operations, map, files);
            });
        }

        [Test]
        public void NoQueryKeyOnBatchObject_ThrowsException()
        {
            var operations = @"[
                {
                    ""query""     : ""query doesnt matter"",
                    ""variables"" : { ""var1"": [{""var2"": [null, null, null] }, {""var3"": [null, null, null] }] },
                    ""operation"" : ""bob""
                },
                {
                    ""variables"" : { ""var4"": [[[null, null],[null, null]],[[null, null],[null, null]]] }
                }
            ]";

            var map = @"{ ""0"": ""variables.var1""}";

            var file = new FileUpload("0", new Mock<IFileUploadStreamContainer>().Object, "text/plain", "myFile.txt");

            var files = new Dictionary<string, FileUpload>();
            files.Add(file.MapKey, file);

            var assembler = new MultipartRequestPayloadAssembler();

            var ex = Assert.ThrowsAsync<InvalidMultiPartOperationException>(async () =>
            {
                await assembler.AssemblePayload(operations, map, files);
            });

            // ensure the error contains the correct failed index
            Assert.IsTrue(ex.Message.Contains("1"));
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

            var file = new FileUpload("0", new Mock<IFileUploadStreamContainer>().Object, "text/plain", "myFile.txt");

            var files = new Dictionary<string, FileUpload>();
            files.Add(file.MapKey, file);

            var assembler = new MultipartRequestPayloadAssembler();

            var ex = Assert.ThrowsAsync<InvalidMultiPartOperationException>(async () =>
            {
                await assembler.AssemblePayload(operations, map, files);
            });

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
                    ""operation"" : 45
                }";

            var map = @"{ ""0"": ""variables.var1""}";

            var file = new FileUpload("0", new Mock<IFileUploadStreamContainer>().Object, "text/plain", "myFile.txt");

            var files = new Dictionary<string, FileUpload>();
            files.Add(file.MapKey, file);

            var assembler = new MultipartRequestPayloadAssembler();

            var ex = Assert.ThrowsAsync<InvalidMultiPartOperationException>(async () =>
            {
                await assembler.AssemblePayload(operations, map, files);
            });
        }

        [Test]
        public void InvalidOperationName_BatchQuery_ThrowsException()
        {
            var operations = @"[
                {
                    ""query""     : ""query doesnt matter"",
                    ""variables"" : { ""var1"": [{""var2"": [null, null, null] }, {""var3"": [null, null, null] }] },
                    ""operation"" : ""bob""
                },
                {
                    ""query""     : ""query doesnt matter"",
                    ""variables"" : { ""var4"": [[[null, null],[null, null]],[[null, null],[null, null]]] },
                    ""operation""     : 45,
                }
            ]";

            var map = @"{ ""0"": ""variables.var1""}";

            var file = new FileUpload("0", new Mock<IFileUploadStreamContainer>().Object, "text/plain", "myFile.txt");

            var files = new Dictionary<string, FileUpload>();
            files.Add(file.MapKey, file);

            var assembler = new MultipartRequestPayloadAssembler();

            var ex = Assert.ThrowsAsync<InvalidMultiPartOperationException>(async () =>
            {
                await assembler.AssemblePayload(operations, map, files);
            });

            // ensure the error contains the correct failed index
            Assert.IsTrue(ex.Message.Contains("1"));
        }

        // map is not an object
        [TestCase(@"95", -1)]

        // no segments
        [TestCase(@"{ ""0"": """"}", -1)]
        [TestCase(@"{ ""0"": []}", -1)]
        [TestCase(@"{ ""0"": null}", -1)]

        // only 1 segment
        [TestCase(@"{ ""0"": ""variables""}", -1)]
        [TestCase(@"{ ""0"": [""variables""]}", -1)]

        // segment is not a string or number
        [TestCase(@"{ ""0"": [""variables"", true]}", 1)]

        // property doesn't exist
        [TestCase(@"{ ""0"": ""variables.var2""}", 1)]
        [TestCase(@"{ ""0"": ""NotVariables.var2""}", 0)]
        [TestCase(@"{ ""0"": [""variables"", ""var2""]}", 1)]
        [TestCase(@"{ ""0"": [""NotVariables"", ""var1""]}", 0)]

        // index doesn't exist
        [TestCase(@"{ ""0"": ""variables.0""}", 1)]
        [TestCase(@"{ ""0"": [""variables"", ""0""]}", 1)]

        // not a batch
        [TestCase(@"{ ""0"": ""0.variables.var1""}", 0)]
        [TestCase(@"{ ""0"": [0, ""variables"", ""var1""]}", 0)]
        [TestCase(@"{ ""0"": [""0"", ""variables"", ""var1""]}", 0)]

        // not a terminal variable
        [TestCase(@"{ ""0"": ""variables.var1""}", 1, @"{ ""var1"": {""var2"": null } }")]
        [TestCase(@"{ ""0"": [""variables"", ""var1""]}", 1, @"{ ""var1"": {""var2"": null } }")]

        // variable value is not provided as null
        [TestCase(@"{ ""0"": ""variables.var1""}", 1, @"{ ""var1"": 35 }")]
        [TestCase(@"{ ""0"": [""variables"", ""var1""]}", 1, @"{ ""var1"": 36 }")]

        // file reference doesnt exist
        [TestCase(@"{ ""notAFile"": ""variables.var1""}", -1)]
        public void InvalidMapValue_SingleQuery_ThrowsException(string map, int expectedFailedIndex, string customVariablestring = null)
        {
            var variables = customVariablestring ?? @"{ ""var1"": null }";
            var operations = @"
                {
                    ""query""     : ""query doesnt matter"",
                    ""variables"" : " + variables + @"
                }";

            var file = new FileUpload("0", new Mock<IFileUploadStreamContainer>().Object, "text/plain", "myFile.txt");

            var files = new Dictionary<string, FileUpload>();
            files.Add(file.MapKey, file);

            var assembler = new MultipartRequestPayloadAssembler();

            var ex = Assert.ThrowsAsync<InvalidMultiPartMapException>(async () =>
            {
                await assembler.AssemblePayload(operations, map, files);
            });

            Assert.AreEqual(expectedFailedIndex, ex.Index);
        }

        // map is not an object
        [TestCase(@"95", -1)]

        // no segments
        [TestCase(@"{ ""0"": """"}", -1)]
        [TestCase(@"{ ""0"": []}", -1)]
        [TestCase(@"{ ""0"": null}", -1)]

        // only 1 segment
        [TestCase(@"{ ""0"": ""variables""}", -1)]
        [TestCase(@"{ ""0"": [""variables""]}", -1)]
        [TestCase(@"{ ""0"": ""0""}", -1)]
        [TestCase(@"{ ""0"": 0}", -1)]
        [TestCase(@"{ ""0"": [""0""]}", -1)]
        [TestCase(@"{ ""0"": [0]}", -1)]

        // segment is not a string or number
        [TestCase(@"{ ""0"": [0, ""variables"", true]}", 2)]

        // property doesn't exist
        [TestCase(@"{ ""0"": ""0.variables.var2""}", 2)]
        [TestCase(@"{ ""0"": ""0.Notvariables.var1""}", 1)]
        [TestCase(@"{ ""0"": [""0"", ""variables"", ""var2""]}", 2)]
        [TestCase(@"{ ""0"": [0, ""variables"", ""var2""]}", 2)]

        // trailing array index doesn't exist
        [TestCase(@"{ ""0"": ""0.variables.0""}", 2)]
        [TestCase(@"{ ""0"": ""0.variabl.0""}", 1)]
        [TestCase(@"{ ""0"": [""0"", ""variables"", ""0""]}", 2)]
        [TestCase(@"{ ""0"": [0, ""variables"", ""0""]}", 2)]

        // batch index out of range
        [TestCase(@"{ ""0"": ""32.variables.0""}", 0)]
        [TestCase(@"{ ""0"": [""32"", ""variables"", ""0""] }", 0)]
        [TestCase(@"{ ""0"": [32, ""variables"", ""0""] }", 0)]

        // not a terminal variable
        [TestCase(@"{ ""0"": ""0.variables.var1""}", 2, @"{ ""var1"": {""var2"": null } }")]
        [TestCase(@"{ ""0"": [0, ""variables"", ""var1""]}", 2, @"{ ""var1"": {""var2"": null } }")]

        // terminal variable value not provided as null
        [TestCase(@"{ ""0"": ""0.variables.var1""}", 2, @"{ ""var1"": 35 }")]
        [TestCase(@"{ ""0"": [0, ""variables"", ""var1""]}", 2, @"{ ""var1"": 23 }")]

        // file reference doesnt exist
        [TestCase(@"{ ""notAFile"": ""0.variables.var1""}", -1)]
        public void InvalidMapValue_BatchQuery_ThrowsException(string map, int expectedFailedIndex, string customVariablestring = null)
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

            var file = new FileUpload("0", new Mock<IFileUploadStreamContainer>().Object, "text/plain", "myFile.txt");

            var files = new Dictionary<string, FileUpload>();
            files.Add(file.MapKey, file);

            var assembler = new MultipartRequestPayloadAssembler();

            var ex = Assert.ThrowsAsync<InvalidMultiPartMapException>(async () =>
            {
                await assembler.AssemblePayload(operations, map, files);
            });

            Assert.AreEqual(expectedFailedIndex, ex.Index);
        }

        [Test]
        public void NoVariablesCollectionDefinedOnTargetOfMap_ThrowsException()
        {
            var queryText = "query { field1 {field2 field3} }";
            var operations = @"
            {
                ""query""     : """ + queryText + @"""
            }";

            string map = @"{""0"": [""variables"", ""var1""] }";

            var file = new FileUpload("0", new Mock<IFileUploadStreamContainer>().Object, "text/plain", "myFile.txt");

            var files = new Dictionary<string, FileUpload>();
            files.Add(file.MapKey, file);

            var assembler = new MultipartRequestPayloadAssembler();

            Assert.ThrowsAsync<InvalidMultiPartMapException>(async () =>
            {
                var payload = await assembler.AssemblePayload(operations, map, files);
            });
        }

        [Test]
        public void NoVariablesDefinedOnTargetOfMap_ThrowsException()
        {
            var queryText = "query { field1 {field2 field3} }";
            var operations = @"
            {
                ""query""     : """ + queryText + @""",
                ""variables"": {}
            }";

            string map = @"{""0"": [""variables"", ""var1""] }";

            var file = new FileUpload("0", new Mock<IFileUploadStreamContainer>().Object, "text/plain", "myFile.txt");

            var files = new Dictionary<string, FileUpload>();
            files.Add(file.MapKey, file);

            var assembler = new MultipartRequestPayloadAssembler();

            Assert.ThrowsAsync<InvalidMultiPartMapException>(async () =>
            {
                var payload = await assembler.AssemblePayload(operations, map, files);
            });
        }

        [Test]
        public void MissingVariableInNestedSequence_ThrowsException()
        {
            var queryText = "query { field1 {field2 field3} }";
            var operations = @"
            {
                ""query""     : """ + queryText + @""",
                ""variables"": {""var1"": {""var2"" : {""var3"" : null } } }
            }";

            string map = @"{""0"": [""variables"", ""var1"", ""var4"", ""var3""] }";

            var file = new FileUpload("0", new Mock<IFileUploadStreamContainer>().Object, "text/plain", "myFile.txt");

            var files = new Dictionary<string, FileUpload>();
            files.Add(file.MapKey, file);

            var assembler = new MultipartRequestPayloadAssembler();

            Assert.ThrowsAsync<InvalidMultiPartMapException>(async () =>
            {
                var payload = await assembler.AssemblePayload(operations, map, files);
            });
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

            var file = new FileUpload("0", new Mock<IFileUploadStreamContainer>().Object, "text/plain", "myFile.txt");

            var files = new Dictionary<string, FileUpload>();
            files.Add(file.MapKey, file);

            var assembler = new MultipartRequestPayloadAssembler();

            Assert.ThrowsAsync<InvalidMultiPartMapException>(async () =>
            {
                var payload = await assembler.AssemblePayload(operations, map, files);
            });
        }
    }
}