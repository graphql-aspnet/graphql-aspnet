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
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Internal;
    using GraphQL.AspNet.Internal.TypeTemplates;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.Engine.TypeMakers.TestData;
    using GraphQL.AspNet.Tests.Framework;
    using NUnit.Framework;

    [TestFixture]
    public class InputObjectGraphTypeMakerTests : GraphTypeMakerTestBase
    {
        [Test]
        public void Parse_FieldAndDependencies_SetCorrectly()
        {
            var server = new TestServerBuilder().Build();

            var typeResult = this.MakeGraphType(typeof(ComplexInputObject), TypeKind.INPUT_OBJECT);
            var graphType = typeResult.GraphType as IInputObjectGraphType;

            Assert.IsNotNull(graphType);

            var inputGraphTypeName = GraphTypeNames.ParseName(typeof(OneMarkedProperty), TypeKind.INPUT_OBJECT);
            Assert.AreEqual(2, graphType.Fields.Count);

            var field = graphType.Fields.FirstOrDefault(x => x.TypeExpression.TypeName == inputGraphTypeName);
            Assert.IsNotNull(field);

            // string, OneMarkedProperty
            Assert.AreEqual(2, Enumerable.Count<DependentType>(typeResult.DependentTypes));
            Assert.IsTrue(Enumerable.Any<DependentType>(typeResult.DependentTypes, x => x.Type == typeof(OneMarkedProperty) && x.ExpectedKind == TypeKind.INPUT_OBJECT));
            Assert.IsTrue(Enumerable.Any<DependentType>(typeResult.DependentTypes, x => x.Type == typeof(string) && x.ExpectedKind == TypeKind.SCALAR));
        }

        [Test]
        public void InputObject_CreateGraphType_OnlyPropertiesAreRead()
        {
            var template = TemplateHelper.CreateInputObjectTemplate<TypeCreationItem>();

            var result = this.MakeGraphType(typeof(TypeCreationItem), TypeKind.INPUT_OBJECT);
            var objectGraphType = result.GraphType as IInputObjectGraphType;

            Assert.IsNotNull(objectGraphType);
            Assert.AreEqual($"Input_{nameof(TypeCreationItem)}", objectGraphType.Name);
            Assert.AreEqual(template.Description, objectGraphType.Description);
            Assert.AreEqual(TypeKind.INPUT_OBJECT, objectGraphType.Kind);

            // Prop1  (there is no __typename on input objects like there is on its object counterpart)
            Assert.AreEqual(1, objectGraphType.Fields.Count);

            // Method1, Method2 should not be parsed on the type at all.
            var method1 = objectGraphType.Fields.FirstOrDefault(x => x.Name == nameof(TypeCreationItem.Method1));
            Assert.IsNull(method1);

            var method2 = objectGraphType.Fields.FirstOrDefault(x => x.Name == nameof(TypeCreationItem.Method2));
            Assert.IsNull(method2);

            var prop1 = objectGraphType.Fields.FirstOrDefault(x => x.Name == nameof(TypeCreationItem.Prop1));
            Assert.IsNotNull(prop1);
            Assert.AreEqual(0, prop1.TypeExpression.Wrappers.Length);
            Assert.AreEqual(Constants.ScalarNames.STRING, prop1.TypeExpression.TypeName);
        }

        [Test]
        public void InputObject_CreateGraphType_WhenPropertyDelcarationIsRequired_DoesNotIncludeUndeclaredProperties()
        {
            var result = this.MakeGraphType(typeof(TypeWithUndeclaredFields), TypeKind.INPUT_OBJECT, TemplateDeclarationRequirements.Property);
            var inputType = result.GraphType as IInputObjectGraphType;

            Assert.IsNotNull(inputType);
            Assert.IsTrue(Enumerable.Any(inputType.Fields, x => x.Name == nameof(TypeWithUndeclaredFields.DeclaredProperty)));
            Assert.IsFalse(Enumerable.Any(inputType.Fields, x => x.Name == nameof(TypeWithUndeclaredFields.UndeclaredProperty)));
        }

        [Test]
        public void CreateGraphType_WhenPropertyDelcarationIsNotRequired_DoesIncludeUndeclaredProperties()
        {
            var result = this.MakeGraphType(typeof(TypeWithUndeclaredFields), TypeKind.INPUT_OBJECT, TemplateDeclarationRequirements.None);
            var inputType = result.GraphType as IInputObjectGraphType;

            Assert.IsNotNull(inputType);
            Assert.IsTrue(Enumerable.Any(inputType.Fields, x => x.Name == nameof(TypeWithUndeclaredFields.DeclaredProperty)));
            Assert.IsTrue(Enumerable.Any(inputType.Fields, x => x.Name == nameof(TypeWithUndeclaredFields.UndeclaredProperty)));
        }

        [Test]
        public void CreateGraphType_WhenPropertyDelcarationIsRequired_ButWithGraphTypeOverride_DoesNotIncludeUndeclaredProperties()
        {
            // config says properties dont require declaration, override on type says it does
            var result = this.MakeGraphType(typeof(TypeWithUndeclaredFieldsWithOverride), TypeKind.INPUT_OBJECT, TemplateDeclarationRequirements.None);
            var objectGraphType = result.GraphType as IInputObjectGraphType;

            Assert.IsNotNull(objectGraphType);
            Assert.IsTrue(Enumerable.Any(objectGraphType.Fields, x => x.Name == nameof(TypeWithUndeclaredFieldsWithOverride.DeclaredProperty)));
            Assert.IsFalse(Enumerable.Any(objectGraphType.Fields, x => x.Name == nameof(TypeWithUndeclaredFieldsWithOverride.UndeclaredProperty)));
        }

        [Test]
        public void InputObject_CreateGraphType_WhenPropertyDelcarationIsNotRequired_ButWithGraphTypeOverride_DoesIncludeUndeclaredProperties()
        {
            // config says properties DO require declaration, override on type says it does not
            var result = this.MakeGraphType(typeof(TypeWithUndeclaredFieldsWithOverrideNone), TypeKind.INPUT_OBJECT, TemplateDeclarationRequirements.Property);
            var inputType = result.GraphType as IInputObjectGraphType;

            Assert.IsNotNull(inputType);
            Assert.IsTrue(Enumerable.Any(inputType.Fields, x => x.Name == nameof(TypeWithUndeclaredFieldsWithOverrideNone.DeclaredProperty)));
            Assert.IsTrue(Enumerable.Any(inputType.Fields, x => x.Name == nameof(TypeWithUndeclaredFieldsWithOverrideNone.UndeclaredProperty)));
        }

        [Test]
        public void InputObject_CreateGraphType_WithSelfReferencingObject_ParsesAsExpected()
        {
            // ensure no stack overflows occur by attempting to create types of types
            // from self references
            // config says properties DO require declaration, override on type says it does not
            var template = TemplateHelper.CreateInputObjectTemplate<SelfReferencingObject>();
            var result = this.MakeGraphType(typeof(SelfReferencingObject), TypeKind.INPUT_OBJECT);

            var inputType = result.GraphType as IInputObjectGraphType;
            Assert.IsNotNull(inputType);
            Assert.AreEqual(template.Name, inputType.Name);
            Assert.IsNotNull(inputType.Fields.FirstOrDefault(x => x.Name == nameof(SelfReferencingObject.Name)));
            Assert.IsNotNull(inputType.Fields.FirstOrDefault(x => x.Name == nameof(SelfReferencingObject.Parent)));
        }

        [Test]
        public void InputObject_CreateGraphType_DirectivesAreApplied()
        {
            // config says properties DO require declaration, override on type says it does not
            var result = this.MakeGraphType(typeof(InputTypeWithDirective), TypeKind.INPUT_OBJECT, TemplateDeclarationRequirements.Property);
            var inputType = result.GraphType as IInputObjectGraphType;

            Assert.IsNotNull(inputType);
            Assert.AreEqual(1, inputType.AppliedDirectives.Count);
            Assert.AreEqual(inputType, inputType.AppliedDirectives.Parent);

            var appliedDirective = Enumerable.FirstOrDefault(inputType.AppliedDirectives);
            Assert.IsNotNull(appliedDirective);
            Assert.AreEqual(typeof(DirectiveWithArgs), appliedDirective.DirectiveType);
            CollectionAssert.AreEqual(new object[] { 44, "input arg" }, appliedDirective.ArgumentValues);
        }
    }
}