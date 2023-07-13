// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Tests.Engine.TypeMakers
{
    using System.Linq;
    using GraphQL.AspNet.Interfaces.Internal;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.Generation;
    using GraphQL.AspNet.Schemas.Generation.TypeMakers;
    using GraphQL.AspNet.Schemas.Generation.TypeTemplates;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Security;
    using GraphQL.AspNet.Tests.CommonHelpers;
    using GraphQL.AspNet.Tests.Engine.TypeMakers.TestData;
    using GraphQL.AspNet.Tests.Framework;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class FieldMaker_StandardFieldTests : GraphTypeMakerTestBase
    {
        [Test]
        public void ActionTemplate_CreateGraphField_WithUnion_UsesUnionNameAsGraphTypeName()
        {
            var action = GraphQLTemplateHelper.CreateActionMethodTemplate<UnionTestController>(nameof(UnionTestController.TwoTypeUnion));
            var field = this.MakeGraphField(action);

            Assert.IsNotNull(field);
            Assert.AreEqual("FragmentData", field.TypeExpression.TypeName);
        }

        [Test]
        public void ActionTemplate_RetrieveRequiredTypes_WithUnion_ReturnsUnionTypes_NotMethodReturnType()
        {
            var action = GraphQLTemplateHelper.CreateActionMethodTemplate<UnionTestController>(nameof(UnionTestController.TwoTypeUnion));
            var field = this.MakeGraphField(action);

            var dependentTypes = action.RetrieveRequiredTypes()?.ToList();
            Assert.IsNotNull(dependentTypes);
            Assert.AreEqual(2, dependentTypes.Count);

            Assert.IsTrue(dependentTypes.Any(x => x.Type == typeof(UnionDataA) && x.ExpectedKind == TypeKind.OBJECT));
            Assert.IsTrue(dependentTypes.Any(x => x.Type == typeof(UnionDataB) && x.ExpectedKind == TypeKind.OBJECT));
        }

        [Test]
        public void Parse_PolicyOnController_IsInheritedByField()
        {
            var server = new TestServerBuilder().Build();
            var template = GraphQLTemplateHelper.CreateControllerTemplate<SecuredController>();

            // method declares no polciies
            // controller declares 1
            var actionMethod = template.Actions.FirstOrDefault(x => x.Name == nameof(SecuredController.DoSomething));

            Assert.AreEqual(1, template.SecurityPolicies.Count());
            Assert.AreEqual(0, actionMethod.SecurityPolicies.Count());

            var factory = server.CreateMakerFactory();

            var graphField = new GraphFieldMaker(server.Schema, factory).CreateField(actionMethod).Field;
            Assert.AreEqual(1, Enumerable.Count<AppliedSecurityPolicyGroup>(graphField.SecurityGroups));

            var group = Enumerable.First<AppliedSecurityPolicyGroup>(graphField.SecurityGroups);
            Assert.AreEqual(template.SecurityPolicies.First(), group.First());
        }

        [Test]
        public void Parse_PolicyOnController_AndOnMethod_IsInheritedByField_InCorrectOrder()
        {
            var server = new TestServerBuilder().Build();
            var template = GraphQLTemplateHelper.CreateControllerTemplate<SecuredController>();

            // controller declares 1 policy
            // method declares 1 policy
            var actionMethod = template.Actions.FirstOrDefault(x => x.Name == nameof(SecuredController.DoSomethingSecure));

            Assert.AreEqual(1, template.SecurityPolicies.Count());
            Assert.AreEqual(1, actionMethod.SecurityPolicies.Count());

            var factory = server.CreateMakerFactory();

            var graphField = new GraphFieldMaker(server.Schema, factory).CreateField(actionMethod).Field;

            Assert.AreEqual(2, Enumerable.Count<AppliedSecurityPolicyGroup>(graphField.SecurityGroups));

            // ensure policy order of controller -> method
            var controllerTemplateGroup = Enumerable.First<AppliedSecurityPolicyGroup>(graphField.SecurityGroups);
            var fieldTemplateGroup = Enumerable.Skip<AppliedSecurityPolicyGroup>(graphField.SecurityGroups, 1).First();
            Assert.AreEqual(template.SecurityPolicies.First(), controllerTemplateGroup.First());
            Assert.AreEqual(actionMethod.SecurityPolicies.First(), fieldTemplateGroup.First());
        }

        [Test]
        public void Parse_MethodWithNullableEnum_ParsesCorrectly()
        {
            var server = new TestServerBuilder().Build();
            var template = GraphQLTemplateHelper.CreateControllerTemplate<NullableEnumController>();

            Assert.AreEqual(1, template.FieldTemplates.Count);

            var field = template.FieldTemplates.FirstOrDefault().Value;
            Assert.AreEqual(nameof(NullableEnumController.ConvertUnit), field.Name);
            Assert.AreEqual(typeof(int), field.ObjectType);

            var arg = field.Arguments[0];
            Assert.AreEqual(typeof(NullableEnumController.LengthType), arg.ObjectType);
            Assert.AreEqual(NullableEnumController.LengthType.Yards, arg.DefaultValue);

            var factory = server.CreateMakerFactory();

            var graphField = new GraphFieldMaker(server.Schema, factory).CreateField(field).Field;
            Assert.IsNotNull(graphField);

            var graphArg = Enumerable.FirstOrDefault(graphField.Arguments);
            Assert.IsNotNull(graphArg);
            Assert.IsEmpty(graphArg.TypeExpression.Wrappers);
        }

        [Test]
        public void PropertyGraphField_DefaultValuesCheck()
        {
            var server = new TestServerBuilder().Build();

            var obj = new Mock<IObjectGraphTypeTemplate>();
            obj.Setup(x => x.Route).Returns(new SchemaItemPath("[type]/Item0"));
            obj.Setup(x => x.InternalFullName).Returns("LongItem0");
            obj.Setup(x => x.InternalName).Returns("Item0");
            obj.Setup(x => x.ObjectType).Returns(typeof(SimplePropertyObject));

            var parent = obj.Object;
            var propInfo = typeof(SimplePropertyObject).GetProperty(nameof(SimplePropertyObject.Name));
            var template = new PropertyGraphFieldTemplate(parent, propInfo, TypeKind.OBJECT);
            template.Parse();
            template.ValidateOrThrow();

            var field = this.MakeGraphField(template);
            Assert.IsNotNull(field);
            Assert.AreEqual(Constants.ScalarNames.STRING, field.TypeExpression.TypeName);
            Assert.AreEqual(0, field.TypeExpression.Wrappers.Length);
        }

        [Test]
        public void PropertyGraphField_GraphName_OverridesDefaultName()
        {
            var obj = new Mock<IObjectGraphTypeTemplate>();
            obj.Setup(x => x.Route).Returns(new SchemaItemPath("[type]/Item0"));
            obj.Setup(x => x.InternalFullName).Returns("LongItem0");
            obj.Setup(x => x.InternalName).Returns("Item0");
            obj.Setup(x => x.ObjectType).Returns(typeof(SimplePropertyObject));

            var parent = obj.Object;
            var propInfo = typeof(SimplePropertyObject).GetProperty(nameof(SimplePropertyObject.Age));
            var template = new PropertyGraphFieldTemplate(parent, propInfo, TypeKind.OBJECT);
            template.Parse();
            template.ValidateOrThrow();

            Assert.AreEqual("SuperAge", template.Name);
            Assert.AreEqual(typeof(int), template.ObjectType);

            var field = this.MakeGraphField(template);
            Assert.IsNotNull(field);
            Assert.AreEqual(Constants.ScalarNames.INT, field.TypeExpression.TypeName);
            CollectionAssert.AreEqual(new MetaGraphTypes[] { MetaGraphTypes.IsNotNull }, field.TypeExpression.Wrappers);
        }

        [Test]
        public void PropertyGraphField_DirectivesAreAppliedToCreatedField()
        {
            var server = new TestServerBuilder().Build();
            var obj = new Mock<IObjectGraphTypeTemplate>();
            obj.Setup(x => x.Route).Returns(new SchemaItemPath("[type]/Item0"));
            obj.Setup(x => x.InternalFullName).Returns("LongItem0");
            obj.Setup(x => x.InternalName).Returns("Item0");
            obj.Setup(x => x.ObjectType).Returns(typeof(SimplePropertyObject));

            var parent = obj.Object;
            var propInfo = typeof(ObjectDirectiveTestItem).GetProperty(nameof(ObjectDirectiveTestItem.Prop1));
            var template = new PropertyGraphFieldTemplate(parent, propInfo, TypeKind.OBJECT);
            template.Parse();
            template.ValidateOrThrow();

            var field = this.MakeGraphField(template);

            Assert.AreEqual(1, field.AppliedDirectives.Count);
            Assert.AreEqual(field, field.AppliedDirectives.Parent);

            var appliedDirective = Enumerable.FirstOrDefault(field.AppliedDirectives);
            Assert.AreEqual(typeof(DirectiveWithArgs), appliedDirective.DirectiveType);
            CollectionAssert.AreEqual(new object[] { 13, "prop field arg" }, appliedDirective.ArgumentValues);
        }

        [Test]
        public void MethodGraphField_DirectivesAreAppliedToCreatedField()
        {
            var server = new TestServerBuilder().Build();
            var obj = new Mock<IObjectGraphTypeTemplate>();
            obj.Setup(x => x.Route).Returns(new SchemaItemPath("[type]/Item0"));
            obj.Setup(x => x.InternalFullName).Returns("LongItem0");
            obj.Setup(x => x.InternalName).Returns("Item0");
            obj.Setup(x => x.ObjectType).Returns(typeof(ObjectDirectiveTestItem));

            var parent = obj.Object;
            var methodInfo = typeof(ObjectDirectiveTestItem).GetMethod(nameof(ObjectDirectiveTestItem.Method1));
            var template = new MethodGraphFieldTemplate(parent, methodInfo, TypeKind.OBJECT);
            template.Parse();
            template.ValidateOrThrow();

            var field = this.MakeGraphField(template);

            Assert.AreEqual(1, field.AppliedDirectives.Count);
            Assert.AreEqual(field, field.AppliedDirectives.Parent);

            var appliedDirective = Enumerable.FirstOrDefault(field.AppliedDirectives);
            Assert.AreEqual(typeof(DirectiveWithArgs), appliedDirective.DirectiveType);
            CollectionAssert.AreEqual(new object[] { 14, "method field arg" }, appliedDirective.ArgumentValues);
        }

        [Test]
        public void Arguments_DirectivesAreApplied()
        {
            var server = new TestServerBuilder().Build();
            var obj = new Mock<IObjectGraphTypeTemplate>();
            obj.Setup(x => x.Route).Returns(new SchemaItemPath("[type]/Item0"));
            obj.Setup(x => x.InternalFullName).Returns("LongItem0");
            obj.Setup(x => x.InternalName).Returns("Item0");
            obj.Setup(x => x.ObjectType).Returns(typeof(ObjectDirectiveTestItem));

            var parent = obj.Object;
            var methodInfo = typeof(ObjectDirectiveTestItem).GetMethod(nameof(ObjectDirectiveTestItem.MethodWithArgDirectives));
            var template = new MethodGraphFieldTemplate(parent, methodInfo, TypeKind.OBJECT);
            template.Parse();
            template.ValidateOrThrow();

            var field = this.MakeGraphField(template);

            Assert.AreEqual(0, field.AppliedDirectives.Count);
            Assert.AreEqual(field, field.AppliedDirectives.Parent);
            var arg = field.Arguments["arg1"];

            Assert.AreEqual(1, arg.AppliedDirectives.Count);
            Assert.AreEqual(arg, arg.AppliedDirectives.Parent);

            var appliedDirective = Enumerable.FirstOrDefault(field.Arguments["arg1"].AppliedDirectives);

            Assert.AreEqual(typeof(DirectiveWithArgs), appliedDirective.DirectiveType);
            CollectionAssert.AreEqual(new object[] { 15, "arg arg" }, appliedDirective.ArgumentValues);
        }
    }
}