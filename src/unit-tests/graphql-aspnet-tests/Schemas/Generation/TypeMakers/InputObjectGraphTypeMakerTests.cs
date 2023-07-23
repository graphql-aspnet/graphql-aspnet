// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Schemas.Generation.TypeMakers
{
    using System.Linq;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.Generation.TypeTemplates;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.Common.CommonHelpers;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Schemas.Generation.TypeMakers.TestData;
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
            Assert.AreEqual(2, typeResult.DependentTypes.Count());
            Assert.IsTrue(typeResult.DependentTypes.Any(x => x.Type == typeof(OneMarkedProperty)));
            Assert.IsTrue(typeResult.DependentTypes.Any(x => x.Type == typeof(string)));
        }

        [Test]
        public void InputObject_CreateGraphType_OnlyPropertiesAreRead()
        {
            var template = GraphQLTemplateHelper.CreateInputObjectTemplate<TypeCreationItem>();

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
            Assert.IsFalse(prop1.IsRequired);
            Assert.IsTrue(prop1.HasDefaultValue);
            Assert.IsNull(prop1.DefaultValue);
        }

        [Test]
        public void InputObject_CreateGraphType_DefaultFieldValuesAreParsed()
        {
            var template = GraphQLTemplateHelper.CreateInputObjectTemplate<TypeCreationItem>();

            var result = this.MakeGraphType(typeof(InputTestObjectWithDefaultFieldValues), TypeKind.INPUT_OBJECT);
            var objectGraphType = result.GraphType as IInputObjectGraphType;

            Assert.IsNotNull(objectGraphType);
            Assert.AreEqual($"Input_{nameof(InputTestObjectWithDefaultFieldValues)}", objectGraphType.Name);
            Assert.AreEqual(template.Description, objectGraphType.Description);
            Assert.AreEqual(TypeKind.INPUT_OBJECT, objectGraphType.Kind);

            // Prop1-4 (there is no __typename on input objects)
            Assert.AreEqual(4, objectGraphType.Fields.Count);

            var prop1 = objectGraphType.Fields.FirstOrDefault(x => x.Name == nameof(InputTestObjectWithDefaultFieldValues.Prop1));
            Assert.IsNotNull(prop1);
            Assert.AreEqual("Int!", prop1.TypeExpression.ToString());
            Assert.IsFalse(prop1.IsRequired);
            Assert.IsTrue(prop1.HasDefaultValue);
            Assert.AreEqual(34, prop1.DefaultValue);

            var prop2 = objectGraphType.Fields.FirstOrDefault(x => x.Name == nameof(InputTestObjectWithDefaultFieldValues.Prop2));
            Assert.IsNotNull(prop2);
            Assert.AreEqual("String", prop2.TypeExpression.ToString());
            Assert.IsFalse(prop2.IsRequired);
            Assert.IsTrue(prop2.HasDefaultValue);
            Assert.AreEqual("default prop2 string", prop2.DefaultValue);

            var prop3 = objectGraphType.Fields.FirstOrDefault(x => x.Name == nameof(InputTestObjectWithDefaultFieldValues.Prop3));
            Assert.IsNotNull(prop3);
            Assert.AreEqual("Int!", prop3.TypeExpression.ToString());
            Assert.IsTrue(prop3.IsRequired);
            Assert.IsFalse(prop3.HasDefaultValue);

            // even though prop3 is an int, defaultValue should still be
            // null since the field is marked required (has no default value)
            Assert.IsNull(prop3.DefaultValue);

            var prop4 = objectGraphType.Fields.FirstOrDefault(x => x.Name == nameof(InputTestObjectWithDefaultFieldValues.Prop4));
            Assert.IsNotNull(prop4);
            Assert.AreEqual("Input_TwoPropertyObject", prop4.TypeExpression.ToString());
            Assert.IsFalse(prop4.IsRequired);
            Assert.IsTrue(prop4.HasDefaultValue);

            // even though its int, defaultValue should still be
            // null since the field is marked required
            var prop4DefaultObj = prop4.DefaultValue as TwoPropertyObject;
            Assert.IsNotNull(prop4DefaultObj);
            Assert.AreEqual("twoPropString1", prop4DefaultObj.Property1);
            Assert.AreEqual(99, prop4DefaultObj.Property2);
        }

        [Test]
        public void InputObject_CreateGraphType_WhenPropertyDelcarationIsRequired_DoesNotIncludeUndeclaredProperties()
        {
            var result = this.MakeGraphType(typeof(TypeWithUndeclaredFields), TypeKind.INPUT_OBJECT, TemplateDeclarationRequirements.Property);
            var inputType = result.GraphType as IInputObjectGraphType;

            Assert.IsNotNull(inputType);
            Assert.IsTrue(inputType.Fields.Any(x => x.Name == nameof(TypeWithUndeclaredFields.DeclaredProperty)));
            Assert.IsFalse(inputType.Fields.Any(x => x.Name == nameof(TypeWithUndeclaredFields.UndeclaredProperty)));
        }

        [Test]
        public void CreateGraphType_WhenPropertyDelcarationIsNotRequired_DoesIncludeUndeclaredProperties()
        {
            var result = this.MakeGraphType(typeof(TypeWithUndeclaredFields), TypeKind.INPUT_OBJECT, TemplateDeclarationRequirements.None);
            var inputType = result.GraphType as IInputObjectGraphType;

            Assert.IsNotNull(inputType);
            Assert.IsTrue(inputType.Fields.Any(x => x.Name == nameof(TypeWithUndeclaredFields.DeclaredProperty)));
            Assert.IsTrue(inputType.Fields.Any(x => x.Name == nameof(TypeWithUndeclaredFields.UndeclaredProperty)));
        }

        [Test]
        public void CreateGraphType_WhenPropertyDelcarationIsRequired_ButWithGraphTypeOverride_DoesNotIncludeUndeclaredProperties()
        {
            // config says properties dont require declaration, override on type says it does
            var result = this.MakeGraphType(typeof(TypeWithUndeclaredFieldsWithOverride), TypeKind.INPUT_OBJECT, TemplateDeclarationRequirements.None);
            var objectGraphType = result.GraphType as IInputObjectGraphType;

            Assert.IsNotNull(objectGraphType);
            Assert.IsTrue(objectGraphType.Fields.Any(x => x.Name == nameof(TypeWithUndeclaredFieldsWithOverride.DeclaredProperty)));
            Assert.IsFalse(objectGraphType.Fields.Any(x => x.Name == nameof(TypeWithUndeclaredFieldsWithOverride.UndeclaredProperty)));
        }

        [Test]
        public void InputObject_CreateGraphType_WhenPropertyDelcarationIsNotRequired_ButWithGraphTypeOverride_DoesIncludeUndeclaredProperties()
        {
            // config says properties DO require declaration, override on type says it does not
            var result = this.MakeGraphType(typeof(TypeWithUndeclaredFieldsWithOverrideNone), TypeKind.INPUT_OBJECT, TemplateDeclarationRequirements.Property);
            var inputType = result.GraphType as IInputObjectGraphType;

            Assert.IsNotNull(inputType);
            Assert.IsTrue(inputType.Fields.Any(x => x.Name == nameof(TypeWithUndeclaredFieldsWithOverrideNone.DeclaredProperty)));
            Assert.IsTrue(inputType.Fields.Any(x => x.Name == nameof(TypeWithUndeclaredFieldsWithOverrideNone.UndeclaredProperty)));
        }

        [Test]
        public void InputObject_CreateGraphType_WithSelfReferencingObject_ParsesAsExpected()
        {
            // ensure no stack overflows occur by attempting to create types of types
            // from self references
            // config says properties DO require declaration, override on type says it does not
            var template = GraphQLTemplateHelper.CreateInputObjectTemplate<SelfReferencingObject>();
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

            var appliedDirective = inputType.AppliedDirectives.FirstOrDefault();
            Assert.IsNotNull(appliedDirective);
            Assert.AreEqual(typeof(DirectiveWithArgs), appliedDirective.DirectiveType);
            CollectionAssert.AreEqual(new object[] { 44, "input arg" }, appliedDirective.ArgumentValues);
        }

        [Test]
        public void InputObject_WithInternalName_HasInternalNameSet()
        {
            // config says properties DO require declaration, override on type says it does not
            var result = this.MakeGraphType(typeof(InputTestObjectWithInternalName), TypeKind.INPUT_OBJECT);
            var inputType = result.GraphType as IInputObjectGraphType;

            Assert.AreEqual("InputObjectInternalName", inputType.InternalName);
        }
    }
}