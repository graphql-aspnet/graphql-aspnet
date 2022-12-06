// *************************************************************
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
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Security;

    /// <summary>
    /// A "maker" capable of producing a qualified <see cref="IDirective"/> from its related <see cref="IGraphDirectiveTemplate"/>.
    /// </summary>
    public class DirectiveMaker : IGraphTypeMaker
    {
        private readonly ISchema _schema;

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectiveMaker"/> class.
        /// </summary>
        /// <param name="schema">The schema.</param>
        public DirectiveMaker(ISchema schema)
        {
            _schema = Validation.ThrowIfNullOrReturn(schema, nameof(schema));
        }

        /// <summary>
        /// Inspects the given type and, in accordance with the rules of this maker, will
        /// generate a complete set of necessary graph types required to support it.
        /// </summary>
        /// <param name="concreteType">The concrete type to incorporate into the schema.</param>
        /// <returns>GraphTypeCreationResult.</returns>
        public GraphTypeCreationResult CreateGraphType(Type concreteType)
        {
            var formatter = _schema.Configuration.DeclarationOptions.GraphNamingFormatter;
            var template = GraphQLProviders.TemplateProvider.ParseType(concreteType) as IGraphDirectiveTemplate;
            if (template == null)
                return null;

            var securityGroups = new List<AppliedSecurityPolicyGroup>();

            if (template.SecurityPolicies?.Count > 0)
                securityGroups.Add(template.SecurityPolicies);

            var result = new GraphTypeCreationResult();

            var directive = new Directive(
                formatter.FormatFieldName(template.Name),
                template.Locations,
                template.ObjectType,
                template.Route,
                template.IsRepeatable,
                template.CreateResolver(),
                securityGroups)
            {
                Description = template.Description,
                Publish = template.Publish,
            };

            // all arguments are required to have the same signature via validation
            // can use any method to fill the arg field list
            var argMaker = new GraphArgumentMaker(_schema);
            foreach (var argTemplate in template.Arguments)
            {
                var argumentResult = argMaker.CreateArgument(directive, argTemplate);
                directive.Arguments.AddArgument(argumentResult.Argument);

                result.MergeDependents(argumentResult);
            }

            result.GraphType = directive;
            result.ConcreteType = concreteType;
            return result;
        }
    }
}