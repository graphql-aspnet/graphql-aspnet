// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Engine
{
    using System;
    using System.Linq;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Engine;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.Generation;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.Common.CommonHelpers;
    using GraphQL.AspNet.Tests.Common.Interfaces;
    using GraphQL.AspNet.Tests.Engine.DefaultSchemaFactoryTestData;
    using GraphQL.AspNet.Tests.Framework.Interfaces;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;

    [TestFixture]
    public class DefaultSchemaFactoryTests
    {
        private IServiceCollection SetupCollection()
        {
            var collection = new ServiceCollection();
            collection.AddTransient<IGraphQLTypeMakerFactory<GraphSchema>, DefaultGraphQLTypeMakerFactory<GraphSchema>>();
            return collection;
        }

        [Test]
        public void OneScalarType_GeneratesCorrectly()
        {
            var collection = this.SetupCollection();

            var factory = new DefaultGraphQLSchemaFactory<GraphSchema>(includeBuiltInDirectives: false);
            var options = new SchemaOptions<GraphSchema>(collection);
            options.DeclarationOptions.DisableIntrospection = true;

            var provider = collection.BuildServiceProvider();

            var scope = provider.CreateScope();
            var config = options.CreateConfiguration();

            var instance = factory.CreateInstance(
                scope,
                config,
                new SchemaTypeToRegister[]
                {
                    new SchemaTypeToRegister(typeof(int)),
                });

            Assert.IsNotNull(instance);
            Assert.AreEqual(3, instance.KnownTypes.Count);
            Assert.IsTrue(instance.KnownTypes.Contains(typeof(int)));
            Assert.IsTrue(instance.KnownTypes.Contains(typeof(string)));
            Assert.IsTrue(instance.Operations.ContainsKey(GraphOperationType.Query));

            var graphType = instance.KnownTypes.FindGraphType(typeof(int));
            Assert.IsNotNull(graphType);
            Assert.IsTrue(graphType is IScalarGraphType);
            Assert.AreEqual(typeof(int), ((IScalarGraphType)graphType).ObjectType);
        }

        [Test]
        public void CustomScalar_AllAssociatedTypesAreRegistered()
        {
            var collection = this.SetupCollection();

            var factory = new DefaultGraphQLSchemaFactory<GraphSchema>(includeBuiltInDirectives: false);
            var options = new SchemaOptions<GraphSchema>(collection);
            options.DeclarationOptions.DisableIntrospection = true;

            var provider = collection.BuildServiceProvider();

            var scope = provider.CreateScope();
            var config = options.CreateConfiguration();

            var instance = factory.CreateInstance(
                scope,
                config,
                new SchemaTypeToRegister[]
                {
                    new SchemaTypeToRegister(typeof(TwoPropertyObjectAsScalar)),
                });

            Assert.IsNotNull(instance);
            Assert.AreEqual(3, instance.KnownTypes.Count);
            Assert.IsNotNull(instance.KnownTypes.SingleOrDefault(x => x.Name == nameof(TwoPropertyObjectAsScalar)));
            Assert.IsNotNull(instance.KnownTypes.SingleOrDefault(x => x.Name == Constants.ScalarNames.STRING));
            Assert.IsNotNull(instance.KnownTypes.SingleOrDefault(x => x.Name == Constants.ReservedNames.QUERY_TYPE_NAME));

            // should find the custom scalar
            var customScalar = instance.KnownTypes.FindGraphType(typeof(TwoPropertyObject)) as IScalarGraphType;
            Assert.IsNotNull(customScalar);
            Assert.AreEqual(typeof(TwoPropertyObject), customScalar.ObjectType);
        }

        [Test]
        public void OneEnum_AllAssociatedTypesAreRegistered()
        {
            var collection = this.SetupCollection();

            var factory = new DefaultGraphQLSchemaFactory<GraphSchema>(includeBuiltInDirectives: false);
            var options = new SchemaOptions<GraphSchema>(collection);
            options.DeclarationOptions.DisableIntrospection = true;

            var provider = collection.BuildServiceProvider();

            var scope = provider.CreateScope();
            var config = options.CreateConfiguration();

            var instance = factory.CreateInstance(
                scope,
                config,
                new SchemaTypeToRegister[]
                {
                    new SchemaTypeToRegister(typeof(CustomEnum)),
                });

            Assert.IsNotNull(instance);
            Assert.AreEqual(3, instance.KnownTypes.Count);
            Assert.IsNotNull(instance.KnownTypes.SingleOrDefault(x => x.Name == nameof(CustomEnum)));
            Assert.IsNotNull(instance.KnownTypes.SingleOrDefault(x => x.Name == Constants.ScalarNames.STRING));
            Assert.IsNotNull(instance.KnownTypes.SingleOrDefault(x => x.Name == Constants.ReservedNames.QUERY_TYPE_NAME));

            // should find the custom scalar
            var customEnum = instance.KnownTypes.FindGraphType(typeof(CustomEnum)) as IEnumGraphType;
            Assert.IsNotNull(customEnum);
            Assert.AreEqual(typeof(CustomEnum), customEnum.ObjectType);

            Assert.AreEqual(2, customEnum.Values.Count);
            Assert.IsNotNull(customEnum.Values.FindByEnumValue(CustomEnum.Value1));
            Assert.IsNotNull(customEnum.Values.FindByEnumValue(CustomEnum.Value2));
        }

        [Test]
        public void OneObjectType_NoArgumentsOnFields_GeneratesCorrectly()
        {
            var collection = this.SetupCollection();

            var factory = new DefaultGraphQLSchemaFactory<GraphSchema>(includeBuiltInDirectives: false);
            var options = new SchemaOptions<GraphSchema>(collection);
            options.DeclarationOptions.DisableIntrospection = true;

            var provider = collection.BuildServiceProvider();

            var scope = provider.CreateScope();
            var config = options.CreateConfiguration();

            var instance = factory.CreateInstance(
                scope,
                config,
                new SchemaTypeToRegister[]
                {
                    new SchemaTypeToRegister(typeof(TwoPropertyObjectV2)),
                });

            Assert.IsNotNull(instance);

            Assert.AreEqual(5, instance.KnownTypes.Count);
            Assert.IsTrue(instance.KnownTypes.Contains(typeof(DateTime))); // field on v2
            Assert.IsTrue(instance.KnownTypes.Contains(typeof(float)));  // field on v2
            Assert.IsTrue(instance.KnownTypes.Contains(typeof(TwoPropertyObjectV2)));  // the object itself
            Assert.IsNotNull(instance.KnownTypes.SingleOrDefault(x => x.Name == Constants.ScalarNames.STRING));  // required for __typename
            Assert.IsNotNull(instance.KnownTypes.SingleOrDefault(x => x.Name == Constants.ReservedNames.QUERY_TYPE_NAME));

            var graphType = instance.KnownTypes.FindGraphType(typeof(TwoPropertyObjectV2));
            Assert.IsNotNull(graphType);
            Assert.IsTrue(graphType is IObjectGraphType);
            Assert.AreEqual(typeof(TwoPropertyObjectV2), ((IObjectGraphType)graphType).ObjectType);

            // the two declared properties + __typekind
            Assert.AreEqual(3, ((IObjectGraphType)graphType).Fields.Count);
        }

        [Test]
        public void OneInputObjectType_GeneratesCorrectly()
        {
            var collection = this.SetupCollection();

            var factory = new DefaultGraphQLSchemaFactory<GraphSchema>(includeBuiltInDirectives: false);
            var options = new SchemaOptions<GraphSchema>(collection);
            options.DeclarationOptions.DisableIntrospection = true;

            var provider = collection.BuildServiceProvider();

            var scope = provider.CreateScope();
            var config = options.CreateConfiguration();

            var instance = factory.CreateInstance(
                scope,
                config,
                new SchemaTypeToRegister[]
                {
                    new SchemaTypeToRegister(typeof(TwoPropertyObjectV2), TypeKind.INPUT_OBJECT),
                });

            Assert.IsNotNull(instance);

            Assert.AreEqual(5, instance.KnownTypes.Count);
            Assert.IsTrue(instance.KnownTypes.Contains(typeof(DateTime))); // field on v2
            Assert.IsTrue(instance.KnownTypes.Contains(typeof(float)));  // field on v2
            Assert.IsTrue(instance.KnownTypes.Contains(typeof(TwoPropertyObjectV2)));  // the object itself
            Assert.IsNotNull(instance.KnownTypes.SingleOrDefault(x => x.Name == Constants.ScalarNames.STRING));  // required for __typename
            Assert.IsNotNull(instance.KnownTypes.SingleOrDefault(x => x.Name == Constants.ReservedNames.QUERY_TYPE_NAME));

            var graphType = instance.KnownTypes.FindGraphType(typeof(TwoPropertyObjectV2));
            Assert.IsNotNull(graphType);
            Assert.IsTrue(graphType is IInputObjectGraphType);
            Assert.AreEqual(typeof(TwoPropertyObjectV2), ((IInputObjectGraphType)graphType).ObjectType);
            Assert.AreEqual(2, ((IInputObjectGraphType)graphType).Fields.Count);
        }

        [Test]
        public void OneInterfaceType_NoArgumentsOnFields_GeneratesCorrectly()
        {
            var collection = this.SetupCollection();

            var factory = new DefaultGraphQLSchemaFactory<GraphSchema>(includeBuiltInDirectives: false);
            var options = new SchemaOptions<GraphSchema>(collection);
            options.DeclarationOptions.DisableIntrospection = true;

            var provider = collection.BuildServiceProvider();

            var scope = provider.CreateScope();
            var config = options.CreateConfiguration();

            var instance = factory.CreateInstance(
                scope,
                config,
                new SchemaTypeToRegister[]
                {
                    new SchemaTypeToRegister(typeof(ISinglePropertyObject)),
                });

            Assert.IsNotNull(instance);

            Assert.AreEqual(3, instance.KnownTypes.Count);
            Assert.IsTrue(instance.KnownTypes.Contains(typeof(ISinglePropertyObject)));  // the object itself
            Assert.IsNotNull(instance.KnownTypes.SingleOrDefault(x => x.Name == Constants.ScalarNames.STRING));
            Assert.IsNotNull(instance.KnownTypes.SingleOrDefault(x => x.Name == Constants.ReservedNames.QUERY_TYPE_NAME));

            var graphType = instance.KnownTypes.FindGraphType(typeof(ISinglePropertyObject));
            Assert.IsNotNull(graphType);
            Assert.IsTrue(graphType is IInterfaceGraphType);
            Assert.AreEqual(typeof(ISinglePropertyObject), ((IInterfaceGraphType)graphType).ObjectType);

            // the one declared field + __typekind
            Assert.AreEqual(2, ((IInterfaceGraphType)graphType).Fields.Count);
        }

        [Test]
        public void OneObjectType_ArgumentsOnField_GeneratesCorrectly()
        {
            var collection = this.SetupCollection();

            var factory = new DefaultGraphQLSchemaFactory<GraphSchema>(includeBuiltInDirectives: false);
            var options = new SchemaOptions<GraphSchema>(collection);
            options.DeclarationOptions.DisableIntrospection = true;

            var provider = collection.BuildServiceProvider();

            var scope = provider.CreateScope();
            var config = options.CreateConfiguration();

            var instance = factory.CreateInstance(
                scope,
                config,
                new SchemaTypeToRegister[]
                {
                    new SchemaTypeToRegister(typeof(CustomObjectWithFieldWithArg)),
                });

            Assert.IsNotNull(instance);

            Assert.AreEqual(5, instance.KnownTypes.Count);
            Assert.IsTrue(instance.KnownTypes.Contains(typeof(decimal))); // argument on field
            Assert.IsTrue(instance.KnownTypes.Contains(typeof(int)));  // return type of field
            Assert.IsTrue(instance.KnownTypes.Contains(typeof(CustomObjectWithFieldWithArg)));  // the object itself
            Assert.IsNotNull(instance.KnownTypes.SingleOrDefault(x => x.Name == Constants.ScalarNames.STRING));  // required for __typename
            Assert.IsNotNull(instance.KnownTypes.SingleOrDefault(x => x.Name == Constants.ReservedNames.QUERY_TYPE_NAME));

            var graphType = instance.KnownTypes.FindGraphType(typeof(CustomObjectWithFieldWithArg));
            Assert.IsNotNull(graphType);
            Assert.IsTrue(graphType is IObjectGraphType);
            Assert.AreEqual(typeof(CustomObjectWithFieldWithArg), ((IObjectGraphType)graphType).ObjectType);

            // the declared method + __typekind
            Assert.AreEqual(2, ((IObjectGraphType)graphType).Fields.Count);
            var field = ((IObjectGraphType)graphType).Fields.SingleOrDefault(x => x.Name != Constants.ReservedNames.TYPENAME_FIELD);

            Assert.IsNotNull(field);
            Assert.AreEqual(1, field.Arguments.Count);
            Assert.AreEqual(typeof(decimal), field.Arguments[0].ObjectType);
            Assert.AreEqual("arg", field.Arguments[0].Name);
        }

        [Test]
        public void OneDirective_GeneratesCorrectly()
        {
            var collection = this.SetupCollection();

            var factory = new DefaultGraphQLSchemaFactory<GraphSchema>(includeBuiltInDirectives: false);
            var options = new SchemaOptions<GraphSchema>(collection);
            options.DeclarationOptions.DisableIntrospection = true;

            var provider = collection.BuildServiceProvider();

            var scope = provider.CreateScope();
            var config = options.CreateConfiguration();

            var instance = factory.CreateInstance(
                scope,
                config,
                new SchemaTypeToRegister[]
                {
                    new SchemaTypeToRegister(typeof(CustomDirective)),
                });

            Assert.IsNotNull(instance);

            Assert.AreEqual(4, instance.KnownTypes.Count);
            Assert.IsTrue(instance.KnownTypes.Contains(typeof(int)));  // argument on directive
            Assert.IsTrue(instance.KnownTypes.Contains(typeof(CustomDirective)));  // the directive itself
            Assert.IsNotNull(instance.KnownTypes.SingleOrDefault(x => x.Name == Constants.ScalarNames.STRING));  // required for __typename
            Assert.IsNotNull(instance.KnownTypes.SingleOrDefault(x => x.Name == Constants.ReservedNames.QUERY_TYPE_NAME));

            var graphType = instance.KnownTypes.FindGraphType(typeof(CustomDirective));
            Assert.IsNotNull(graphType);
            Assert.IsTrue(graphType is IDirective);
            Assert.AreEqual(typeof(CustomDirective), ((IDirective)graphType).ObjectType);

            // the declared method + __typekind
            Assert.AreEqual(1, ((IDirective)graphType).Arguments.Count);
            Assert.AreEqual(typeof(int), ((IDirective)graphType).Arguments[0].ObjectType);
        }

        [Test]
        public void OneController_GeneratesCorrectly()
        {
            var collection = this.SetupCollection();

            var factory = new DefaultGraphQLSchemaFactory<GraphSchema>(includeBuiltInDirectives: false);
            var options = new SchemaOptions<GraphSchema>(collection);
            options.DeclarationOptions.DisableIntrospection = true;

            var provider = collection.BuildServiceProvider();

            var scope = provider.CreateScope();
            var config = options.CreateConfiguration();

            var instance = factory.CreateInstance(
                scope,
                config,
                new SchemaTypeToRegister[]
                {
                    new SchemaTypeToRegister(typeof(CustomController)),
                });

            Assert.IsNotNull(instance);

            Assert.AreEqual(5, instance.KnownTypes.Count);
            Assert.IsTrue(instance.KnownTypes.Contains(typeof(VirtualResolvedObject)));  // intermediary resolved value on the controller
            Assert.IsTrue(instance.KnownTypes.Contains(typeof(int)));  // arg of controller field
            Assert.IsNotNull(instance.KnownTypes.FindGraphType("Query_Custom"));  // intermediate type for the controller
            Assert.IsNotNull(instance.KnownTypes.SingleOrDefault(x => x.Name == Constants.ScalarNames.STRING));  // required for __typename
            Assert.IsNotNull(instance.KnownTypes.SingleOrDefault(x => x.Name == Constants.ReservedNames.QUERY_TYPE_NAME));
        }

        [Test]
        public void ClassArgumentToAField_ThatIsRegisteredAsAScalar_IsNamedProperly()
        {
            var collection = this.SetupCollection();

            var factory = new DefaultGraphQLSchemaFactory<GraphSchema>(includeBuiltInDirectives: false);
            var options = new SchemaOptions<GraphSchema>(collection);
            options.DeclarationOptions.DisableIntrospection = true;

            var provider = collection.BuildServiceProvider();

            var scope = provider.CreateScope();
            var config = options.CreateConfiguration();

            var instance = factory.CreateInstance(
                scope,
                config,
                new SchemaTypeToRegister[]
                {
                    new SchemaTypeToRegister(typeof(TwoPropertyObjectAsScalar)),
                    new SchemaTypeToRegister(typeof(CustomObjectWithCustomScalarArgument)),
                });

            Assert.IsNotNull(instance);

            var graphType = instance.KnownTypes.FindGraphType(typeof(CustomObjectWithCustomScalarArgument)) as IObjectGraphType;
            var field = graphType.Fields.SingleOrDefault(x => string.Compare(x.Name, nameof(CustomObjectWithCustomScalarArgument.FieldWithScalarArg), true) == 0);
            Assert.IsNotNull(field);

            var arg = field.Arguments[0];

            Assert.AreEqual("obj", arg.Name);

            // ensure the type expression points to the scalar name
            // not the name as if it was an input object
            Assert.AreEqual(nameof(TwoPropertyObjectAsScalar), arg.TypeExpression.TypeName);
        }

        [Test]
        public void ReturnValueOfAField_ThatIsRegisteredAsAScalar_IsNamedProperly()
        {
            var collection = this.SetupCollection();

            var factory = new DefaultGraphQLSchemaFactory<GraphSchema>(includeBuiltInDirectives: false);
            var options = new SchemaOptions<GraphSchema>(collection);
            options.DeclarationOptions.DisableIntrospection = true;

            var provider = collection.BuildServiceProvider();

            var scope = provider.CreateScope();
            var config = options.CreateConfiguration();

            var instance = factory.CreateInstance(
                scope,
                config,
                new SchemaTypeToRegister[]
                {
                    new SchemaTypeToRegister(typeof(TwoPropertyObjectAsScalar)),
                    new SchemaTypeToRegister(typeof(CustomObjectWithCustomScalarField)),
                });

            Assert.IsNotNull(instance);

            var graphType = instance.KnownTypes.FindGraphType(typeof(CustomObjectWithCustomScalarField)) as IObjectGraphType;
            var field = graphType.Fields.SingleOrDefault(x => string.Compare(x.Name, nameof(CustomObjectWithCustomScalarField.FieldWithScalarReturnValue), true) == 0);
            Assert.IsNotNull(field);

            // ensure the type expression points to the scalar name
            // not the name as if it was an input object
            Assert.AreEqual(nameof(TwoPropertyObjectAsScalar), field.TypeExpression.TypeName);
        }
    }
}