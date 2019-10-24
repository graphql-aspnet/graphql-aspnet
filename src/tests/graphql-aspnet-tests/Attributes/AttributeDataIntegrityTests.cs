// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Attributes
{
    using System.Linq;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using NUnit.Framework;

    /// <summary>
    /// Attribute functionality is at the core of the library, ensure that all properties on all
    /// attributes always set and return expected values. Attribute coverage should always be 100%.
    /// </summary>
    [TestFixture]
    public class AttributeDataIntegrityTests
    {
        [Test]
        public void GraphFieldAttribute_EmptyConstructor_PropertyCheck()
        {
            var attrib = new GraphFieldAttribute();
            Assert.AreEqual(GraphCollection.Types, attrib.FieldType);
            Assert.AreEqual(false, attrib.IsRootFragment);
            Assert.AreEqual(null, attrib.UnionTypeName);
            Assert.AreEqual(Constants.Routing.ACTION_METHOD_META_NAME, attrib.Template);
            Assert.AreEqual(TypeExpressions.Auto, attrib.TypeExpression);
            Assert.AreEqual(0, attrib.Types.Count);
            Assert.AreEqual(FieldResolutionMode.PerSourceItem,  attrib.ExecutionMode);
        }

        [Test]
        public void GraphFieldAttribute_EmptyConstructor_TypeModifersAdded_PropertyCheck()
        {
            // simulate a user supplying "TypeModifiers = XXXX" in an attribute initializer
            // and esnure the logic of setting notnull list etc.
            var attrib = new GraphFieldAttribute();
            Assert.AreEqual(TypeExpressions.Auto, attrib.TypeExpression);

            attrib.TypeExpression = TypeExpressions.IsNotNullList;
            CollectionAssert.AreEqual(GraphTypeExpression.RequiredList, attrib.TypeDefinition);
        }

        [Test]
        public void GraphFieldAttribute_FieldNameConstructor_PropertyCheck()
        {
            var attrib = new GraphFieldAttribute("myFieldName");
            Assert.AreEqual(GraphCollection.Types, attrib.FieldType);
            Assert.AreEqual(false, attrib.IsRootFragment);
            Assert.AreEqual(null, attrib.UnionTypeName);
            Assert.AreEqual("myFieldName", attrib.Template);
            Assert.AreEqual(TypeExpressions.Auto, attrib.TypeExpression);
            Assert.AreEqual(0, attrib.Types.Count);
            Assert.AreEqual(FieldResolutionMode.PerSourceItem, attrib.ExecutionMode);
        }

        [Test]
        public void QueryAttribute_EmptyConstructor_PropertyCheck()
        {
            var attrib = new QueryAttribute();
            Assert.AreEqual(GraphCollection.Query, attrib.FieldType);
            Assert.AreEqual(false, attrib.IsRootFragment);
            Assert.AreEqual(null, attrib.UnionTypeName);
            Assert.AreEqual(Constants.Routing.ACTION_METHOD_META_NAME, attrib.Template);
            Assert.AreEqual(TypeExpressions.Auto, attrib.TypeExpression);
            Assert.AreEqual(0, attrib.Types.Count);
            Assert.AreEqual(FieldResolutionMode.PerSourceItem, attrib.ExecutionMode);
        }

        [Test]
        public void QueryAttribute_TemplateConstructor_PropertyCheck()
        {
            var attrib = new QueryAttribute("myQueryRoute");
            Assert.AreEqual(GraphCollection.Query, attrib.FieldType);
            Assert.AreEqual(false, attrib.IsRootFragment);
            Assert.AreEqual(null, attrib.UnionTypeName);
            Assert.AreEqual("myQueryRoute", attrib.Template);
            Assert.AreEqual(TypeExpressions.Auto, attrib.TypeExpression);
            Assert.AreEqual(0, attrib.Types.Count);
            Assert.AreEqual(FieldResolutionMode.PerSourceItem, attrib.ExecutionMode);
        }

        [Test]
        public void QueryAttribute_ReturnTypeConstructor_PropertyCheck()
        {
            var attrib = new QueryAttribute(typeof(AttributeDataIntegrityTests));
            Assert.AreEqual(GraphCollection.Query, attrib.FieldType);
            Assert.AreEqual(false, attrib.IsRootFragment);
            Assert.AreEqual(null, attrib.UnionTypeName);
            Assert.AreEqual(Constants.Routing.ACTION_METHOD_META_NAME, attrib.Template);
            Assert.AreEqual(TypeExpressions.Auto, attrib.TypeExpression);
            Assert.AreEqual(1, attrib.Types.Count);
            Assert.AreEqual(typeof(AttributeDataIntegrityTests), attrib.Types[0]);
            Assert.AreEqual(FieldResolutionMode.PerSourceItem, attrib.ExecutionMode);
        }

        [Test]
        public void QueryAttribute_MultiTypeConstructor_PropertyCheck()
        {
            var attrib = new QueryAttribute(typeof(AttributeDataIntegrityTests), typeof(GraphFieldAttribute));
            Assert.AreEqual(GraphCollection.Query, attrib.FieldType);
            Assert.AreEqual(false, attrib.IsRootFragment);
            Assert.AreEqual(null, attrib.UnionTypeName);
            Assert.AreEqual(Constants.Routing.ACTION_METHOD_META_NAME, attrib.Template);
            Assert.AreEqual(TypeExpressions.Auto, attrib.TypeExpression);
            Assert.AreEqual(2, attrib.Types.Count);
            Assert.AreEqual(typeof(AttributeDataIntegrityTests), attrib.Types[0]);
            Assert.AreEqual(typeof(GraphFieldAttribute), attrib.Types[1]);
            Assert.AreEqual(FieldResolutionMode.PerSourceItem, attrib.ExecutionMode);
        }

        [Test]
        public void QueryAttribute_TemplateMultiTypeConstructor_PropertyCheck()
        {
            var attrib = new QueryAttribute("myField", typeof(AttributeDataIntegrityTests), typeof(GraphFieldAttribute));
            Assert.AreEqual(GraphCollection.Query, attrib.FieldType);
            Assert.AreEqual(false, attrib.IsRootFragment);
            Assert.AreEqual(null, attrib.UnionTypeName);
            Assert.AreEqual("myField", attrib.Template);
            Assert.AreEqual(TypeExpressions.Auto, attrib.TypeExpression);
            Assert.AreEqual(2, attrib.Types.Count);
            Assert.AreEqual(typeof(AttributeDataIntegrityTests), attrib.Types[0]);
            Assert.AreEqual(typeof(GraphFieldAttribute), attrib.Types[1]);
            Assert.AreEqual(FieldResolutionMode.PerSourceItem, attrib.ExecutionMode);
        }

        [Test]
        public void QueryAttribute_UnionConstructor_PropertyCheck()
        {
            var attrib = new QueryAttribute("myField", "myUnionType", typeof(AttributeDataIntegrityTests), typeof(GraphFieldAttribute));
            Assert.AreEqual(GraphCollection.Query, attrib.FieldType);
            Assert.AreEqual(false, attrib.IsRootFragment);
            Assert.AreEqual("myUnionType", attrib.UnionTypeName);
            Assert.AreEqual("myField", attrib.Template);
            Assert.AreEqual(TypeExpressions.Auto, attrib.TypeExpression);
            Assert.AreEqual(2, attrib.Types.Count);
            Assert.AreEqual(typeof(AttributeDataIntegrityTests), attrib.Types[0]);
            Assert.AreEqual(typeof(GraphFieldAttribute), attrib.Types[1]);
            Assert.AreEqual(FieldResolutionMode.PerSourceItem, attrib.ExecutionMode);
        }

        [Test]
        public void MutationAttribute_EmptyConstructor_PropertyCheck()
        {
            var attrib = new MutationAttribute();
            Assert.AreEqual(GraphCollection.Mutation, attrib.FieldType);
            Assert.AreEqual(false, attrib.IsRootFragment);
            Assert.AreEqual(null, attrib.UnionTypeName);
            Assert.AreEqual(Constants.Routing.ACTION_METHOD_META_NAME, attrib.Template);
            Assert.AreEqual(TypeExpressions.Auto, attrib.TypeExpression);
            Assert.AreEqual(0, attrib.Types.Count);
            Assert.AreEqual(FieldResolutionMode.PerSourceItem, attrib.ExecutionMode);
        }

        [Test]
        public void MutationAttribute_TemplateConstructor_PropertyCheck()
        {
            var attrib = new MutationAttribute("myMutationRoute");
            Assert.AreEqual(GraphCollection.Mutation, attrib.FieldType);
            Assert.AreEqual(false, attrib.IsRootFragment);
            Assert.AreEqual(null, attrib.UnionTypeName);
            Assert.AreEqual("myMutationRoute", attrib.Template);
            Assert.AreEqual(TypeExpressions.Auto, attrib.TypeExpression);
            Assert.AreEqual(0, attrib.Types.Count);
            Assert.AreEqual(FieldResolutionMode.PerSourceItem, attrib.ExecutionMode);
        }

        [Test]
        public void MutationAttribute_ReturnTypeConstructor_PropertyCheck()
        {
            var attrib = new MutationAttribute(typeof(AttributeDataIntegrityTests));
            Assert.AreEqual(GraphCollection.Mutation, attrib.FieldType);
            Assert.AreEqual(false, attrib.IsRootFragment);
            Assert.AreEqual(null, attrib.UnionTypeName);
            Assert.AreEqual(Constants.Routing.ACTION_METHOD_META_NAME, attrib.Template);
            Assert.AreEqual(TypeExpressions.Auto, attrib.TypeExpression);
            Assert.AreEqual(1, attrib.Types.Count);
            Assert.AreEqual(typeof(AttributeDataIntegrityTests), attrib.Types[0]);
            Assert.AreEqual(FieldResolutionMode.PerSourceItem, attrib.ExecutionMode);
        }

        [Test]
        public void MutationAttribute_MultiTypeConstructor_PropertyCheck()
        {
            var attrib = new MutationAttribute(typeof(AttributeDataIntegrityTests), typeof(GraphFieldAttribute));
            Assert.AreEqual(GraphCollection.Mutation, attrib.FieldType);
            Assert.AreEqual(false, attrib.IsRootFragment);
            Assert.AreEqual(null, attrib.UnionTypeName);
            Assert.AreEqual(Constants.Routing.ACTION_METHOD_META_NAME, attrib.Template);
            Assert.AreEqual(TypeExpressions.Auto, attrib.TypeExpression);
            Assert.AreEqual(2, attrib.Types.Count);
            Assert.AreEqual(typeof(AttributeDataIntegrityTests), attrib.Types[0]);
            Assert.AreEqual(typeof(GraphFieldAttribute), attrib.Types[1]);
            Assert.AreEqual(FieldResolutionMode.PerSourceItem, attrib.ExecutionMode);
        }

        [Test]
        public void MutationAttribute_TemplateMultiTypeConstructor_PropertyCheck()
        {
            var attrib = new MutationAttribute("myField", typeof(AttributeDataIntegrityTests), typeof(GraphFieldAttribute));
            Assert.AreEqual(GraphCollection.Mutation, attrib.FieldType);
            Assert.AreEqual(false, attrib.IsRootFragment);
            Assert.AreEqual(null, attrib.UnionTypeName);
            Assert.AreEqual("myField", attrib.Template);
            Assert.AreEqual(TypeExpressions.Auto, attrib.TypeExpression);
            Assert.AreEqual(2, attrib.Types.Count);
            Assert.AreEqual(typeof(AttributeDataIntegrityTests), attrib.Types[0]);
            Assert.AreEqual(typeof(GraphFieldAttribute), attrib.Types[1]);
            Assert.AreEqual(FieldResolutionMode.PerSourceItem, attrib.ExecutionMode);
        }

        [Test]
        public void MutationAttribute_UnionConstructor_PropertyCheck()
        {
            var attrib = new MutationAttribute("myField", "myUnionType", typeof(AttributeDataIntegrityTests), typeof(GraphFieldAttribute));
            Assert.AreEqual(GraphCollection.Mutation, attrib.FieldType);
            Assert.AreEqual(false, attrib.IsRootFragment);
            Assert.AreEqual("myUnionType", attrib.UnionTypeName);
            Assert.AreEqual("myField", attrib.Template);
            Assert.AreEqual(TypeExpressions.Auto, attrib.TypeExpression);
            Assert.AreEqual(2, attrib.Types.Count);
            Assert.AreEqual(typeof(AttributeDataIntegrityTests), attrib.Types[0]);
            Assert.AreEqual(typeof(GraphFieldAttribute), attrib.Types[1]);
            Assert.AreEqual(FieldResolutionMode.PerSourceItem, attrib.ExecutionMode);
        }

        [Test]
        public void QueryRootAttribute_EmptyConstructor_PropertyCheck()
        {
            var attrib = new QueryRootAttribute();
            Assert.AreEqual(GraphCollection.Query, attrib.FieldType);
            Assert.AreEqual(true, attrib.IsRootFragment);
            Assert.AreEqual(null, attrib.UnionTypeName);
            Assert.AreEqual(Constants.Routing.ACTION_METHOD_META_NAME, attrib.Template);
            Assert.AreEqual(TypeExpressions.Auto, attrib.TypeExpression);
            Assert.AreEqual(0, attrib.Types.Count);
            Assert.AreEqual(FieldResolutionMode.PerSourceItem, attrib.ExecutionMode);
        }

        [Test]
        public void QueryRootAttribute_TemplateConstructor_PropertyCheck()
        {
            var attrib = new QueryRootAttribute("myQueryRootRoute");
            Assert.AreEqual(GraphCollection.Query, attrib.FieldType);
            Assert.AreEqual(true, attrib.IsRootFragment);
            Assert.AreEqual(null, attrib.UnionTypeName);
            Assert.AreEqual("myQueryRootRoute", attrib.Template);
            Assert.AreEqual(TypeExpressions.Auto, attrib.TypeExpression);
            Assert.AreEqual(0, attrib.Types.Count);
            Assert.AreEqual(FieldResolutionMode.PerSourceItem, attrib.ExecutionMode);
        }

        [Test]
        public void QueryRootAttribute_ReturnTypeConstructor_PropertyCheck()
        {
            var attrib = new QueryRootAttribute(typeof(AttributeDataIntegrityTests));
            Assert.AreEqual(GraphCollection.Query, attrib.FieldType);
            Assert.AreEqual(true, attrib.IsRootFragment);
            Assert.AreEqual(null, attrib.UnionTypeName);
            Assert.AreEqual(Constants.Routing.ACTION_METHOD_META_NAME, attrib.Template);
            Assert.AreEqual(TypeExpressions.Auto, attrib.TypeExpression);
            Assert.AreEqual(1, attrib.Types.Count);
            Assert.AreEqual(typeof(AttributeDataIntegrityTests), attrib.Types[0]);
            Assert.AreEqual(FieldResolutionMode.PerSourceItem, attrib.ExecutionMode);
        }

        [Test]
        public void QueryRootAttribute_MultiTypeConstructor_PropertyCheck()
        {
            var attrib = new QueryRootAttribute(typeof(AttributeDataIntegrityTests), typeof(GraphFieldAttribute));
            Assert.AreEqual(GraphCollection.Query, attrib.FieldType);
            Assert.AreEqual(true, attrib.IsRootFragment);
            Assert.AreEqual(null, attrib.UnionTypeName);
            Assert.AreEqual(Constants.Routing.ACTION_METHOD_META_NAME, attrib.Template);
            Assert.AreEqual(TypeExpressions.Auto, attrib.TypeExpression);
            Assert.AreEqual(2, attrib.Types.Count);
            Assert.AreEqual(typeof(AttributeDataIntegrityTests), attrib.Types[0]);
            Assert.AreEqual(typeof(GraphFieldAttribute), attrib.Types[1]);
            Assert.AreEqual(FieldResolutionMode.PerSourceItem, attrib.ExecutionMode);
        }

        [Test]
        public void QueryRootAttribute_TemplateMultiTypeConstructor_PropertyCheck()
        {
            var attrib = new QueryRootAttribute("myField", typeof(AttributeDataIntegrityTests), typeof(GraphFieldAttribute));
            Assert.AreEqual(GraphCollection.Query, attrib.FieldType);
            Assert.AreEqual(true, attrib.IsRootFragment);
            Assert.AreEqual(null, attrib.UnionTypeName);
            Assert.AreEqual("myField", attrib.Template);
            Assert.AreEqual(2, attrib.Types.Count);
            Assert.AreEqual(typeof(AttributeDataIntegrityTests), attrib.Types[0]);
            Assert.AreEqual(typeof(GraphFieldAttribute), attrib.Types[1]);
            Assert.AreEqual(FieldResolutionMode.PerSourceItem, attrib.ExecutionMode);
        }

        [Test]
        public void QueryRootAttribute_UnionConstructor_PropertyCheck()
        {
            var attrib = new QueryRootAttribute("myField", "myUnionType", typeof(AttributeDataIntegrityTests), typeof(GraphFieldAttribute));
            Assert.AreEqual(GraphCollection.Query, attrib.FieldType);
            Assert.AreEqual(true, attrib.IsRootFragment);
            Assert.AreEqual("myUnionType", attrib.UnionTypeName);
            Assert.AreEqual("myField", attrib.Template);
            Assert.AreEqual(2, attrib.Types.Count);
            Assert.AreEqual(typeof(AttributeDataIntegrityTests), attrib.Types[0]);
            Assert.AreEqual(typeof(GraphFieldAttribute), attrib.Types[1]);
            Assert.AreEqual(FieldResolutionMode.PerSourceItem, attrib.ExecutionMode);
        }

        [Test]
        public void MutationRootAttribute_EmptyConstructor_PropertyCheck()
        {
            var attrib = new MutationRootAttribute();
            Assert.AreEqual(GraphCollection.Mutation, attrib.FieldType);
            Assert.AreEqual(true, attrib.IsRootFragment);
            Assert.AreEqual(null, attrib.UnionTypeName);
            Assert.AreEqual(Constants.Routing.ACTION_METHOD_META_NAME, attrib.Template);
            Assert.AreEqual(TypeExpressions.Auto, attrib.TypeExpression);
            Assert.AreEqual(0, attrib.Types.Count);
            Assert.AreEqual(FieldResolutionMode.PerSourceItem, attrib.ExecutionMode);
        }

        [Test]
        public void MutationRootAttribute_TemplateConstructor_PropertyCheck()
        {
            var attrib = new MutationRootAttribute("myMutationRootRoute");
            Assert.AreEqual(GraphCollection.Mutation, attrib.FieldType);
            Assert.AreEqual(true, attrib.IsRootFragment);
            Assert.AreEqual(null, attrib.UnionTypeName);
            Assert.AreEqual("myMutationRootRoute", attrib.Template);
            Assert.AreEqual(TypeExpressions.Auto, attrib.TypeExpression);
            Assert.AreEqual(0, attrib.Types.Count);
            Assert.AreEqual(FieldResolutionMode.PerSourceItem, attrib.ExecutionMode);
        }

        [Test]
        public void MutationRootAttribute_ReturnTypeConstructor_PropertyCheck()
        {
            var attrib = new MutationRootAttribute(typeof(AttributeDataIntegrityTests));
            Assert.AreEqual(GraphCollection.Mutation, attrib.FieldType);
            Assert.AreEqual(true, attrib.IsRootFragment);
            Assert.AreEqual(null, attrib.UnionTypeName);
            Assert.AreEqual(Constants.Routing.ACTION_METHOD_META_NAME, attrib.Template);
            Assert.AreEqual(TypeExpressions.Auto, attrib.TypeExpression);
            Assert.AreEqual(1, attrib.Types.Count);
            Assert.AreEqual(typeof(AttributeDataIntegrityTests), attrib.Types[0]);
            Assert.AreEqual(FieldResolutionMode.PerSourceItem, attrib.ExecutionMode);
        }

        [Test]
        public void MutationRootAttribute_MultiTypeConstructor_PropertyCheck()
        {
            var attrib = new MutationRootAttribute(typeof(AttributeDataIntegrityTests), typeof(GraphFieldAttribute));
            Assert.AreEqual(GraphCollection.Mutation, attrib.FieldType);
            Assert.AreEqual(true, attrib.IsRootFragment);
            Assert.AreEqual(null, attrib.UnionTypeName);
            Assert.AreEqual(Constants.Routing.ACTION_METHOD_META_NAME, attrib.Template);
            Assert.AreEqual(TypeExpressions.Auto, attrib.TypeExpression);
            Assert.AreEqual(2, attrib.Types.Count);
            Assert.AreEqual(typeof(AttributeDataIntegrityTests), attrib.Types[0]);
            Assert.AreEqual(typeof(GraphFieldAttribute), attrib.Types[1]);
            Assert.AreEqual(FieldResolutionMode.PerSourceItem, attrib.ExecutionMode);
        }

        [Test]
        public void MutationRootAttribute_TemplateMultiTypeConstructor_PropertyCheck()
        {
            var attrib = new MutationRootAttribute("myField", typeof(AttributeDataIntegrityTests), typeof(GraphFieldAttribute));
            Assert.AreEqual(GraphCollection.Mutation, attrib.FieldType);
            Assert.AreEqual(true, attrib.IsRootFragment);
            Assert.AreEqual(null, attrib.UnionTypeName);
            Assert.AreEqual("myField", attrib.Template);
            Assert.AreEqual(TypeExpressions.Auto, attrib.TypeExpression);
            Assert.AreEqual(2, attrib.Types.Count);
            Assert.AreEqual(typeof(AttributeDataIntegrityTests), attrib.Types[0]);
            Assert.AreEqual(typeof(GraphFieldAttribute), attrib.Types[1]);
            Assert.AreEqual(FieldResolutionMode.PerSourceItem, attrib.ExecutionMode);
        }

        [Test]
        public void MutationRootAttribute_UnionConstructor_PropertyCheck()
        {
            var attrib = new MutationRootAttribute("myField", "myUnionType", typeof(AttributeDataIntegrityTests), typeof(GraphFieldAttribute));
            Assert.AreEqual(GraphCollection.Mutation, attrib.FieldType);
            Assert.AreEqual(true, attrib.IsRootFragment);
            Assert.AreEqual("myUnionType", attrib.UnionTypeName);
            Assert.AreEqual("myField", attrib.Template);
            Assert.AreEqual(TypeExpressions.Auto, attrib.TypeExpression);
            Assert.AreEqual(2, attrib.Types.Count);
            Assert.AreEqual(typeof(AttributeDataIntegrityTests), attrib.Types[0]);
            Assert.AreEqual(typeof(GraphFieldAttribute), attrib.Types[1]);
            Assert.AreEqual(FieldResolutionMode.PerSourceItem, attrib.ExecutionMode);
        }

        [Test]
        public void GraphRoot_EmptyConstructor_PropertyCheck()
        {
            var attrib = new GraphRootAttribute();
            Assert.AreEqual(true, attrib.IgnoreControllerField);
            Assert.AreEqual(null, attrib.Template);
        }

        [Test]
        public void GraphRoute_TemplateConstructor_PropertyCheck()
        {
            var attrib = new GraphRouteAttribute("myTemplate");
            Assert.AreEqual(false, attrib.IgnoreControllerField);
            Assert.AreEqual("myTemplate", attrib.Template);
        }

        [Test]
        public void GraphRoute_TemplateConstructor_IsTrimmed()
        {
            var attrib = new GraphRouteAttribute("    myTemplate   ");
            Assert.AreEqual(false, attrib.IgnoreControllerField);
            Assert.AreEqual("myTemplate", attrib.Template);
        }

        [Test]
        public void Deprecated_EmptyConstructor_PropertyCheck()
        {
            var attrib = new DeprecatedAttribute();
            Assert.AreEqual(null, attrib.Reason);
        }

        [Test]
        public void Deprecated_WithReason_PropertyCheck()
        {
            var attrib = new DeprecatedAttribute("Because");
            Assert.AreEqual("Because", attrib.Reason);
        }

        [Test]
        public void Deprecated_WithReason_IsTrimmed()
        {
            var attrib = new DeprecatedAttribute("   Because  ");
            Assert.AreEqual("Because", attrib.Reason);
        }

        [Test]
        public void PossibleTypes_AllTypesReturned_PropertyCheck()
        {
            var attrib = new PossibleTypesAttribute(typeof(AttributeDataIntegrityTests), typeof(GraphRootAttribute));
            Assert.AreEqual(2, attrib.PossibleTypes.Count);
            Assert.IsTrue(attrib.PossibleTypes.Contains(typeof(AttributeDataIntegrityTests)));
            Assert.IsTrue(attrib.PossibleTypes.Contains(typeof(GraphRootAttribute)));
        }

        [Test]
        public void PossibleTypes_NullsAreRemoved()
        {
            var attrib = new PossibleTypesAttribute(null, typeof(GraphRootAttribute));
            Assert.AreEqual(1, attrib.PossibleTypes.Count);
            Assert.IsTrue(attrib.PossibleTypes.Contains(typeof(GraphRootAttribute)));
        }

        [Test]
        public void EnumValue_EmptyConstructor_PropertyCheck()
        {
            var attrib = new GraphEnumValueAttribute();
            Assert.AreEqual(Constants.Routing.ENUM_VALUE_META_NAME, attrib.Name);
        }

        [Test]
        public void EnumValue_NameConstructor_PropertyCheck()
        {
            var attrib = new GraphEnumValueAttribute("myName");
            Assert.AreEqual("myName", attrib.Name);
        }

        [Test]
        public void EnumValue_NameConstructor_NullIsCarried()
        {
            var attrib = new GraphEnumValueAttribute(null);
            Assert.AreEqual(null, attrib.Name);
        }

        [Test]
        public void EnumValue_NameConstructor_IsTrimmed()
        {
            var attrib = new GraphEnumValueAttribute("  myName  ");
            Assert.AreEqual("myName", attrib.Name);
        }

        [Test]
        public void GraphType_EmptyConstructor_PropertyCheck()
        {
            var attrib = new GraphTypeAttribute();
            Assert.AreEqual(null, attrib.Name);
            Assert.AreEqual(null, attrib.InputName);
            Assert.AreEqual(false, attrib.PreventAutoInclusion);
            Assert.AreEqual(false, attrib.RequirementsWereDeclared);
            Assert.AreEqual(TemplateDeclarationRequirements.None, attrib.FieldDeclarationRequirements);
        }

        [Test]
        public void GraphType_NameConstructor_PropertyCheck()
        {
            var attrib = new GraphTypeAttribute("bob");
            Assert.AreEqual("bob", attrib.Name);
            Assert.AreEqual(null, attrib.InputName);
            Assert.AreEqual(false, attrib.PreventAutoInclusion);
            Assert.AreEqual(false, attrib.RequirementsWereDeclared);
            Assert.AreEqual(TemplateDeclarationRequirements.None, attrib.FieldDeclarationRequirements);
        }

        [Test]
        public void GraphType_NameWithInputNameConstructor_PropertyCheck()
        {
            var attrib = new GraphTypeAttribute("bob", "jane");
            Assert.AreEqual("bob", attrib.Name);
            Assert.AreEqual("jane", attrib.InputName);
            Assert.AreEqual(false, attrib.PreventAutoInclusion);
            Assert.AreEqual(false, attrib.RequirementsWereDeclared);
            Assert.AreEqual(TemplateDeclarationRequirements.None, attrib.FieldDeclarationRequirements);
        }

        [Test]
        public void GraphType_NamesAreTrimmed()
        {
            var attrib = new GraphTypeAttribute("bob   ", "   jane  \r");
            Assert.AreEqual("bob", attrib.Name);
            Assert.AreEqual("jane", attrib.InputName);
            Assert.AreEqual(false, attrib.PreventAutoInclusion);
            Assert.AreEqual(false, attrib.RequirementsWereDeclared);
            Assert.AreEqual(TemplateDeclarationRequirements.None, attrib.FieldDeclarationRequirements);
        }

        [Test]
        public void GraphType_SettingDeclarationReqs()
        {
            var attrib = new GraphTypeAttribute("bob");
            attrib.FieldDeclarationRequirements = TemplateDeclarationRequirements.Method;
            Assert.AreEqual(true, attrib.RequirementsWereDeclared);
            Assert.AreEqual(TemplateDeclarationRequirements.Method, attrib.FieldDeclarationRequirements);
        }

        [Test]
        public void GraphType_SettingPreventionFlag()
        {
            var attrib = new GraphTypeAttribute("bob");
            attrib.PreventAutoInclusion = true;
            Assert.AreEqual(true, attrib.PreventAutoInclusion);
        }

        [Test]
        public void FromGraphQL_NameContructor_PropertyCheck()
        {
            var attrib = new FromGraphQLAttribute("myArg");
            Assert.AreEqual("myArg", attrib.ArgumentName);
            Assert.AreEqual(TypeExpressions.Auto, attrib.TypeExpression);
        }

        [Test]
        public void FromGraphQL_ModifiersContructor_PropertyCheck()
        {
            var attrib = new FromGraphQLAttribute(TypeExpressions.IsList);
            Assert.AreEqual(Constants.Routing.PARAMETER_META_NAME, attrib.ArgumentName);
            CollectionAssert.AreEqual(GraphTypeExpression.List, attrib.TypeDefinition);
        }

        [Test]
        public void FromGraphQL_FullContructor_ArgNameTrimmed_PropertyCheck()
        {
            var attrib = new FromGraphQLAttribute("myName  ", TypeExpressions.IsList);
            Assert.AreEqual("myName", attrib.ArgumentName);
            CollectionAssert.AreEqual(GraphTypeExpression.List, attrib.TypeDefinition);
        }

        [Test]
        public void FromGraphQL_TypeModifersAdded_PropertyCheck()
        {
            // simulate a user supplying "TypeModifiers = XXXX" in an attribute initializer
            // and esnure the logic of setting notnull list etc.
            var attrib = new FromGraphQLAttribute(TypeExpressions.IsNotNullList);
            CollectionAssert.AreEqual(GraphTypeExpression.RequiredList, attrib.TypeDefinition);
        }

        [Test]
        public void TypeExtension_TypeConsturctor_PropertyCheck()
        {
            var attrib = new TypeExtensionAttribute(typeof(AttributeDataIntegrityTests), "myField");
            Assert.AreEqual("myField", attrib.Template);
            Assert.AreEqual(typeof(AttributeDataIntegrityTests), attrib.TypeToExtend);
            Assert.AreEqual(0, attrib.Types.Count);
        }

        [Test]
        public void TypeExtension_ReturnConsturctor_PropertyCheck()
        {
            var attrib = new TypeExtensionAttribute(typeof(AttributeDataIntegrityTests), "myField", typeof(string));
            Assert.AreEqual("myField", attrib.Template);
            Assert.AreEqual(typeof(AttributeDataIntegrityTests), attrib.TypeToExtend);
            Assert.AreEqual(1, attrib.Types.Count);
            Assert.IsTrue(attrib.Types.Contains(typeof(string)));
        }

        [Test]
        public void TypeExtension_UnionConstructor_PropertyCheck()
        {
            var attrib = new TypeExtensionAttribute(typeof(AttributeDataIntegrityTests), "myField", "myUnionType",  typeof(GraphFieldAttribute), typeof(string));

            Assert.AreEqual(GraphCollection.Types, attrib.FieldType);
            Assert.AreEqual(typeof(AttributeDataIntegrityTests), attrib.TypeToExtend);
            Assert.AreEqual("myField", attrib.Template);
            Assert.AreEqual("myUnionType", attrib.UnionTypeName);
            Assert.AreEqual(2, attrib.Types.Count);
            Assert.AreEqual(typeof(GraphFieldAttribute), attrib.Types[0]);
            Assert.AreEqual(typeof(string), attrib.Types[1]);
            Assert.AreEqual(FieldResolutionMode.PerSourceItem, attrib.ExecutionMode);
        }

        [Test]
        public void BatchTypeExtension_TypeConsturctor_PropertyCheck()
        {
            var attrib = new BatchTypeExtensionAttribute(typeof(AttributeDataIntegrityTests), "myField");
            Assert.AreEqual("myField", attrib.Template);
            Assert.AreEqual(typeof(AttributeDataIntegrityTests), attrib.TypeToExtend);
            Assert.AreEqual(0, attrib.Types.Count);
            Assert.AreEqual(FieldResolutionMode.Batch, attrib.ExecutionMode);
        }

        [Test]
        public void BatchTypeExtension_ReturnConsturctor_PropertyCheck()
        {
            var attrib = new BatchTypeExtensionAttribute(typeof(AttributeDataIntegrityTests), "myField", typeof(string));
            Assert.AreEqual("myField", attrib.Template);
            Assert.AreEqual(typeof(AttributeDataIntegrityTests), attrib.TypeToExtend);
            Assert.AreEqual(1, attrib.Types.Count);
            Assert.IsTrue(attrib.Types.Contains(typeof(string)));
            Assert.AreEqual(FieldResolutionMode.Batch, attrib.ExecutionMode);
        }

        [Test]
        public void BatchTypeExtension_UnionConstructor_PropertyCheck()
        {
            var attrib = new BatchTypeExtensionAttribute(typeof(AttributeDataIntegrityTests), "myField", "myUnionType", typeof(GraphFieldAttribute), typeof(string));

            Assert.AreEqual(GraphCollection.Types, attrib.FieldType);
            Assert.AreEqual(typeof(AttributeDataIntegrityTests), attrib.TypeToExtend);
            Assert.AreEqual("myField", attrib.Template);
            Assert.AreEqual("myUnionType", attrib.UnionTypeName);
            Assert.AreEqual(2, attrib.Types.Count);
            Assert.AreEqual(typeof(GraphFieldAttribute), attrib.Types[0]);
            Assert.AreEqual(typeof(string), attrib.Types[1]);
            Assert.AreEqual(FieldResolutionMode.Batch, attrib.ExecutionMode);
        }
    }
}