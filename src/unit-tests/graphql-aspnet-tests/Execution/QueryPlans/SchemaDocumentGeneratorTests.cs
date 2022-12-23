// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution.QueryPlans
{
    using System.Linq;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.Document;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.Document.Parts;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Schemas.TypeSystem.Scalars;
    using GraphQL.AspNet.Tests.Execution.QueryPlans.PlanGenerationTestData;
    using GraphQL.AspNet.Tests.Framework;
    using NUnit.Framework;

    [TestFixture]
    public class SchemaDocumentGeneratorTests
    {
        private IQueryDocument CreateDocument(string text, out ISchema schema)
        {
            var server = new TestServerBuilder()
                .AddType<TestUserController>()
                .AddType<FieldLevelDirective>()
                .Build();

            schema = server.Schema;
            return server.CreateDocument(text);
        }

        private IQueryDocument CreateDocument(string text)
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

            var operation = document.Operations[string.Empty];

            Assert.AreEqual(0, operation.Variables.Count);

            Assert.IsNotNull(operation.FieldSelectionSet);
            Assert.AreEqual(1, operation.FieldSelectionSet.ExecutableFields.Count());

            var field = operation.FieldSelectionSet.ExecutableFields[0];
            Assert.AreEqual("retrieveUsers", field.Alias.ToString());

            Assert.IsNotNull(field.FieldSelectionSet);
            Assert.AreEqual(3, field.FieldSelectionSet.ExecutableFields.Count());

            var birthDay = field.FieldSelectionSet.ExecutableFields[0];
            var name = field.FieldSelectionSet.ExecutableFields[1];
            var location = field.FieldSelectionSet.ExecutableFields[2];

            Assert.AreEqual("birthDay", birthDay.Alias.ToString());
            Assert.IsNotNull(birthDay.Arguments);
            Assert.IsNotNull(birthDay.GraphType);
            Assert.AreEqual(Constants.ScalarNames.DATETIME, birthDay.GraphType.Name);

            Assert.AreEqual("name", name.Alias.ToString());
            Assert.IsNotNull(name.Arguments);
            Assert.IsNotNull(name.GraphType);
            Assert.AreEqual(Constants.ScalarNames.STRING, name.GraphType.Name);

            Assert.AreEqual("location", location.Alias.ToString());
            Assert.IsNotNull(location.Arguments);
            Assert.IsNotNull(location.GraphType);
            Assert.AreEqual(Constants.ScalarNames.STRING, location.GraphType.Name);
        }

        [Test]
        public void DirectiveDeclaredOnField_IsNotPropegatedToChildFields()
        {
            var document = this.CreateDocument("query { retrieveUsers @fieldLevel(arg: 3) { birthDay, name, location } }");

            Assert.IsNotNull(document);
            Assert.AreEqual(0, document.Messages.Count);
            Assert.AreEqual(1, document.Operations.Count);

            var operation = document.Operations[string.Empty];
            Assert.IsNotNull(operation);

            Assert.AreEqual(1, operation.FieldSelectionSet.ExecutableFields.Count());
            var field = operation.FieldSelectionSet.ExecutableFields[0];
            Assert.AreEqual("retrieveUsers", field.Name.ToString());

            // first field should have a directive
            Assert.AreEqual(1, field.Directives.Count);
            Assert.AreEqual("fieldLevel", field.Directives.First().DirectiveName);

            // all child fields sohuld have no directives
            Assert.AreEqual(3, field.FieldSelectionSet.ExecutableFields.Count());
            Assert.AreEqual(0, field.FieldSelectionSet.ExecutableFields[0].Directives.Count);
            Assert.AreEqual(0, field.FieldSelectionSet.ExecutableFields[1].Directives.Count);
            Assert.AreEqual(0, field.FieldSelectionSet.ExecutableFields[2].Directives.Count);
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

            var operation = document.Operations[string.Empty];
            Assert.IsNotNull(operation);

            Assert.AreEqual(1, operation.FieldSelectionSet.ExecutableFields.Count());
            var field = operation.FieldSelectionSet.ExecutableFields[0];
            Assert.AreEqual("complexUserMethod", field.Name.ToString());

            Assert.AreEqual(2, field.Arguments.Count);

            var graphArg = schema.KnownTypes.FindGraphType(typeof(TestUser), TypeKind.INPUT_OBJECT);
            var arg1 = field.Arguments["arg1"];

            var complexValue = arg1.Value as IComplexSuppliedValueDocumentPart;
            Assert.IsNotNull(complexValue);
            Assert.AreEqual(3, complexValue.Fields.Count);

            Assert.AreEqual(graphArg, complexValue.Parent.GraphType);
            var complexArg1 = complexValue.Fields["birthDay"];
            var complexArg2 = complexValue.Fields["name"];
            var complexArg3 = complexValue.Fields["location"];

            var value = complexArg1.Value as IScalarSuppliedValue;
            Assert.AreEqual(ScalarValueType.Number, value.ValueType);
            Assert.AreEqual("1234", value.Value.ToString());

            value = complexArg2.Value as IScalarSuppliedValue;
            Assert.AreEqual(ScalarValueType.String, value.ValueType);
            Assert.AreEqual("\"Jane\"", value.Value.ToString());

            value = complexArg3.Value as IScalarSuppliedValue;
            Assert.AreEqual(ScalarValueType.String, value.ValueType);
            Assert.AreEqual("\"Outside\"", value.Value.ToString());

            var arg2 = field.Arguments["arg2"];
            value = arg2.Value as IScalarSuppliedValue;
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

            var operation = document.Operations[string.Empty];
            Assert.IsNotNull(operation);

            Assert.AreEqual(1, operation.FieldSelectionSet.ExecutableFields.Count());
            var field = operation.FieldSelectionSet.ExecutableFields[0];
            Assert.AreEqual("nestedInputObjectMethod", field.Name.ToString());

            Assert.AreEqual(2, field.Arguments.Count);

            var userHomeGraphType = schema.KnownTypes.FindGraphType(typeof(TestUserHome), TypeKind.INPUT_OBJECT);
            var userGraphType = schema.KnownTypes.FindGraphType(typeof(TestUser), TypeKind.INPUT_OBJECT);
            var arg1 = field.Arguments["arg1"];

            var arg1Value = arg1.Value as IComplexSuppliedValueDocumentPart;
            Assert.IsNotNull(arg1Value);
            Assert.AreEqual(3, arg1Value.Fields.Count);

            Assert.AreEqual(userHomeGraphType, arg1Value.Parent.GraphType);
            var houseArgUser = arg1Value.Fields["user"];
            var houseArgId = arg1Value.Fields["id"];
            var houseArgName = arg1Value.Fields["houseName"];

            var childUser = houseArgUser.Value as IComplexSuppliedValueDocumentPart;
            Assert.IsNotNull(childUser);
            Assert.AreEqual(userGraphType, childUser.Parent.GraphType);
            var childUserbirthDay = childUser.Fields["birthDay"];
            var childUserName = childUser.Fields["name"];
            var childUserLocation = childUser.Fields["location"];

            var value = childUserbirthDay.Value as IScalarSuppliedValue;
            Assert.AreEqual(ScalarValueType.Number, value.ValueType);
            Assert.AreEqual("1234", value.Value.ToString());

            value = childUserName.Value as IScalarSuppliedValue;
            Assert.AreEqual(ScalarValueType.String, value.ValueType);
            Assert.AreEqual("\"Jane\"", value.Value.ToString());

            value = childUserLocation.Value as IScalarSuppliedValue;
            Assert.AreEqual(ScalarValueType.String, value.ValueType);
            Assert.AreEqual("\"Outside\"", value.Value.ToString());

            value = houseArgId.Value as IScalarSuppliedValue;
            Assert.AreEqual("15", value.Value.ToString());

            value = houseArgName.Value as IScalarSuppliedValue;
            Assert.AreEqual("\"BobHouse\"", value.Value.ToString());

            var arg2 = field.Arguments["arg2"];
            value = arg2.Value as IScalarSuppliedValue;
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

            var operation = document.Operations[string.Empty];
            Assert.IsNotNull(operation);

            Assert.AreEqual(1, operation.FieldSelectionSet.ExecutableFields.Count());
            var field = operation.FieldSelectionSet.ExecutableFields[0];
            Assert.AreEqual("multiScalarInput", field.Name.ToString());

            Assert.AreEqual(2, field.Arguments.Count);

            var arg1 = field.Arguments["arg1"];

            var arg1Value = arg1.Value as IListSuppliedValueDocumentPart;
            Assert.IsNotNull(arg1Value);
            Assert.AreEqual(4, arg1Value.ListItems.Count);

            Assert.IsTrue(arg1Value.ListItems[0] is IScalarSuppliedValue scalar && scalar.Value.ToString() == "5");
            Assert.IsTrue(arg1Value.ListItems[1] is IScalarSuppliedValue scalar1 && scalar1.Value.ToString() == "15");
            Assert.IsTrue(arg1Value.ListItems[2] is IScalarSuppliedValue scalar2 && scalar2.Value.ToString() == "18");
            Assert.IsTrue(arg1Value.ListItems[3] is IScalarSuppliedValue scalar3 && scalar3.Value.ToString() == "95");

            var arg2 = field.Arguments["arg2"];
            var value = arg2.Value as IScalarSuppliedValue;
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

            var operation = document.Operations[string.Empty];
            Assert.IsNotNull(operation);
            Assert.AreEqual(1, operation.FieldSelectionSet.ExecutableFields.Count());
            var field = operation.FieldSelectionSet.ExecutableFields[0];
            Assert.AreEqual("multiScalarOfScalarInput", field.Name.ToString());

            Assert.AreEqual(1, field.Arguments.Count);

            var arg1 = field.Arguments["arg1"];

            var arg1Value = arg1.Value as IListSuppliedValueDocumentPart;
            Assert.IsNotNull(arg1Value);
            Assert.AreEqual(3, arg1Value.ListItems.Count);

            var listItem1 = arg1Value.ListItems[0] as IListSuppliedValueDocumentPart;
            var listItem2 = arg1Value.ListItems[1] as IListSuppliedValueDocumentPart;
            var listItem3 = arg1Value.ListItems[2] as IListSuppliedValueDocumentPart;

            Assert.IsNotNull(listItem1);
            Assert.AreEqual(2, listItem1.ListItems.Count);
            Assert.AreEqual(arg1Value, listItem1.Parent);
            Assert.IsTrue(listItem1.ListItems[0] is IScalarSuppliedValue svn1A && svn1A.Value.ToString() == "1");
            Assert.IsTrue(listItem1.ListItems[1] is IScalarSuppliedValue svn1B && svn1B.Value.ToString() == "5");

            Assert.IsNotNull(listItem2);
            Assert.AreEqual(2, listItem2.ListItems.Count);
            Assert.AreEqual(arg1Value, listItem2.Parent);
            Assert.IsTrue(listItem2.ListItems[0] is IScalarSuppliedValue svn2A && svn2A.Value.ToString() == "10");
            Assert.IsTrue(listItem2.ListItems[1] is IScalarSuppliedValue svn2B && svn2B.Value.ToString() == "15");

            Assert.IsNotNull(listItem3);
            Assert.AreEqual(2, listItem3.ListItems.Count);
            Assert.AreEqual(arg1Value, listItem3.Parent);
            Assert.IsTrue(listItem3.ListItems[0] is IScalarSuppliedValue svn3A && svn3A.Value.ToString() == "20");
            Assert.IsTrue(listItem3.ListItems[1] is IScalarSuppliedValue svn3B && svn3B.Value.ToString() == "30");
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

            var operation = document.Operations[string.Empty];
            Assert.IsNotNull(operation);

            Assert.AreEqual(1, operation.FieldSelectionSet.ExecutableFields.Count());
            var field = operation.FieldSelectionSet.ExecutableFields[0];
            Assert.AreEqual("multiUserInput", field.Name.ToString());

            Assert.AreEqual(2, field.Arguments.Count);

            var arg1 = field.Arguments["arg1"];

            var arg1Value = arg1.Value as IListSuppliedValueDocumentPart;
            Assert.IsNotNull(arg1Value);
            Assert.AreEqual(2, arg1Value.ListItems.Count);

            var user1 = arg1Value.ListItems[0] as IComplexSuppliedValueDocumentPart;
            var user2 = arg1Value.ListItems[1] as IComplexSuppliedValueDocumentPart;

            Assert.IsNotNull(user1);

            Assert.IsNotNull(user2);

            var arg2 = field.Arguments["arg2"];
            var value = arg2.Value as IScalarSuppliedValue;
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

            var operation = document.Operations[string.Empty];
            Assert.IsNotNull(operation);

            Assert.AreEqual(1, operation.FieldSelectionSet.ExecutableFields.Count());
            var field = operation.FieldSelectionSet.ExecutableFields[0];
            Assert.AreEqual("retrieveUser", field.Name.ToString());

            Assert.AreEqual(1, field.Arguments.Count);

            var arg1 = field.Arguments["id"];

            var arg1Value = arg1.Value as IVariableUsageDocumentPart;
            Assert.IsNotNull(arg1Value);
            Assert.AreEqual("var1", arg1Value.VariableName.ToString());
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

            var operation = document.Operations[string.Empty];
            Assert.IsNotNull(operation);

            Assert.AreEqual(1, operation.FieldSelectionSet.ExecutableFields.Count());
            var field = operation.FieldSelectionSet.ExecutableFields[0];
            Assert.AreEqual("multiScalarOfScalarInput", field.Name.ToString());

            Assert.AreEqual(1, field.Arguments.Count);

            var arg1 = field.Arguments["arg1"];

            var arg1Value = arg1.Value as IListSuppliedValueDocumentPart;
            Assert.IsNotNull(arg1Value);
            Assert.AreEqual(2, arg1Value.ListItems.Count);

            var listItem1 = arg1Value.ListItems[0] as IListSuppliedValueDocumentPart;
            var listItem2 = arg1Value.ListItems[1] as IListSuppliedValueDocumentPart;

            Assert.IsNotNull(listItem1);
            Assert.AreEqual(2, listItem1.ListItems.Count);
            Assert.AreEqual(arg1Value, listItem1.Parent);
            Assert.IsTrue(listItem1.ListItems[0] is IScalarSuppliedValue svn1A && svn1A.Value.ToString() == "1");
            Assert.IsTrue(listItem1.ListItems[1] is IScalarSuppliedValue svn1B && svn1B.Value.ToString() == "5");

            Assert.IsNotNull(listItem2);
            Assert.AreEqual(2, listItem2.ListItems.Count);
            Assert.AreEqual(arg1Value, listItem2.Parent);
            Assert.IsTrue(listItem2.ListItems[0] is IVariableUsageDocumentPart qiv && qiv.VariableName.ToString() == "var1");
            Assert.IsTrue(listItem2.ListItems[1] is IScalarSuppliedValue svn3B && svn3B.Value.ToString() == "15");
        }

        [Test]
        public void Variables_NoDefaultValue_ParseInto_DocumentElements()
        {
            var document = this.CreateDocument("query($var1: Int!) { retrieveUser(id: $var1) { birthDay } }");

            Assert.IsNotNull(document);
            Assert.AreEqual(0, document.Messages.Count);
            Assert.AreEqual(1, document.Operations.Count);

            var operation = document.Operations[string.Empty];

            Assert.IsNotNull(operation.Variables);
            Assert.AreEqual(1, operation.Variables.Count);

            var var1 = operation.Variables["var1"];
            Assert.AreEqual("Int!", var1.TypeExpression.ToString());
            Assert.AreEqual(null, var1.DefaultValue);
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

            var operation = document.Operations[string.Empty];

            Assert.IsNotNull(operation.Variables);
            Assert.AreEqual(1, operation.Variables.Count);

            var graphType = schema.KnownTypes.FindGraphType(typeof(TestUser), TypeKind.INPUT_OBJECT);

            var var1 = operation.Variables["var1"];
            Assert.AreEqual("Input_TestUser", var1.TypeExpression.ToString());
            Assert.AreEqual(graphType, var1.GraphType);

            var defaultValue = var1.DefaultValue as IComplexSuppliedValueDocumentPart;
            Assert.IsNotNull(defaultValue);
            Assert.AreEqual(3, defaultValue.Fields.Count);
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
        }
    }
}