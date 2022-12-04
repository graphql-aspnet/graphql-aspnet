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
    using GraphQL.AspNet.Engine;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.Engine.TypeMakers.TestData;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;
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
            var template = TemplateHelper.CreateGraphTypeTemplate<SelfReferencingObject>(TypeKind.OBJECT) as IObjectGraphTypeTemplate;
            var result = this.MakeGraphType(typeof(SelfReferencingObject), TypeKind.OBJECT);

            var inputType = result.GraphType as IObjectGraphType;
            Assert.IsNotNull(inputType);
            Assert.AreEqual(template.Name, inputType.Name);
            Assert.IsNotNull(inputType.Fields.FirstOrDefault(x => x.Name == nameof(SelfReferencingObject.Name)));
            Assert.IsNotNull(inputType.Fields.FirstOrDefault(x => x.Name == nameof(SelfReferencingObject.Parent)));
        }

        [Test]
        public void Interface_CreateGraphType_ParsesCorrectly()
        {
            var server = new TestServerBuilder(TestOptions.UseCodeDeclaredNames).Build();
            var template = TemplateHelper.CreateGraphTypeTemplate<TypeCreationItem>();
            var typeMaker = new DefaultGraphTypeMakerProvider();

            var objectGraphType = typeMaker.CreateTypeMaker(server.Schema, TypeKind.OBJECT).CreateGraphType(typeof(TypeCreationItem)).GraphType as IObjectGraphType;

            Assert.IsNotNull(objectGraphType);
            Assert.AreEqual(nameof(TypeCreationItem), objectGraphType.Name);
            Assert.AreEqual(template.Description, objectGraphType.Description);
            Assert.AreEqual(TypeKind.OBJECT, objectGraphType.Kind);

            // __typename, Method1, Method2, Prop1
            Assert.AreEqual(4, objectGraphType.Fields.Count);

            var method1 = objectGraphType.Fields.FirstOrDefault(x => x.Name == nameof(TypeCreationItem.Method1));

            Assert.IsNotNull(method1);
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
        }

        [Test]
        public void CreateGraphType_WhenMethodDelcarationIsRequired_DoesNotIncludeUndeclaredMethods()
        {
            var objectGraphType = this.MakeGraphType(typeof(TypeWithUndeclaredFields), TypeKind.OBJECT, TemplateDeclarationRequirements.Method).GraphType as IObjectGraphType;

            Assert.IsNotNull(objectGraphType);
            Assert.IsTrue(Enumerable.Any(objectGraphType.Fields, x => x.Name == nameof(TypeWithUndeclaredFields.DeclaredMethod)));
            Assert.IsFalse(Enumerable.Any(objectGraphType.Fields, x => x.Name == nameof(TypeWithUndeclaredFields.UndeclaredMethod)));
        }

        [Test]
        public void CreateGraphType_WhenMethodDelcarationIsNotRequired_DoesIncludeUndeclaredMethods()
        {
            var objectGraphType = this.MakeGraphType(typeof(TypeWithUndeclaredFields), TypeKind.OBJECT, TemplateDeclarationRequirements.None).GraphType as IObjectGraphType;

            Assert.IsNotNull(objectGraphType);
            Assert.IsTrue(Enumerable.Any(objectGraphType.Fields, x => x.Name == nameof(TypeWithUndeclaredFields.DeclaredMethod)));
            Assert.IsTrue(Enumerable.Any(objectGraphType.Fields, x => x.Name == nameof(TypeWithUndeclaredFields.UndeclaredMethod)));
        }

        [Test]
        public void CreateGraphType_AsObject_WhenPropertyDelcarationIsRequired_DoesNotIncludeUndeclaredProperties()
        {
            var objectGraphType = this.MakeGraphType(typeof(TypeWithUndeclaredFields), TypeKind.OBJECT, TemplateDeclarationRequirements.Property).GraphType as IObjectGraphType;

            Assert.IsNotNull(objectGraphType);
            Assert.IsTrue(Enumerable.Any(objectGraphType.Fields, x => x.Name == nameof(TypeWithUndeclaredFields.DeclaredProperty)));
            Assert.IsFalse(Enumerable.Any(objectGraphType.Fields, x => x.Name == nameof(TypeWithUndeclaredFields.UndeclaredProperty)));
        }

        [Test]
        public void CreateGraphType_AsObject_WhenPropertyDelcarationIsNotRequired_DoesIncludeUndeclaredProperties()
        {
            var objectGraphType = this.MakeGraphType(typeof(TypeWithUndeclaredFields), TypeKind.OBJECT, TemplateDeclarationRequirements.None).GraphType as IObjectGraphType;

            Assert.IsNotNull(objectGraphType);
            Assert.IsTrue(Enumerable.Any(objectGraphType.Fields, x => x.Name == nameof(TypeWithUndeclaredFields.DeclaredProperty)));
            Assert.IsTrue(Enumerable.Any(objectGraphType.Fields, x => x.Name == nameof(TypeWithUndeclaredFields.UndeclaredProperty)));
        }

        [Test]
        public void CreateGraphType_AsObject_WhenMethodDelcarationIsRequired_ButWithGraphTypeOverride_DoesNotIncludeUndeclaredMethods()
        {
            var objectGraphType = this.MakeGraphType(typeof(TypeWithUndeclaredFieldsWithOverride), TypeKind.OBJECT, TemplateDeclarationRequirements.None).GraphType as IObjectGraphType;

            Assert.IsNotNull(objectGraphType);
            Assert.IsTrue(Enumerable.Any(objectGraphType.Fields, x => x.Name == nameof(TypeWithUndeclaredFieldsWithOverride.DeclaredMethod)));
            Assert.IsFalse(Enumerable.Any(objectGraphType.Fields, x => x.Name == nameof(TypeWithUndeclaredFieldsWithOverride.UndeclaredMethod)));
        }

        [Test]
        public void CreateGraphType_AsObject_WhenMethodDelcarationIsNotRequired_ButWithGraphTypeOverride_DoesIncludeUndeclaredMethods()
        {
            var objectGraphType = this.MakeGraphType(typeof(TypeWithUndeclaredFieldsWithOverrideNone), TypeKind.OBJECT, TemplateDeclarationRequirements.Method).GraphType as IObjectGraphType;

            Assert.IsNotNull(objectGraphType);
            Assert.IsTrue(Enumerable.Any(objectGraphType.Fields, x => x.Name == nameof(TypeWithUndeclaredFieldsWithOverrideNone.DeclaredMethod)));
            Assert.IsTrue(Enumerable.Any(objectGraphType.Fields, x => x.Name == nameof(TypeWithUndeclaredFieldsWithOverrideNone.UndeclaredMethod)));
        }

        [Test]
        public void CreateGraphType_AsObject_WhenPropertyDelcarationIsRequired_ButWithGraphTypeOverride_DoesNotIncludeUndeclaredProperties()
        {
            var objectGraphType = this.MakeGraphType(typeof(TypeWithUndeclaredFieldsWithOverride), TypeKind.OBJECT, TemplateDeclarationRequirements.None).GraphType as IObjectGraphType;

            Assert.IsNotNull(objectGraphType);
            Assert.IsTrue(Enumerable.Any(objectGraphType.Fields, x => x.Name == nameof(TypeWithUndeclaredFields.DeclaredProperty)));
            Assert.IsFalse(Enumerable.Any(objectGraphType.Fields, x => x.Name == nameof(TypeWithUndeclaredFields.UndeclaredProperty)));
        }

        [Test]
        public void CreateGraphType_AsObject_WhenPropertyDelcarationIsNotRequired_ButWithGraphTypeOverride_DoesIncludeUndeclaredProperties()
        {
            var objectGraphType = this.MakeGraphType(typeof(TypeWithUndeclaredFieldsWithOverrideNone), TypeKind.OBJECT, TemplateDeclarationRequirements.Property).GraphType as IObjectGraphType;

            Assert.IsNotNull(objectGraphType);
            Assert.IsTrue(Enumerable.Any(objectGraphType.Fields, x => x.Name == nameof(TypeWithUndeclaredFields.DeclaredProperty)));
            Assert.IsTrue(Enumerable.Any(objectGraphType.Fields, x => x.Name == nameof(TypeWithUndeclaredFields.UndeclaredProperty)));
        }

        [Test]
        public void CreateGraphType_AsObject_WithSelfReferencingObject_ParsesAsExpected()
        {
            // ensure no stack overflows occur by attempting to create types of types
            // from self references
            var template = TemplateHelper.CreateObjectTemplate<SelfReferencingObject>();
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

            var appliedDirective = Enumerable.FirstOrDefault(objectType.AppliedDirectives);
            Assert.IsNotNull(appliedDirective);
            Assert.AreEqual(typeof(DirectiveWithArgs), appliedDirective.DirectiveType);
            CollectionAssert.AreEqual(new object[] { 12, "object directive" }, appliedDirective.ArgumentValues);
        }
    }
}