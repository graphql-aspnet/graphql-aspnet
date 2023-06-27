// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Internal.TypeTemplates
{
    using System.Collections.Generic;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Configuration.Templates;
    using GraphQL.AspNet.Interfaces.Internal;

    /// <summary>
    /// A "controller template" representing a single runtime configured field (e.g. minimal api).
    /// This template is never cached.
    /// </summary>
    internal class RuntimeGraphControllerTemplate : GraphControllerTemplate
    {
        private readonly IFieldMemberInfoProvider _fieldProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="RuntimeGraphControllerTemplate" /> class.
        /// </summary>
        /// <param name="fieldDefinition">A single, runtime configured, field definition
        /// to templatize for a specfic schema.</param>
        public RuntimeGraphControllerTemplate(IGraphQLResolvableSchemaItemDefinition fieldDefinition)
            : base(typeof(RuntimeFieldExecutionController))
        {
            if (fieldDefinition.Resolver?.Method == null)
            {
                throw new GraphTypeDeclarationException(
                    $"Unable to templatize the runtime field definition of '{fieldDefinition.Route}' the resolver " +
                    $"is not properly configured.");
            }

            _fieldProvider = new MemberInfoProvider(
                fieldDefinition.Resolver.Method,
                new RuntimeSchemaItemAttributeProvider(fieldDefinition));
        }

        /// <inheritdoc />
        protected override IEnumerable<IFieldMemberInfoProvider> GatherPossibleFieldTemplates()
        {
            yield return _fieldProvider;
        }

        /// <inheritdoc />
        protected override bool CouldBeGraphField(IFieldMemberInfoProvider fieldProvider)
        {
            return fieldProvider != null && fieldProvider == _fieldProvider;
        }
    }
}