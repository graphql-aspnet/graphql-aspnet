// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.PlanGeneration
{
    using System.Linq;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs.Values;
    using GraphQL.AspNet.PlanGeneration.Document.Parts.QueryInputValues;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.PlanGeneration.PlanGenerationTestData;
    using NUnit.Framework;

    [TestFixture]
    public class SchemaDocumentGeneratorTests
    {
        private IGraphQueryDocument CreateDocument(string text, out ISchema schema)
        {
            var server = new TestServerBuilder()
                .AddGraphType<TestUserController>()
                .AddGraphType<FieldLevelDirective>()
                .Build();

            schema = server.Schema;
            return server.CreateDocument(text);
        }

        private IGraphQueryDocument CreateDocument(string text)
        {
            return this.CreateDocument(text, out var _);
        }

        [Test]
        public void NoFragments_OneOperation_OneField_ParsesDocumentCorrectly()
        {
            var document = this.CreateDocument("query { retrieveUsers { birthDay, name, location } }");

            Assert.IsNotNull(document);
            Assert.AreEqual(0, document.Messages.Count);
            Assert.AreEqual(1, document.Operations.Count);
            Assert.AreEqual(2, document.MaxDepth);

            var operation = document.Operations[string.Empty];

            Assert.IsNull(operation.Variables);

            Assert.IsNotNull(operation.FieldSelectionSet);
            Assert.AreEqual(1, operation.FieldSelectionSet.Count);

            var field = operation.FieldSelectionSet[0];
            Assert.AreEqual("retrieveUsers", field.Alias.ToString());

            Assert.IsNotNull(field.FieldSelectionSet);
            Assert.AreEqual(3, field.FieldSelectionSet.Count);

            var birthDay = field.FieldSelectionSet[0];
            var name = field.FieldSelectionSet[1];
            var location = field.FieldSelectionSet[2];

            Assert.AreEqual("birthDay", birthDay.Alias.ToString());
            Assert.IsNotNull(birthDay.Arguments);
            Assert.IsNotNull(birthDay.GraphType);
            Assert.AreEqual(Constants.ScalarNames.DATETIME, birthDay.GraphType.Name);
            Assert.IsNull(birthDay.TargetGraphType);

            Assert.AreEqual("name", name.Alias.ToString());
            Assert.IsNotNull(name.Arguments);
            Assert.IsNotNull(name.GraphType);
            Assert.AreEqual(Constants.ScalarNames.STRING, name.GraphType.Name);
            Assert.IsNull(name.TargetGraphType);

            Assert.AreEqual("location", location.Alias.ToString());
            Assert.IsNotNull(location.Arguments);
            Assert.IsNotNull(location.GraphType);
            Assert.AreEqual(Constants.ScalarNames.STRING, location.GraphType.Name);
            Assert.IsNull(location.TargetGraphType);
        }

        [Test]
        public void DirectiveDeclaredOnField_IsNotPropegatedToChildFields()
        {
            var document = this.CreateDocument("query { retrieveUsers @fieldLevel(arg: 3) { birthDay, name, location } }");

            Assert.IsNotNull(document);
            Assert.AreEqual(0, document.Messages.Count);
            Assert.AreEqual(1, document.Operations.Count);
            Assert.AreEqual(2, document.MaxDepth);

            var operation = document.Operations[string.Empty];
            Assert.IsNotNull(operation);

            Assert.AreEqual(1, operation.FieldSelectionSet.Count);
            var field = operation.FieldSelectionSet[0];
            Assert.AreEqual("retrieveUsers", field.Name.ToString());

            // first field should have a directive
            Assert.AreEqual(1, field.Directives.Count());
            Assert.AreEqual("fieldLevel", field.Directives.First().Name);

            // all child fields sohuld have no directives
            Assert.AreEqual(3, field.FieldSelectionSet.Count);
            Assert.IsEmpty(field.FieldSelectionSet.First().Directives);
            Assert.IsEmpty(field.FieldSelectionSet.Skip(1).First().Directives);
            Assert.IsEmpty(field.FieldSelectionSet.Skip(2).First().Directives);
        }

        [Test]
        public void ComplexValueOnInput_IsParsedIntoQueryArgumentsCorrectly()
        {
            var document = this.CreateDocument(
                "query { complexUserMethod(arg1: {birthDay: 1234, name: \"Jane\" location: \"Outside\"}  arg2: 5){ birthDay name} }",
                out var schema);

            Assert.IsNotNull(document);
            Assert.AreEqual(0, document.Messages.Count);
            Assert.AreEqual(1, document.Operations.Count);
            Assert.AreEqual(2, document.MaxDepth);

            var operation = document.Operations[string.Empty];
            Assert.IsNotNull(operation);

            Assert.AreEqual(1, operation.FieldSelectionSet.Count);
            var field = operation.FieldSelectionSet[0];
            Assert.AreEqual("complexUserMethod", field.Name.ToString());

            Assert.AreEqual(2, field.Arguments.Count);

            var graphArg = schema.KnownTypes.FindGraphType(typeof(TestUser), TypeKind.INPUT_OBJECT);
            var arg1 = field.Arguments["arg1"];

            var complexValue = arg1.Value as QueryComplexInputValue;
            Assert.IsNotNull(complexValue);
            Assert.IsTrue(complexValue.ValueNode is ComplexValueNode);
            Assert.AreEqual(3, complexValue.Arguments.Count);

            Assert.AreEqual(graphArg, complexValue.OwnerArgument.GraphType);
            var complexArg1 = complexValue.Arguments["birthDay"];
            var complexArg2 = complexValue.Arguments["name"];
            var complexArg3 = complexValue.Arguments["location"];

            var value = complexArg1.Value as QueryScalarInputValue;
            Assert.AreEqual(ScalarValueType.Number, value.ValueType);
            Assert.AreEqual("1234", value.Value.ToString());

            value = complexArg2.Value as QueryScalarInputValue;
            Assert.AreEqual(ScalarValueType.String, value.ValueType);
            Assert.AreEqual("\"Jane\"", value.Value.ToString());

            value = complexArg3.Value as QueryScalarInputValue;
            Assert.AreEqual(ScalarValueType.String, value.ValueType);
            Assert.AreEqual("\"Outside\"", value.Value.ToString());

            var arg2 = field.Arguments["arg2"];
            value = arg2.Value as QueryScalarInputValue;
            Assert.AreEqual("5", value.Value.ToString());
        }

        [Test]
        public void NestedComplexValueOnInput_IsParsedIntoQueryArgumentsCorrectly()
        {
            var document = this.CreateDocument(
                @"query { nestedInputObjectMethod(
                                    arg1: {user: {birthDay: 1234, name: ""Jane"" location: ""Outside""}, id: 15, houseName: ""BobHouse""}
                                    arg2: 5){
                                        birthDay name
                                } }",
                out var schema);

            Assert.IsNotNull(document);
            Assert.AreEqual(0, document.Messages.Count);
            Assert.AreEqual(1, document.Operations.Count);
            Assert.AreEqual(2, document.MaxDepth);

            var operation = document.Operations[string.Empty];
            Assert.IsNotNull(operation);

            Assert.AreEqual(1, operation.FieldSelectionSet.Count);
            var field = operation.FieldSelectionSet[0];
            Assert.AreEqual("nestedInputObjectMethod", field.Name.ToString());

            Assert.AreEqual(2, field.Arguments.Count);

            var userHomeGraphType = schema.KnownTypes.FindGraphType(typeof(TestUserHome), TypeKind.INPUT_OBJECT);
            var userGraphType = schema.KnownTypes.FindGraphType(typeof(TestUser), TypeKind.INPUT_OBJECT);
            var arg1 = field.Arguments["arg1"];

            var arg1Value = arg1.Value as QueryComplexInputValue;
            Assert.IsNotNull(arg1Value);
            Assert.IsTrue(arg1Value.ValueNode is ComplexValueNode);
            Assert.AreEqual(3, arg1Value.Arguments.Count);

            Assert.AreEqual(userHomeGraphType, arg1Value.OwnerArgument.GraphType);
            var houseArgUser = arg1Value.Arguments["user"];
            var houseArgId = arg1Value.Arguments["id"];
            var houseArgName = arg1Value.Arguments["houseName"];

            var childUser = houseArgUser.Value as QueryComplexInputValue;
            Assert.IsNotNull(childUser);
            Assert.AreEqual(userGraphType, childUser.OwnerArgument.GraphType);
            var childUserbirthDay = childUser.Arguments["birthDay"];
            var childUserName = childUser.Arguments["name"];
            var childUserLocation = childUser.Arguments["location"];

            var value = childUserbirthDay.Value as QueryScalarInputValue;
            Assert.AreEqual(ScalarValueType.Number, value.ValueType);
            Assert.AreEqual("1234", value.Value.ToString());

            value = childUserName.Value as QueryScalarInputValue;
            Assert.AreEqual(ScalarValueType.String, value.ValueType);
            Assert.AreEqual("\"Jane\"", value.Value.ToString());

            value = childUserLocation.Value as QueryScalarInputValue;
            Assert.AreEqual(ScalarValueType.String, value.ValueType);
            Assert.AreEqual("\"Outside\"", value.Value.ToString());

            value = houseArgId.Value as QueryScalarInputValue;
            Assert.AreEqual("15", value.Value.ToString());

            value = houseArgName.Value as QueryScalarInputValue;
            Assert.AreEqual("\"BobHouse\"", value.Value.ToString());

            var arg2 = field.Arguments["arg2"];
            value = arg2.Value as QueryScalarInputValue;
            Assert.AreEqual("5", value.Value.ToString());
        }

        [Test]
        public void ListValueOnInput_IsParsedIntoQueryArgumentsCorrectly()
        {
            var document = this.CreateDocument(
                @"query { multiScalarInput(arg1: [5, 15, 18, 95] arg2: 5)
                                {
                                        birthDay name
                                } }",
                out var _);

            Assert.IsNotNull(document);
            Assert.AreEqual(0, document.Messages.Count);
            Assert.AreEqual(1, document.Operations.Count);
            Assert.AreEqual(2, document.MaxDepth);

            var operation = document.Operations[string.Empty];
            Assert.IsNotNull(operation);

            Assert.AreEqual(1, operation.FieldSelectionSet.Count);
            var field = operation.FieldSelectionSet[0];
            Assert.AreEqual("multiScalarInput", field.Name.ToString());

            Assert.AreEqual(2, field.Arguments.Count);

            var arg1 = field.Arguments["arg1"];

            var arg1Value = arg1.Value as QueryListInputValue;
            Assert.IsNotNull(arg1Value);
            Assert.IsTrue(arg1Value.ValueNode is ListValueNode);
            Assert.AreEqual(4, arg1Value.ListItems.Count);

            Assert.IsTrue(arg1Value.ListItems[0] is QueryScalarInputValue scalar && scalar.Value.ToString() == "5");
            Assert.IsTrue(arg1Value.ListItems[1] is QueryScalarInputValue scalar1 && scalar1.Value.ToString() == "15");
            Assert.IsTrue(arg1Value.ListItems[2] is QueryScalarInputValue scalar2 && scalar2.Value.ToString() == "18");
            Assert.IsTrue(arg1Value.ListItems[3] is QueryScalarInputValue scalar3 && scalar3.Value.ToString() == "95");

            var arg2 = field.Arguments["arg2"];
            var value = arg2.Value as QueryScalarInputValue;
            Assert.AreEqual("5", value.Value.ToString());
        }

        [Test]
        public void ListValueOnListValueOnInput_IsParsedIntoQueryArgumentsCorrectly()
        {
            var document = this.CreateDocument(
                                @"query { multiScalarOfScalarInput(arg1: [[1, 5], [10, 15], [20, 30]])
                                {
                                        birthDay name
                                } }");

            Assert.IsNotNull(document);
            Assert.AreEqual(0, document.Messages.Count);
            Assert.AreEqual(1, document.Operations.Count);
            Assert.AreEqual(2, document.MaxDepth);

            var operation = document.Operations[string.Empty];
            Assert.IsNotNull(operation);
            Assert.AreEqual(1, operation.FieldSelectionSet.Count);
            var field = operation.FieldSelectionSet[0];
            Assert.AreEqual("multiScalarOfScalarInput", field.Name.ToString());

            Assert.AreEqual(1, field.Arguments.Count);

            var arg1 = field.Arguments["arg1"];

            var arg1Value = arg1.Value as QueryListInputValue;
            Assert.IsNotNull(arg1Value);
            Assert.IsTrue(arg1Value.ValueNode is ListValueNode);
            Assert.AreEqual(3, arg1Value.ListItems.Count);

            var listItem1 = arg1Value.ListItems[0] as QueryListInputValue;
            var listItem2 = arg1Value.ListItems[1] as QueryListInputValue;
            var listItem3 = arg1Value.ListItems[2] as QueryListInputValue;

            Assert.IsNotNull(listItem1);
            Assert.AreEqual(2, listItem1.ListItems.Count);
            Assert.AreEqual(arg1Value.OwnerArgument, listItem1.OwnerArgument);
            Assert.IsTrue(listItem1.ListItems[0] is QueryScalarInputValue svn1A && svn1A.Value.ToString() == "1");
            Assert.IsTrue(listItem1.ListItems[1] is QueryScalarInputValue svn1B && svn1B.Value.ToString() == "5");

            Assert.IsNotNull(listItem2);
            Assert.AreEqual(2, listItem2.ListItems.Count);
            Assert.AreEqual(arg1Value.OwnerArgument, listItem2.OwnerArgument);
            Assert.IsTrue(listItem2.ListItems[0] is QueryScalarInputValue svn2A && svn2A.Value.ToString() == "10");
            Assert.IsTrue(listItem2.ListItems[1] is QueryScalarInputValue svn2B && svn2B.Value.ToString() == "15");

            Assert.IsNotNull(listItem3);
            Assert.AreEqual(2, listItem3.ListItems.Count);
            Assert.AreEqual(arg1Value.OwnerArgument, listItem3.OwnerArgument);
            Assert.IsTrue(listItem3.ListItems[0] is QueryScalarInputValue svn3A && svn3A.Value.ToString() == "20");
            Assert.IsTrue(listItem3.ListItems[1] is QueryScalarInputValue svn3B && svn3B.Value.ToString() == "30");
        }

        [Test]
        public void ListValueOfComplexInputs_IsParsedIntoQueryArgumentsCorrectly()
        {
            var document = this.CreateDocument(@"query { multiUserInput(arg1: [
                                                            {birthDay: 1234, name: ""Jane"" location: ""Outside""},
                                                            {birthDay: 5678, name: ""John"" location: ""Inside""}
                                                        ] arg2: 5)
                                {
                                        birthDay name
                                } }");

            Assert.IsNotNull(document);
            Assert.AreEqual(0, document.Messages.Count);
            Assert.AreEqual(1, document.Operations.Count);
            Assert.AreEqual(2, document.MaxDepth);

            var operation = document.Operations[string.Empty];
            Assert.IsNotNull(operation);

            Assert.AreEqual(1, operation.FieldSelectionSet.Count);
            var field = operation.FieldSelectionSet[0];
            Assert.AreEqual("multiUserInput", field.Name.ToString());

            Assert.AreEqual(2, field.Arguments.Count);

            var arg1 = field.Arguments["arg1"];

            var arg1Value = arg1.Value as QueryListInputValue;
            Assert.IsNotNull(arg1Value);
            Assert.IsTrue(arg1Value.ValueNode is ListValueNode);
            Assert.AreEqual(2, arg1Value.ListItems.Count);

            var user1 = arg1Value.ListItems[0] as QueryComplexInputValue;
            var user2 = arg1Value.ListItems[1] as QueryComplexInputValue;

            Assert.IsNotNull(user1);

            Assert.IsNotNull(user2);

            var arg2 = field.Arguments["arg2"];
            var value = arg2.Value as QueryScalarInputValue;
            Assert.AreEqual("5", value.Value.ToString());
        }

        [Test]
        public void VariableOnInput_IsParsedIntoQueryArgumentsCorrectly()
        {
            var document = this.CreateDocument(
                @"query($var1: Int!) { 
                    retrieveUser(id: $var1) {
                        birthDay name
                    } 
                }");

            Assert.IsNotNull(document);
            Assert.AreEqual(0, document.Messages.Count);
            Assert.AreEqual(1, document.Operations.Count);
            Assert.AreEqual(2, document.MaxDepth);

            var operation = document.Operations[string.Empty];
            Assert.IsNotNull(operation);

            Assert.AreEqual(1, operation.FieldSelectionSet.Count);
            var field = operation.FieldSelectionSet[0];
            Assert.AreEqual("retrieveUser", field.Name.ToString());

            Assert.AreEqual(1, field.Arguments.Count);

            var arg1 = field.Arguments["id"];

            var arg1Value = arg1.Value as QueryVariableReferenceInputValue;
            Assert.IsNotNull(arg1Value);
            Assert.IsTrue(arg1Value.ValueNode is VariableValueNode);
            Assert.AreEqual("var1", arg1Value.VariableName);
            Assert.IsNotNull(arg1Value.Variable);
            Assert.AreEqual("var1", arg1Value.Variable.Name);
            Assert.IsTrue(arg1Value.Variable.IsReferenced);
        }

        [Test]
        public void NestedVariable_IsParsedIntoQueryArgumentsCorrectly()
        {
            var document = this.CreateDocument(
                                @"query($var1 : Int!) { multiScalarOfScalarInput(arg1: [[1, 5], [$var1, 15]])
                                {
                                        birthDay name
                                } }");

            Assert.IsNotNull(document);
            Assert.AreEqual(0, document.Messages.Count);
            Assert.AreEqual(1, document.Operations.Count);
            Assert.AreEqual(2, document.MaxDepth);

            var operation = document.Operations[string.Empty];
            Assert.IsNotNull(operation);

            Assert.AreEqual(1, operation.FieldSelectionSet.Count);
            var field = operation.FieldSelectionSet[0];
            Assert.AreEqual("multiScalarOfScalarInput", field.Name.ToString());

            Assert.AreEqual(1, field.Arguments.Count);

            var arg1 = field.Arguments["arg1"];

            var arg1Value = arg1.Value as QueryListInputValue;
            Assert.IsNotNull(arg1Value);
            Assert.IsTrue(arg1Value.ValueNode is ListValueNode);
            Assert.AreEqual(2, arg1Value.ListItems.Count);

            var listItem1 = arg1Value.ListItems[0] as QueryListInputValue;
            var listItem2 = arg1Value.ListItems[1] as QueryListInputValue;

            Assert.IsNotNull(listItem1);
            Assert.AreEqual(2, listItem1.ListItems.Count);
            Assert.AreEqual(arg1Value.OwnerArgument, listItem1.OwnerArgument);
            Assert.IsTrue(listItem1.ListItems[0] is QueryScalarInputValue svn1A && svn1A.Value.ToString() == "1");
            Assert.IsTrue(listItem1.ListItems[1] is QueryScalarInputValue svn1B && svn1B.Value.ToString() == "5");

            Assert.IsNotNull(listItem2);
            Assert.AreEqual(2, listItem2.ListItems.Count);
            Assert.AreEqual(arg1Value.OwnerArgument, listItem2.OwnerArgument);
            Assert.IsTrue(listItem2.ListItems[0] is QueryVariableReferenceInputValue qiv && qiv.VariableName == "var1");
            Assert.IsTrue(listItem2.ListItems[1] is QueryScalarInputValue svn3B && svn3B.Value.ToString() == "15");
        }

        [Test]
        public void Variables_NoDefaultValue_ParseInto_DocumentElements()
        {
            var document = this.CreateDocument("query($var1: Int!) { retrieveUser(id: $var1) { birthDay } }");

            Assert.IsNotNull(document);
            Assert.AreEqual(0, document.Messages.Count);
            Assert.AreEqual(1, document.Operations.Count);
            Assert.AreEqual(2, document.MaxDepth);

            var operation = document.Operations[string.Empty];

            Assert.IsNotNull(operation.Variables);
            Assert.AreEqual(1, operation.Variables.Count);

            var var1 = operation.Variables["var1"];
            Assert.AreEqual("Int!", var1.TypeExpression.ToString());
            Assert.AreEqual(null, var1.Value);
        }

        [Test]
        public void Variables_WithDefaultValue_ParseInto_DocumentElements()
        {
            var document = this.CreateDocument(
                @"query($var1: Input_TestUser = {birthDay: 5 name: ""Bob"" location: ""Somewhere""}) {
                    complexUserMethod(arg1: $var1 arg2: 5) {
                        birthDay
                    }
                }",
                out var schema);

            Assert.IsNotNull(document);
            Assert.AreEqual(0, document.Messages.Count);
            Assert.AreEqual(1, document.Operations.Count);
            Assert.AreEqual(2, document.MaxDepth);

            var operation = document.Operations[string.Empty];

            Assert.IsNotNull(operation.Variables);
            Assert.AreEqual(1, operation.Variables.Count);

            var graphType = schema.KnownTypes.FindGraphType(typeof(TestUser), TypeKind.INPUT_OBJECT);

            var var1 = operation.Variables["var1"];
            Assert.AreEqual("Input_TestUser", var1.TypeExpression.ToString());
            Assert.AreEqual(graphType, var1.GraphType);

            var defaultValue = var1.Value as QueryComplexInputValue;
            Assert.IsNotNull(defaultValue);
            Assert.AreEqual(3, defaultValue.Arguments.Count);
        }

        [Test]
        public void MultiLevelNestedQuery_SetsMaxDepthAppropriately()
        {
            var document = this.CreateDocument(
                @"query{
                    multiLevelOutput() {
                        user {
                            name
                            birthDay
                        }
                        id
                        houseName
                    }
                }",
                out var _);

            Assert.IsNotNull(document);
            Assert.AreEqual(0, document.Messages.Count);
            Assert.AreEqual(1, document.Operations.Count);
            Assert.AreEqual(3, document.MaxDepth);
        }
    }
}