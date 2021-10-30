// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Configuration
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using GraphQL.AspNet.Configuration.Formatting;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.Configuration;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// A set of configurations that will be applied when this  <see cref="ISchema" /> generated from a set of objects.
    /// </summary>
    [DebuggerDisplay("Schema Declaration Configuration")]
    public class SchemaDeclarationConfiguration : ISchemaDeclarationConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaDeclarationConfiguration"/> class.
        /// </summary>
        public SchemaDeclarationConfiguration()
        {
            this.AllowedOperations = new HashSet<GraphCollection>();
            this.AllowedOperations.Add(GraphCollection.Query);
            this.AllowedOperations.Add(GraphCollection.Mutation);
        }

        /// <summary>
        /// Merges the specified configuration setttings into this instance.
        /// </summary>
        /// <param name="config">The configuration.</param>
        public void Merge(ISchemaDeclarationConfiguration config)
        {
            if (config == null)
                return;

            this.DisableIntrospection = config.DisableIntrospection;
            this.FieldDeclarationRequirements = config.FieldDeclarationRequirements;
            this.GraphNamingFormatter = config.GraphNamingFormatter;

            if (config.AllowedOperations != null)
            {
                foreach (var op in config.AllowedOperations)
                    this.AllowedOperations.Add(op);
            }
        }

        /// <inheritdoc />
        public bool DisableIntrospection { get; set; }

        /// <inheritdoc />
        public TemplateDeclarationRequirements FieldDeclarationRequirements { get; set; } = TemplateDeclarationRequirements.Default;

        /// <inheritdoc />
        public GraphNameFormatter GraphNamingFormatter { get; set; } = new GraphNameFormatter();

        /// <inheritdoc />
        public HashSet<GraphCollection> AllowedOperations { get; }
    }
}