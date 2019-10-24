// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Internal.Templating
{
    using System;
    using System.Linq;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Configuration.Formatting;
    using GraphQL.AspNet.Defaults;
    using GraphQL.AspNet.Defaults.TypeMakers;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Internal;
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Internal.TypeTemplates;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.CommonHelpers;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Internal.Templating.ActionTestData;
    using GraphQL.AspNet.Tests.Internal.Templating.ControllerTestData;
    using GraphQL.AspNet.Tests.Internal.Templating.EnumTestData;
    using GraphQL.AspNet.Tests.Internal.Templating.InterfaceTestData;
    using GraphQL.AspNet.Tests.Internal.Templating.ObjectTypeTests;
    using GraphQL.AspNet.Tests.Internal.Templating.PropertyTestData;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class GraphTypeMakerTests
    {
        private GraphTypeCreationResult MakeGraphType(Type type, TypeKind kind, TemplateDeclarationRequirements? requirements = null)
        {
            var builder = new TestServerBuilder(TestOptions.CodeDeclaredNames);
            if (requirements.HasValue)
            {
                builder.AddGraphQL(o =>
                {
                    o.DeclarationOptions.FieldDeclarationRequirements = requirements.Value;
                });
            }

            var typeMaker = new DefaultGraphTypeMakerProvider();
            var testServer = builder.Build();
            var maker = typeMaker.CreateTypeMaker(testServer.Schema, kind);
            return maker.CreateGraphType(type);
        }

        private IGraphField MakeGraphField(IGraphTypeFieldTemplate fieldTemplate)
        {
            var testServer = new TestServerBuilder().Build();
            var maker = new GraphFieldMaker(testServer.Schema);
            return maker.CreateField(fieldTemplate).Field;
        }

        [Test]
        public void DefaultFactory_NoSchema_YieldsNoMaker()
        {
            var factory = new DefaultGraphTypeMakerProvider();
            var instance = factory.CreateTypeMaker(null, TypeKind.OBJECT);
            Assert.IsNull(instance);
        }

        [Test]
        public void DefaultFactory_UnknownTypeKInd_YieldsNoMaker()
        {
            var schema = new GraphSchema();
            var factory = new DefaultGraphTypeMakerProvider();
            var instance = factory.CreateTypeMaker(schema, TypeKind.LIST);
            Assert.IsNull(instance);
        }

        [Test]
        public void Parse_EnumWithUndeclaredValues_WhenConfigRequiresDeclaration_DoesntIncludeUndeclared_InGraphType()
        {
            var template = TemplateHelper.CreateEnumTemplate<EnumWithUndeclaredValues>();

            var builder = new TestServerBuilder();
            builder.AddGraphQL(o =>
            {
                o.DeclarationOptions.FieldDeclarationRequirements = TemplateDeclarationRequirements.EnumValue;
            });

            var server = builder.Build();

            var graphType = server.CreateGraphType(template.ObjectType, TypeKind.ENUM).GraphType as IEnumGraphType;
            Assert.AreEqual(2, graphType.Values.Count);
            Assert.IsTrue(graphType.Values.ContainsKey("DECLAREDVALUE1"));
            Assert.IsTrue(graphType.Values.ContainsKey("VALUE_AWESOME"));
        }

        [Test]
        public void Parse_EnumWithUndeclaredValues_WhenConfigDoesNotRequireDeclaration_DoesIncludeUndeclared_InGraphType()
        {
            var builder = new TestServerBuilder()
                .AddGraphType<EnumWithUndeclaredValues>();
            builder.AddGraphQL(o =>
            {
                o.DeclarationOptions.FieldDeclarationRequirements = TemplateDeclarationRequirements.None;
            });

            var server = builder.Build();
            var graphType = server.CreateGraphType(typeof(EnumWithUndeclaredValues), TypeKind.ENUM).GraphType as IEnumGraphType;
            Assert.AreEqual(3, graphType.Values.Count);
            Assert.IsTrue(graphType.Values.ContainsKey("DECLAREDVALUE1"));
            Assert.IsTrue(graphType.Values.ContainsKey("VALUE_AWESOME"));
            Assert.IsTrue(graphType.Values.ContainsKey("UNDECLAREDVALUE1"));
        }

        [Test]
        public void CreateGraphType_ParsesAsExpected()
        {
            var builder = new TestServerBuilder();
            builder.AddGraphQL(o =>
            {
                o.DeclarationOptions.GraphNamingFormatter =
                    new GraphNameFormatter(enumValueStrategy: GraphNameFormatStrategy.NoChanges);
            });

            var server = builder.Build();
            var template = TemplateHelper.CreateEnumTemplate<EnumWithDescriptionOnValues>();

            var type = server.CreateGraphType(typeof(EnumWithDescriptionOnValues), TypeKind.ENUM).GraphType as IEnumGraphType;
            Assert.IsNotNull(type);
            Assert.AreEqual(template.Name, type.Name);

            Assert.IsTrue(type is EnumGraphType);
            Assert.AreEqual(4, ((EnumGraphType)type).Values.Count);
        }

        [Test]
        public void Parse_FieldAndDependencies_SetCorrectly()
        {
            var server = new TestServerBuilder().Build();

            var template = TemplateHelper.CreateGraphTypeTemplate<ComplexInputObject>(TypeKind.INPUT_OBJECT) as IInputObjectGraphTypeTemplate;

            var typeResult = this.MakeGraphType(typeof(ComplexInputObject), TypeKind.INPUT_OBJECT);
            var graphType = typeResult.GraphType as IInputObjectGraphType;

            Assert.IsNotNull(graphType);

            var inputGraphTypeName = GraphTypeNames.ParseName(typeof(OneMarkedProperty), TypeKind.INPUT_OBJECT);
            Assert.AreEqual(2, graphType.Fields.Count);

            var field = graphType.Fields.FirstOrDefault(x => x.TypeExpression.TypeName == inputGraphTypeName);
            Assert.IsNotNull(field);

            // string, OneMarkedProperty
            Assert.AreEqual(2, typeResult.DependentTypes.Count());
            Assert.IsTrue(typeResult.DependentTypes.Any(x => x.Type == typeof(OneMarkedProperty) && x.ExpectedKind == TypeKind.INPUT_OBJECT));
            Assert.IsTrue(typeResult.DependentTypes.Any(x => x.Type == typeof(string) && x.ExpectedKind == TypeKind.SCALAR));
        }

        [Test]
        public void InputObject_CreateGraphType_ParsesCorrectly()
        {
            var template = TemplateHelper.CreateGraphTypeTemplate<TypeCreationItem>(TypeKind.INPUT_OBJECT) as IInputObjectGraphTypeTemplate;

            var result = this.MakeGraphType(typeof(TypeCreationItem), TypeKind.INPUT_OBJECT);
            var objectGraphType = result.GraphType as IInputObjectGraphType;

            Assert.IsNotNull(objectGraphType);
            Assert.AreEqual($"Input_{nameof(TypeCreationItem)}", objectGraphType.Name);
            Assert.AreEqual(template.Description, objectGraphType.Description);
            Assert.AreEqual(TypeKind.INPUT_OBJECT, objectGraphType.Kind);

            // Prop1  (there is no __typename on input objects like there is on its object counterpart)
            Assert.AreEqual(1, objectGraphType.Fields.Count);

            // Method1, Method2 are on the type but should not be created as a field for an input type
            var method1 = objectGraphType.Fields.FirstOrDefault(x => x.Name == nameof(TypeCreationItem.Method1));
            Assert.IsNull(method1);

            var method2 = objectGraphType.Fields.FirstOrDefault(x => x.Name == nameof(TypeCreationItem.Method2));
            Assert.IsNull(method2);

            var prop1 = objectGraphType.Fields.FirstOrDefault(x => x.Name == nameof(TypeCreationItem.Prop1));
            Assert.IsNotNull(prop1);
            CollectionAssert.AreEqual(TypeExpressions.None.ToTypeWrapperSet(), prop1.TypeExpression.Wrappers);
            Assert.AreEqual(Constants.ScalarNames.STRING, prop1.TypeExpression.TypeName);
        }

        [Test]
        public void InputObject_CreateGraphType_WhenPropertyDelcarationIsRequired_DoesNotIncludeUndeclaredProperties()
        {
            var template = TemplateHelper.CreateGraphTypeTemplate<TypeWithUndeclaredFields>(TypeKind.INPUT_OBJECT) as IInputObjectGraphTypeTemplate;
            var result = this.MakeGraphType(typeof(TypeWithUndeclaredFields), TypeKind.INPUT_OBJECT, TemplateDeclarationRequirements.Property);
            var objectGraphType = result.GraphType as IInputObjectGraphType;

            Assert.IsNotNull(objectGraphType);
            Assert.IsTrue(objectGraphType.Fields.Any(x => x.Name == nameof(TypeWithUndeclaredFields.DeclaredProperty)));
            Assert.IsFalse(objectGraphType.Fields.Any(x => x.Name == nameof(TypeWithUndeclaredFields.UndeclaredProperty)));
        }

        [Test]
        public void CreateGraphType_WhenPropertyDelcarationIsNotRequired_DoesIncludeUndeclaredProperties()
        {
            var template = TemplateHelper.CreateGraphTypeTemplate<TypeWithUndeclaredFields>() as IInputObjectGraphTypeTemplate;
            var result = this.MakeGraphType(typeof(TypeWithUndeclaredFields), TypeKind.INPUT_OBJECT, TemplateDeclarationRequirements.None);
            var objectGraphType = result.GraphType as IInputObjectGraphType;

            Assert.IsNotNull(objectGraphType);
            Assert.IsTrue(objectGraphType.Fields.Any(x => x.Name == nameof(TypeWithUndeclaredFields.DeclaredProperty)));
            Assert.IsTrue(objectGraphType.Fields.Any(x => x.Name == nameof(TypeWithUndeclaredFields.UndeclaredProperty)));
        }

        [Test]
        public void CreateGraphType_WhenPropertyDelcarationIsRequired_ButWithGraphTypeOverride_DoesNotIncludeUndeclaredProperties()
        {
            // config says properties dont require declaration, override on type says it does
            var template = TemplateHelper.CreateGraphTypeTemplate<TypeWithUndeclaredFieldsWithOverride>() as IInputObjectGraphTypeTemplate;
            var result = this.MakeGraphType(typeof(TypeWithUndeclaredFieldsWithOverride), TypeKind.INPUT_OBJECT, TemplateDeclarationRequirements.None);
            var objectGraphType = result.GraphType as IInputObjectGraphType;

            Assert.IsNotNull(objectGraphType);
            Assert.IsTrue(objectGraphType.Fields.Any(x => x.Name == nameof(TypeWithUndeclaredFields.DeclaredProperty)));
            Assert.IsFalse(objectGraphType.Fields.Any(x => x.Name == nameof(TypeWithUndeclaredFields.UndeclaredProperty)));
        }

        [Test]
        public void InputObject_CreateGraphType_WhenPropertyDelcarationIsNotRequired_ButWithGraphTypeOverride_DoesIncludeUndeclaredProperties()
        {
            // config says properties DO require declaration, override on type says it does not
            var template = TemplateHelper.CreateGraphTypeTemplate<TypeWithUndeclaredFieldsWithOverrideNone>() as IInputObjectGraphTypeTemplate;
            var result = this.MakeGraphType(typeof(TypeWithUndeclaredFieldsWithOverrideNone), TypeKind.INPUT_OBJECT, TemplateDeclarationRequirements.Property);
            var objectGraphType = result.GraphType as IInputObjectGraphType;

            Assert.IsNotNull(objectGraphType);
            Assert.IsTrue(objectGraphType.Fields.Any(x => x.Name == nameof(TypeWithUndeclaredFields.DeclaredProperty)));
            Assert.IsTrue(objectGraphType.Fields.Any(x => x.Name == nameof(TypeWithUndeclaredFields.UndeclaredProperty)));
        }

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
        public void InputObject_CreateGraphType_WithSelfReferencingObject_ParsesAsExpected()
        {
            // ensure no stack overflows occur by attempting to create types of types
            // from self references
            // config says properties DO require declaration, override on type says it does not
            var template = TemplateHelper.CreateGraphTypeTemplate<SelfReferencingObject>(TypeKind.INPUT_OBJECT) as IInputObjectGraphTypeTemplate;
            var result = this.MakeGraphType(typeof(SelfReferencingObject), TypeKind.INPUT_OBJECT);

            var inputType = result.GraphType as IInputObjectGraphType;
            Assert.IsNotNull(inputType);
            Assert.AreEqual(template.Name, inputType.Name);
            Assert.IsNotNull(inputType.Fields.FirstOrDefault(x => x.Name == nameof(SelfReferencingObject.Name)));
            Assert.IsNotNull(inputType.Fields.FirstOrDefault(x => x.Name == nameof(SelfReferencingObject.Parent)));
        }

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
        public void ActionTemplate_CreateUnionType_PropertyCheck()
        {
            var schema = new GraphSchema();
            schema.SetNoAlterationConfiguration();

            var action = TemplateHelper.CreateActionMethodTemplate<UnionTestController>(nameof(UnionTestController.TwoTypeUnion));
            var union = new UnionGraphTypeMaker(schema).CreateGraphType(action.UnionProxy, TypeKind.OBJECT);

            Assert.IsNotNull(union);
            Assert.IsTrue(union is UnionGraphType);
            Assert.AreEqual("FragmentData", union.Name);
            Assert.IsNull(union.Description);

            Assert.AreEqual(2, union.PossibleGraphTypeNames.Count());
            Assert.AreEqual(2, union.PossibleConcreteTypes.Count());

            Assert.IsTrue(union.PossibleGraphTypeNames.Contains(nameof(UnionDataA)));
            Assert.IsTrue(union.PossibleGraphTypeNames.Contains(nameof(UnionDataB)));
            Assert.IsTrue(union.PossibleConcreteTypes.Contains(typeof(UnionDataA)));
            Assert.IsTrue(union.PossibleConcreteTypes.Contains(typeof(UnionDataB)));
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
        public void Parse_FromInterface_CreateGraphType_PropertyCheck()
        {
            var server = new TestServerBuilder().Build();
            var template = TemplateHelper.CreateInterfaceTemplate<ISimpleInterface>();
            var typeMaker = new DefaultGraphTypeMakerProvider();

            var graphType = typeMaker.CreateTypeMaker(server.Schema, TypeKind.INTERFACE)
                .CreateGraphType(typeof(ISimpleInterface)).GraphType as IInterfaceGraphType;
            Assert.IsNotNull(graphType);

            Assert.AreEqual(template.Name, graphType.Name);
            Assert.AreEqual(TypeKind.INTERFACE, graphType.Kind);

            // Property1, Property2, __typename
            Assert.AreEqual(3, graphType.Fields.Count());
        }

        [Test]
        public void Interface_CreateGraphType_ParsesCorrectly()
        {
            var server = new TestServerBuilder(TestOptions.CodeDeclaredNames).Build();
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
            CollectionAssert.AreEqual(TypeExpressions.IsNotNull.ToTypeWrapperSet(), method1.TypeExpression.Wrappers); // double cant return as null
            Assert.AreEqual(Constants.ScalarNames.DOUBLE, method1.TypeExpression.TypeName);
            Assert.AreEqual(3, method1.Arguments.Count);

            var arg1 = method1.Arguments["arg1"];
            var arg2 = method1.Arguments["arg2"];
            var arg3 = method1.Arguments["arg3"];

            Assert.IsNotNull(arg1);
            Assert.AreEqual(typeof(string), arg1.ObjectType);
            Assert.IsEmpty(arg1.TypeExpression.Wrappers);
            Assert.AreEqual(null, arg1.DefaultValue);

            Assert.IsNotNull(arg2);
            Assert.AreEqual(typeof(int), arg2.ObjectType);
            CollectionAssert.AreEqual(GraphTypeExpression.RequiredSingleItem, arg2.TypeExpression.Wrappers);
            Assert.AreEqual(null, arg2.DefaultValue);

            // arg3 has a default value therefore can be null on request even though the type is not nullable
            Assert.IsNotNull(arg3);
            Assert.AreEqual(typeof(int), arg3.ObjectType);
            CollectionAssert.AreEqual(GraphTypeExpression.SingleItem, arg3.TypeExpression.Wrappers);
            Assert.AreEqual(5, arg3.DefaultValue);

            var method2 = objectGraphType.Fields.FirstOrDefault(x => x.Name == nameof(TypeCreationItem.Method2));
            Assert.IsNotNull(method2);
            Assert.IsEmpty(method2.TypeExpression.Wrappers);
            Assert.AreEqual(nameof(TwoPropertyObject), method2.TypeExpression.TypeName);

            arg1 = method2.Arguments["arg1"];
            arg2 = method2.Arguments["arg2"];

            Assert.IsNotNull(arg1);
            Assert.AreEqual(typeof(long), arg1.ObjectType);
            CollectionAssert.AreEqual(TypeExpressions.IsNotNull.ToTypeWrapperSet(), arg1.TypeExpression.Wrappers);
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
            var template = TemplateHelper.CreateObjectTemplate<SelfReferencingObject>();
            var objectGraphType = this.MakeGraphType(typeof(SelfReferencingObject), TypeKind.OBJECT).GraphType as IObjectGraphType;

            Assert.IsNotNull(objectGraphType);
            Assert.AreEqual(template.Name, objectGraphType.Name);
            Assert.IsNotNull(objectGraphType.Fields.FirstOrDefault(x => x.Name == nameof(SelfReferencingObject.Name)));
            Assert.IsNotNull(objectGraphType.Fields.FirstOrDefault(x => x.Name == nameof(SelfReferencingObject.Parent)));
        }

        [Test]
        public void Parse_DefaultValuesCheck()
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
        public void Parse_GraphName_OverridesDefaultName()
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
    }
}