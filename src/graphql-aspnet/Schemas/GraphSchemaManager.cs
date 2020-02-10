// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Schemas
{
    using System;
    using System.Collections.Generic;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Configuration.Formatting;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Internal;
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Internal.Introspection.Fields;
    using GraphQL.AspNet.Internal.Introspection.Model;
    using GraphQL.AspNet.Internal.Introspection.Types;
    using GraphQL.AspNet.Internal.TypeTemplates;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// Manages the relationships between <see cref="GraphController"/>s and an <see cref="ISchema"/>.
    /// Assists with the conversion, creation and reporting of <see cref="IGraphType"/> data
    /// parsed from any <see cref="GraphController"/> or added manually.
    /// </summary>
    public class GraphSchemaManager
    {
        private readonly GraphNameFormatter _formatter;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphSchemaManager" /> class.
        /// </summary>
        /// <param name="schema">The schema.</param>
        public GraphSchemaManager(ISchema schema)
        {
            this.Schema = Validation.ThrowIfNullOrReturn(schema, nameof(schema));
            _formatter = this.Schema.Configuration.DeclarationOptions.GraphNamingFormatter;
            this.EnsureGraphOperationType(GraphCollection.Query);
        }

        /// <summary>
        /// Adds the internal introspection fields to the query operation type if and only if the contained schema allows
        /// it through its internal configuration. This method is idempotent.
        /// </summary>
        public void AddIntrospectionFields()
        {
            if (this.Schema.Configuration.DeclarationOptions.DisableIntrospection)
                return;

            this.EnsureGraphOperationType(GraphCollection.Query);
            var queryField = this.Schema.OperationTypes[GraphCollection.Query];

            // Note: introspection fields are defined by the graphql spec, no custom name or item formatting is allowed
            // for Type and field name formatting.
            // spec: https://graphql.github.io/graphql-spec/June2018/#sec-Schema-Introspection
            if (!queryField.Fields.ContainsKey(Constants.ReservedNames.SCHEMA_FIELD))
            {
                var introspectedSchema = new IntrospectedSchema(this.Schema);
                queryField.Extend(new Introspection_SchemaField(introspectedSchema));
                queryField.Extend(new Introspection_TypeGraphField(introspectedSchema));

                this.EnsureGraphType(typeof(string));
                this.EnsureGraphType(typeof(bool));
                this.Schema.KnownTypes.EnsureGraphType(Introspection_DirectiveLocationType.Instance, typeof(DirectiveLocation));
                this.Schema.KnownTypes.EnsureGraphType(Introspection_DirectiveType.Instance, typeof(IntrospectedDirective));
                this.Schema.KnownTypes.EnsureGraphType(Introspection_EnumValueType.Instance, typeof(IntrospectedEnumValue));
                this.Schema.KnownTypes.EnsureGraphType(Introspection_FieldType.Instance, typeof(IntrospectedField));
                this.Schema.KnownTypes.EnsureGraphType(Introspection_InputValueType.Instance, typeof(IntrospectedInputValueType));
                this.Schema.KnownTypes.EnsureGraphType(Introspection_SchemaType.Instance, typeof(IntrospectedSchema));
                this.Schema.KnownTypes.EnsureGraphType(Introspection_TypeKindType.Instance, typeof(TypeKind));
                this.Schema.KnownTypes.EnsureGraphType(Introspection_TypeType.Instance, typeof(IntrospectedType));
            }
        }

        /// <summary>
        /// Adds the built in directives supported by this server.
        /// </summary>
        public void AddBuiltInDirectives()
        {
            foreach (var type in Constants.GlobalDirectives)
            {
                this.EnsureGraphType(type);
            }
        }

        /// <summary>
        /// Adds the <see cref="GraphControllerTemplate"/> and all associated types and virtual types to the schema.
        /// </summary>
        /// <param name="gcd">The template fully describing the controller.</param>
        private void AddController(IGraphControllerTemplate gcd)
        {
            foreach (var action in gcd.Actions)
                this.AddAction(action);

            foreach (var extension in gcd.Extensions)
                this.AddTypeExtension(extension);
        }

        /// <summary>
        /// Adds the type extension to the schema for the configured concrete type. If the type
        /// is not registered to the schema the field extension is queued for when it is added (if ever).
        /// </summary>
        /// <param name="extension">The extension to add.</param>
        private void AddTypeExtension(IGraphTypeFieldTemplate extension)
        {
            var fieldMaker = GraphQLProviders.GraphTypeMakerProvider.CreateFieldMaker(this.Schema);
            var fieldResult = fieldMaker.CreateField(extension);

            if (fieldResult != null)
            {
                this.Schema.KnownTypes.EnsureGraphFieldExtension(extension.SourceObjectType, fieldResult.Field);
                this.EnsureDependents(fieldResult);
            }
        }

        /// <summary>
        /// Adds the <see cref="ControllerActionGraphFieldTemplate"/> to the schema. Any required parent fields
        /// will be automatically created if necessary to ensure proper nesting.
        /// </summary>
        /// <param name="action">The action to add to the schema.</param>
        private void AddAction(IGraphTypeFieldTemplate action)
        {
            if (this.Schema.Configuration.DeclarationOptions.AllowedOperations.Contains(action.Route.RootCollection))
            {
                this.EnsureGraphOperationType(action.Route.RootCollection);
                var parentField = this.AddOrRetrieveRoutePath(action);
                this.AddActionAsField(parentField, action);
            }
            else
            {
                throw new ArgumentOutOfRangeException(
                    nameof(action),
                    $"The '{action.InternalFullName}' action's operation root ({action.Route.RootCollection}) is not " +
                    $"allowed by the schema's current configuration (Schema: {this.Schema.Name}).");
            }
        }

        /// <summary>
        /// Ensures that the root operation type (query, mutation etc.) exists on this schema and the associated virtual
        /// type representing it also exists in the schema's type collection.
        /// </summary>
        /// <param name="operationType">Type of the operation.</param>
        private void EnsureGraphOperationType(GraphCollection operationType)
        {
            if (!this.Schema.OperationTypes.ContainsKey(operationType))
            {
                var name = _formatter.FormatGraphTypeName(operationType.ToString());
                var operation = new GraphOperation(operationType, name);
                this.Schema.KnownTypes.EnsureGraphType(operation);
                this.Schema.OperationTypes.Add(operation.OperationType, operation);
            }
        }

        /// <summary>
        /// Inspects the root and ensures that any intermediate, virtual fields
        /// are accounted for and returns a reference to the immediate parent this action should be added to.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns>IGraphField.</returns>
        private IObjectGraphType AddOrRetrieveRoutePath(IGraphTypeFieldTemplate action)
        {
            var pathSegments = action.Route.GenerateParentPathSegments();

            // loop through all parent path parts of this action
            // creating virtual fields as necessary or using existing ones and adding on to them
            IObjectGraphType parentType = this.Schema.OperationTypes[action.Route.RootCollection];

            for (var i = 0; i < pathSegments.Count; i++)
            {
                var segment = pathSegments[i];
                var formattedName = _formatter.FormatFieldName(segment.Name);
                if (parentType.Fields.ContainsKey(formattedName))
                {
                    var field = parentType[formattedName];
                    var foundType = Schema.KnownTypes.FindGraphType(field.TypeExpression.TypeName);
                    if (foundType is IObjectGraphType ogt)
                    {
                        parentType = ogt;
                        continue;
                    }

                    throw new GraphTypeDeclarationException(
                        $"The action '{action.Name}' attempted to nest itself under the grpah type '{foundType?.Name}' but the graph type " +
                        "does not exist or does not accept fields.");
                }

                var fieldType = this.CreateVirtualFieldOnParent(
                    parentType,
                    formattedName,
                    segment,
                    i == 0 ? action.Parent : null);
                parentType = fieldType;
            }

            return parentType;
        }

        /// <summary>
        /// Performs an out-of-band append of a new graph field to a parent. Accounts for type updates in this schema ONLY.
        /// </summary>
        /// <param name="parentType">the parent type to add the new field to.</param>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="path">The path segment to represent the new field.</param>
        /// <param name="definition">The definition item from which graph attributes should be used, if any. Attributes will be set to an empty string if not supplied.</param>
        /// <returns>The type associated with the field added to the parent type.</returns>
        private IObjectGraphType CreateVirtualFieldOnParent(
            IObjectGraphType parentType,
            string fieldName,
            GraphFieldPath path,
            IGraphItemTemplate definition = null)
        {
            var childField = new VirtualGraphField(
                fieldName,
                path,
                this.MakeSafeTypeNameFromRoutePath(path))
            {
                IsDepreciated = false,
                DepreciationReason = string.Empty,
                Description = definition?.Description ?? string.Empty,
            };

            parentType.Extend(childField);
            this.Schema.KnownTypes.EnsureGraphType(childField.AssociatedGraphType);
            this.EnsureDependents(childField);

            return childField.AssociatedGraphType;
        }

        /// <summary>
        /// Makes the unique route being used for this virtual field type safe, removing special control characters
        /// but retaining its uniqueness.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>System.String.</returns>
        private string MakeSafeTypeNameFromRoutePath(GraphFieldPath path)
        {
            var segments = new List<string>();
            foreach (var pathSegmentName in path)
            {
                switch (pathSegmentName)
                {
                    case Constants.Routing.QUERY_ROOT:
                        segments.Add(Constants.ReservedNames.QUERY_TYPE_NAME);
                        break;

                    case Constants.Routing.MUTATION_ROOT:
                        segments.Add(Constants.ReservedNames.MUTATION_TYPE_NAME);
                        break;

                    case Constants.Routing.SUBSCRIPTION_ROOT:
                        segments.Add(Constants.ReservedNames.SUBSCRIPTION_TYPE_NAME);
                        break;

                    default:
                        segments.Add(_formatter.FormatGraphTypeName(pathSegmentName));
                        break;
                }
            }

            segments.Reverse();
            return string.Join("_", segments);
        }

        /// <summary>
        /// Iterates the given <see cref="ControllerActionGraphFieldTemplate" /> and adds
        /// all found types to the type system for this <see cref="ISchema" />. Generates
        /// a field reference on the provided parent with a resolver pointing to the provided graph action.
        /// </summary>
        /// <param name="parentField">The parent which will own the generated action field.</param>
        /// <param name="action">The action.</param>
        private void AddActionAsField(IObjectGraphType parentField, IGraphTypeFieldTemplate action)
        {
            // apend the action as a field on the parent
            var maker = GraphQLProviders.GraphTypeMakerProvider.CreateFieldMaker(this.Schema);
            var fieldResult = maker.CreateField(action);

            if (fieldResult != null)
            {
                parentField.Extend(fieldResult.Field);
                this.EnsureDependents(fieldResult);
            }
        }

        /// <summary>
        /// Inspects and adds the given type to the schema as a graph type or a registered controller depending
        /// on the type. The type kind will be automatically inferred or an error will be thrown.
        /// </summary>
        /// <typeparam name="TItem">The type of the item to add to the schema.</typeparam>
        public void EnsureGraphType<TItem>()
        {
            this.EnsureGraphType(typeof(TItem));
        }

        /// <summary>
        /// Inspects and adds the given type to the schema as a graph type or a registered controller depending
        /// on the type. The type kind will be automatically inferred or an error will be thrown.
        /// </summary>
        /// <typeparam name="TItem">The type of the item to add to the schema.</typeparam>
        /// <param name="kind">The kind of graph type to create from the supplied concrete type.</param>
        public void EnsureGraphType<TItem>(TypeKind kind)
        {
            this.EnsureGraphType(typeof(TItem), kind);
        }

        /// <summary>
        /// Adds the given type to the schema as a graph type or a registered controller depending
        /// on the type.
        /// </summary>
        /// <param name="type">The concrete type to add.</param>
        /// <param name="kind">The kind of graph type to create from the supplied concrete type. If not supplied the concrete type will
        /// attempt to auto assign a type of scalar, enum or object as necessary.</param>
        public void EnsureGraphType(Type type, TypeKind? kind = null)
        {
            if (Validation.IsCastable<GraphController>(type))
            {
                if (GraphQLProviders.TemplateProvider.ParseType(type) is IGraphControllerTemplate controllerDefinition)
                    this.AddController(controllerDefinition);

                return;
            }

            type = GraphValidation.EliminateWrappersFromCoreType(type);

            // if the type is already registered, early exit no point in running it through again
            var actualKind = GraphValidation.ResolveTypeKindOrThrow(type, kind);
            if (this.Schema.KnownTypes.Contains(type, actualKind))
                return;

            var maker = GraphQLProviders.GraphTypeMakerProvider.CreateTypeMaker(this.Schema, actualKind);
            if (maker != null)
            {
                var result = maker.CreateGraphType(type);
                if (result != null)
                {
                    this.Schema.KnownTypes.EnsureGraphType(result.GraphType, result.ConcreteType);
                    this.EnsureDependents(result);
                }
            }
        }

        /// <summary>
        /// Ensures the dependents.
        /// </summary>
        /// <param name="dependencySet">The dependency set.</param>
        private void EnsureDependents(IGraphItemDependencies dependencySet)
        {
            foreach (var abstractType in dependencySet.AbstractGraphTypes)
            {
                this.Schema.KnownTypes.EnsureGraphType(abstractType);
            }

            foreach (var dependent in dependencySet.DependentTypes)
            {
                this.EnsureGraphType(dependent.Type, dependent.ExpectedKind);
            }
        }

        /// <summary>
        /// Clears, builds and caches the introspection metadata to describe this schema. If introspection
        /// fields have not been added to the schema this method does nothing.
        /// </summary>
        public void BuildIntrospectionData()
        {
            if (this.Schema.Configuration.DeclarationOptions.DisableIntrospection)
                return;

            this.EnsureGraphOperationType(GraphCollection.Query);
            var queryType = this.Schema.OperationTypes[GraphCollection.Query];
            if (!queryType.Fields.ContainsKey(Constants.ReservedNames.SCHEMA_FIELD))
                return;

            var field = queryType.Fields[Constants.ReservedNames.SCHEMA_FIELD] as Introspection_SchemaField;
            field.IntrospectedSchema.Rebuild();
        }

        /// <summary>
        /// Gets the schema.
        /// </summary>
        /// <value>The schema.</value>
        public ISchema Schema { get; }
    }
}