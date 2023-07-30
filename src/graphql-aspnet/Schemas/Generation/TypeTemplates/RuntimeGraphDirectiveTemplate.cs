// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Schemas.Generation.TypeTemplates
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Internal;
    using GraphQL.AspNet.Interfaces.Schema.RuntimeDefinitions;

    /// <summary>
    /// A "controller template" representing a single runtime configured field (e.g. minimal api).
    /// This template is never cached.
    /// </summary>
    [DebuggerDisplay("{Route.Name} - RuntimeDirective")]
    internal class RuntimeGraphDirectiveTemplate : GraphDirectiveTemplate
    {
        private readonly IMemberInfoProvider _fieldProvider;
        private readonly IGraphQLRuntimeDirectiveDefinition _directiveDefinition;

        /// <summary>
        /// Initializes a new instance of the <see cref="RuntimeGraphDirectiveTemplate" /> class.
        /// </summary>
        /// <param name="directiveDef">A single, runtime configured, directive definition
        /// to templatize for a specfic schema.</param>
        public RuntimeGraphDirectiveTemplate(IGraphQLRuntimeDirectiveDefinition directiveDef)
            : base(typeof(RuntimeExecutionDirective), new RuntimeSchemaItemAttributeProvider(directiveDef))
        {
            _directiveDefinition = directiveDef;
            if (directiveDef?.Resolver?.Method != null)
            {
                _fieldProvider = new MemberInfoProvider(
                    directiveDef.Resolver.Method,
                    new RuntimeSchemaItemAttributeProvider(directiveDef));
            }
        }

        /// <inheritdoc />
        protected override void ParseTemplateDefinition()
        {
            base.ParseTemplateDefinition();

            if (!string.IsNullOrWhiteSpace(_directiveDefinition?.InternalName))
                this.InternalName = _directiveDefinition.InternalName;
        }

        /// <inheritdoc />
        protected override IEnumerable<IMemberInfoProvider> GatherPossibleDirectiveExecutionMethods()
        {
            yield return this._fieldProvider;
        }

        /// <inheritdoc />
        protected override bool CouldBeDirectiveExecutionMethod(IMemberInfoProvider memberInfgo)
        {
            return memberInfgo != null
                && memberInfgo == _fieldProvider
                && base.CouldBeDirectiveExecutionMethod(memberInfgo);
        }

        /// <inheritdoc />
        protected override string DetermineDirectiveName()
        {
            if (_directiveDefinition != null)
                return _directiveDefinition?.Route.Name;

            return base.DetermineDirectiveName();
        }

        /// <inheritdoc />
        public override void ValidateOrThrow()
        {
            if (_directiveDefinition?.Resolver?.Method == null)
            {
                throw new GraphTypeDeclarationException(
                $"Unable to templatize the runtime directive definition of '{_directiveDefinition?.Route.Path ?? "~null~"}' the resolver " +
                    $"is not properly configured.");
            }

            base.ValidateOrThrow();
        }

        /// <inheritdoc />
        public override ItemSource TemplateSource => ItemSource.Runtime;
    }
}