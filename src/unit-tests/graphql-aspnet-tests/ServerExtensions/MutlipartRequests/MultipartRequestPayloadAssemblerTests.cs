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
    using System.Threading.Tasks;
    using GraphQL.AspNet.Execution.Variables;
    using GraphQL.AspNet.Interfaces.Execution.Variables;
    using GraphQL.AspNet.ServerExtensions.MultipartRequests;
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
        public async Task SingleQuery_SingleFile_ArrayMember_AddedToQueryVariableCorrectly()
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
    }
}