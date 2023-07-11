// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Engine
{
    using System;
    using System.Collections.Generic;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Internal;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Internal.TypeTemplates;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// The default schema factory, capable of creating singleton instances of
    /// schemas, fully populated and ready to serve requests.
    /// </summary>
    public partial class DefaultGraphQLSchemaFactory<TSchema>
    {
        /// <summary>
        /// Incorpates the templated controller into the schema.
        /// </summary>
        /// <param name="template">The template of the controller to add.</param>
        protected virtual void AddController(IGraphControllerTemplate template)
        {
            Validation.ThrowIfNull(template, nameof(template));

            foreach (var action in template.Actions)
                this.AddAction(action);

            foreach (var extension in template.Extensions)
                this.AddTypeExtension(extension);
        }

        /// <summary>
        /// Adds the type extension to the schema for the configured concrete type. If the type
        /// is not registered to the schema the field extension is queued for when it is added (if ever).
        /// </summary>
        /// <param name="extension">The extension to add.</param>
        protected virtual void AddTypeExtension(IGraphFieldTemplate extension)
        {
            var fieldMaker = this.MakerFactory.CreateFieldMaker();
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
        protected virtual void AddAction(IGraphFieldTemplate action)
        {
            var operation = action.Route.RootCollection.ToGraphOperationType();
            if (this.Schema.Configuration.DeclarationOptions.AllowedOperations.Contains(operation))
            {
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
        /// Inspects the root and ensures that any intermediate, virtual fields
        /// are accounted for and returns a reference to the immediate parent this action should be added to.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns>IGraphField.</returns>
        protected virtual IObjectGraphType AddOrRetrieveControllerRoutePath(IGraphFieldTemplate action)
        {
            var pathSegments = action.Route.GenerateParentPathSegments();

            // loop through all parent path parts of this action
            // creating virtual fields as necessary or using existing ones and adding on to them
            IObjectGraphType parentType = this.Schema.Operations[action.Route.RootCollection.ToGraphOperationType()];

            for (var i = 0; i < pathSegments.Count; i++)
            {
                var segment = pathSegments[i];
                var formattedName = this.Schema.Configuration.DeclarationOptions.GraphNamingFormatter.FormatFieldName(segment.Name);
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
        protected virtual IObjectGraphType CreateVirtualFieldOnParent(
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
        protected virtual string MakeSafeTypeNameFromRoutePath(SchemaItemPath path)
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
                        segments.Add(this.Schema.Configuration.DeclarationOptions.GraphNamingFormatter.FormatGraphTypeName(pathSegmentName));
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
        protected virtual void AddActionAsField(IObjectGraphType parentType, IGraphFieldTemplate action)
        {
            // apend the action as a field on the parent
            var maker = this.MakerFactory.CreateFieldMaker();
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
    }
}