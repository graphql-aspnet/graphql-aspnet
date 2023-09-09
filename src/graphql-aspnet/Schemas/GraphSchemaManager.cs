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
    using System.Linq;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Configuration.Formatting;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Engine.TypeMakers;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Internal;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Internal;
    using GraphQL.AspNet.Internal.TypeTemplates;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Schemas.TypeSystem.Introspection;
    using GraphQL.AspNet.Schemas.TypeSystem.Introspection.Fields;
    using GraphQL.AspNet.Schemas.TypeSystem.Introspection.Model;

    /// <summary>
    /// Assists with the creation of <see cref="IGraphType"/> data
    /// parsed from any <see cref="GraphController"/> or any manually added <see cref="Type"/>
    /// to the <see cref="ISchema"/> this instance is managing.
    /// </summary>
    internal sealed class GraphSchemaManager
    {
        private readonly GraphNameFormatter _formatter;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphSchemaManager" /> class.
        /// </summary>
        /// <param name="schema">The schema instance to be managed.</param>
        public GraphSchemaManager(ISchema schema)
        {
            this.Schema = Validation.ThrowIfNullOrReturn(schema, nameof(schema));
            _formatter = this.Schema.Configuration.DeclarationOptions.GraphNamingFormatter;
            this.EnsureSchemaDependencies();
            this.EnsureGraphOperationType(GraphOperationType.Query);
        }

        /// <summary>
        /// Inspects the schema instance for any necessary dependencies
        /// and adds them to itself (e.g. schema declared directives).
        /// </summary>
        private void EnsureSchemaDependencies()
        {
            // all schemas depend on String because of the __typename field
            this.EnsureGraphType<string>();

            // ensure top level schema directives are accounted for
            foreach (var directive in this.Schema.GetType().ExtractAppliedDirectives())
            {
                this.Schema.AppliedDirectives.Add(directive);
            }

            foreach (var appliedDirective in this.Schema.AppliedDirectives.Where(x => x.DirectiveType != null))
            {
                this.EnsureGraphType(
                    appliedDirective.DirectiveType,
                    TypeKind.DIRECTIVE);
            }
        }

        /// <summary>
        /// Adds the internal introspection fields to the query operation type if and only if the contained schema allows
        /// it through its internal configuration. This method is idempotent.
        /// </summary>
        private void AddIntrospectionFields()
        {
            this.EnsureGraphOperationType(GraphOperationType.Query);
            var queryField = this.Schema.Operations[GraphOperationType.Query];

            // Note: introspection fields are defined by the graphql spec, no custom name or item formatting is allowed
            // for Type and field name formatting.
            // spec: https://graphql.github.io/graphql-spec/October2021/#sec-Schema-Introspection
            if (!queryField.Fields.ContainsKey(Constants.ReservedNames.SCHEMA_FIELD))
            {
                var introspectedSchema = new IntrospectedSchema(this.Schema);
                queryField.Extend(new Introspection_SchemaField(introspectedSchema));
                queryField.Extend(new Introspection_TypeGraphField(introspectedSchema));

                this.EnsureGraphType(typeof(string));
                this.EnsureGraphType(typeof(bool));
                this.Schema.KnownTypes.EnsureGraphType(new Introspection_DirectiveLocationType(), typeof(DirectiveLocation));
                this.Schema.KnownTypes.EnsureGraphType(new Introspection_DirectiveType(), typeof(IntrospectedDirective));
                this.Schema.KnownTypes.EnsureGraphType(new Introspection_EnumValueType(), typeof(IntrospectedEnumValue));
                this.Schema.KnownTypes.EnsureGraphType(new Introspection_FieldType(), typeof(IntrospectedField));
                this.Schema.KnownTypes.EnsureGraphType(new Introspection_InputValueType(), typeof(IntrospectedInputValueType));
                this.Schema.KnownTypes.EnsureGraphType(new Introspection_SchemaType(), typeof(IntrospectedSchema));
                this.Schema.KnownTypes.EnsureGraphType(new Introspection_TypeKindType(), typeof(TypeKind));
                this.Schema.KnownTypes.EnsureGraphType(new Introspection_TypeType(), typeof(IntrospectedType));
            }
        }

        /// <summary>
        /// Adds the built in directives supported by the graphql runtime.
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
        private void AddTypeExtension(IGraphFieldTemplate extension)
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
        private void AddAction(IGraphFieldTemplate action)
        {
            if (this.Schema.Configuration.DeclarationOptions.AllowedOperations.Contains(action.Route.RootCollection.ToGraphOperationType()))
            {
                var operation = action.Route.RootCollection.ToGraphOperationType();
                this.EnsureGraphOperationType(operation);
                var parentField = this.AddOrRetrieveControllerRoutePath(action);
                this.AddActionAsField(parentField, action);
            }
            else
            {
                throw new ArgumentOutOfRangeException(
                    nameof(action),
                    $"The '{action.InternalFullName}' action's operation root ({action.Route.RootCollection}) is not " +
                    $"allowed by the target schema (Name: {this.Schema.Name}).");
            }
        }

        /// <summary>
        /// Ensures that the root operation type (query, mutation etc.) exists on this schema and the associated virtual
        /// type representing it also exists in the schema's type collection.
        /// </summary>
        /// <param name="operationType">Type of the operation.</param>
        private void EnsureGraphOperationType(GraphOperationType operationType)
        {
            if (operationType == GraphOperationType.Unknown)
            {
                throw new ArgumentOutOfRangeException($"The operation type '{operationType}' is " +
                    $"not supported by graphql.");
            }

            if (!this.Schema.Operations.ContainsKey(operationType))
            {
                var operation = new GraphOperation(operationType);
                this.Schema.KnownTypes.EnsureGraphType(operation);
                this.Schema.Operations.Add(operation.OperationType, operation);
            }
        }

        /// <summary>
        /// Inspects the root and ensures that any intermediate, virtual fields
        /// are accounted for and returns a reference to the immediate parent this action should be added to.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns>IGraphField.</returns>
        private IObjectGraphType AddOrRetrieveControllerRoutePath(IGraphFieldTemplate action)
        {
            var pathSegments = action.Route.GenerateParentPathSegments();

            // loop through all parent path parts of this action
            // creating virtual fields as necessary or using existing ones and adding on to them
            IObjectGraphType parentType = this.Schema.Operations[action.Route.RootCollection.ToGraphOperationType()];

            for (var i = 0; i < pathSegments.Count; i++)
            {
                var segment = pathSegments[i];
                var formattedName = _formatter.FormatFieldName(segment.Name);
                if (parentType.Fields.ContainsKey(formattedName))
                {
                    var field = parentType[formattedName];
                    var foundType = Schema.KnownTypes.FindGraphType(field.TypeExpression.TypeName);

                    var ogt = foundType as IObjectGraphType;
                    if (ogt != null)
                    {
                        if (ogt.IsVirtual)
                        {
                            parentType = ogt;
                            continue;
                        }

                        throw new GraphTypeDeclarationException(
                            $"The action '{action.Route}' attempted to nest itself under the {foundType.Kind} graph type '{foundType.Name}', which is returned by " +
                            $"the route '{field.Route}'.  Actions can only be added to virtual graph types created by their parent controller.");
                    }

                    if (foundType != null)
                    {
                        throw new GraphTypeDeclarationException(
                            $"The action '{action.Route.Path}' attempted to nest itself under the graph type '{foundType.Name}'. {foundType.Kind} graph types cannot " +
                            "accept fields.");
                    }
                    else
                    {
                        throw new GraphTypeDeclarationException(
                            $"The action '{action.Route.Path}' attempted to nest itself under the field '{field.Route}' but no graph type was found " +
                            "that matches its type.");
                    }
                }

                parentType = this.CreateVirtualFieldOnParent(
                    parentType,
                    formattedName,
                    segment,
                    i == 0 ? action.Parent : null);
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
            SchemaItemPath path,
            ISchemaItemTemplate definition = null)
        {
            var childField = new VirtualGraphField(
                parentType,
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
        private string MakeSafeTypeNameFromRoutePath(SchemaItemPath path)
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
        /// <param name="parentType">The parent which will own the generated action field.</param>
        /// <param name="action">The action.</param>
        private void AddActionAsField(IObjectGraphType parentType, IGraphFieldTemplate action)
        {
            // apend the action as a field on the parent
            var maker = GraphQLProviders.GraphTypeMakerProvider.CreateFieldMaker(this.Schema);
            var fieldResult = maker.CreateField(action);

            if (fieldResult != null)
            {
                if (parentType.Fields.ContainsKey(fieldResult.Field.Name))
                {
                    throw new GraphTypeDeclarationException(
                        $"The '{parentType.Kind}' graph type '{parentType.Name}' already contains a field named '{fieldResult.Field.Name}'. " +
                        $"The action method '{action.InternalFullName}' cannot be added to the graph type with the same name.");
                }

                parentType.Extend(fieldResult.Field);
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
        /// <param name="kind">The kind of graph type to create from the supplied concrete type.
        /// Only necessary to differentiate between OBJECT and INPUT_OBJECT types.</param>
        public void EnsureGraphType<TItem>(TypeKind? kind = null)
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
            Validation.ThrowIfNull(type, nameof(type));
            try
            {
                this.EnsureGraphTypeInternal(type, kind);
            }
            catch (GraphTypeDeclarationException ex)
            {
                if (ex.FailedObjectType == type)
                    throw;

                // wrap a thrown exception to be nested within this type that was being parsed
                throw new GraphTypeDeclarationException(
                    $"An error occured while trying to add a dependent of '{type.FriendlyName()}' " +
                    $"to the target schema. See inner exception for details.",
                    type,
                    ex);
            }
        }

        private void EnsureGraphTypeInternal(Type type, TypeKind? kind = null)
        {
            Validation.ThrowIfNull(type, nameof(type));
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

            GraphTypeCreationResult makerResult = null;
            var maker = GraphQLProviders.GraphTypeMakerProvider.CreateTypeMaker(this.Schema, actualKind);
            if (maker != null)
            {
                // if a maker can be assigned for this graph type
                // create the graph type directly
                makerResult = maker.CreateGraphType(type);
            }
            else if (Validation.IsCastable<IGraphUnionProxy>(type))
            {
                // if this type represents a well-known union proxy try and add in the union proxy
                // directly
                var unionProxy = GraphQLProviders.GraphTypeMakerProvider.CreateUnionProxyFromType(type);
                if (unionProxy != null)
                {
                    var unionMaker = GraphQLProviders.GraphTypeMakerProvider.CreateUnionMaker(this.Schema);
                    makerResult = unionMaker.CreateUnionFromProxy(unionProxy);
                }
            }

            if (makerResult != null)
            {
                this.Schema.KnownTypes.EnsureGraphType(makerResult.GraphType, makerResult.ConcreteType);
                this.EnsureDependents(makerResult);
            }
        }

        /// <summary>
        /// Ensures the dependents in teh given collection are part of the target <see cref="Schema"/>.
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
        /// fields have not been added to the schema this method does nothing. No changes to the schema
        /// items themselves happens during this method.
        /// </summary>
        public void RebuildIntrospectionData()
        {
            if (this.Schema.Configuration.DeclarationOptions.DisableIntrospection)
                return;

            this.EnsureGraphOperationType(GraphOperationType.Query);
            this.AddIntrospectionFields();

            var queryType = this.Schema.Operations[GraphOperationType.Query];
            if (!queryType.Fields.ContainsKey(Constants.ReservedNames.SCHEMA_FIELD))
                return;

            var field = queryType.Fields[Constants.ReservedNames.SCHEMA_FIELD] as Introspection_SchemaField;
            field.IntrospectedSchema.Rebuild();
        }

        /// <summary>
        /// Gets the schema being managed and built by this instance.
        /// </summary>
        /// <value>The schema.</value>
        public ISchema Schema { get; }
    }
}