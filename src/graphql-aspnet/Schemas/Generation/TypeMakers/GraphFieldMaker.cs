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
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Common.Generics;
    using GraphQL.AspNet.Configuration.Formatting;
    using GraphQL.AspNet.Engine.TypeMakers;
    using GraphQL.AspNet.Execution.RulesEngine.RuleSets.DocumentValidation.FieldSelectionSteps;
    using GraphQL.AspNet.Interfaces.Configuration;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.Internal;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Internal;
    using GraphQL.AspNet.Internal.TypeTemplates;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Schemas.TypeSystem.Scalars;
    using GraphQL.AspNet.Security;

    /// <summary>
    /// A maker capable of turning a <see cref="IGraphFieldTemplateBase"/> into a usable field in an object graph.
    /// </summary>
    public class GraphFieldMaker : IGraphFieldMaker
    {
        private readonly ISchema _schema;
        private readonly ISchemaConfiguration _config;
        private readonly IGraphQLTypeMakerFactory _makerFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphFieldMaker" /> class.
        /// </summary>
        /// <param name="schema">The schema instance to reference when creating fields.</param>
        /// <param name="makerFactory">The maker factory to create dependnet makers
        /// if and when necessary.</param>
        public GraphFieldMaker(ISchema schema, IGraphQLTypeMakerFactory makerFactory)
        {
            _schema = Validation.ThrowIfNullOrReturn(schema, nameof(schema));
            _config = _schema.Configuration;
            _makerFactory = Validation.ThrowIfNullOrReturn(makerFactory, nameof(makerFactory));
        }

        /// <inheritdoc />
        public virtual GraphFieldCreationResult<IGraphField> CreateField(IGraphFieldTemplate template)
        {
            var formatter = _config.DeclarationOptions.GraphNamingFormatter;
            var result = new GraphFieldCreationResult<IGraphField>();

            // if the owner of this field declared top level objects append them to the
            // field for evaluation
            var securityGroups = new List<AppliedSecurityPolicyGroup>();

            if (template.Parent?.SecurityPolicies?.Count > 0)
                securityGroups.Add(template.Parent.SecurityPolicies);

            if (template.SecurityPolicies?.Count > 0)
                securityGroups.Add(template.SecurityPolicies);

            MethodGraphField field = this.InstantiateField(formatter, template, securityGroups);

            field.Description = template.Description;
            field.Complexity = template.Complexity;
            field.FieldSource = template.FieldSource;

            if (template.Arguments != null && template.Arguments.Count > 0)
            {
                var argMaker = _makerFactory.CreateArgumentMaker();
                foreach (var argTemplate in template.Arguments)
                {
                    if (argTemplate.ArgumentModifiers.IsPartOfTheSchema())
                    {
                        var argumentResult = argMaker.CreateArgument(field, argTemplate);
                        field.Arguments.AddArgument(argumentResult.Argument);

                        result.MergeDependents(argumentResult);
                    }
                }
            }

            result.AddDependentRange(template.RetrieveRequiredTypes());

            if (template.UnionProxy != null)
            {
                var unionMaker = _makerFactory.CreateUnionMaker();
                var unionResult = unionMaker.CreateUnionFromProxy(template.UnionProxy);
                if (unionResult != null)
                {
                    result.AddAbstractDependent(unionResult.GraphType);
                    result.MergeDependents(unionResult);
                }
            }

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
            // and is usable as an input type then just use the name
            if (existingGraphType != null && existingGraphType.Kind.IsValidInputKind())
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
                // this is guaranteed correct for all but scalars
                schemaTypeName = GraphTypeNames.ParseName(template.ObjectType, template.OwnerTypeKind);
            }

            // enforce non-renaming standards in the maker since the
            // directly controls the formatter
            if (GlobalScalars.CanBeRenamed(schemaTypeName))
                schemaTypeName = _schema.Configuration.DeclarationOptions.GraphNamingFormatter.FormatGraphTypeName(schemaTypeName);

            return schemaTypeName;
        }

        /// <summary>
        /// Instantiates the graph field according to the data provided.
        /// </summary>
        /// <param name="formatter">The formatter.</param>
        /// <param name="template">The template.</param>
        /// <param name="securityGroups">The security groups.</param>
        /// <returns>MethodGraphField.</returns>
        protected virtual MethodGraphField InstantiateField(
            GraphNameFormatter formatter,
            IGraphFieldTemplate template,
            List<AppliedSecurityPolicyGroup> securityGroups)
        {
            var directives = template.CreateAppliedDirectives();

            var schemaTypeName = this.PrepareTypeName(template);
            var typeExpression = template.TypeExpression.CloneTo(schemaTypeName);

            switch (template.FieldSource)
            {
                case GraphFieldSource.Method:
                case GraphFieldSource.Action:
                    return new MethodGraphField(
                        formatter.FormatFieldName(template.Name),
                        typeExpression,
                        template.Route,
                        template.DeclaredName,
                        template.ObjectType,
                        template.DeclaredReturnType,
                        template.Mode,
                        template.CreateResolver(),
                        securityGroups,
                        directives);

                case GraphFieldSource.Property:
                    return new PropertyGraphField(
                        formatter.FormatFieldName(template.Name),
                        typeExpression,
                        template.Route,
                        template.DeclaredName,
                        template.ObjectType,
                        template.DeclaredReturnType,
                        template.Mode,
                        template.CreateResolver(),
                        securityGroups,
                        directives);

                default:
                    throw new ArgumentOutOfRangeException($"Template field source of {template.FieldSource.ToString()} is not supported by {this.GetType().FriendlyName()}.");
            }
        }

        /// <inheritdoc />
        public GraphFieldCreationResult<IInputGraphField> CreateField(IInputGraphFieldTemplate template)
        {
            var formatter = _config.DeclarationOptions.GraphNamingFormatter;

            var defaultInputObject = InstanceFactory.CreateInstance(template.Parent.ObjectType);
            var propGetters = InstanceFactory.CreatePropertyGetterInvokerCollection(template.Parent.ObjectType);

            object defaultValue = null;

            if (!template.IsRequired && propGetters.ContainsKey(template.InternalName))
            {
                defaultValue = propGetters[template.InternalName](ref defaultInputObject);
            }

            var result = new GraphFieldCreationResult<IInputGraphField>();

            var directives = template.CreateAppliedDirectives();

            var field = new InputGraphField(
                    formatter.FormatFieldName(template.Name),
                    template.TypeExpression.CloneTo(formatter.FormatGraphTypeName(template.TypeExpression.TypeName)),
                    template.Route,
                    template.DeclaredName,
                    template.ObjectType,
                    template.DeclaredReturnType,
                    template.IsRequired,
                    defaultValue,
                    directives);

            field.Description = template.Description;

            result.AddDependentRange(template.RetrieveRequiredTypes());

            result.Field = field;
            return result;
        }
    }
}