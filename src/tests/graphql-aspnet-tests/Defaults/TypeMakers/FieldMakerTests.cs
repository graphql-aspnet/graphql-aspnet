// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Tests.Defaults.TypeMakers
{
    using System.Linq;
    using GraphQL.AspNet.Defaults.TypeMakers;
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Internal.TypeTemplates;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.Defaults.TypeMakers.TestData;
    using GraphQL.AspNet.Tests.Framework;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class FieldMakerTests : GraphTypeMakerTestBase
    {
        [Test]
        public void ActionTemplate_CreateGraphField_WithUnion_UsesUnionNameAsGraphTypeName()
        {
            var action = TemplateHelper.CreateActionMethodTemplate<UnionTestController>(nameof(UnionTestController.TwoTypeUnion));
            var field = this.MakeGraphField(action);

            Assert.IsNotNull(field);
            Assert.AreEqual("FragmentData", field.TypeExpression.TypeName);
        }

        [Test]
        public void ActionTemplate_RetrieveRequiredTypes_WithUnion_ReturnsUnionTypes_NotMethodReturnType()
        {
            var action = TemplateHelper.CreateActionMethodTemplate<UnionTestController>(nameof(UnionTestController.TwoTypeUnion));
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
            var template = TemplateHelper.CreateControllerTemplate<SecuredController>();

            // method declares no polciies
            // controller declares 1
            var actionMethod = template.Actions.FirstOrDefault(x => x.Name == nameof(SecuredController.DoSomething));

            Assert.AreEqual(1, template.SecurityPolicies.Count());
            Assert.AreEqual(0, actionMethod.SecurityPolicies.Count());

            var graphField = new GraphFieldMaker(server.Schema).CreateField(actionMethod).Field;
            Assert.AreEqual(1, graphField.SecurityGroups.Count());

            var group = graphField.SecurityGroups.First();
            Assert.AreEqual(template.SecurityPolicies.First(), group.First());
        }

        [Test]
        public void Parse_PolicyOnController_AndOnMethod_IsInheritedByField_InCorrectOrder()
        {
            var server = new TestServerBuilder().Build();
            var template = TemplateHelper.CreateControllerTemplate<SecuredController>();

            // controller declares 1 policy
            // method declares 1 policy
            var actionMethod = template.Actions.FirstOrDefault(x => x.Name == nameof(SecuredController.DoSomethingSecure));

            Assert.AreEqual(1, template.SecurityPolicies.Count());
            Assert.AreEqual(1, actionMethod.SecurityPolicies.Count());

            var graphField = new GraphFieldMaker(server.Schema).CreateField(actionMethod).Field;

            Assert.AreEqual(2, graphField.SecurityGroups.Count());

            // ensure policy order of controller -> method
            var controllerTemplateGroup = graphField.SecurityGroups.First();
            var fieldTemplateGroup = graphField.SecurityGroups.Skip(1).First();
            Assert.AreEqual(template.SecurityPolicies.First(), controllerTemplateGroup.First());
            Assert.AreEqual(actionMethod.SecurityPolicies.First(), fieldTemplateGroup.First());
        }

        [Test]
        public void Parse_MethodWithNullableEnum_ParsesCorrectly()
        {
            var server = new TestServerBuilder().Build();
            var template = TemplateHelper.CreateControllerTemplate<NullableEnumController>();

            Assert.AreEqual(1, template.FieldTemplates.Count);

            var field = template.FieldTemplates.FirstOrDefault().Value;
            Assert.AreEqual(nameof(NullableEnumController.ConvertUnit), field.Name);
            Assert.AreEqual(typeof(int), field.ObjectType);

            var arg = field.Arguments[0];
            Assert.AreEqual(typeof(NullableEnumController.LengthType), arg.ObjectType);
            Assert.AreEqual(NullableEnumController.LengthType.Yards, arg.DefaultValue);

            var graphField = new GraphFieldMaker(server.Schema).CreateField(field).Field;
            Assert.IsNotNull(graphField);

            var graphArg = graphField.Arguments.FirstOrDefault();
            Assert.IsNotNull(graphArg);
            Assert.IsEmpty(graphArg.TypeExpression.Wrappers);
            Assert.AreEqual(GraphArgumentModifiers.None, graphArg.ArgumentModifiers);
        }

        [Test]
        public void PropertyGraphField_DefaultValuesCheck()
        {
            var server = new TestServerBuilder().Build();

            var obj = new Mock<IObjectGraphTypeTemplate>();
            obj.Setup(x => x.Route).Returns(new GraphFieldPath("[type]/Item0"));
            obj.Setup(x => x.InternalFullName).Returns("Item0");

            var parent = obj.Object;
            var propInfo = typeof(SimplePropertyObject).GetProperty(nameof(SimplePropertyObject.Name));
            var template = new PropertyGraphFieldTemplate(parent, propInfo, TypeKind.OBJECT);
            template.Parse();
            template.ValidateOrThrow();

            var field = this.MakeGraphField(template);
            Assert.IsNotNull(field);
            Assert.AreEqual(Constants.ScalarNames.STRING, field.TypeExpression.TypeName);
            CollectionAssert.AreEqual(TypeExpressions.None.ToTypeWrapperSet(), field.TypeExpression.Wrappers);
        }

        [Test]
        public void PropertyGraphField_GraphName_OverridesDefaultName()
        {
            var server = new TestServerBuilder().Build();
            var obj = new Mock<IObjectGraphTypeTemplate>();
            obj.Setup(x => x.Route).Returns(new GraphFieldPath("[type]/Item0"));
            obj.Setup(x => x.InternalFullName).Returns("Item0");

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
            CollectionAssert.AreEqual(TypeExpressions.IsNotNull.ToTypeWrapperSet(), field.TypeExpression.Wrappers);
        }

        [Test]
        public void PropertyGraphField_DirectivesAreAppliedToCreatedField()
        {
            var server = new TestServerBuilder().Build();
            var obj = new Mock<IObjectGraphTypeTemplate>();
            obj.Setup(x => x.Route).Returns(new GraphFieldPath("[type]/Item0"));
            obj.Setup(x => x.InternalFullName).Returns("Item0");

            var parent = obj.Object;
            var propInfo = typeof(ObjectDirectiveTestItem).GetProperty(nameof(ObjectDirectiveTestItem.Prop1));
            var template = new PropertyGraphFieldTemplate(parent, propInfo, TypeKind.OBJECT);
            template.Parse();
            template.ValidateOrThrow();

            var field = this.MakeGraphField(template);

            Assert.AreEqual(1, field.AppliedDirectives.Count);
            Assert.AreEqual(field, field.AppliedDirectives.Parent);

            var appliedDirective = field.AppliedDirectives.FirstOrDefault();
            Assert.AreEqual(typeof(DirectiveWithArgs), appliedDirective.DirectiveType);
            CollectionAssert.AreEqual(new object[] { 13, "prop field arg" }, appliedDirective.Arguments);
        }

        [Test]
        public void MethodGraphField_DirectivesAreAppliedToCreatedField()
        {
            var server = new TestServerBuilder().Build();
            var obj = new Mock<IObjectGraphTypeTemplate>();
            obj.Setup(x => x.Route).Returns(new GraphFieldPath("[type]/Item0"));
            obj.Setup(x => x.InternalFullName).Returns("Item0");

            var parent = obj.Object;
            var methodInfo = typeof(ObjectDirectiveTestItem).GetMethod(nameof(ObjectDirectiveTestItem.Method1));
            var template = new MethodGraphFieldTemplate(parent, methodInfo, TypeKind.OBJECT);
            template.Parse();
            template.ValidateOrThrow();

            var field = this.MakeGraphField(template);

            Assert.AreEqual(1, field.AppliedDirectives.Count);
            Assert.AreEqual(field, field.AppliedDirectives.Parent);

            var appliedDirective = field.AppliedDirectives.FirstOrDefault();
            Assert.AreEqual(typeof(DirectiveWithArgs), appliedDirective.DirectiveType);
            CollectionAssert.AreEqual(new object[] { 14, "method field arg" }, appliedDirective.Arguments);
        }

        [Test]
        public void Arguments_DirectivesAreApplied()
        {
            var server = new TestServerBuilder().Build();
            var obj = new Mock<IObjectGraphTypeTemplate>();
            obj.Setup(x => x.Route).Returns(new GraphFieldPath("[type]/Item0"));
            obj.Setup(x => x.InternalFullName).Returns("Item0");

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

            var appliedDirective = field.Arguments["arg1"].AppliedDirectives.FirstOrDefault();

            Assert.AreEqual(typeof(DirectiveWithArgs), appliedDirective.DirectiveType);
            CollectionAssert.AreEqual(new object[] { 15, "arg arg" }, appliedDirective.Arguments);
        }
    }
}