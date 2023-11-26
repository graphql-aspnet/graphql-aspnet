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
    using GraphQL.AspNet.Engine;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Internal;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.Generation.TypeMakers;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.Common.CommonHelpers;
    using GraphQL.AspNet.Tests.CommonHelpers;
    using GraphQL.AspNet.Tests.Engine.TypeMakers.TestData;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Schemas.Generation.TypeMakers.TestData;
    using NUnit.Framework;

    [TestFixture]
    public class ObjectGraphTypeMakerTests : GraphTypeMakerTestBase
    {
        [Test]
        public void Object_CreateGraphType_WithSelfReferencingObject_ParsesAsExpected()
        {
            // ensure no stack overflows occur by attempting to create types of types
            // from self references
            // config says properties DO require declaration, override on type says it does not
            var template = GraphQLTemplateHelper.CreateGraphTypeTemplate<SelfReferencingObject>(TypeKind.OBJECT) as IObjectGraphTypeTemplate;
            var result = this.MakeGraphType(typeof(SelfReferencingObject), TypeKind.OBJECT);

            var inputType = result.GraphType as IObjectGraphType;
            Assert.IsNotNull(inputType);
            Assert.AreEqual(template.Name, inputType.Name);
            Assert.IsNotNull(inputType.Fields.FirstOrDefault(x => x.Name == nameof(SelfReferencingObject.Name)));
            Assert.IsNotNull(inputType.Fields.FirstOrDefault(x => x.Name == nameof(SelfReferencingObject.Parent)));
        }

        [Test]
        public void Object_CreateGraphType_ParsesCorrectly()
        {
            var server = new TestServerBuilder(TestOptions.UseCodeDeclaredNames).Build();
            var template = GraphQLTemplateHelper.CreateGraphTypeTemplate<TypeCreationItem>(TypeKind.OBJECT);

            var objectGraphType = new GraphTypeMakerFactory(server.Schema)
                .CreateTypeMaker(typeof(TypeCreationItem))
                .CreateGraphType(template).GraphType as IObjectGraphType;

            Assert.IsNotNull(objectGraphType);
            Assert.AreEqual(nameof(TypeCreationItem), objectGraphType.Name);
            Assert.AreEqual(template.Description, objectGraphType.Description);
            Assert.AreEqual(TypeKind.OBJECT, objectGraphType.Kind);

            // __typename, Method1, Method2, Prop1
            Assert.AreEqual(4, objectGraphType.Fields.Count);

            var method1 = objectGraphType.Fields.FirstOrDefault(x => x.Name == nameof(TypeCreationItem.Method1));

            Assert.IsNotNull(method1);
            Assert.AreEqual(objectGraphType, method1.Parent);
            CollectionAssert.AreEqual(new MetaGraphTypes[] { MetaGraphTypes.IsNotNull }, method1.TypeExpression.Wrappers); // double cant return as null
            Assert.AreEqual(Constants.ScalarNames.DOUBLE, method1.TypeExpression.TypeName);
            Assert.AreEqual(3, method1.Arguments.Count);

            var arg1 = method1.Arguments["arg1"];
            var arg2 = method1.Arguments["arg2"];
            var arg3 = method1.Arguments["arg3"];

            // string is nullable by deafult and no alterations exist on the declaration
            Assert.IsNotNull(arg1);
            Assert.AreEqual(typeof(string), arg1.ObjectType);
            CollectionAssert.AreEqual(GraphTypeExpression.SingleItem, arg1.TypeExpression.Wrappers);
            Assert.IsEmpty(arg1.TypeExpression.Wrappers);
            Assert.AreEqual(null, arg1.DefaultValue);

            // int is "not null" by default
            Assert.IsNotNull(arg2);
            Assert.AreEqual(typeof(int), arg2.ObjectType);
            CollectionAssert.AreEqual(GraphTypeExpression.RequiredSingleItem, arg2.TypeExpression.Wrappers);
            Assert.AreEqual(null, arg2.DefaultValue);

            // even though a default arg is declared, int is still "not null" by default
            Assert.IsNotNull(arg3);
            Assert.AreEqual(typeof(int), arg3.ObjectType);
            CollectionAssert.AreEqual(GraphTypeExpression.RequiredSingleItem, arg3.TypeExpression.Wrappers);
            Assert.AreEqual(5, arg3.DefaultValue);

            var method2 = objectGraphType.Fields.FirstOrDefault(x => x.Name == nameof(TypeCreationItem.Method2));
            Assert.IsNotNull(method2);
            Assert.IsEmpty(method2.TypeExpression.Wrappers);
            Assert.AreEqual(nameof(TwoPropertyObject), method2.TypeExpression.TypeName);
            Assert.AreEqual(objectGraphType, method2.Parent);

            arg1 = method2.Arguments["arg1"];
            arg2 = method2.Arguments["arg2"];

            Assert.IsNotNull(arg1);
            Assert.AreEqual(typeof(long), arg1.ObjectType);
            CollectionAssert.AreEqual(new MetaGraphTypes[] { MetaGraphTypes.IsNotNull }, arg1.TypeExpression.Wrappers);
            Assert.AreEqual(null, arg1.DefaultValue);

            // is a nullable<T> type therefor can be null even without a supplied value
            Assert.IsNotNull(arg2);
            Assert.AreEqual(typeof(decimal), arg2.ObjectType);
            Assert.IsEmpty(arg2.TypeExpression.Wrappers);
            Assert.AreEqual(null, arg2.DefaultValue);

            var prop1 = objectGraphType.Fields.FirstOrDefault(x => x.Name == nameof(TypeCreationItem.Prop1));
            Assert.IsNotNull(prop1);
            Assert.IsEmpty(prop1.TypeExpression.Wrappers);
            Assert.AreEqual(Constants.ScalarNames.STRING, prop1.TypeExpression.TypeName);
            Assert.AreEqual(objectGraphType, prop1.Parent);
        }

        [Test]
        public void CreateGraphType_WhenMethodDelcarationIsRequired_DoesNotIncludeUndeclaredMethods()
        {
            var objectGraphType = this.MakeGraphType(typeof(TypeWithUndeclaredFields), TypeKind.OBJECT, TemplateDeclarationRequirements.Method).GraphType as IObjectGraphType;

            Assert.IsNotNull(objectGraphType);
            Assert.IsTrue(objectGraphType.Fields.Any(x => x.Name == nameof(TypeWithUndeclaredFields.DeclaredMethod)));
            Assert.IsFalse(objectGraphType.Fields.Any(x => x.Name == nameof(TypeWithUndeclaredFields.UndeclaredMethod)));
        }

        [Test]
        public void CreateGraphType_WhenMethodDelcarationIsNotRequired_DoesIncludeUndeclaredMethods()
        {
            var objectGraphType = this.MakeGraphType(typeof(TypeWithUndeclaredFields), TypeKind.OBJECT, TemplateDeclarationRequirements.None).GraphType as IObjectGraphType;

            Assert.IsNotNull(objectGraphType);
            Assert.IsTrue(objectGraphType.Fields.Any(x => x.Name == nameof(TypeWithUndeclaredFields.DeclaredMethod)));
            Assert.IsTrue(objectGraphType.Fields.Any(x => x.Name == nameof(TypeWithUndeclaredFields.UndeclaredMethod)));
        }

        [Test]
        public void CreateGraphType_AsObject_WhenPropertyDelcarationIsRequired_DoesNotIncludeUndeclaredProperties()
        {
            var objectGraphType = this.MakeGraphType(typeof(TypeWithUndeclaredFields), TypeKind.OBJECT, TemplateDeclarationRequirements.Property).GraphType as IObjectGraphType;

            Assert.IsNotNull(objectGraphType);
            Assert.IsTrue(objectGraphType.Fields.Any(x => x.Name == nameof(TypeWithUndeclaredFields.DeclaredProperty)));
            Assert.IsFalse(objectGraphType.Fields.Any(x => x.Name == nameof(TypeWithUndeclaredFields.UndeclaredProperty)));
        }

        [Test]
        public void CreateGraphType_AsObject_WhenPropertyDelcarationIsNotRequired_DoesIncludeUndeclaredProperties()
        {
            var objectGraphType = this.MakeGraphType(typeof(TypeWithUndeclaredFields), TypeKind.OBJECT, TemplateDeclarationRequirements.None).GraphType as IObjectGraphType;

            Assert.IsNotNull(objectGraphType);
            Assert.IsTrue(objectGraphType.Fields.Any(x => x.Name == nameof(TypeWithUndeclaredFields.DeclaredProperty)));
            Assert.IsTrue(objectGraphType.Fields.Any(x => x.Name == nameof(TypeWithUndeclaredFields.UndeclaredProperty)));
        }

        [Test]
        public void CreateGraphType_AsObject_WhenMethodDelcarationIsRequired_ButWithGraphTypeOverride_DoesNotIncludeUndeclaredMethods()
        {
            var objectGraphType = this.MakeGraphType(typeof(TypeWithUndeclaredFieldsWithOverride), TypeKind.OBJECT, TemplateDeclarationRequirements.None).GraphType as IObjectGraphType;

            Assert.IsNotNull(objectGraphType);
            Assert.IsTrue(objectGraphType.Fields.Any(x => x.Name == nameof(TypeWithUndeclaredFieldsWithOverride.DeclaredMethod)));
            Assert.IsFalse(objectGraphType.Fields.Any(x => x.Name == nameof(TypeWithUndeclaredFieldsWithOverride.UndeclaredMethod)));
        }

        [Test]
        public void CreateGraphType_AsObject_WhenMethodDelcarationIsNotRequired_ButWithGraphTypeOverride_DoesIncludeUndeclaredMethods()
        {
            var objectGraphType = this.MakeGraphType(typeof(TypeWithUndeclaredFieldsWithOverrideNone), TypeKind.OBJECT, TemplateDeclarationRequirements.Method).GraphType as IObjectGraphType;

            Assert.IsNotNull(objectGraphType);
            Assert.IsTrue(objectGraphType.Fields.Any(x => x.Name == nameof(TypeWithUndeclaredFieldsWithOverrideNone.DeclaredMethod)));
            Assert.IsTrue(objectGraphType.Fields.Any(x => x.Name == nameof(TypeWithUndeclaredFieldsWithOverrideNone.UndeclaredMethod)));
        }

        [Test]
        public void CreateGraphType_AsObject_WhenPropertyDelcarationIsRequired_ButWithGraphTypeOverride_DoesNotIncludeUndeclaredProperties()
        {
            var objectGraphType = this.MakeGraphType(typeof(TypeWithUndeclaredFieldsWithOverride), TypeKind.OBJECT, TemplateDeclarationRequirements.None).GraphType as IObjectGraphType;

            Assert.IsNotNull(objectGraphType);
            Assert.IsTrue(objectGraphType.Fields.Any(x => x.Name == nameof(TypeWithUndeclaredFields.DeclaredProperty)));
            Assert.IsFalse(objectGraphType.Fields.Any(x => x.Name == nameof(TypeWithUndeclaredFields.UndeclaredProperty)));
        }

        [Test]
        public void CreateGraphType_AsObject_WhenPropertyDelcarationIsNotRequired_ButWithGraphTypeOverride_DoesIncludeUndeclaredProperties()
        {
            var objectGraphType = this.MakeGraphType(typeof(TypeWithUndeclaredFieldsWithOverrideNone), TypeKind.OBJECT, TemplateDeclarationRequirements.Property).GraphType as IObjectGraphType;

            Assert.IsNotNull(objectGraphType);
            Assert.IsTrue(objectGraphType.Fields.Any(x => x.Name == nameof(TypeWithUndeclaredFields.DeclaredProperty)));
            Assert.IsTrue(objectGraphType.Fields.Any(x => x.Name == nameof(TypeWithUndeclaredFields.UndeclaredProperty)));
        }

        [Test]
        public void CreateGraphType_AsObject_WithSelfReferencingObject_ParsesAsExpected()
        {
            // ensure no stack overflows occur by attempting to create types of types
            // from self references
            var template = GraphQLTemplateHelper.CreateObjectTemplate<SelfReferencingObject>();
            var objectGraphType = this.MakeGraphType(typeof(SelfReferencingObject), TypeKind.OBJECT).GraphType as IObjectGraphType;

            Assert.IsNotNull(objectGraphType);
            Assert.AreEqual(template.Name, objectGraphType.Name);
            Assert.IsNotNull(objectGraphType.Fields.FirstOrDefault(x => x.Name == nameof(SelfReferencingObject.Name)));
            Assert.IsNotNull(objectGraphType.Fields.FirstOrDefault(x => x.Name == nameof(SelfReferencingObject.Parent)));
        }

        [Test]
        public void CreateGraphType_DirectivesAreApplied()
        {
            var result = this.MakeGraphType(typeof(ObjectDirectiveTestItem), TypeKind.OBJECT);
            var objectType = result.GraphType as IObjectGraphType;

            Assert.IsNotNull(objectType);
            Assert.AreEqual(1, objectType.AppliedDirectives.Count);
            Assert.AreEqual(objectType, objectType.AppliedDirectives.Parent);

            var appliedDirective = objectType.AppliedDirectives.FirstOrDefault();
            Assert.IsNotNull(appliedDirective);
            Assert.AreEqual(typeof(DirectiveWithArgs), appliedDirective.DirectiveType);
            CollectionAssert.AreEqual(new object[] { 12, "object directive" }, appliedDirective.ArgumentValues);
        }

        [Test]
        public void CreateGraphType_AsObject_WhenMethodOnBaseObjectIsNotExplicitlyDeclared_WhenExplicitDeclarationisRequired_IsNotIncluded()
        {
            var result = this.MakeGraphType(
                typeof(ObjectWithInheritedUndeclaredMethodField),
                TypeKind.OBJECT,
                TemplateDeclarationRequirements.Method);

            var objectType = result.GraphType as IObjectGraphType;

            // inherited, undeclared method field should not be counted
            Assert.IsNotNull(objectType);

            // property field + __typename
            Assert.AreEqual(2, objectType.Fields.Count);
            Assert.IsTrue(objectType.Fields.Any(x => string.Equals(x.Name, nameof(ObjectWithInheritedUndeclaredMethodField.FieldOnClass), System.StringComparison.OrdinalIgnoreCase)));
            Assert.IsTrue(objectType.Fields.Any(x => string.Equals(x.Name, Constants.ReservedNames.TYPENAME_FIELD)));
        }

        [Test]
        public void CreateGraphType_AsObject_WhenMethodOnBaseObjectIsNotExplicitlyDeclared_WhenExplicitDeclarationIsNotRequired_IsIncluded()
        {
            var result = this.MakeGraphType(
                typeof(ObjectWithInheritedUndeclaredMethodField),
                TypeKind.OBJECT,
                TemplateDeclarationRequirements.None);

            var objectType = result.GraphType as IObjectGraphType;

            // inherited, undeclared method field should be counted
            Assert.IsNotNull(objectType);

            // property field + base field + __typename
            Assert.AreEqual(3, objectType.Fields.Count);
            Assert.IsTrue(objectType.Fields.Any(x => string.Equals(x.Name, nameof(ObjectWithInheritedUndeclaredMethodField.FieldOnClass), System.StringComparison.OrdinalIgnoreCase)));
            Assert.IsTrue(objectType.Fields.Any(x => string.Equals(x.Name, nameof(ObjectWithUndeclaredMethodField.FieldOnBaseObject), System.StringComparison.OrdinalIgnoreCase)));
            Assert.IsTrue(objectType.Fields.Any(x => string.Equals(x.Name, Constants.ReservedNames.TYPENAME_FIELD)));
        }

        [Test]
        public void CreateGraphType_AsObject_WhenMethodOnBaseObjectIsExplicitlyDeclared_IsIncluded()
        {
            var result = this.MakeGraphType(
                typeof(ObjectWithInheritedDeclaredMethodField),
                TypeKind.OBJECT,
                TemplateDeclarationRequirements.None);

            var objectType = result.GraphType as IObjectGraphType;

            // inherited and declared method field should not be counted
            Assert.IsNotNull(objectType);

            // property field + base field + __typename
            Assert.AreEqual(3, objectType.Fields.Count);
            Assert.IsTrue(objectType.Fields.Any(x => string.Equals(x.Name, nameof(ObjectWithInheritedDeclaredMethodField.FieldOnClass), System.StringComparison.OrdinalIgnoreCase)));
            Assert.IsTrue(objectType.Fields.Any(x => string.Equals(x.Name, nameof(ObjectWithUndeclaredMethodField.FieldOnBaseObject), System.StringComparison.OrdinalIgnoreCase)));
            Assert.IsTrue(objectType.Fields.Any(x => string.Equals(x.Name, Constants.ReservedNames.TYPENAME_FIELD)));
        }

        [Test]
        public void InternalName_OnObjectGraphType_IsRendered()
        {
            var result = this.MakeGraphType(
                typeof(ObjectWithInternalName),
                TypeKind.OBJECT,
                TemplateDeclarationRequirements.None);

            var objectType = result.GraphType as IObjectGraphType;

            Assert.AreEqual("Object_Internal_Name", objectType.InternalName);
        }

        [Test]
        public void CreateGraphType_AsObject_WhenOverloadedMethodIncludesOnce_IsCorrectlyCreated()
        {
            var result = this.MakeGraphType(
                typeof(ObjectWithOverloadedMethodFields),
                TypeKind.OBJECT,
                TemplateDeclarationRequirements.Default);

            var objectType = result.GraphType as IObjectGraphType;

            // inherited and declared method field should not be counted
            Assert.IsNotNull(objectType);

            // declared field method + __typename
            Assert.AreEqual(2, objectType.Fields.Count);
            Assert.IsTrue(objectType.Fields.Any(x => string.Equals(x.Name, nameof(ObjectWithOverloadedMethodFields.Field1), System.StringComparison.OrdinalIgnoreCase)));
            Assert.IsTrue(objectType.Fields.Any(x => string.Equals(x.Name, Constants.ReservedNames.TYPENAME_FIELD)));
        }

        [Test]
        public void CreateGraphType_AsObject_WhenOverloadedMethodIncludesTwice_ThrowsError()
        {
            var ex = Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                var result = this.MakeGraphType(
                    typeof(ObjectWithOverloadedMethodFields),
                    TypeKind.OBJECT,
                    TemplateDeclarationRequirements.None);
            });
        }

        [Test]
        public void CreateGraphType_AsObject_WithNoFields_ThrowsError()
        {
            var ex = Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                var result = this.MakeGraphType(
                    typeof(ObjectWithNoFields),
                    TypeKind.OBJECT,
                    TemplateDeclarationRequirements.None);
            });
        }
    }
}