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
    using GraphQL.AspNet.Interfaces.Configuration;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// A set of configuration options that will be applied when this  <see cref="ISchema" />
    /// generated from a set of objects.
    /// </summary>
    [DebuggerDisplay("Schema Declaration Configuration")]
    public class SchemaDeclarationConfiguration : ISchemaDeclarationConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaDeclarationConfiguration"/> class.
        /// </summary>
        public SchemaDeclarationConfiguration()
        {
            this.AllowedOperations = new HashSet<GraphOperationType>();
            this.ArgumentBindingRules = new HashSet<SchemaArgumentBindingRules>();

            this.AllowedOperations.Add(GraphOperationType.Query);
            this.AllowedOperations.Add(GraphOperationType.Mutation);

            this.ArgumentBindingRules
                .Add(Configuration.SchemaArgumentBindingRules.ArgumentsPreferQueryResolution);
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

            if (config.ArgumentBindingRules != null)
            {
                foreach (var rule in config.ArgumentBindingRules)
                    this.ArgumentBindingRules.Add(rule);
            }
        }

        /// <inheritdoc />
        public bool DisableIntrospection { get; set; }

        /// <inheritdoc />
        public HashSet<SchemaArgumentBindingRules> ArgumentBindingRules { get;  }

        /// <inheritdoc />
        public TemplateDeclarationRequirements FieldDeclarationRequirements { get; set; } = TemplateDeclarationRequirements.Default;

        /// <inheritdoc />
        public GraphNameFormatter GraphNamingFormatter { get; set; } = new GraphNameFormatter();

        /// <inheritdoc />
        public HashSet<GraphOperationType> AllowedOperations { get; }
    }
}