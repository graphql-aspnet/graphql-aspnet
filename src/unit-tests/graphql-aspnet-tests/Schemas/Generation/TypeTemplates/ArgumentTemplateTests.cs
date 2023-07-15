// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Schemas.Generation.TypeTemplates
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Internal;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.Generation.TypeTemplates;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.CommonHelpers;
    using GraphQL.AspNet.Tests.Schemas.Generation.TypeTemplates.DirectiveTestData;
    using GraphQL.AspNet.Tests.Schemas.Generation.TypeTemplates.ParameterTestData;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class ArgumentTemplateTests
    {
        private GraphArgumentTemplate ExtractParameterTemplate(string paramName, out ParameterInfo paramInfo)
        {
            paramInfo = typeof(ParameterTestClass)
                .GetMethod(nameof(ParameterTestClass.TestMethod))
                .GetParameters()
                .FirstOrDefault(x => x.Name == paramName);

            var mockMethod = new Mock<IGraphFieldTemplateBase>();
            mockMethod.Setup(x => x.InternalFullName)
                .Returns($"{nameof(ParameterTestClass)}.{nameof(ParameterTestClass.TestMethod)}");
            mockMethod.Setup(x => x.ObjectType).Returns(typeof(ParameterTestClass));
            mockMethod.Setup(x => x.Arguments).Returns(new List<IGraphArgumentTemplate>());

            var route = new SchemaItemPath(SchemaItemPath.Join(
                SchemaItemCollections.Query,
                nameof(ParameterTestClass),
                nameof(ParameterTestClass.TestMethod)));
            mockMethod.Setup(x => x.Route).Returns(route);

            var argTemplate = new GraphArgumentTemplate(mockMethod.Object, paramInfo);
            argTemplate.Parse();
            argTemplate.ValidateOrThrow();

            return argTemplate;
        }

        [Test]
        public void StringParam_ParsesCorrectly()
        {
            var template = this.ExtractParameterTemplate("stringArg", out var paramInfo);
            Assert.AreEqual(paramInfo.Name, template.Name);
            Assert.AreEqual(paramInfo, template.Parameter);
            Assert.AreEqual(null, template.Description);
            Assert.IsEmpty((IEnumerable)template.TypeExpression.Wrappers);
            Assert.IsNull(template.DefaultValue);
            Assert.AreEqual($"{nameof(ParameterTestClass)}.{nameof(ParameterTestClass.TestMethod)}.stringArg", template.InternalFullName);
            Assert.AreEqual("stringArg", template.InternalName);
        }

        [Test]
        public void InvalidParamName_ThrowsException()
        {
            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                var template = this.ExtractParameterTemplate("__invalidGraphNameArg", out var paramInfo);
            });
        }

        [Test]
        public void ParamWithDescription_SetsCorrectly()
        {
            var template = this.ExtractParameterTemplate("argWithDescription", out var paramInfo);
            Assert.AreEqual("This argument has a description", template.Description);
        }

        [Test]
        public void InvalidNameWithOverride_SetsCorrectly()
        {
            var template = this.ExtractParameterTemplate("__invalidGraphNameWithOverride", out var paramInfo);
            Assert.AreEqual("validArgNameOveride", template.Name);
        }

        [Test]
        public void ValueTypeParam_SetsCorrectly()
        {
            var template = this.ExtractParameterTemplate("intArg", out var paramInfo);
            Assert.AreEqual("intArg", template.Name);
        }

        [Test]
        public void NullableTParam_WithNoDefaultValue_SetsFieldOptionsAsNotNull()
        {
            var template = this.ExtractParameterTemplate("nullableIntArg", out var paramInfo);
            Assert.IsEmpty((IEnumerable)template.TypeExpression.Wrappers);
        }

        [Test]
        public void ReferenceParam_ParsesCorrectly()
        {
            var template = this.ExtractParameterTemplate("objectArg", out var paramInfo);
            Assert.IsEmpty((IEnumerable)template.TypeExpression.Wrappers);
        }

        [Test]
        public void ReferenceParam_NotNull_ParsesCorrectly()
        {
            var template = this.ExtractParameterTemplate("objectArgNotNull", out var paramInfo);
            CollectionAssert.AreEqual(GraphTypeExpression.RequiredSingleItem, template.TypeExpression.Wrappers);
        }

        [Test]
        public void IEnumerableT_AllowedAsList_SetsDataCorrectly()
        {
            var template = this.ExtractParameterTemplate("enumerableIntArg", out var paramInfo);
            CollectionAssert.AreEqual(GraphTypeExpression.ListRequiredItem, template.TypeExpression.Wrappers);
        }

        [Test]
        public void IEnumerableNullableT_AllowedAsListWithNullableItems()
        {
            var template = this.ExtractParameterTemplate("enumerableIntArgWithNullableItemButNoDefault", out var paramInfo);
            CollectionAssert.AreEqual(GraphTypeExpression.List, template.TypeExpression.Wrappers);
        }

        [Test]
        public void IEnumerableOfNotNull_WithAttributeForNonNulls_AllowedAsList()
        {
            var template = this.ExtractParameterTemplate("enumerableIntArgWithAttribForbidsNullItems", out var paramInfo);
            CollectionAssert.AreEqual(GraphTypeExpression.RequiredListRequiredItem, template.TypeExpression.Wrappers);
        }

        [Test]
        public void AllAttributesCombined_SetsCorrectly()
        {
            var template = this.ExtractParameterTemplate("__lotsOfAttributes", out var paramInfo);
            Assert.AreEqual("validArgNameOverride1", template.Name);
            Assert.AreEqual("This Graph Field is Amazing", template.Description);
            CollectionAssert.AreEqual(GraphTypeExpression.RequiredList, template.TypeExpression.Wrappers);
        }

        [Test]
        public void ObjectReference_WithNullDefaultValue_SetsCorrectly()
        {
            var template = this.ExtractParameterTemplate("defaultValueObjectArg", out var paramInfo);
            Assert.AreEqual(typeof(Person), template.Parameter.ParameterType);
            CollectionAssert.AreEqual(GraphTypeExpression.SingleItem, template.TypeExpression.Wrappers);
            Assert.IsNull(template.DefaultValue);
        }

        [Test]
        public void StringReference_WithNullDefaultValue_SetsCorrectly()
        {
            var template = this.ExtractParameterTemplate("defaultValueStringArg", out var paramInfo);
            Assert.AreEqual(typeof(string), template.Parameter.ParameterType);
            Assert.IsEmpty((IEnumerable)template.TypeExpression.Wrappers);
            Assert.IsNull(template.DefaultValue);
        }

        [Test]
        public void StringReference_WithASuppliedDefaultValue_SetsCorrectly()
        {
            var template = this.ExtractParameterTemplate("defaultValueStringArgWithValue", out var paramInfo);
            Assert.AreEqual(typeof(string), template.Parameter.ParameterType);
            Assert.IsEmpty((IEnumerable)template.TypeExpression.Wrappers);
            Assert.AreEqual("abc", template.DefaultValue);
        }

        [Test]
        public void NullableTParam_WithDefaultValue_SetsFieldOptionsAsNullable()
        {
            var template = this.ExtractParameterTemplate("defaultValueNullableIntArg", out var paramInfo);
            Assert.IsEmpty((IEnumerable)template.TypeExpression.Wrappers);
            Assert.AreEqual(5, template.DefaultValue);
        }

        [Test]
        public void ArrayOfObjects_ThrowsException()
        {
            var template = this.ExtractParameterTemplate("arrayOfObjects", out var paramInfo);
            Assert.AreEqual(typeof(Person[]), template.Parameter.ParameterType);
            Assert.AreEqual("[Type]", template.TypeExpression.ToString());
            Assert.AreEqual(null, template.DefaultValue);
        }

        [Test]
        public void ArrayOfEnumerableOfObject_ThrowsException()
        {
            var template = this.ExtractParameterTemplate("arrayOfEnumerableOfObject", out var paramInfo);
            Assert.AreEqual(typeof(IEnumerable<Person>[]), template.Parameter.ParameterType);
            Assert.AreEqual("[[Type]]", template.TypeExpression.ToString());
            Assert.AreEqual(null, template.DefaultValue);
        }

        [Test]
        public void EnumerableArrayOfObjects_ThrowsException()
        {
            var template = this.ExtractParameterTemplate("enumerableOfArrayOfObjects", out var paramInfo);
            Assert.AreEqual(typeof(IEnumerable<Person[]>), template.Parameter.ParameterType);
            Assert.AreEqual("[[Type]]", template.TypeExpression.ToString());
            Assert.AreEqual(null, template.DefaultValue);
        }

        [Test]
        public void EnumerableArrayOfArrryOfObjects_ParsesTypeExpression()
        {
            var template = this.ExtractParameterTemplate("arrayOfEnumerableOfArrayOfObjects", out var paramInfo);
            Assert.AreEqual(typeof(IEnumerable<Person[]>[]), template.Parameter.ParameterType);
            Assert.AreEqual("[[[Type]]]", template.TypeExpression.ToString());
            Assert.AreEqual(null, template.DefaultValue);
        }

        [Test]
        public void StupidDeepArray_ParsesCorrectTypeExpression()
        {
            var template = this.ExtractParameterTemplate("deepArray", out var paramInfo);
            Assert.AreEqual(typeof(Person[][][][][][][][][][][][][][][][][][][]), template.Parameter.ParameterType);
            Assert.AreEqual("[[[[[[[[[[[[[[[[[[[Type]]]]]]]]]]]]]]]]]]]", template.TypeExpression.ToString());
            Assert.AreEqual(null, template.DefaultValue);
        }

        [Test]
        public void Parse_AssignedDirective_IsTemplatized()
        {
            var template = this.ExtractParameterTemplate("paramDirective", out var paramInfo);
            Assert.AreEqual(1, Enumerable.Count<IAppliedDirectiveTemplate>(template.AppliedDirectives));

            var appliedDirective = Enumerable.First<IAppliedDirectiveTemplate>(template.AppliedDirectives);
            Assert.AreEqual(typeof(DirectiveWithArgs), appliedDirective.DirectiveType);
            Assert.AreEqual(new object[] { 77, "param arg" }, appliedDirective.Arguments);
        }

        [Test]
        public void CompatiableDeclaredTypeExpression_IsAllowed()
        {
            // actual type expression "Int"
            // declared as Int!
            var template = this.ExtractParameterTemplate("compatiableTypeExpressionSingle", out var paramInfo);
            Assert.AreEqual("Type!", template.TypeExpression.ToString());
        }

        [Test]
        public void CompatiableDeclaredTypeExpressionOnList_IsAllowed()
        {
            // actual type expression [Int]
            // declared as [Int!]!
            var template = this.ExtractParameterTemplate("compatiableTypeExpressionList", out var paramInfo);
            Assert.AreEqual("[Type!]!", template.TypeExpression.ToString());
        }

        [Test]
        public void InvalidTypeExpression_ThrowsException()
        {
            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                var template = this.ExtractParameterTemplate("invalidTypeExpression", out var paramInfo);
            });
        }

        [Test]
        public void IncompatiableTypeExpressionListToSingle_ThrowsException()
        {
            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                var template = this.ExtractParameterTemplate("incompatiableTypeExpressionListToSingle", out var paramInfo);
            });
        }

        [Test]
        public void IncompatiableTypeExpressionSingleToList_ThrowsException()
        {
            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                var template = this.ExtractParameterTemplate("incompatiableTypeExpressionSingleToList", out var paramInfo);
            });
        }

        [Test]
        public void IncompatiableTypeExpressionNullToNotNull_ThrowsException()
        {
            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                var template = this.ExtractParameterTemplate("incompatiableTypeExpressionNullToNotNull", out var paramInfo);
            });
        }

        [Test]
        public void FromGraphQLDeclaration_SetsParamModifierAppropriately()
        {
            var template = this.ExtractParameterTemplate("justFromGraphQLDeclaration", out var paramInfo);
            Assert.AreEqual(GraphArgumentModifiers.ExplicitSchemaItem, template.ArgumentModifier);
        }

        [Test]
        public void FromServiceDeclaration_SetsParamModifierAppropriately()
        {
            var template = this.ExtractParameterTemplate("justFromServicesDeclaration", out var paramInfo);
            Assert.AreEqual(GraphArgumentModifiers.ExplicitInjected, template.ArgumentModifier);
        }

        [Test]
        public void ArgumentAttributedasFromGraphQLAndFromServices_ThrowsException()
        {
            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                var template = this.ExtractParameterTemplate("doubleDeclaredObject", out var paramInfo);
            });
        }
    }
}