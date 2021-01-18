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
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Configuration.Formatting;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.Configuration;
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// A set of configurations that will be applied when this  <see cref="ISchema"/> generated from a set of objects.
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

        /// <summary>
        /// Gets or sets a value indicating whether to disable introspection queries. When disabled,
        /// queries against "__schema" and "__type" will generate a server error. In addition to the obfuscation, this
        /// may provide a performance boost for some large schemas as the initialization will not attempt to
        /// extract and configure metadata from each scalar, object, method and property added to the schema.
        /// (Default: false).
        /// </summary>
        /// <value><c>true</c> if introspection meta fields should NOT be included in the schema; otherwise, <c>false</c>.</value>
        public bool DisableIntrospection { get; set; }

        /// <summary>
        /// <para>Gets or sets a value indicating which fields the templating engine will attempt to include in graph types created from your source code.
        /// You can alter how methods, properties, enum values etc. are injected into your schema by changing the declaration requirements. Setting
        /// this property will affect all graph types for your schema unless they declare an explicit override with the <see cref="GraphTypeAttribute"/>.
        /// </para>
        /// <para>(Default: Require Method Declaration).</para>
        /// </summary>
        /// <value>The declaration requirements for this schema.</value>
        public TemplateDeclarationRequirements FieldDeclarationRequirements { get; set; } = TemplateDeclarationRequirements.Default;

        /// <summary>
        /// <para>Gets or sets an object used to format the declared names in your C# code as various items in the type system
        /// for this <see cref="ISchema" />.
        /// </para>
        ///
        /// <para>
        /// Defaults:
        /// Graph Type Names : ProperCase.<br/>
        /// Field Names: camelCase.<br/>
        /// Enum Values: UPPERCASE.
        /// </para>
        /// </summary>
        /// <value>The graph naming formatter.</value>
        public GraphNameFormatter GraphNamingFormatter { get; set; } = new GraphNameFormatter();

        /// <summary>
        /// Gets the set of operations that can be registered to this schema.
        /// </summary>
        /// <value>The allowed operations.</value>
        public HashSet<GraphCollection> AllowedOperations { get; }
    }
}