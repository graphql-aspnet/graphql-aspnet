// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Schemas.RuntimeSchemaItemDefinitions
{
    using System.Collections.Generic;
    using System.Linq;
    using GraphQL.AspNet.Interfaces.Configuration.Templates;

    /// <summary>
    /// Represents a single controller that contains ALL of the runtime configured
    /// fields. (e.g. all the fields created via minimal api calls).
    /// </summary>
    internal class RuntimeControllerDefinition
    {
        private List<IGraphQLRuntimeResolvedFieldDefinition> _fields;

        /// <summary>
        /// Initializes a new instance of the <see cref="RuntimeControllerDefinition"/> class.
        /// </summary>
        /// <param name="fields">The fields that are to be a part of this controller.</param>
        public RuntimeControllerDefinition(IEnumerable<IGraphQLRuntimeResolvedFieldDefinition> fields)
        {
            _fields = new List<IGraphQLRuntimeResolvedFieldDefinition>(fields ?? Enumerable.Empty<IGraphQLRuntimeResolvedFieldDefinition>());
        }
    }
}