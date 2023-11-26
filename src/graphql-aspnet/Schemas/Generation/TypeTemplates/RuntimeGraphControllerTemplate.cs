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
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Internal;
    using GraphQL.AspNet.Interfaces.Schema.RuntimeDefinitions;

    /// <summary>
    /// A "controller template" representing a single runtime configured field (e.g. minimal api).
    /// This template is never cached.
    /// </summary>
    internal class RuntimeGraphControllerTemplate : GraphControllerTemplate
    {
        private readonly IMemberInfoProvider _fieldProvider;
        private readonly IGraphQLRuntimeResolvedFieldDefinition _fieldDefinition;

        /// <summary>
        /// Initializes a new instance of the <see cref="RuntimeGraphControllerTemplate" /> class.
        /// </summary>
        /// <param name="fieldDefinition">A single, runtime configured, field definition
        /// to templatize for a specfic schema.</param>
        public RuntimeGraphControllerTemplate(IGraphQLRuntimeResolvedFieldDefinition fieldDefinition)
            : base(typeof(RuntimeFieldExecutionController))
        {
            _fieldDefinition = fieldDefinition;
            if (fieldDefinition.Resolver?.Method != null)
            {
                _fieldProvider = new MemberInfoProvider(
                    fieldDefinition.Resolver.Method,
                    new RuntimeSchemaItemAttributeProvider(fieldDefinition));
            }
        }

        /// <inheritdoc />
        protected override IEnumerable<IMemberInfoProvider> GatherPossibleFieldTemplates()
        {
            yield return _fieldProvider;
        }

        /// <inheritdoc />
        protected override bool CouldBeGraphField(IMemberInfoProvider fieldProvider)
        {
            return fieldProvider != null && fieldProvider == _fieldProvider;
        }

        /// <inheritdoc />
        public override void ValidateOrThrow(bool validateChildren = true)
        {
            if (_fieldDefinition?.Resolver?.Method == null)
            {
                throw new GraphTypeDeclarationException(
                $"Unable to templatize the runtime field definition of '{_fieldDefinition?.ItemPath.Path ?? "~null~"}' the resolver " +
                    $"is not properly configured.");
            }

            base.ValidateOrThrow(validateChildren);
        }

        /// <inheritdoc />
        public override ItemSource TemplateSource => ItemSource.Runtime;
    }
}