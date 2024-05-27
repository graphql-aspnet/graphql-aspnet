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
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Internal;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.Generation.TypeTemplates;
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

            template.Parse();
            template.ValidateOrThrow();

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
            var operation = action.ItemPath.Root.ToGraphOperationType();
            if (this.Schema.Configuration.DeclarationOptions.AllowedOperations.Contains(operation))
            {
                this.EnsureGraphOperationType(operation);
                var parentField = this.AddOrRetrieveVirtualTypeOwner(action);
                this.AddActionAsField(parentField, action);
            }
            else
            {
                throw new ArgumentOutOfRangeException(
                    nameof(action),
                    $"The '{action.InternalName}' action's operation root ({action.ItemPath.Root}) is not " +
                    $"allowed by the target schema (Name: {this.Schema.Name}).");
            }
        }

        /// <summary>
        /// Inspects the field template and ensures that any intermediate, virtual types (and fields)
        /// are accounted for. This method  then returns a reference to the intermediate graph type
        /// this action should be added to as a field reference.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns>IGraphField.</returns>
        protected virtual IObjectGraphType AddOrRetrieveVirtualTypeOwner(IGraphFieldTemplate action)
        {
            var pathSegments = action.ItemPath.GenerateParentPathSegments();

            // loop through all parent path parts of this action
            // creating virtual fields as necessary or using existing ones and adding on to them
            IObjectGraphType parentType = this.Schema.Operations[action.ItemPath.Root.ToGraphOperationType()];

            for (var i = 0; i < pathSegments.Count; i++)
            {
                var segment = pathSegments[i];

                (var virtualField, var virtualFieldGaphType) = this.CreateVirtualField(
                    parentType,
                    segment.Name,
                    segment,
                    i == 0 ? action.Parent : null);

                // its possible this field and its graph type already exist because its
                // been referenced in path segments elsewhere on this controller.
                // If this is the case reference the already added version on the schema
                // then perform checks to ensure that the value in the schema matches expectations
                // for this newly created virtual field.
                // Once verified just use the existing field and continue down the tree.
                if (parentType.Fields.ContainsKey(virtualField.Name))
                {
                    var existingChildField = parentType[virtualField.Name];
                    var foundType = Schema.KnownTypes.FindGraphType(existingChildField.TypeExpression.TypeName);

                    var ogt = foundType as IObjectGraphType;
                    if (ogt != null)
                    {
                        if (ogt.IsVirtual)
                        {
                            parentType = ogt;
                            continue;
                        }

                        throw new GraphTypeDeclarationException(
                            $"The action '{action.ItemPath}' attempted to nest itself under the {foundType.Kind} graph type '{foundType.Name}', which is returned by " +
                            $"the path '{existingChildField.ItemPath}'.  Actions can only be added to virtual graph types created by their parent controller.");
                    }

                    if (foundType != null)
                    {
                        throw new GraphTypeDeclarationException(
                            $"The action '{action.ItemPath.Path}' attempted to nest itself under the graph type '{foundType.Name}'. {foundType.Kind} graph types cannot " +
                            "accept fields.");
                    }
                    else
                    {
                        throw new GraphTypeDeclarationException(
                            $"The action '{action.ItemPath.Path}' attempted to nest itself under the field '{existingChildField.ItemPath}' but no graph type was found " +
                            "that matches its type.");
                    }
                }

                // field doesn't already exist on the schema
                // add it and continue down the tree
                parentType.Extend(virtualField);
                this.Schema.KnownTypes.EnsureGraphType(virtualFieldGaphType);
                this.EnsureDependents(virtualField);
                parentType = virtualFieldGaphType;
            }

            return parentType;
        }

        /// <summary>
        /// Generates a fully qualified field and an associated graph type that can be added to
        /// the schema to represent a segment in a field path definition added by the developer.
        /// </summary>
        /// <param name="parentType">the parent type to add the new field to.</param>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="pathTemplate">The path segment to represent the new field.</param>
        /// <param name="definition">The definition item from which graph attributes should be used, if any. Attributes will be set to an empty string if not supplied.</param>
        /// <returns>A reference to the new field and the graph type created to represent the fields returned value.</returns>
        protected virtual (VirtualGraphField VirtualField, IObjectGraphType VirtualFieldGraphType) CreateVirtualField(
            IObjectGraphType parentType,
            string fieldName,
            ItemPath pathTemplate,
            ISchemaItemTemplate definition = null)
        {
            var returnedGraphType = VirtualObjectGraphType.FromControllerFieldPathTemplate(pathTemplate);

            returnedGraphType = this.Schema
                .Configuration
                .DeclarationOptions
                .SchemaFormatStrategy?
                .ApplySchemaItemRules(
                        this.Schema.Configuration,
                        returnedGraphType) ?? returnedGraphType;

            var typeExpression = GraphTypeExpression.FromDeclaration(returnedGraphType.Name);

            var childField = new VirtualGraphField(
                fieldName,
                pathTemplate,
                typeExpression)
            {
                IsDepreciated = false,
                DepreciationReason = string.Empty,
                Description = definition?.Description ?? string.Empty,
            };

            // configure the field for the schema
            // and add it to its appropriate parent
            childField = this.Schema
                .Configuration
                .DeclarationOptions
                .SchemaFormatStrategy?
                .ApplySchemaItemRules(
                        this.Schema.Configuration,
                        childField) ?? childField;

            return (childField, returnedGraphType);
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
                var field = this.Schema
                    .Configuration
                    .DeclarationOptions
                    .SchemaFormatStrategy?
                    .ApplySchemaItemRules(
                            this.Schema.Configuration,
                            fieldResult.Field) ?? fieldResult.Field;

                if (parentType.Fields.ContainsKey(fieldResult.Field.Name))
                {
                    throw new GraphTypeDeclarationException(
                        $"The '{parentType.Kind}' graph type '{parentType.Name}' already contains a field named '{fieldResult.Field.Name}'. " +
                        $"The action method '{action.InternalName}' cannot be added to the graph type with the same name.");
                }

                parentType.Extend(field);
                this.EnsureDependents(fieldResult);
            }
        }
    }
}