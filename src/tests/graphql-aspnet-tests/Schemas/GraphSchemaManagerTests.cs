// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Schemas
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Internal.Resolvers;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.CommonHelpers;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Schemas.SchemaTestData;
    using NUnit.Framework;

    [TestFixture]
    public class GraphSchemaManagerTests
    {
        public enum RandomEnum
        {
            Value0,
            Value1,
            Value2,
        }

        [Test]
        public void AddIntrospectionFields_AllFieldsAdded()
        {
            var schema = new GraphSchema() as ISchema;
            var manager = new GraphSchemaManager(schema);
            manager.AddIntrospectionFields();

            Assert.AreEqual(2, schema.KnownTypes.Count(x => x.Kind == TypeKind.SCALAR));
            Assert.IsTrue(schema.KnownTypes.Contains(GraphQLProviders.ScalarProvider.RetrieveScalar(Constants.ScalarNames.STRING)));
            Assert.IsTrue(schema.KnownTypes.Contains(GraphQLProviders.ScalarProvider.RetrieveScalar(Constants.ScalarNames.BOOLEAN)));

            Assert.AreEqual(2, schema.KnownTypes.Count(x => x.Kind == TypeKind.ENUM));
            Assert.IsTrue(schema.KnownTypes.Contains(typeof(DirectiveLocation), TypeKind.ENUM));
            Assert.IsTrue(schema.KnownTypes.Contains(typeof(TypeKind), TypeKind.ENUM));

            Assert.AreEqual(7, schema.KnownTypes.Count(x => x.Kind == TypeKind.OBJECT));
            Assert.IsTrue(schema.KnownTypes.Contains(Constants.ReservedNames.QUERY_TYPE_NAME));
            Assert.IsTrue(schema.KnownTypes.Contains(Constants.ReservedNames.DIRECTIVE_TYPE));
            Assert.IsTrue(schema.KnownTypes.Contains(Constants.ReservedNames.ENUM_VALUE_TYPE));
            Assert.IsTrue(schema.KnownTypes.Contains(Constants.ReservedNames.FIELD_TYPE));
            Assert.IsTrue(schema.KnownTypes.Contains(Constants.ReservedNames.INPUT_VALUE_TYPE));
            Assert.IsTrue(schema.KnownTypes.Contains(Constants.ReservedNames.SCHEMA_TYPE));
            Assert.IsTrue(schema.KnownTypes.Contains(Constants.ReservedNames.TYPE_TYPE));
        }

        [Test]
        public void AddSingleQueryAction_AllDefaults_EnsureFieldStructure()
        {
            var schema = new GraphSchema() as ISchema;
            schema.SetNoAlterationConfiguration();
            var manager = new GraphSchemaManager(schema);
            manager.EnsureGraphType<SimpleMethodController>();

            var action = TemplateHelper.CreateFieldTemplate<SimpleMethodController>(nameof(SimpleMethodController.TestActionMethod));

            // query root exists, mutation does not (nothing was added to it)
            Assert.IsTrue(schema.OperationTypes.ContainsKey(GraphCollection.Query));
            Assert.IsFalse(schema.OperationTypes.ContainsKey(GraphCollection.Mutation));

            // field for the controller exists
            var topFieldName = nameof(SimpleMethodController).Replace("Controller", string.Empty);
            Assert.IsTrue(schema.OperationTypes[GraphCollection.Query].Fields.ContainsKey(topFieldName));

            // ensure the field on the query is the right name (or throw)
            var topField = schema.OperationTypes[GraphCollection.Query][topFieldName];
            Assert.IsNotNull(topField);

            var type = schema.KnownTypes.FindGraphType(topField) as IObjectGraphType;

            // ensure the action was put into the field collection of the controller operation
            Assert.IsTrue(type.Fields.ContainsKey(action.Route.Name));
        }

        [Test]
        public void AddSingleQueryAction_NestedRouting_EnsureFieldStructure()
        {
            var schema = new GraphSchema() as ISchema;
            var manager = new GraphSchemaManager(schema);
            manager.EnsureGraphType<NestedQueryMethodController>();

            // query root exists, mutation does not (nothing was added to it)
            Assert.IsTrue(schema.OperationTypes.ContainsKey(GraphCollection.Query));
            Assert.IsFalse(schema.OperationTypes.ContainsKey(GraphCollection.Mutation));

            // field for the controller exists
            var fieldName = "path0";
            Assert.IsTrue(schema.OperationTypes[GraphCollection.Query].Fields.ContainsKey(fieldName));

            var topField = schema.OperationTypes[GraphCollection.Query][fieldName];
            var type = schema.KnownTypes.FindGraphType(topField) as IObjectGraphType;
            Assert.IsNotNull(type);
            Assert.AreEqual(1, type.Fields.Count);

            // field contains 1 field for first path segment
            Assert.IsTrue(type.Fields.ContainsKey("path1"));
            var firstField = type["path1"] as VirtualGraphField;
            var firstFieldType = schema.KnownTypes.FindGraphType(firstField) as IObjectGraphType;

            Assert.IsNotNull(firstFieldType);
            Assert.AreEqual(1, firstFieldType.Fields.Count);
            Assert.IsTrue(firstFieldType.Fields.ContainsKey("path2"));

            var actionField = firstFieldType.Fields["path2"];
            Assert.IsNotNull(actionField);
            Assert.IsNotNull(actionField.Resolver as GraphControllerActionResolver);
            Assert.AreEqual(typeof(TwoPropertyObjectV2), ((GraphControllerActionResolver)actionField.Resolver).ObjectType);
        }

        [Test]
        public void AddSingleQueryAction_AllDefaults_EnsureTypeStructure()
        {
            var schema = new GraphSchema() as ISchema;
            var manager = new GraphSchemaManager(schema);
            manager.EnsureGraphType<SimpleMethodController>();

            // scalars for arguments on the method exists
            Assert.IsTrue(schema.KnownTypes.Contains(GraphQLProviders.ScalarProvider.RetrieveScalar(Constants.ScalarNames.STRING)));
            Assert.IsTrue(schema.KnownTypes.Contains(GraphQLProviders.ScalarProvider.RetrieveScalar(Constants.ScalarNames.INT)));

            // return type exists as an object type
            var returnType = typeof(SimpleMethodController).GetMethod(nameof(SimpleMethodController.TestActionMethod)).ReturnType;
            Assert.IsTrue(schema.KnownTypes.Contains(returnType, TypeKind.OBJECT));
        }

        [Test]
        public void AddSingleQueryAction_NestedRouting_EnsureTypeStructure()
        {
            var schema = new GraphSchema();
            schema.SetNoAlterationConfiguration();

            var manager = new GraphSchemaManager(schema);
            manager.EnsureGraphType<NestedQueryMethodController>();

            // query root exists, mutation does not (nothing was added to it)
            Assert.AreEqual(1, schema.OperationTypes.Count);
            Assert.IsTrue(schema.OperationTypes.ContainsKey(GraphCollection.Query));
            Assert.IsFalse(schema.OperationTypes.ContainsKey(GraphCollection.Mutation));

            Assert.AreEqual(7, schema.KnownTypes.Count);

            // expect 2 scalars
            Assert.IsTrue(schema.KnownTypes.Contains(GraphQLProviders.ScalarProvider.RetrieveScalar(Constants.ScalarNames.FLOAT)));
            Assert.IsTrue(schema.KnownTypes.Contains(GraphQLProviders.ScalarProvider.RetrieveScalar(Constants.ScalarNames.DATETIME)));

            // expect 5 types to be generated
            // ----------------------------------
            // the query operation type
            // the top level field representing the controller, "path0"
            // the middle level defined on the method "path1"
            // the return type from the method itself
            // the return type of the routes
            Assert.IsTrue(schema.KnownTypes.Contains("Query"));
            Assert.IsTrue(schema.KnownTypes.Contains("Query_path0"));
            Assert.IsTrue(schema.KnownTypes.Contains("Query_path0_path1"));
            Assert.IsTrue(schema.KnownTypes.Contains(nameof(TwoPropertyObjectV2)));
            Assert.IsTrue(schema.KnownTypes.Contains(nameof(VirtualResolvedObject)));

            // expect 2 actual reference type to be assigned
            //      the return type from the method and the virtual result fro the route
            // all others are virtual types in this instance
            Assert.IsTrue(schema.KnownTypes.Contains(typeof(TwoPropertyObjectV2), TypeKind.OBJECT));
            Assert.IsTrue(schema.KnownTypes.Contains(typeof(VirtualResolvedObject), TypeKind.OBJECT));
        }

        [Test]
        public void AddSingleQueryAction_NestedObjectsOnReturnType_EnsureAllTypesAreAdded()
        {
            var schema = new GraphSchema() as ISchema;
            schema.SetNoAlterationConfiguration();
            var manager = new GraphSchemaManager(schema);
            manager.EnsureGraphType<NestedMutationMethodController>();

            // mutation root exists and query exists (it must by definition even if blank)
            Assert.AreEqual(2, schema.OperationTypes.Count);
            Assert.IsTrue(schema.OperationTypes.ContainsKey(GraphCollection.Query));
            Assert.IsTrue(schema.OperationTypes.ContainsKey(GraphCollection.Mutation));

            // 5 distinct scalars (int, uint, float, decimal, string)
            Assert.AreEqual(5, schema.KnownTypes.Count(x => x.Kind == TypeKind.SCALAR));
            Assert.IsTrue(schema.KnownTypes.Contains(GraphQLProviders.ScalarProvider.RetrieveScalar(Constants.ScalarNames.STRING)));
            Assert.IsTrue(schema.KnownTypes.Contains(GraphQLProviders.ScalarProvider.RetrieveScalar(Constants.ScalarNames.INT)));
            Assert.IsTrue(schema.KnownTypes.Contains(GraphQLProviders.ScalarProvider.RetrieveScalar(Constants.ScalarNames.DECIMAL)));
            Assert.IsTrue(schema.KnownTypes.Contains(GraphQLProviders.ScalarProvider.RetrieveScalar(Constants.ScalarNames.FLOAT)));
            Assert.IsTrue(schema.KnownTypes.Contains(GraphQLProviders.ScalarProvider.RetrieveScalar(Constants.ScalarNames.UINT)));

            // 8 types
            // ----------------------
            //  mutation operation-type
            //  query operation-type
            //  path0 segment
            //  PersonData
            //  JobData
            //  AddressData
            //  CountryData
            //  VirtualResolvedObject
            Assert.AreEqual(8, schema.KnownTypes.Count(x => x.Kind == TypeKind.OBJECT));

            // expect a type for the root operation type
            Assert.IsTrue(schema.KnownTypes.Contains(Constants.ReservedNames.MUTATION_TYPE_NAME));
            Assert.IsTrue(schema.KnownTypes.Contains(Constants.ReservedNames.QUERY_TYPE_NAME));

            // expect a type representing the controller top level path
            Assert.IsTrue(schema.KnownTypes.Contains($"{Constants.ReservedNames.MUTATION_TYPE_NAME}_path0"));

            // expect a type for the method return type
            Assert.IsTrue(schema.KnownTypes.Contains(nameof(PersonData)));

            // person data contains job data
            Assert.IsTrue(schema.KnownTypes.Contains(typeof(JobData), TypeKind.OBJECT));

            // person data contains address data
            Assert.IsTrue(schema.KnownTypes.Contains(typeof(AddressData), TypeKind.OBJECT));

            // address data contains country data
            Assert.IsTrue(schema.KnownTypes.Contains(typeof(CountryData), TypeKind.OBJECT));
        }

        [Test]
        public void KitchenSinkController_SchemaInjection_FullFieldStructureAndTypeCheck()
        {
            var schema = new GraphSchema() as ISchema;
            schema.SetNoAlterationConfiguration();
            var manager = new GraphSchemaManager(schema);

            manager.EnsureGraphType<KitchenSinkController>();

            // mutation and query
            Assert.AreEqual(2, schema.OperationTypes.Count);

            // Query Inspection
            // ------------------------------

            // the controller segment: "Query_path0"
            // the method "myActionOperation" should register as a root query
            // The Query root itself contains the `__typename` metafield
            Assert.AreEqual(3, schema.OperationTypes[GraphCollection.Query].Fields.Count);
            var controllerQueryField = schema.OperationTypes[GraphCollection.Query]["path0"];
            var methodAsQueryRootField = schema.OperationTypes[GraphCollection.Query]["myActionOperation"];
            Assert.IsNotNull(schema.OperationTypes[GraphCollection.Query][Constants.ReservedNames.TYPENAME_FIELD]);

            // deep inspection of the created controller-query-field
            Assert.IsNotNull(controllerQueryField);
            Assert.AreEqual(0, controllerQueryField.Arguments.Count);
            Assert.AreEqual("Kitchen sinks are great", controllerQueryField.Description);

            // the top level controller field should have one field on it
            // created from the sub path on the controller route definition "path1"
            // that field should be registered as a virtual field
            var controllerQueryFieldType = schema.KnownTypes.FindGraphType(controllerQueryField) as IObjectGraphType;
            Assert.AreEqual(1, controllerQueryFieldType.Fields.Count);
            var queryPath1 = controllerQueryFieldType.Fields["path1"];
            Assert.IsTrue(queryPath1 is VirtualGraphField);
            Assert.AreEqual(string.Empty, queryPath1.Description);

            // the virtual field (path1) should have two real actions (TestActionMethod, TestAction2)
            // and 1 virtual field ("path2") hung off it
            var queryPath1Type = schema.KnownTypes.FindGraphType(queryPath1) as IObjectGraphType;
            Assert.IsTrue(queryPath1Type is VirtualObjectGraphType);
            Assert.AreEqual(2, queryPath1Type.Fields.Count);

            Assert.IsTrue(queryPath1Type.Fields.Any(x => x.Name == "TestActionMethod"));
            Assert.IsTrue(queryPath1Type.Fields.Any(x => x.Name == "TestAction2"));

            // path 2 is only declared on mutations
            Assert.IsFalse(queryPath1Type.Fields.ContainsKey("path2"));

            // top level query field made from a controller method
            Assert.IsNotNull(methodAsQueryRootField);
            Assert.AreEqual("myActionOperation", methodAsQueryRootField.Name);
            Assert.AreEqual("This is my\n Top Level Query Field", methodAsQueryRootField.Description);

            // Mutation Inspection
            // ------------------------------
            var controllerMutationField = schema.OperationTypes[GraphCollection.Mutation]["path0"];
            var methodAsMutationTopLevelField = schema.OperationTypes[GraphCollection.Mutation]["SupeMutation"];

            // deep inspection of the created controller-mutation-field
            Assert.IsNotNull(controllerMutationField);
            Assert.AreEqual(0, controllerMutationField.Arguments.Count);
            Assert.AreEqual("Kitchen sinks are great", controllerMutationField.Description);

            // the controller field on the mutation side should have one field on it
            // created from the sub path on the controller route definition "path1"
            // that field should be registered as a virtual field
            var controllerMutationFieldType = schema.KnownTypes.FindGraphType(controllerMutationField) as IObjectGraphType;
            Assert.AreEqual(1, controllerMutationFieldType.Fields.Count);
            var mutationPath1 = controllerMutationFieldType.Fields["path1"];
            Assert.IsTrue(mutationPath1 is VirtualGraphField);
            Assert.AreEqual(string.Empty, mutationPath1.Description);

            // walk down the mutationPath through all its nested layers to the action method
            // let an exception be thrown (incorrectly) if any path segment doesnt exist
            var mutationPath1Type = schema.KnownTypes.FindGraphType(mutationPath1) as IObjectGraphType;
            var childField = mutationPath1Type.Fields["path2"];
            var childFieldType = schema.KnownTypes.FindGraphType(childField) as IObjectGraphType;
            Assert.AreEqual(string.Empty, childField.Description);
            Assert.IsFalse(childField.IsDeprecated);

            childField = childFieldType.Fields["PAth3"];
            childFieldType = schema.KnownTypes.FindGraphType(childField) as IObjectGraphType;
            Assert.AreEqual(string.Empty, childField.Description);
            Assert.IsFalse(childField.IsDeprecated);

            childField = childFieldType.Fields["PaTh4"];
            childFieldType = schema.KnownTypes.FindGraphType(childField) as IObjectGraphType;
            Assert.AreEqual(string.Empty, childField.Description);
            Assert.IsFalse(childField.IsDeprecated);

            childField = childFieldType.Fields["PAT_H5"];
            childFieldType = schema.KnownTypes.FindGraphType(childField) as IObjectGraphType;
            Assert.AreEqual(string.Empty, childField.Description);
            Assert.IsFalse(childField.IsDeprecated);

            childField = childFieldType.Fields["pathSix"];
            childFieldType = schema.KnownTypes.FindGraphType(childField) as IObjectGraphType;
            Assert.AreEqual(string.Empty, childField.Description);
            Assert.IsFalse(childField.IsDeprecated);

            var mutationAction = childFieldType.Fields["deepNestedMethod"];
            Assert.AreEqual("This is a mutation", mutationAction.Description);
            Assert.IsTrue(mutationAction.IsDeprecated);
            Assert.AreEqual("To be removed tomorrow", mutationAction.DeprecationReason);

            // check the top level mutation  field
            Assert.AreEqual("SupeMutation", methodAsMutationTopLevelField.Name);
            Assert.AreEqual("This is my\n Top Level MUtation Field!@@!!", methodAsMutationTopLevelField.Description);

            // Type Checks
            // -----------------------------------------------------
            // scalars (2): string, int (from TwoPropertyObject)
            // scalars (2): float, datetime (from TwoPropertyObjectV2)
            // scalars (2): ulong, long (from method declarations)
            // scalars (1): dceimal (from CompletePropertyObject)
            // the nullable<T> types resolve to their non-nullable scalar in the type list
            Assert.AreEqual(7, schema.KnownTypes.Count(x => x.Kind == TypeKind.SCALAR));
            Assert.IsTrue(schema.KnownTypes.Contains(GraphQLProviders.ScalarProvider.RetrieveScalar(typeof(string))));
            Assert.IsTrue(schema.KnownTypes.Contains(GraphQLProviders.ScalarProvider.RetrieveScalar(typeof(int))));
            Assert.IsTrue(schema.KnownTypes.Contains(GraphQLProviders.ScalarProvider.RetrieveScalar(typeof(ulong))));
            Assert.IsTrue(schema.KnownTypes.Contains(GraphQLProviders.ScalarProvider.RetrieveScalar(typeof(DateTime))));
            Assert.IsTrue(schema.KnownTypes.Contains(GraphQLProviders.ScalarProvider.RetrieveScalar(typeof(DateTime?))));
            Assert.IsTrue(schema.KnownTypes.Contains(GraphQLProviders.ScalarProvider.RetrieveScalar(typeof(long))));
            Assert.IsTrue(schema.KnownTypes.Contains(GraphQLProviders.ScalarProvider.RetrieveScalar(typeof(long?))));
            Assert.IsTrue(schema.KnownTypes.Contains(GraphQLProviders.ScalarProvider.RetrieveScalar(typeof(decimal))));

            Assert.IsFalse(schema.KnownTypes.Contains(GraphQLProviders.ScalarProvider.RetrieveScalar(typeof(double))));

            // enumerations
            Assert.AreEqual(1, schema.KnownTypes.Count(x => x.Kind == TypeKind.ENUM));
            Assert.IsTrue(schema.KnownTypes.Contains(typeof(TestEnumerationOptions), TypeKind.ENUM));

            // input type checks  (TwoPropertyObject, EmptyObject)
            Assert.AreEqual(2, schema.KnownTypes.Count(x => x.Kind == TypeKind.INPUT_OBJECT));
            Assert.IsTrue(schema.KnownTypes.Contains(typeof(EmptyObject), TypeKind.INPUT_OBJECT));
            Assert.IsTrue(schema.KnownTypes.Contains(typeof(TwoPropertyObject), TypeKind.INPUT_OBJECT));

            // general object types
            var concreteTypes = schema.KnownTypes.Where(x => (x is ObjectGraphType)).ToList();
            Assert.AreEqual(5, concreteTypes.Count);
            Assert.IsTrue(schema.KnownTypes.Contains(typeof(Person), TypeKind.OBJECT));
            Assert.IsTrue(schema.KnownTypes.Contains(typeof(CompletePropertyObject), TypeKind.OBJECT));
            Assert.IsTrue(schema.KnownTypes.Contains(typeof(TwoPropertyObject), TypeKind.OBJECT));
            Assert.IsTrue(schema.KnownTypes.Contains(typeof(TwoPropertyObjectV2), TypeKind.OBJECT));
            Assert.IsTrue(schema.KnownTypes.Contains(typeof(VirtualResolvedObject), TypeKind.OBJECT));

            // 9 "route" types should ahve been created
            // -----------------------------
            // 1.  controller query field (path0)
            // 2.       query virtual path segment path1
            // 3.  controller mutation field (path0)
            // 4.        mutation virtual path segment path1
            // 5.        mutation virtual path segment path2
            // 6.        mutation virtual path segment PAth3
            // 7.        mutation virtual path segment PaTh4
            // 8.        mutation virtual path segment PAT_H5
            // 9.        mutation virtual path segment pathSix
            var virtualTypes = schema.KnownTypes.Where(x => x is VirtualObjectGraphType).ToList();
            Assert.AreEqual(9, virtualTypes.Count);

            // pathSix should have one "real" field, the method named 'deepNestedMethod'
            var pathSix = virtualTypes.FirstOrDefault(x => x.Name.Contains("pathSix")) as IObjectGraphType;
            Assert.IsNotNull(pathSix);
            Assert.AreEqual(1, pathSix.Fields.Count);
            Assert.IsNotNull(pathSix["deepNestedMethod"]);

            // query_path1 should have two "real" fields, the method named 'TestActionMethod' and 'TestAction2'
            var querPath1 = virtualTypes.FirstOrDefault(x => x.Name.Contains("Query_path0_path1")) as IObjectGraphType;

            Assert.IsNotNull(querPath1);
            Assert.AreEqual(2, querPath1.Fields.Count);
            Assert.IsNotNull(querPath1[nameof(KitchenSinkController.TestActionMethod)]);
            Assert.IsNotNull(querPath1["TestAction2"]);
        }

        [Test]
        public void EnsureGraphType_NormalObject_IsAddedWithTypeReference()
        {
            var schema = new GraphSchema() as ISchema;
            var manager = new GraphSchemaManager(schema);

            manager.EnsureGraphType(typeof(CountryData));

            // CountryData, string, float?, Query
            Assert.AreEqual(4, schema.KnownTypes.Count);
            Assert.AreEqual(3, schema.KnownTypes.TypeReferences.Count());

            Assert.IsTrue(schema.KnownTypes.Contains(typeof(CountryData), TypeKind.OBJECT));
            Assert.IsTrue(schema.KnownTypes.Contains("CountryData"));
        }

        [Test]
        public void EnsureGraphType_Enum_WithNoKindSupplied_IsAddedCorrectly()
        {
            var schema = new GraphSchema() as ISchema;
            var manager = new GraphSchemaManager(schema);
            manager.EnsureGraphType(typeof(RandomEnum));
            Assert.AreEqual(2, schema.KnownTypes.Count);  // added type + query
            Assert.IsTrue(schema.KnownTypes.Contains(typeof(RandomEnum)));
        }

        [Test]
        public void EnsureGraphType_Enum_WithKindSupplied_IsAddedCorrectly()
        {
            var schema = new GraphSchema() as ISchema;
            var manager = new GraphSchemaManager(schema);
            manager.EnsureGraphType(typeof(RandomEnum), TypeKind.ENUM);
            Assert.AreEqual(2, schema.KnownTypes.Count);  // added type + query
            Assert.IsTrue(schema.KnownTypes.Contains(typeof(RandomEnum), TypeKind.ENUM));
        }

        [Test]
        public void EnsureGraphType_Enum_WithIncorrectKindSupplied_ThrowsException()
        {
            var schema = new GraphSchema() as ISchema;
            var manager = new GraphSchemaManager(schema);
            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                manager.EnsureGraphType(typeof(RandomEnum), TypeKind.SCALAR);
            });
        }

        [Test]
        public void EnsureGraphType_Enum_WithIncorrectKindSupplied_ThatIsCoercable_AddsCorrectly()
        {
            var schema = new GraphSchema() as ISchema;
            var manager = new GraphSchemaManager(schema);

            // object will be coerced to enum
            manager.EnsureGraphType(typeof(RandomEnum), TypeKind.OBJECT);
            Assert.AreEqual(2, schema.KnownTypes.Count);  // added type + query
            Assert.IsTrue(schema.KnownTypes.Contains(typeof(RandomEnum), TypeKind.ENUM));
            Assert.IsFalse(schema.KnownTypes.Contains(typeof(RandomEnum), TypeKind.OBJECT));
        }

        [Test]
        public void EnsureGraphType_Scalar_WithNoKindSupplied_IsAddedCorrectly()
        {
            var schema = new GraphSchema() as ISchema;
            var manager = new GraphSchemaManager(schema);
            manager.EnsureGraphType(typeof(string));
            Assert.AreEqual(2, schema.KnownTypes.Count);  // added type + query
            Assert.IsTrue(schema.KnownTypes.Contains(typeof(string)));
        }

        [Test]
        public void EnsureGraphType_Scalar_WithKindSupplied_IsAddedCorrectly()
        {
            var schema = new GraphSchema() as ISchema;
            var manager = new GraphSchemaManager(schema);
            manager.EnsureGraphType(typeof(string), TypeKind.SCALAR);
            Assert.AreEqual(2, schema.KnownTypes.Count);  // added type + query
            Assert.IsTrue(schema.KnownTypes.Contains(typeof(string), TypeKind.SCALAR));
        }

        [Test]
        public void EnsureGraphType_Scalar_WithIncorrectKindSupplied_ThrowsException()
        {
            var schema = new GraphSchema() as ISchema;
            var manager = new GraphSchemaManager(schema);
            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                manager.EnsureGraphType(typeof(string), TypeKind.ENUM);
            });
        }

        [Test]
        public void EnsureGraphType_ScalarTwice_EndsUpInScalarCollectionOnce()
        {
            var schema = new GraphSchema() as ISchema;
            var manager = new GraphSchemaManager(schema);
            manager.EnsureGraphType(typeof(int));
            manager.EnsureGraphType(typeof(int));

            Assert.AreEqual(2, schema.KnownTypes.Count);  // added type + query
            Assert.IsTrue(schema.KnownTypes.Contains(GraphQLProviders.ScalarProvider.RetrieveScalar(typeof(int))));
            Assert.IsTrue(schema.KnownTypes.Contains(GraphQLProviders.ScalarProvider.RetrieveScalar(Constants.ScalarNames.INT)));
            Assert.IsTrue(schema.KnownTypes.Contains(typeof(int)));
        }

        [Test]
        public void EnsureGraphType_TwoScalar_EndsUpInScalarCollectionOnceEach()
        {
            var schema = new GraphSchema() as ISchema;
            var manager = new GraphSchemaManager(schema);
            manager.EnsureGraphType(typeof(int));
            manager.EnsureGraphType(typeof(long));

            Assert.AreEqual(3, schema.KnownTypes.Count); // added types + query
            Assert.IsTrue(schema.KnownTypes.Contains(GraphQLProviders.ScalarProvider.RetrieveScalar(typeof(int))));
            Assert.IsTrue(schema.KnownTypes.Contains(GraphQLProviders.ScalarProvider.RetrieveScalar(Constants.ScalarNames.INT)));
            Assert.IsTrue(schema.KnownTypes.Contains(GraphQLProviders.ScalarProvider.RetrieveScalar(typeof(long))));
            Assert.IsTrue(schema.KnownTypes.Contains(GraphQLProviders.ScalarProvider.RetrieveScalar(Constants.ScalarNames.LONG)));
        }

        [Test]
        public void EnsureGraphType_IEnumerableT_EndsWithTInGraphType()
        {
            var schema = new GraphSchema() as ISchema;
            var manager = new GraphSchemaManager(schema);
            manager.EnsureGraphType(typeof(IEnumerable<int>));

            Assert.AreEqual(2, schema.KnownTypes.Count);  // added type + query
            Assert.IsTrue(schema.KnownTypes.Contains(typeof(int)));
            Assert.IsTrue(schema.KnownTypes.Contains(Constants.ScalarNames.INT));
        }

        [Test]
        public void EnsureGraphType_ListT_EndsWithTInGraphType()
        {
            var schema = new GraphSchema() as ISchema;
            var manager = new GraphSchemaManager(schema);
            manager.EnsureGraphType(typeof(List<int>));

            Assert.AreEqual(2, schema.KnownTypes.Count);  // added type + query
            Assert.IsTrue(schema.KnownTypes.Contains(typeof(int)));
            Assert.IsTrue(schema.KnownTypes.Contains(Constants.ScalarNames.INT));
        }

        [Test]
        public void EnsureGraphType_ListT_AfterManualAddOfScalar_Succeeds()
        {
            var schema = new GraphSchema() as ISchema;
            var manager = new GraphSchemaManager(schema);

            var intScalar = GraphQLProviders.ScalarProvider.RetrieveScalar(typeof(int));
            schema.KnownTypes.EnsureGraphType(intScalar, typeof(int));
            manager.EnsureGraphType(typeof(List<int>));

            Assert.AreEqual(2, schema.KnownTypes.Count);  // added type + query
            Assert.IsTrue(schema.KnownTypes.Contains(typeof(int)));
            Assert.IsTrue(schema.KnownTypes.Contains(Constants.ScalarNames.INT));
        }

        [Test]
        public void EnsureGraphType_DictionaryTK_ThrowsExeption()
        {
            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                var schema = new GraphSchema() as ISchema;
                var manager = new GraphSchemaManager(schema);
                manager.EnsureGraphType(typeof(Dictionary<string, int>));
            });
        }

        [Test]
        public void EnsureGraphType_WithSubTypes_AreAddedCorrectly()
        {
            var schema = new GraphSchema() as ISchema;
            schema.SetNoAlterationConfiguration();
            var manager = new GraphSchemaManager(schema);
            manager.EnsureGraphType(typeof(PersonData));

            Assert.AreEqual(10, schema.KnownTypes.Count); // added types + query
            Assert.IsTrue(schema.KnownTypes.Contains(typeof(AddressData)));
            Assert.IsTrue(schema.KnownTypes.Contains(typeof(PersonData)));
            Assert.IsTrue(schema.KnownTypes.Contains(typeof(CountryData)));
            Assert.IsTrue(schema.KnownTypes.Contains(typeof(JobData)));
            Assert.IsTrue(schema.KnownTypes.Contains(typeof(string)));
            Assert.IsTrue(schema.KnownTypes.Contains(typeof(float)));
            Assert.IsTrue(schema.KnownTypes.Contains(typeof(float?)));
            Assert.IsTrue(schema.KnownTypes.Contains(typeof(uint)));
            Assert.IsTrue(schema.KnownTypes.Contains(typeof(decimal)));
        }
    }
}