// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Configuration.Mvc
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Internal;
    using GraphQL.AspNet.Internal.Interfaces;

    /// <summary>
    /// Preloads data related to a schema  into memory.
    /// </summary>
    internal class SchemaPreCacher
    {
        private readonly HashSet<Type> _parsedTypes;

        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaPreCacher"/> class.
        /// </summary>
        public SchemaPreCacher()
        {
            _parsedTypes = new HashSet<Type>();
        }

        /// <summary>
        /// Iterates over the schema types and ensures the are pre-loaded into the global template cache.
        /// </summary>
        /// <param name="schemaTypes">The schema types.</param>
        public void PrecacheTemplates(IEnumerable<Type> schemaTypes)
        {
            if (schemaTypes == null)
                return;

            foreach (var type in schemaTypes)
            {
                this.PreParseGraphTypeAndChildren(type);
            }
        }

        /// <summary>
        /// Attempts to preparse the graph type (loading it in the global static cache) and if it contains
        /// any child types, parses them as well.
        /// </summary>
        /// <param name="type">The type.</param>
        private void PreParseGraphTypeAndChildren(Type type)
        {
            if (_parsedTypes.Contains(type))
                return;

            _parsedTypes.Add(type);

            // can't preparse scalars (no parsing nessceary)
            if (GraphQLProviders.ScalarProvider.IsScalar(type))
                return;

            // can't preparse union proxies (they aren't parsable graph types)
            if (!GraphValidation.IsParseableType(type))
                return;

            var template = GraphQLProviders.TemplateProvider.ParseType(type);

            if (template is IGraphTypeFieldTemplateContainer fieldContainer)
            {
                foreach (var dependent in fieldContainer.FieldTemplates.Values.SelectMany(x => x.RetrieveRequiredTypes()))
                {
                    this.PreParseGraphTypeAndChildren(dependent.Type);
                }
            }
        }
    }
}