// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Schemas.Generation.TypeMakers
{
    using System;
    using System.Collections.Generic;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Common.Generics;
    using GraphQL.AspNet.Configuration.Formatting;
    using GraphQL.AspNet.Interfaces.Configuration;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.Internal;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas.Generation.TypeTemplates;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Security;

    /// <summary>
    /// A maker capable of turning a <see cref="IGraphFieldTemplateBase"/> into a usable field in an object graph.
    /// </summary>
    public class GraphFieldMaker : IGraphFieldMaker
    {
        private readonly ISchema _schema;
        private readonly ISchemaConfiguration _config;
        private readonly IGraphArgumentMaker _argMaker;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphFieldMaker" /> class.
        /// </summary>
        /// <param name="schema">The schema instance to reference when creating fields.</param>
        /// <param name="argMaker">A maker that can make arguments declared on this field.</param>
        public GraphFieldMaker(ISchema schema, IGraphArgumentMaker argMaker)
        {
            _schema = Validation.ThrowIfNullOrReturn(schema, nameof(schema));
            _argMaker = Validation.ThrowIfNullOrReturn(argMaker, nameof(argMaker));
            _config = _schema.Configuration;
        }

        /// <inheritdoc />
        public virtual GraphFieldCreationResult<IGraphField> CreateField(IGraphFieldTemplate template)
        {
            Validation.ThrowIfNull(template, nameof(template));

            template.Parse();
            template.ValidateOrThrow(false);

            var result = new GraphFieldCreationResult<IGraphField>();

            // if the owner of this field declared top level objects append them to the
            // field for evaluation
            var securityGroups = new List<AppliedSecurityPolicyGroup>();

            if (template.Parent?.SecurityPolicies?.Count > 0)
                securityGroups.Add(template.Parent.SecurityPolicies);

            if (template.SecurityPolicies?.Count > 0)
                securityGroups.Add(template.SecurityPolicies);

            MethodGraphField field = this.InstantiateField(template, securityGroups);

            field = _config
                .DeclarationOptions
                .SchemaFormatStrategy?
                .ApplyFormatting(_config, field) ?? field;

            field.Description = template.Description;
            field.Complexity = template.Complexity;
            field.FieldSource = template.FieldSource;

            if (template.Arguments != null && template.Arguments.Count > 0)
            {
                foreach (var argTemplate in template.Arguments)
                {
                    if (GraphArgumentMaker.IsArgumentPartOfSchema(argTemplate, _schema))
                    {
                        var argumentResult = _argMaker.CreateArgument(field, argTemplate);

                        var argument = argumentResult.Argument.Clone(field);
                        field.Arguments.AddArgument(argument);

                        result.MergeDependents(argumentResult);
                    }
                }
            }

            result.AddDependentRange(template.RetrieveRequiredTypes());
            if (template.UnionProxy != null)
            {
                var dependentUnion = new DependentType(template.UnionProxy, TypeKind.UNION);
                result.AddDependent(dependentUnion);
            }

            result.Field = field;
            return result;
        }

        /// <inheritdoc />
        public GraphFieldCreationResult<IInputGraphField> CreateField(IInputGraphFieldTemplate template)
        {
            Validation.ThrowIfNull(template, nameof(template));

            var defaultInputObject = InstanceFactory.CreateInstance(template.Parent.ObjectType);
            var propGetters = InstanceFactory.CreatePropertyGetterInvokerCollection(template.Parent.ObjectType);

            object defaultValue = null;

            if (!template.IsRequired && propGetters.ContainsKey(template.DeclaredName))
            {
                defaultValue = propGetters[template.DeclaredName](ref defaultInputObject);
            }

            var result = new GraphFieldCreationResult<IInputGraphField>();

            var directives = template.CreateAppliedDirectives();
            var schemaTypeName = this.PrepareTypeName(template);

            var field = new InputGraphField(
                    template.Name,
                    template.InternalName,
                    template.TypeExpression.Clone(schemaTypeName),
                    template.Route,
                    template.ObjectType,
                    template.DeclaredName,
                    template.DeclaredReturnType,
                    template.IsRequired,
                    defaultValue,
                    directives);

            field.Description = template.Description;

            field = _config
                .DeclarationOptions
                .SchemaFormatStrategy?
                .ApplyFormatting(_config, field) ?? field;

            result.AddDependentRange(template.RetrieveRequiredTypes());

            result.Field = field;
            return result;
        }

        /// <summary>
        /// Finds and applies proper casing to the graph type name returned by this field.
        /// </summary>
        /// <param name="template">The template to inspect.</param>
        /// <returns>System.String.</returns>
        protected virtual string PrepareTypeName(IGraphFieldTemplate template)
        {
            // all fields return either an object, interface, union, scalar or enum
            IGraphType existingGraphType;
            string fallbackTypeName;
            if (template.UnionProxy != null)
            {
                existingGraphType = _schema.KnownTypes.FindGraphType(template.UnionProxy.Name);
                fallbackTypeName = template.UnionProxy.Name;
            }
            else
            {
                existingGraphType = _schema.KnownTypes.FindGraphType(template.ObjectType, template.OwnerTypeKind);
                fallbackTypeName = null;
            }

            string schemaTypeName;

            // when the type already exists on the target schema
            // and is usable as a type name then just use the name
            if (existingGraphType != null)
            {
                schemaTypeName = existingGraphType.Name;
            }
            else if (fallbackTypeName != null)
            {
                schemaTypeName = fallbackTypeName;
            }
            else
            {
                // guess on what the name of the schema item will be
                // this is guaranteed correct (minus casing) for all but scalars
                schemaTypeName = GraphTypeNames.ParseName(template.ObjectType, template.OwnerTypeKind);
            }

            return schemaTypeName;
        }

        /// <summary>
        /// Finds and applies proper casing to the graph type name returned by this input field.
        /// </summary>
        /// <param name="template">The template to inspect.</param>
        /// <returns>System.String.</returns>
        protected virtual string PrepareTypeName(IInputGraphFieldTemplate template)
        {
            // all input fields return either an object, scalar or enum (never a union or interface)
            string schemaTypeName;
            var existingGraphType = _schema.KnownTypes.FindGraphType(template.ObjectType, template.OwnerTypeKind);

            // when the type already exists on the target schema
            // and is usable as a type for an input field then just use the name
            // an OBJECT type may be registered for the target `template.ObjectType` which might get found
            // but the coorisponding INPUT_OBJECT may not yet be discovered
            if (existingGraphType != null && existingGraphType.Kind.IsValidInputKind())
            {
                schemaTypeName = existingGraphType.Name;
            }
            else
            {
                // guess on what the unformatted name of the schema item will be
                // this is guaranteed correct for all but scalars and scalars should be registered by the time
                // input objects are registered
                schemaTypeName = GraphTypeNames.ParseName(template.ObjectType, template.OwnerTypeKind);
            }

            return schemaTypeName;
        }

        /// <summary>
        /// Instantiates the graph field according to the data provided.
        /// </summary>
        /// <param name="template">The template to create a field from.</param>
        /// <param name="securityGroups">The complete set of
        /// security groups to apply to the field.</param>
        /// <returns>MethodGraphField.</returns>
        protected virtual MethodGraphField InstantiateField(
            IGraphFieldTemplate template,
            List<AppliedSecurityPolicyGroup> securityGroups)
        {
            var directives = template.CreateAppliedDirectives();

            var schemaTypeName = this.PrepareTypeName(template);
            var typeExpression = template.TypeExpression.Clone(schemaTypeName);

            switch (template.FieldSource)
            {
                case GraphFieldSource.Method:
                case GraphFieldSource.Property:
                case GraphFieldSource.Action:
                    return new MethodGraphField(
                        template.Name,
                        template.InternalName,
                        typeExpression,
                        template.Route,
                        template.DeclaredReturnType,
                        template.ObjectType,
                        template.Mode,
                        template.CreateResolver(),
                        securityGroups,
                        directives);

                default:
                    throw new ArgumentOutOfRangeException($"Template field source of {template.FieldSource.ToString()} is not supported by {this.GetType().FriendlyName()}.");
            }
        }
    }
}