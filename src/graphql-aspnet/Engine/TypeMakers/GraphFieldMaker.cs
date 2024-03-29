﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Engine.TypeMakers
{
    using System;
    using System.Collections.Generic;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Common.Generics;
    using GraphQL.AspNet.Configuration.Formatting;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.Internal;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Internal.TypeTemplates;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Security;

    /// <summary>
    /// A maker capable of turning a <see cref="IGraphFieldTemplateBase"/> into a usable field in an object graph.
    /// </summary>
    public class GraphFieldMaker : IGraphFieldMaker
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphFieldMaker"/> class.
        /// </summary>
        /// <param name="schema">The schema.</param>
        public GraphFieldMaker(ISchema schema)
        {
            this.Schema = Validation.ThrowIfNullOrReturn(schema, nameof(schema));
        }

        /// <inheritdoc />
        public virtual GraphFieldCreationResult<IGraphField> CreateField(IGraphFieldTemplate template)
        {
            Validation.ThrowIfNull(template, nameof(template));

            template.ValidateOrThrow(false);

            var formatter = this.Schema.Configuration.DeclarationOptions.GraphNamingFormatter;
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

            if (template.Arguments != null)
            {
                var argumentMaker = this.CreateArgumentMaker();
                Validation.ThrowIfNull(argumentMaker, nameof(argumentMaker));

                foreach (var argTemplate in template.Arguments)
                {
                    var argumentResult = argumentMaker.CreateArgument(field, argTemplate);
                    field.Arguments.AddArgument(argumentResult.Argument);

                    result.MergeDependents(argumentResult);
                }
            }

            result.AddDependentRange(template.RetrieveRequiredTypes());

            if (template.UnionProxy != null)
            {
                var unionMaker = GraphQLProviders.GraphTypeMakerProvider.CreateUnionMaker(this.Schema);
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
        /// Creates an argument maker that will be used to create all the
        /// arguments of a given field.
        /// </summary>
        /// <returns>IGraphArgumentMaker.</returns>
        protected virtual IGraphArgumentMaker CreateArgumentMaker()
        {
            return new GraphArgumentMaker(this.Schema);
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

            switch (template.FieldSource)
            {
                case GraphFieldSource.Method:
                case GraphFieldSource.Action:
                    return new MethodGraphField(
                        formatter.FormatFieldName(template.Name),
                        template.TypeExpression.CloneTo(formatter.FormatGraphTypeName(template.TypeExpression.TypeName)),
                        template.Route,
                        template.ObjectType,
                        template.DeclaredReturnType,
                        template.Mode,
                        template.CreateResolver(),
                        securityGroups,
                        directives);

                case GraphFieldSource.Property:
                    return new PropertyGraphField(
                        formatter.FormatFieldName(template.Name),
                        template.TypeExpression.CloneTo(formatter.FormatGraphTypeName(template.TypeExpression.TypeName)),
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
            var formatter = this.Schema.Configuration.DeclarationOptions.GraphNamingFormatter;

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

        /// <summary>
        /// Gets the schema this field maker is creating fields for.
        /// </summary>
        /// <value>The schema.</value>
        protected ISchema Schema { get; }
    }
}