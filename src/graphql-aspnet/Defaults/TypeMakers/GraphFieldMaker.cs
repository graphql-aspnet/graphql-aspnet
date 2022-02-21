// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Defaults.TypeMakers
{
    using System;
    using System.Collections.Generic;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Configuration.Formatting;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Internal.TypeTemplates;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Security;

    /// <summary>
    /// A maker capable of turning a <see cref="IGraphFieldBaseTemplate"/> into a usable field in an object graph.
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
        public virtual GraphFieldCreationResult CreateField(IGraphTypeFieldTemplate template)
        {
            var formatter = this.Schema.Configuration.DeclarationOptions.GraphNamingFormatter;
            var result = new GraphFieldCreationResult();

            // if the owner of this field declared top level objects append them to the
            // field for evaluation
            var securityGroups = new List<AppliedSecurityPolicyGroup>();

            if (template.Parent?.SecurityPolicies?.Count > 0)
                securityGroups.Add(template.Parent.SecurityPolicies);

            if (template.SecurityPolicies?.Count > 0)
                securityGroups.Add(template.SecurityPolicies);

            MethodGraphField field = this.InstantiateField(formatter, template, securityGroups);

            field.Description = template.Description;
            field.IsDeprecated = template.IsDeprecated;
            field.DeprecationReason = template.DeprecationReason;
            field.Complexity = template.Complexity;
            field.FieldSource = template.FieldSource;

            if (template.Arguments != null)
            {
                var argumentMaker = this.CreateArgumentMaker();
                Validation.ThrowIfNull(argumentMaker, nameof(argumentMaker));

                foreach (var argTemplate in template.Arguments)
                {
                    var argumentResult = argumentMaker.CreateArgument(argTemplate);
                    field.Arguments.AddArgument(argumentResult.Argument);

                    result.MergeDependents(argumentResult);
                }
            }

            result.AddDependentRange(template.RetrieveRequiredTypes());

            if (template.UnionProxy != null)
            {
                var unionMaker = new UnionGraphTypeMaker(this.Schema);
                result.AddDependent(unionMaker.CreateGraphType(template.UnionProxy));
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
            IGraphTypeFieldTemplate template,
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

        /// <summary>
        /// Gets the schema this field maker is creating fields for.
        /// </summary>
        /// <value>The schema.</value>
        protected ISchema Schema { get; }
    }
}