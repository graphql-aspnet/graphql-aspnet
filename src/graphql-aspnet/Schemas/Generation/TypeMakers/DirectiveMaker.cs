﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Schemas.Generation.TypeMakers
{
    using System.Collections.Generic;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Interfaces.Configuration;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.Internal;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Security;

    /// <summary>
    /// A "maker" capable of producing a qualified <see cref="IDirective"/> from its related <see cref="IGraphDirectiveTemplate"/>.
    /// </summary>
    public class DirectiveMaker : IGraphTypeMaker
    {
        private readonly ISchema _schema;
        private readonly IGraphArgumentMaker _argMaker;

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectiveMaker" /> class.
        /// </summary>
        /// <param name="schema">The schema this maker will create directives for.</param>
        /// <param name="argumentMaker">The maker used to generate new arguments on any created directives.</param>
        public DirectiveMaker(ISchema schema, IGraphArgumentMaker argumentMaker)
        {
            _schema = Validation.ThrowIfNullOrReturn(schema, nameof(schema));
            _argMaker = Validation.ThrowIfNullOrReturn(argumentMaker, nameof(argumentMaker));
        }

        /// <inheritdoc />
        public virtual GraphTypeCreationResult CreateGraphType(IGraphTypeTemplate typeTemplate)
        {
            if (!(typeTemplate is IGraphDirectiveTemplate template))
                return null;

            var formatter = _schema.Configuration.DeclarationOptions.GraphNamingFormatter;

            var securityGroups = new List<AppliedSecurityPolicyGroup>();

            if (template.SecurityPolicies?.Count > 0)
                securityGroups.Add(template.SecurityPolicies);

            var result = new GraphTypeCreationResult();

            var directive = new Directive(
                formatter.FormatFieldName(template.Name),
                template.InternalName,
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
            foreach (var argTemplate in template.Arguments)
            {
                if (GraphArgumentMaker.IsArgumentPartOfSchema(argTemplate, _schema))
                {
                    var argumentResult = _argMaker.CreateArgument(directive, argTemplate);
                    directive.Arguments.AddArgument(argumentResult.Argument);

                    result.MergeDependents(argumentResult);
                }
            }

            result.GraphType = directive;

            // only assign a concrete type if one was declared
            if (template.ObjectType != typeof(RuntimeExecutionDirective))
                result.ConcreteType = template.ObjectType;

            return result;
        }
    }
}